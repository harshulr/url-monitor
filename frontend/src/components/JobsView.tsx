import { useCallback, useEffect, useState } from 'react';
import { Accordion, Badge, Center, Group, Loader, Paper, Text, Title } from '@mantine/core';
import { getJobResults, getJobs, type JobResult, type JobSummary } from '../api/client';
import { DataTable, type Column } from './DataTable';
import { TriggerBadge } from './TriggerBadge';

const resultColumns: Column<JobResult>[] = [
  { header: 'Endpoint', render: (r) => r.name },
  {
    header: 'Status',
    render: (r) => (
      <Badge color={r.isSuccess ? 'green' : 'red'}>{r.statusCode ?? (r.isSuccess ? 'OK' : 'ERR')}</Badge>
    ),
  },
  { header: 'Latency', render: (r) => `${r.responseTimeMs} ms` },
  { header: 'Detail', render: (r) => <Text size="xs" c="dimmed" lineClamp={1}>{r.errorMessage ?? r.url}</Text> },
];

/// Job History: past runs as an accordion; expanding a run lazy-loads the endpoints it checked.
export function JobsView() {
  const [jobs, setJobs] = useState<JobSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [openId, setOpenId] = useState<string | null>(null);
  const [resultsByJob, setResultsByJob] = useState<Record<string, JobResult[]>>({});
  const [loadingId, setLoadingId] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setJobs(await getJobs());
      setError(null);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load jobs');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const handleToggle = (value: string | null) => {
    setOpenId(value);
    if (value && !resultsByJob[value]) {
      setLoadingId(value);
      getJobResults(value)
        .then((data) => setResultsByJob((prev) => ({ ...prev, [value]: data })))
        .catch(() => setResultsByJob((prev) => ({ ...prev, [value]: [] })))
        .finally(() => setLoadingId(null));
    }
  };

  if (loading) {
    return (
      <Center h={200}>
        <Loader />
      </Center>
    );
  }

  if (error) {
    return (
      <Paper withBorder p="md" radius="md">
        <Text c="red">Failed to load: {error}</Text>
      </Paper>
    );
  }

  if (jobs.length === 0) {
    return <Text c="dimmed">No runs yet. Wait for the scheduler or hit “Sync Now”.</Text>;
  }

  return (
    <>
      <Title order={4} mb="sm">
        Past Runs
      </Title>

      <Accordion variant="separated" value={openId} onChange={handleToggle}>
        {jobs.map((j) => (
          <Accordion.Item key={j.id} value={j.id}>
            <Accordion.Control>
              <Group justify="space-between" pr="md">
                <Group gap="sm">
                  <Text fw={500}>{new Date(j.executedAt).toLocaleString()}</Text>
                  <TriggerBadge trigger={j.triggerType} />
                </Group>
                <Group gap="xs">
                  <Badge color="green" variant="light">
                    {j.successCount} ok
                  </Badge>
                  <Badge color="red" variant="light">
                    {j.failureCount} failed
                  </Badge>
                </Group>
              </Group>
            </Accordion.Control>
            <Accordion.Panel>
              {loadingId === j.id ? (
                <Center h={80}>
                  <Loader size="sm" />
                </Center>
              ) : (
                <DataTable columns={resultColumns} rows={resultsByJob[j.id] ?? []} rowKey={(r) => r.resultId} striped />
              )}
            </Accordion.Panel>
          </Accordion.Item>
        ))}
      </Accordion>
    </>
  );
}

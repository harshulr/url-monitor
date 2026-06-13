import { useState } from 'react';
import { Center, Loader, Paper, Text } from '@mantine/core';
import type { UrlStatus } from '../api/client';
import { SummaryCards } from './SummaryCards';
import { EndpointsTable } from './EndpointsTable';
import { HistoryPanel } from './HistoryPanel';

interface DashboardProps {
  urls: UrlStatus[];
  loading: boolean;
  error: string | null;
}

/// Composes the dashboard: summary metrics + endpoints grid + per-URL history drawer.
export function Dashboard({ urls, loading, error }: DashboardProps) {
  const [selected, setSelected] = useState<UrlStatus | null>(null);

  if (loading && urls.length === 0) {
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

  return (
    <>
      <SummaryCards urls={urls} />
      <EndpointsTable urls={urls} onSelect={setSelected} />
      <HistoryPanel url={selected} onClose={() => setSelected(null)} />
    </>
  );
}

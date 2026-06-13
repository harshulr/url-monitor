import { useCallback, useEffect, useState } from 'react';
import { AppShell, Button, Container, Group, Title } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconActivityHeartbeat, IconRefresh } from '@tabler/icons-react';

import { getUrls, triggerSync, type UrlStatus } from './api/client';
import { Dashboard } from './components/Dashboard';

const POLL_INTERVAL_MS = 15_000;

export default function App() {
  const [urls, setUrls] = useState<UrlStatus[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [syncing, setSyncing] = useState(false);

  const refresh = useCallback(async () => {
    try {
      setUrls(await getUrls());
      setError(null);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to load');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refresh();
    const id = setInterval(refresh, POLL_INTERVAL_MS);
    return () => clearInterval(id);
  }, [refresh]);

  const handleSync = async () => {
    setSyncing(true);
    try {
      await triggerSync();
      notifications.show({
        title: 'Sync queued',
        message: 'A health check run has been pushed into the queue.',
        color: 'teal',
      });
      setTimeout(refresh, 3000); // give the engine a moment, then pull fresh results
    } catch (e: unknown) {
      notifications.show({
        title: 'Sync failed',
        message: e instanceof Error ? e.message : 'Could not reach the backend.',
        color: 'red',
      });
    } finally {
      setSyncing(false);
    }
  };

  return (
    <AppShell header={{ height: 60 }} padding="md">
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between">
          <Group gap="xs">
            <IconActivityHeartbeat size={24} />
            <Title order={3}>URL Health Monitor</Title>
          </Group>
          <Button leftSection={<IconRefresh size={16} />} loading={syncing} onClick={handleSync}>
            Sync Now
          </Button>
        </Group>
      </AppShell.Header>

      <AppShell.Main>
        <Container size="lg">
          <Dashboard urls={urls} loading={loading} error={error} />
        </Container>
      </AppShell.Main>
    </AppShell>
  );
}

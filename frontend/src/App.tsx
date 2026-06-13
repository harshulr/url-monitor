import { useCallback, useEffect, useState } from 'react';
import { AppShell, Container, Group, Title } from '@mantine/core';
import { IconActivityHeartbeat } from '@tabler/icons-react';

import { getUrls, type UrlStatus } from './api/client';
import { Dashboard } from './components/Dashboard';

const POLL_INTERVAL_MS = 15_000;

export default function App() {
  const [urls, setUrls] = useState<UrlStatus[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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

  return (
    <AppShell header={{ height: 60 }} padding="md">
      <AppShell.Header>
        <Group h="100%" px="md" gap="xs">
          <IconActivityHeartbeat size={24} />
          <Title order={3}>URL Health Monitor</Title>
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

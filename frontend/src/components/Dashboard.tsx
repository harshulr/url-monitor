import { Center, Loader, Paper, Text } from '@mantine/core';
import type { UrlStatus } from '../api/client';
import { SummaryCards } from './SummaryCards';
import { EndpointsTable } from './EndpointsTable';

interface DashboardProps {
  urls: UrlStatus[];
  loading: boolean;
  error: string | null;
}

/// Composes the dashboard: summary metrics + the endpoints grid. Handles loading/error states.
export function Dashboard({ urls, loading, error }: DashboardProps) {
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
      <EndpointsTable urls={urls} />
    </>
  );
}

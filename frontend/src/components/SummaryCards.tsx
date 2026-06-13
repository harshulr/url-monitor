import { SimpleGrid } from '@mantine/core';
import type { UrlStatus } from '../api/client';
import { StatCard } from './StatCard';

/// Top-of-dashboard metrics: total tracked, online, offline.
export function SummaryCards({ urls }: { urls: UrlStatus[] }) {
  const online = urls.filter((u) => u.lastIsSuccess === true).length;
  const offline = urls.filter((u) => u.lastIsSuccess === false).length;

  return (
    <SimpleGrid cols={{ base: 1, sm: 3 }} mb="lg">
      <StatCard label="Total Tracked" value={urls.length} />
      <StatCard label="Online" value={online} color="green" />
      <StatCard label="Offline" value={offline} color={offline > 0 ? 'red' : undefined} />
    </SimpleGrid>
  );
}

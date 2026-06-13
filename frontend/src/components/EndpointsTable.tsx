import { Anchor, Title } from '@mantine/core';
import type { UrlStatus } from '../api/client';
import { StatusBadge } from './StatusBadge';
import { DataTable, type Column } from './DataTable';

const columns: Column<UrlStatus>[] = [
  { header: 'Name', render: (u) => u.name },
  {
    header: 'URL',
    render: (u) => (
      <Anchor href={u.url} target="_blank" rel="noreferrer" onClick={(e) => e.stopPropagation()}>
        {u.url}
      </Anchor>
    ),
  },
  { header: 'Status', render: (u) => <StatusBadge status={u} /> },
  { header: 'Latency', render: (u) => (u.lastResponseTimeMs != null ? `${u.lastResponseTimeMs} ms` : '—') },
  { header: 'Last Checked', render: (u) => (u.lastCheckedAt ? new Date(u.lastCheckedAt).toLocaleTimeString() : '—') },
];

/// The main tracking grid. Clicking a row opens its history.
export function EndpointsTable({ urls, onSelect }: { urls: UrlStatus[]; onSelect: (url: UrlStatus) => void }) {
  return (
    <>
      <Title order={4} mb="sm">
        Monitored Endpoints
      </Title>
      <DataTable columns={columns} rows={urls} rowKey={(u) => u.id} onRowClick={onSelect} minWidth={600} />
    </>
  );
}

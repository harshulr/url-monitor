import { Anchor, Table, Title } from '@mantine/core';
import type { UrlStatus } from '../api/client';
import { StatusBadge } from './StatusBadge';

/// The main tracking grid: one row per monitored endpoint with its current status.
export function EndpointsTable({ urls }: { urls: UrlStatus[] }) {
  const rows = urls.map((u) => (
    <Table.Tr key={u.id}>
      <Table.Td>{u.name}</Table.Td>
      <Table.Td>
        <Anchor href={u.url} target="_blank" rel="noreferrer">
          {u.url}
        </Anchor>
      </Table.Td>
      <Table.Td>
        <StatusBadge status={u} />
      </Table.Td>
      <Table.Td>{u.lastResponseTimeMs != null ? `${u.lastResponseTimeMs} ms` : '—'}</Table.Td>
      <Table.Td>{u.lastCheckedAt ? new Date(u.lastCheckedAt).toLocaleTimeString() : '—'}</Table.Td>
    </Table.Tr>
  ));

  return (
    <>
      <Title order={4} mb="sm">
        Monitored Endpoints
      </Title>

      <Table.ScrollContainer minWidth={600}>
        <Table highlightOnHover verticalSpacing="sm">
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Name</Table.Th>
              <Table.Th>URL</Table.Th>
              <Table.Th>Status</Table.Th>
              <Table.Th>Latency</Table.Th>
              <Table.Th>Last Checked</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>{rows}</Table.Tbody>
        </Table>
      </Table.ScrollContainer>
    </>
  );
}

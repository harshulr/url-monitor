import { useEffect, useState } from 'react';
import { Badge, Center, Drawer, Loader, Text } from '@mantine/core';
import { getHistory, type HistoryItem, type UrlStatus } from '../api/client';
import { DataTable, type Column } from './DataTable';
import { TriggerBadge } from './TriggerBadge';

const columns: Column<HistoryItem>[] = [
  { header: 'Time', render: (h) => new Date(h.timestamp).toLocaleString() },
  {
    header: 'Status',
    render: (h) => (
      <Badge color={h.isSuccess ? 'green' : 'red'} variant="light">
        {h.statusCode ?? (h.isSuccess ? 'OK' : 'ERR')}
      </Badge>
    ),
  },
  { header: 'Latency', render: (h) => `${h.responseTimeMs} ms` },
  { header: 'Trigger', render: (h) => <TriggerBadge trigger={h.triggerType} /> },
];

/// Sliding panel showing the chronological check log for one endpoint.
export function HistoryPanel({ url, onClose }: { url: UrlStatus | null; onClose: () => void }) {
  const [items, setItems] = useState<HistoryItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!url) return;

    let cancelled = false;
    setLoading(true);
    setError(null);

    getHistory(url.id)
      .then((data) => !cancelled && setItems(data))
      .catch((e: unknown) => !cancelled && setError(e instanceof Error ? e.message : 'Failed to load history'))
      .finally(() => !cancelled && setLoading(false));

    return () => {
      cancelled = true;
    };
  }, [url]);

  return (
    <Drawer opened={url !== null} onClose={onClose} position="right" size="lg" title={url?.name}>
      {loading ? (
        <Center h={120}>
          <Loader />
        </Center>
      ) : error ? (
        <Text c="red">{error}</Text>
      ) : items.length === 0 ? (
        <Text c="dimmed">No checks recorded yet.</Text>
      ) : (
        <DataTable columns={columns} rows={items} rowKey={(h) => h.id} striped />
      )}
    </Drawer>
  );
}

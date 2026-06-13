import { Badge } from '@mantine/core';
import type { UrlStatus } from '../api/client';

/// Maps a URL's latest state to a colored badge: green 2xx, red error, gray not-yet-checked.
export function StatusBadge({ status }: { status: UrlStatus }) {
  if (status.lastIsSuccess === null) {
    return <Badge color="gray" variant="light">Pending</Badge>;
  }
  if (status.lastIsSuccess) {
    return <Badge color="green">{status.lastStatusCode ?? 'OK'}</Badge>;
  }
  return <Badge color="red">{status.lastStatusCode ?? 'ERR'}</Badge>;
}

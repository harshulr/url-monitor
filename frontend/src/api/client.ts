const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5139';

export interface UrlStatus {
  id: string;
  name: string;
  url: string;
  isActive: boolean;
  lastStatusCode: number | null;
  lastIsSuccess: boolean | null;
  lastResponseTimeMs: number | null;
  lastCheckedAt: string | null;
  lastErrorMessage: string | null;
}

export interface HistoryItem {
  id: string;
  timestamp: string;
  statusCode: number | null;
  responseTimeMs: number;
  isSuccess: boolean;
  errorMessage: string | null;
}

async function getJson<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`);
  if (!res.ok) throw new Error(`Request failed: ${res.status} ${res.statusText}`);
  return res.json() as Promise<T>;
}

export function getUrls(): Promise<UrlStatus[]> {
  return getJson<UrlStatus[]>('/api/urls');
}

export function getHistory(id: string): Promise<HistoryItem[]> {
  return getJson<HistoryItem[]>(`/api/urls/${id}/history`);
}

export async function triggerSync(): Promise<void> {
  const res = await fetch(`${BASE_URL}/api/urls/sync`, { method: 'POST' });
  if (!res.ok && res.status !== 202) throw new Error(`Sync failed: ${res.status} ${res.statusText}`);
}

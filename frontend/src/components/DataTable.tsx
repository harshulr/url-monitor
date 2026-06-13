import type { ReactNode } from 'react';
import { Table } from '@mantine/core';

export interface Column<T> {
  header: string;
  render: (row: T) => ReactNode;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  rows: T[];
  rowKey: (row: T) => string;
  onRowClick?: (row: T) => void;
  minWidth?: number;
  striped?: boolean;
}

/// Generic, column-driven table. Reused by EndpointsTable, HistoryPanel, and JobsView.
export function DataTable<T>({ columns, rows, rowKey, onRowClick, minWidth, striped }: DataTableProps<T>) {
  const body = rows.map((row) => (
    <Table.Tr
      key={rowKey(row)}
      onClick={onRowClick ? () => onRowClick(row) : undefined}
      style={onRowClick ? { cursor: 'pointer' } : undefined}
    >
      {columns.map((col) => (
        <Table.Td key={col.header}>{col.render(row)}</Table.Td>
      ))}
    </Table.Tr>
  ));

  const table = (
    <Table highlightOnHover={!!onRowClick} striped={striped} verticalSpacing="sm">
      <Table.Thead>
        <Table.Tr>
          {columns.map((col) => (
            <Table.Th key={col.header}>{col.header}</Table.Th>
          ))}
        </Table.Tr>
      </Table.Thead>
      <Table.Tbody>{body}</Table.Tbody>
    </Table>
  );

  return minWidth ? <Table.ScrollContainer minWidth={minWidth}>{table}</Table.ScrollContainer> : table;
}

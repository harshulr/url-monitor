import { Paper, Text } from '@mantine/core';

interface StatCardProps {
  label: string;
  value: number;
  color?: string;
}

/// A single summary metric card.
export function StatCard({ label, value, color }: StatCardProps) {
  return (
    <Paper withBorder p="md" radius="md">
      <Text size="xs" c="dimmed" tt="uppercase" fw={700}>
        {label}
      </Text>
      <Text size="xl" fw={700} c={color}>
        {value}
      </Text>
    </Paper>
  );
}

import { Badge } from '@mantine/core';

/// Badge for a run's trigger type: Manual (grape) vs Scheduled (blue).
export function TriggerBadge({ trigger }: { trigger: string }) {
  return (
    <Badge variant="outline" color={trigger === 'Manual' ? 'grape' : 'blue'}>
      {trigger}
    </Badge>
  );
}

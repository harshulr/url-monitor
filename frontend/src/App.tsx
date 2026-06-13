import { AppShell, Container, Group, Title } from '@mantine/core';
import { IconActivityHeartbeat } from '@tabler/icons-react';

export default function App() {
  return (
    <AppShell header={{ height: 60 }} padding="md">
      <AppShell.Header>
        <Group h="100%" px="md" gap="xs">
          <IconActivityHeartbeat size={24} />
          <Title order={3}>URL Health Monitor</Title>
        </Group>
      </AppShell.Header>

      <AppShell.Main>
        <Container size="lg">
        </Container>
      </AppShell.Main>
    </AppShell>
  );
}

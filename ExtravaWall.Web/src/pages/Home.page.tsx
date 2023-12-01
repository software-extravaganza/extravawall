import { Welcome } from '../components/Welcome/Welcome';
import { ColorSchemeToggle } from '../components/ColorSchemeToggle/ColorSchemeToggle';
import { Box, Title } from '@mantine/core';
import Page from '@/components/Page';

export function HomePage() {
  return (
    <Page>
      <>
        <Welcome />
        <ColorSchemeToggle />
        <Title align="center">This is title</Title>
      </>
    </Page>
  );
}

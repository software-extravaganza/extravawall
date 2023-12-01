import {
  Anchor,
  AppShell,
  Badge,
  Breadcrumbs,
  Burger,
  NavLink,
  ScrollArea,
  useComputedColorScheme,
  useMantineColorScheme,
} from '@mantine/core';
import { useColorScheme, useDisclosure } from '@mantine/hooks';
import React from 'react';
import { DockviewApi, DockviewReact, DockviewReadyEvent, IDockviewPanelProps } from 'dockview';
import '../../node_modules/dockview/dist/styles/dockview.css';
import { IconHome2, IconWall } from '@tabler/icons-react';
import LogoCompany from './LogoCompany';
import LogoMark from './LogoMark';
export class BladeManager {
  onReady: ((event: DockviewReadyEvent) => void) | undefined;
  dockviewApi: DockviewApi | undefined;
  uniqueId: number = 0;
  bladeComponents: any = {
    // default: (props: IDockviewPanelProps) => {
    //   return <div>{props.api.title}</div>;
    // }
    //test
  };

  constructor() {}

  addBlade(name: string, blade: React.ReactNode) {
    this.uniqueId++;
    if (this.bladeComponents[name] == undefined) {
      this.bladeComponents[name] = (props: IDockviewPanelProps) => blade;
    }

    this.dockviewApi?.addPanel({
      id: this.uniqueId + '',
      component: name,
      title: name,
    });
  }

  removeBlade(name: string) {
    if (this.bladeComponents[name] !== undefined) {
      this.bladeComponents = this.bladeComponents.filter((bladeName: string) => bladeName !== name);
    }
  }

  setOnReady(onReady: (event: DockviewReadyEvent) => void) {
    this.onReady = onReady;
  }

  setDockviewApi(api: DockviewApi) {
    this.dockviewApi = api;
    for (let name in this.bladeComponents) {
      this.addBlade(name, this.bladeComponents[name]);
    }
  }
}

interface Props {
  getBladeManager: (manager: BladeManager) => void;
}

function Layout({ getBladeManager }: Props) {
  const [opened, { toggle }] = useDisclosure();
  const { colorScheme } = useMantineColorScheme();
  const computedColorScheme = useComputedColorScheme('dark', { getInitialValueInEffect: true });
  const bladeManager = new BladeManager();
  const onReady = (event: DockviewReadyEvent) => {
    bladeManager.setDockviewApi(event.api);
  };

  getBladeManager?.(bladeManager);

  const items = [
    { title: 'Mantine', href: '#' },
    { title: 'Mantine hooks', href: '#' },
    { title: 'use-id', href: '#' },
  ].map((item, index) => (
    <Anchor href={item.href} key={index}>
      {item.title}
    </Anchor>
  ));

  return (
    <AppShell
      header={{ height: 40 }}
      navbar={{ width: 200, breakpoint: 'sm', collapsed: { mobile: !opened } }}
      padding="md"
    >
      <AppShell.Header>
        <Burger
          opened={opened}
          onClick={toggle}
          hiddenFrom="sm"
          size="sm"
          className="extrava-burger"
        />
        <LogoMark />
        <Breadcrumbs className="extrava-breakcrumbs">{items}</Breadcrumbs>
      </AppShell.Header>

      <AppShell.Navbar p="xs">
        {' '}
        <NavLink label="Home" leftSection={<IconHome2 size="1rem" stroke={1.5} />} />
        <NavLink
          label="Firewall"
          childrenOffset={28}
          leftSection={<IconWall size="1rem" stroke={1.5} />}
        >
          <NavLink
            label="Rules"
            leftSection={
              <Badge size="xs" variant="filled" color="red" w={16} h={16} p={0}>
                3
              </Badge>
            }
          />
        </NavLink>
      </AppShell.Navbar>

      <AppShell.Main>
        <DockviewReact
          className={
            computedColorScheme === 'dark'
              ? 'dockview-theme-dracula extrava-dockview'
              : 'dockview-theme-light extrava-dockview'
          }
          disableAutoResizing={false}
          onReady={onReady}
          components={bladeManager.bladeComponents}
        />
      </AppShell.Main>
    </AppShell>
  );
}

export default Layout;

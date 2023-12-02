import {
  Anchor,
  AppShell,
  Badge,
  Breadcrumbs,
  Burger,
  Container,
  NavLink,
  ScrollArea,
  useComputedColorScheme,
  useMantineColorScheme,
} from '@mantine/core';
import { useColorScheme, useDisclosure } from '@mantine/hooks';
import React, { Component, MouseEventHandler, ReactElement, ReactNode } from 'react';
import {
  DefaultTab,
  DockviewApi,
  DockviewReact,
  DockviewReadyEvent,
  IDockviewPanelProps,
  IGroupPanelInitParameters,
  ITabRenderer,
  Parameters,
  PanelUpdateEvent,
  DockviewGroupPanel,
  GroupPanelPartInitParameters,
  DockviewPanelApi,
} from 'dockview';
import '../../node_modules/dockview/dist/styles/dockview.css';
import { IconHome2, IconPin, IconSquareRoundedX, IconWall, IconX } from '@tabler/icons-react';
import LogoCompany from './LogoCompany';
import LogoMark from './LogoMark';
import ReactDOM from 'react-dom';
import { as } from 'vitest/dist/reporters-5f784f42';

type ExtravaTabProps = {
  title: string;
  api: DockviewPanelApi;
};

class ExtravaTab extends React.Component<ExtravaTabProps, any, any> implements ITabRenderer {
  private _element: ReactElement;
  private _content: ReactElement;
  private closeAction: ReactElement;
  private pinAction: ReactElement;

  //
  private title: string;
  private api: DockviewPanelApi;

  get element(): HTMLElement {
    return this._element as any as HTMLElement;
  }

  constructor(props: ExtravaTabProps) {
    super(props);
    this.title = props.title;
    this.api = props.api;
    this._content = React.createElement(() => <div className="tab-content"></div>);
    this.closeAction = React.createElement(() => (
      <div className="tab-action" onClick={(ev) => this.onCloseClicked(ev)}>
        <IconX size="1rem" stroke={1.5} />
      </div>
    ));

    this.pinAction = React.createElement(() => (
      <div className="tab-action" onClick={(ev) => this.onPinClicked(ev)}>
        <IconPin size="1rem" stroke={1.5} />
      </div>
    ));

    this._element = React.createElement(() => {
      return (
        <div className="extrava-tab default-tab">
          {this._content}
          <div className="action-container">
            <ul className="tab-list">
              {this.pinAction}
              {this.closeAction}
            </ul>
          </div>
        </div>
      );
    });
  }

  public onCloseClicked(ev: React.MouseEvent<HTMLDivElement>): void {
    ev.preventDefault();
    this.api?.close();
  }

  public onPinClicked(ev: React.MouseEvent<HTMLDivElement>): void {
    ev.preventDefault();
  }

  public update(event: PanelUpdateEvent): void {
    this.render();
  }

  focus(): void {}

  public init(params: GroupPanelPartInitParameters): void {}

  onGroupChange(_group: DockviewGroupPanel): void {
    this.render();
  }

  onPanelVisibleChange(_isPanelVisible: boolean): void {
    this.render();
  }

  public layout(_width: number, _height: number): void {}

  public render(): ReactNode {
    // console.table(this._content);
    // if (this._content.props.textContent !== this.params.title) {
    //   this._content.props.textContent = this.params.title;
    // }

    if (this.title !== undefined) {
      this._content = React.createElement(() => (
        <div className="tab-content">
          <span>{this.title}</span>
        </div>
      ));
    }

    return this._element;
  }
}

export type ComponentConstructor<T> = {
  new (id: string, component: string): T;
};

export class BladeManager {
  onReady: ((event: DockviewReadyEvent) => void) | undefined;
  dockviewApi: DockviewApi | undefined;
  uniqueId: number = 0;
  bladeComponents: any = {};
  // default: (props: IDockviewPanelProps) => {
  //   return <div>{props.api.title}</div>;
  // }
  tabComponents: any = {
    default: (props: IDockviewPanelProps) => {
      return <ExtravaTab title={props.params.title} api={props.api} />;
    },
  };

  constructor() {}

  addBlade(name: string, blade: React.ReactNode) {
    this.uniqueId++;
    if (this.bladeComponents[name] == undefined) {
      this.bladeComponents[name] = (props: IDockviewPanelProps) => (
        <ScrollArea className="extrava-dockscroll">{blade}</ScrollArea>
      );
    }

    this.dockviewApi?.addPanel({
      id: this.uniqueId + '',
      component: name,
      tabComponent: 'default',
      title: name,
      params: { title: name },
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
          tabComponents={bladeManager.tabComponents}
        />
      </AppShell.Main>
    </AppShell>
  );
}

export default Layout;
function addDisposableListener(action: HTMLElement, arg1: string, arg2: (ev: any) => void) {
  throw new Error('Function not implemented.');
}
function convert_to_react(arg0: any, arg1: string) {
  throw new Error('Function not implemented.');
}

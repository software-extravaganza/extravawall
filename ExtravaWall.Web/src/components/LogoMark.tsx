import { useComputedColorScheme } from '@mantine/core';

function LogoMark() {
  const computedColorScheme = useComputedColorScheme('dark', { getInitialValueInEffect: true });

  return (
    <a
      href="/"
      className={
        computedColorScheme === 'dark' ? 'logo-mark logo-mark-dark' : 'logo-mark logo-mark-light'
      }
    ></a>
  );
}
export default LogoMark;

import { useComputedColorScheme } from '@mantine/core';

function LogoCompany() {
  const computedColorScheme = useComputedColorScheme('dark', { getInitialValueInEffect: true });
  return (
    <div
      className={
        computedColorScheme === 'dark'
          ? 'logo-company logo-company-dark'
          : 'logo-company logo-company-light'
      }
    ></div>
  );
}

export default LogoCompany;

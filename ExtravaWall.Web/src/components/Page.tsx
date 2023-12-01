import React from 'react';

interface Props {
  children: React.ReactNode;
}

function Page({ children }: Props) {
  return <div>{children}</div>;
}

export default Page;

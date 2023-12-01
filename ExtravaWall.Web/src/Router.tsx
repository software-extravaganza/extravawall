import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { HomePage } from './pages/Home.page';
import Layout, { BladeManager } from './components/Layout';

const router = createBrowserRouter([
  {
    path: '/',
    element: <HomePage />,
  },
]);

function getBladeManager(bladeManager: BladeManager) {
  console.log('getBladeManager');
  bladeManager.addBlade('home', <RouterProvider router={router} />);
}

export function Router() {
  return (
    <>
      <Layout getBladeManager={getBladeManager} />
    </>
  );
}

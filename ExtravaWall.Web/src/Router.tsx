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
  bladeManager.addBlade('Home', <RouterProvider router={router} />);
  bladeManager.addBlade('Home2 sdfsdf sdf sdf sd', <RouterProvider router={router} />);
}

export function Router() {
  return (
    <>
      <Layout getBladeManager={getBladeManager} />
    </>
  );
}

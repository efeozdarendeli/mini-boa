import { Outlet } from 'react-router';
import { ConnectionBanner } from '../container/ConnectionBanner';
import { ContainerProvider } from '../container/ContainerProvider';

/** Her ekran ContainerProvider içinde çalışır; böylece hepsi hata sayfası ve dialog gösterebilir. */
export function RootLayout() {
  return (
    <ContainerProvider>
      <ConnectionBanner />
      <Outlet />
    </ContainerProvider>
  );
}

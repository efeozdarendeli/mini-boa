import { Navigate, Outlet } from 'react-router';
import { useSession } from '../proxy/useSession';
import { Routes } from './routes';

/**
 * Giriş gerektiren ekranların kapısı. Oturum yoksa (hiç giriş yapılmadıysa ya da süresi dolduysa)
 * giriş ekranına yönlendirir. Asıl güvenlik backend'de; bu sadece kullanıcıyı doğru ekrana götürür.
 */
export function RequireAuth() {
  const session = useSession();
  return session ? <Outlet /> : <Navigate to={Routes.Login} replace />;
}

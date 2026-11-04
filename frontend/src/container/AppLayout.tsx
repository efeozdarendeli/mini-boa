import { LogOut } from 'lucide-react';
import { Link, Outlet } from 'react-router';
import { Routes } from '../app/routes';
import { useMessaging } from '../messaging/useMessaging';
import { useSession } from '../proxy/useSession';
import { BrandLogo } from '../shared/components/BrandLogo';
import { Button } from '../shared/components/Button';
import { LanguageSwitch } from '../shared/components/LanguageSwitch';
import styles from './AppLayout.module.css';
import { InactivityGuard } from './InactivityGuard';
import { useLogout } from './useLogout';

/** Karşılama ekranı dışındaki tüm ekranların ortak çerçevesi: üst bar + içerik. */
export function AppLayout() {
  const { getMessage } = useMessaging();
  const session = useSession();
  const logout = useLogout();

  return (
    <>
      <header className={styles.header}>
        <div className={styles.inner}>
          <Link to={session ? Routes.Dashboard : Routes.Welcome} className={styles.brandLink}>
            <BrandLogo />
          </Link>
          <div className={styles.actions}>
            <LanguageSwitch compact />
            {session && (
              <Button variant="outline" icon={<LogOut size={18} aria-hidden="true" />} onClick={() => void logout()}>
                {getMessage('Common', 'Logout')}
              </Button>
            )}
          </div>
        </div>
      </header>
      <main className={styles.content}>
        <Outlet />
      </main>
      {/* Giriş yapılmışsa hareketsizlik izlenir; çıkışta bileşen kalkar, zamanlayıcılar da durur. */}
      {session && <InactivityGuard />}
    </>
  );
}

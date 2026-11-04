import { Clock } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import { formatMessage } from '../messaging/messagingHelper';
import { useMessaging } from '../messaging/useMessaging';
import { Button } from '../shared/components/Button';
import dialogStyles from './Dialog.module.css';
import { useLogout } from './useLogout';

/** Bu kadar süre hiçbir etkileşim olmazsa uyarı gösterilir. */
export const IDLE_WARNING_MS = 2 * 60_000;

/** Uyarı gösterildikten sonra bu kadar saniye içinde "Devam et" denmezse çıkış yapılır. */
export const LOGOUT_COUNTDOWN_SECONDS = 30;

const ACTIVITY_EVENTS = ['pointerdown', 'keydown', 'wheel', 'touchstart'] as const;

/**
 * Self-servis cihazlarda müşteri işini bitirip cihazın başından ayrılabilir; oturum açık kalırsa
 * arkadan gelen kişi hesaba erişir. Bu bileşen hareketsizliği izler: 2 dakika sonra uyarır,
 * 30 saniye içinde cevap gelmezse oturumu kapatır.
 */
export function InactivityGuard() {
  const { getMessage } = useMessaging();
  const logout = useLogout();
  const [isWarning, setIsWarning] = useState(false);
  const [secondsLeft, setSecondsLeft] = useState(LOGOUT_COUNTDOWN_SECONDS);
  const idleTimer = useRef<number | undefined>(undefined);

  // Her etkileşimde zamanlayıcı baştan başlar; uyarı açıkken etkileşim sayılmaz ("Devam et" gerekir).
  useEffect(() => {
    if (isWarning) {
      return;
    }

    const restart = () => {
      window.clearTimeout(idleTimer.current);
      idleTimer.current = window.setTimeout(() => {
        setSecondsLeft(LOGOUT_COUNTDOWN_SECONDS);
        setIsWarning(true);
      }, IDLE_WARNING_MS);
    };

    restart();
    ACTIVITY_EVENTS.forEach((event) => window.addEventListener(event, restart, { passive: true }));
    return () => {
      window.clearTimeout(idleTimer.current);
      ACTIVITY_EVENTS.forEach((event) => window.removeEventListener(event, restart));
    };
  }, [isWarning]);

  // Uyarı açılınca çıkış için tek bir zamanlayıcı kurulur; saniye sayacı sadece ekranda gösterim içindir.
  // (Çıkışı sayacın sıfıra inmesine bağlamak, ekran güncellemesi gecikirse çıkışı da geciktirirdi.)
  useEffect(() => {
    if (!isWarning) {
      return;
    }

    const logoutTimer = window.setTimeout(() => void logout(), LOGOUT_COUNTDOWN_SECONDS * 1000);
    const countdown = window.setInterval(() => setSecondsLeft((seconds) => Math.max(seconds - 1, 0)), 1000);
    return () => {
      window.clearTimeout(logoutTimer);
      window.clearInterval(countdown);
    };
  }, [isWarning, logout]);

  if (!isWarning) {
    return null;
  }

  return (
    <div className={dialogStyles.backdrop}>
      <div role="alertdialog" aria-modal="true" aria-labelledby="idle-title" className={dialogStyles.dialog}>
        <Clock size={44} className={dialogStyles.Warning} aria-hidden="true" />
        <h2 id="idle-title" className={dialogStyles.title}>
          {getMessage('Session', 'IdleTitle')}
        </h2>
        <p className={dialogStyles.message} aria-live="polite">
          {formatMessage(getMessage('Session', 'IdleMessage'), secondsLeft)}
        </p>
        <Button fullWidth autoFocus onClick={() => setIsWarning(false)}>
          {getMessage('Session', 'StayLoggedIn')}
        </Button>
      </div>
    </div>
  );
}

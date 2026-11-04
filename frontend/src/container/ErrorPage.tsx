import { TriangleAlert } from 'lucide-react';
import { useNavigate } from 'react-router';
import { Routes } from '../app/routes';
import { useMessaging } from '../messaging/useMessaging';
import { Button } from '../shared/components/Button';
import { Card } from '../shared/components/Card';
import styles from './ErrorPage.module.css';

interface ErrorPageProps {
  message: string;
  onDismiss: () => void;
}

/** İşlem devam edemeyecek şekilde başarısız olduğunda ekranın yerine gösterilir. */
export function ErrorPage({ message, onDismiss }: ErrorPageProps) {
  const { getMessage } = useMessaging();
  const navigate = useNavigate();

  const returnHome = () => {
    onDismiss();
    navigate(Routes.Welcome);
  };

  return (
    <main className={styles.page}>
      <Card className={styles.card} role="alert">
        <TriangleAlert className={styles.icon} size={56} aria-hidden="true" />
        <p className={styles.message}>{message}</p>
        <Button onClick={returnHome}>{getMessage('Common', 'ReturnHome')}</Button>
      </Card>
    </main>
  );
}

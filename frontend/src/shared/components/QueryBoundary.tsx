import type { UseQueryResult } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { useMessaging } from '../../messaging/useMessaging';
import { Alert } from './Alert';
import { Button } from './Button';
import { Spinner } from './Spinner';

interface QueryBoundaryProps<T> {
  query: UseQueryResult<T>;
  children: (data: T) => ReactNode;
}

/**
 * Sunucudan veri bekleyen her bölüm için aynı davranış: yüklenirken spinner,
 * hata olursa mesaj + "Tekrar dene", veri gelince içerik. Ekranlar bu üç durumu tek tek yazmaz.
 */
export function QueryBoundary<T>({ query, children }: QueryBoundaryProps<T>) {
  const { getMessage } = useMessaging();

  if (query.isPending) {
    return <Spinner label={getMessage('Common', 'Loading')} />;
  }

  if (query.isError) {
    return (
      <Alert
        tone="error"
        action={
          <Button variant="outline" onClick={() => void query.refetch()}>
            {getMessage('Common', 'Retry')}
          </Button>
        }
      >
        {query.error.message}
      </Alert>
    );
  }

  return <>{children(query.data)}</>;
}

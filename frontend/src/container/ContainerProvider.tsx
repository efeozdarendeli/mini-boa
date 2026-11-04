import { useMemo, useState, type ReactNode } from 'react';
import { useLocation } from 'react-router';
import { ContainerContext, DialogTypes, type ContainerManager, type DialogType } from './containerContext';
import { Dialog } from './Dialog';
import { ErrorPage } from './ErrorPage';

interface DialogState {
  message: string;
  type: DialogType;
  pathname: string;
}

interface ErrorState {
  message: string;
  pathname: string;
}

/** Tüm ekranları sarar; hata sayfası ve dialog gösterimini tek yerden yönetir. */
export function ContainerProvider({ children }: { children: ReactNode }) {
  const { pathname } = useLocation();
  const [error, setError] = useState<ErrorState | null>(null);
  const [dialog, setDialog] = useState<DialogState | null>(null);

  // Adres router'dan alınır (window.location'dan değil); hata/dialog açıldığı ekranla birlikte saklanır.
  const manager = useMemo<ContainerManager>(
    () => ({
      showErrorPage: (message) => setError({ message, pathname }),
      showDialog: (message, type = DialogTypes.Info) => setDialog({ message, type, pathname }),
    }),
    [pathname],
  );

  // Hata ve dialog açıldıkları ekrana aittir; başka bir ekrana geçilince artık gösterilmezler.
  const visibleError = error?.pathname === pathname ? error : null;
  const visibleDialog = dialog?.pathname === pathname ? dialog : null;

  return (
    <ContainerContext.Provider value={manager}>
      {visibleError ? <ErrorPage message={visibleError.message} onDismiss={() => setError(null)} /> : children}
      {visibleDialog && (
        <Dialog message={visibleDialog.message} type={visibleDialog.type} onClose={() => setDialog(null)} />
      )}
    </ContainerContext.Provider>
  );
}

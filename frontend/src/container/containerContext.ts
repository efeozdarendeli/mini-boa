import { createContext, useContext } from 'react';

/** BOA'daki DialogTypes karşılığı. */
export const DialogTypes = {
  Info: 'Info',
  Warning: 'Warning',
  Error: 'Error',
  Success: 'Success',
} as const;

export type DialogType = (typeof DialogTypes)[keyof typeof DialogTypes];

export interface ContainerManager {
  /** Ekranın yerine tam sayfa hata gösterir (BOA: ContainerManager.ShowErrorPage). */
  showErrorPage: (message: string) => void;

  /** Ekranın üstünde kapatılabilir bir mesaj kutusu gösterir. */
  showDialog: (message: string, type?: DialogType) => void;
}

export const ContainerContext = createContext<ContainerManager | null>(null);

/**
 * Ekranların hata/mesaj göstermek için kullandığı tek giriş noktası
 * (BOA'daki UIManagerFactory.ContainerManager karşılığı).
 */
export function useContainerManager(): ContainerManager {
  const manager = useContext(ContainerContext);
  if (!manager) {
    throw new Error('useContainerManager must be used inside <ContainerProvider>.');
  }
  return manager;
}

/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Backend API adresi; verilmezse http://localhost:5080 kullanılır. */
  readonly VITE_API_BASE_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

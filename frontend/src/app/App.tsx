import { QueryClientProvider } from '@tanstack/react-query';
// react-router/dom'daki RouterProvider, React DOM'un flushSync'ini bağlar (bkz. AppLayout'taki çıkış).
import { RouterProvider } from 'react-router/dom';
import { queryClient } from './queryClient';
import { router } from './router';

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  );
}

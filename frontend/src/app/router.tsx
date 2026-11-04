import { createBrowserRouter, Navigate, type RouteObject } from 'react-router';
import { AppLayout } from '../container/AppLayout';
import { AccountsView } from '../ui/xtm/Accounts/AccountsView';
import { BecomeACustomerView } from '../ui/xtm/BecomeACustomer/BecomeACustomerView';
import { DashboardView } from '../ui/xtm/Dashboard/DashboardView';
import { ForgotPasswordView } from '../ui/xtm/ForgotPassword/ForgotPasswordView';
import { LoginView } from '../ui/xtm/Login/LoginView';
import { OpenAccountView } from '../ui/xtm/OpenAccount/OpenAccountView';
import { ProfileView } from '../ui/xtm/Profile/ProfileView';
import { RatesView } from '../ui/xtm/Rates/RatesView';
import { TransferView } from '../ui/xtm/Transfer/TransferView';
import { WelcomeView } from '../ui/xtm/Welcome/WelcomeView';
import { RequireAuth } from './RequireAuth';
import { RootLayout } from './RootLayout';
import { Routes } from './routes';

/** Ekran ağacı; testler aynı ağacı bellekteki bir router ile kullanır. */
export const routes: RouteObject[] = [
  {
    element: <RootLayout />,
    children: [
      { path: Routes.Welcome, element: <WelcomeView /> },
      {
        element: <AppLayout />,
        children: [
          { path: Routes.Login, element: <LoginView /> },
          { path: Routes.ForgotPassword, element: <ForgotPasswordView /> },
          { path: Routes.BecomeACustomer, element: <BecomeACustomerView /> },
          { path: Routes.Rates, element: <RatesView /> },
          {
            // Bu gruptaki ekranlar giriş gerektirir.
            element: <RequireAuth />,
            children: [
              { path: Routes.Dashboard, element: <DashboardView /> },
              { path: Routes.Accounts, element: <AccountsView /> },
              { path: Routes.OpenAccount, element: <OpenAccountView /> },
              { path: Routes.Transfer, element: <TransferView /> },
              { path: Routes.Profile, element: <ProfileView /> },
            ],
          },
        ],
      },
      { path: '*', element: <Navigate to={Routes.Welcome} replace /> },
    ],
  },
];

export const router = createBrowserRouter(routes);

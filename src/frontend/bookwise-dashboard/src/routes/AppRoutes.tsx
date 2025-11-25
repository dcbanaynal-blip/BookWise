import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { DashboardLayout } from '../layouts/DashboardLayout'
import { DashboardPage } from '../pages/DashboardPage'
import { TransactionsPage } from '../pages/TransactionsPage'
import { ReceiptsPage } from '../pages/ReceiptsPage'
import { ReportsPage } from '../pages/ReportsPage'
import { ProtectedRoute } from '../components/ProtectedRoute'
import { LoginPage } from '../pages/LoginPage'

const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <DashboardLayout>
          <DashboardPage />
        </DashboardLayout>
      </ProtectedRoute>
    ),
  },
  {
    path: '/transactions',
    element: (
      <ProtectedRoute>
        <DashboardLayout>
          <TransactionsPage />
        </DashboardLayout>
      </ProtectedRoute>
    ),
  },
  {
    path: '/receipts',
    element: (
      <ProtectedRoute>
        <DashboardLayout>
          <ReceiptsPage />
        </DashboardLayout>
      </ProtectedRoute>
    ),
  },
  {
    path: '/reports',
    element: (
      <ProtectedRoute>
        <DashboardLayout>
          <ReportsPage />
        </DashboardLayout>
      </ProtectedRoute>
    ),
  },
])

export function AppRoutes() {
  return <RouterProvider router={router} />
}

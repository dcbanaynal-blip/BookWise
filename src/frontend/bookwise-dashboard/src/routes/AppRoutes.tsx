import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { DashboardLayout } from '../layouts/DashboardLayout'
import { DashboardPage } from '../pages/DashboardPage'
import { TransactionsPage } from '../pages/TransactionsPage'
import { ReceiptsPage } from '../pages/ReceiptsPage'
import { ReportsPage } from '../pages/ReportsPage'

const router = createBrowserRouter([
  {
    path: '/',
    element: (
      <DashboardLayout>
        <DashboardPage />
      </DashboardLayout>
    ),
  },
  {
    path: '/transactions',
    element: (
      <DashboardLayout>
        <TransactionsPage />
      </DashboardLayout>
    ),
  },
  {
    path: '/receipts',
    element: (
      <DashboardLayout>
        <ReceiptsPage />
      </DashboardLayout>
    ),
  },
  {
    path: '/reports',
    element: (
      <DashboardLayout>
        <ReportsPage />
      </DashboardLayout>
    ),
  },
])

export function AppRoutes() {
  return <RouterProvider router={router} />
}

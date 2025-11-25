import { Navigate, Route, Routes } from 'react-router-dom'
import { Dashboard, Auth } from '@/layouts'
import { ProtectedRoute } from './components/ProtectedRoute'

export function App() {
  return (
    <Routes>
      <Route
        path="/dashboard/*"
        element={
          <ProtectedRoute>
            <Dashboard />
          </ProtectedRoute>
        }
      />
      <Route path="/auth/*" element={<Auth />} />
      <Route path="*" element={<Navigate to="/dashboard/home" replace />} />
    </Routes>
  )
}

export default App

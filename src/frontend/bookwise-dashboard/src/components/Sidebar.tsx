import { NAV_ROUTES } from '../config/navigation'
import { Link, useLocation } from 'react-router-dom'

export function Sidebar() {
  const location = useLocation()

  return (
    <aside className="hidden w-64 flex-shrink-0 border-r border-slate-200 bg-white/80 pt-8 lg:flex">
      <nav className="w-full">
        <div className="px-6 pb-6 text-center">
          <p className="text-xl font-bold text-primary">BookWise</p>
          <p className="text-xs uppercase tracking-widest text-slate-400">
            Creative Tim x React
          </p>
        </div>
        <ul className="space-y-1 px-4">
          {NAV_ROUTES.map((route) => {
            const Icon = route.icon
            const isActive = location.pathname === route.path
            return (
              <li key={route.path}>
                <Link
                  to={route.path}
                  className={`flex items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium transition ${
                    isActive
                      ? 'bg-primary/10 text-primary'
                      : 'text-slate-500 hover:bg-slate-100'
                  }`}
                >
                  <Icon className="h-5 w-5" />
                  {route.label}
                </Link>
              </li>
            )
          })}
        </ul>
      </nav>
    </aside>
  )
}

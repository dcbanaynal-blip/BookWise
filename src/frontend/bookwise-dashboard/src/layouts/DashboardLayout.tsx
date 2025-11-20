import { Sidebar } from '../components/Sidebar'
import { TopBar } from '../components/TopBar'
import type { PropsWithChildren } from 'react'

export function DashboardLayout({ children }: PropsWithChildren) {
  return (
    <div className="flex min-h-screen bg-slate-100/60">
      <Sidebar />
      <main className="flex flex-1 flex-col">
        <TopBar />
        <section className="flex-1 px-4 py-6 lg:px-8">{children}</section>
      </main>
    </div>
  )
}

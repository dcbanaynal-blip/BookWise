import { StatBadge } from '@bookwise/component-library'
import { KpiCard } from '../components/KpiCard'
import { ReceiptUploader } from '../components/ReceiptUploader'

const kpis = [
  { label: 'Revenue (MTD)', value: '$120,450', trend: '+8.5% vs last month' },
  { label: 'Expenses (MTD)', value: '$64,110', trend: '+2.1% vs last month' },
  { label: 'Net Income', value: '$56,340', trend: '+14.3% vs last month' },
  { label: 'Open Receipts', value: '38 pending', trend: 'All SLA: 98%' },
]

export function DashboardPage() {
  return (
    <div className="space-y-6">
      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {kpis.map((kpi) => (
          <KpiCard key={kpi.label} {...kpi} />
        ))}
      </section>

      <section className="grid gap-6 xl:grid-cols-3">
        <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm shadow-slate-100 xl:col-span-2">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-lg font-semibold text-slate-900">
                Income Statement Snapshot
              </h2>
              <p className="text-sm text-slate-500">
                Revenue vs Expense trend for the current quarter
              </p>
            </div>
            <select className="rounded-xl border border-slate-200 px-3 py-1 text-sm text-slate-600">
              <option>This Quarter</option>
              <option>Last Quarter</option>
              <option>Year to Date</option>
            </select>
          </div>
          <div className="mt-8 h-64 rounded-xl border border-dashed border-slate-200 bg-gradient-to-r from-primary/10 via-white to-accent/10"></div>
          <div className="mt-6 flex flex-wrap gap-3">
            <StatBadge label="OCR Success" value="98.2%" tone="success" />
            <StatBadge label="Receipts in Queue" value="12" tone="warning" />
          </div>
        </div>
        <ReceiptUploader />
      </section>
    </div>
  )
}

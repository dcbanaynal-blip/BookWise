type Props = {
  label: string
  value: string
  trend?: string
}

export function KpiCard({ label, value, trend }: Props) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm shadow-slate-100">
      <p className="text-xs font-semibold uppercase tracking-widest text-slate-400">
        {label}
      </p>
      <p className="mt-2 text-2xl font-bold text-slate-900">{value}</p>
      {trend && <p className="text-sm text-emerald-500">{trend}</p>}
    </div>
  )
}

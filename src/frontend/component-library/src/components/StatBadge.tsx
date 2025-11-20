import type { ReactNode } from 'react'

export type StatBadgeProps = {
  label: string
  value: ReactNode
  tone?: 'primary' | 'success' | 'warning'
}

const toneMap: Record<Required<StatBadgeProps>['tone'], string> = {
  primary: 'bg-blue-100 text-blue-700',
  success: 'bg-emerald-100 text-emerald-700',
  warning: 'bg-amber-100 text-amber-700',
}

export function StatBadge({ label, value, tone = 'primary' }: StatBadgeProps) {
  return (
    <div className={`inline-flex flex-col rounded-2xl px-4 py-3 ${toneMap[tone]}`}>
      <span className="text-xs uppercase tracking-widest opacity-70">{label}</span>
      <span className="text-lg font-semibold">{value}</span>
    </div>
  )
}

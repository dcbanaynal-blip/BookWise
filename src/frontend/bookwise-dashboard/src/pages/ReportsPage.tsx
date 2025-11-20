export function ReportsPage() {
  return (
    <div className="space-y-6">
      <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm shadow-slate-100">
        <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <h2 className="text-lg font-semibold text-slate-900">Income Statement</h2>
            <p className="text-sm text-slate-500">
              Run on-demand reports by fiscal period and export to CSV/PDF.
            </p>
          </div>
          <div className="flex gap-2">
            <select className="rounded-xl border border-slate-200 px-3 py-1.5 text-sm text-slate-600">
              <option>Monthly</option>
              <option>Quarterly</option>
              <option>Yearly</option>
            </select>
            <button className="rounded-xl border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-600">
              Export CSV
            </button>
          </div>
        </div>
        <div className="mt-6 space-y-3 text-sm">
          <div className="flex justify-between border-b border-dashed border-slate-200 pb-2">
            <span className="text-slate-500">Revenue</span>
            <span className="font-semibold text-slate-900">$420,000</span>
          </div>
          <div className="flex justify-between border-b border-dashed border-slate-200 pb-2">
            <span className="text-slate-500">Cost of Goods Sold</span>
            <span className="font-semibold text-slate-900">$120,000</span>
          </div>
          <div className="flex justify-between border-b border-dashed border-slate-200 pb-2">
            <span className="text-slate-500">Operating Expenses</span>
            <span className="font-semibold text-slate-900">$190,000</span>
          </div>
          <div className="flex justify-between pt-2">
            <span className="text-sm font-semibold uppercase tracking-widest text-slate-500">
              Net Income
            </span>
            <span className="text-lg font-bold text-emerald-600">$110,000</span>
          </div>
        </div>
      </section>
      <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm shadow-slate-100">
        <h2 className="text-lg font-semibold text-slate-900">Report Schedule</h2>
        <p className="text-sm text-slate-500">
          Configure automation windows that drop statements into cloud storage.
        </p>
        <div className="mt-4 grid gap-4 md:grid-cols-2">
          {['Daily OCR Digest', 'Weekly Income Statement'].map((label) => (
            <div key={label} className="rounded-xl border border-slate-200 p-4">
              <p className="font-semibold text-slate-900">{label}</p>
              <p className="text-xs uppercase tracking-widest text-slate-400">Active</p>
              <button className="mt-3 rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
                Edit schedule
              </button>
            </div>
          ))}
        </div>
      </section>
    </div>
  )
}

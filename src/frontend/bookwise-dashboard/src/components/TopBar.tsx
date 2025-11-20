import { MagnifyingGlassIcon, BellIcon } from '@heroicons/react/24/outline'

export function TopBar() {
  return (
    <header className="flex flex-col gap-4 border-b border-slate-200 bg-white px-6 py-4 lg:flex-row lg:items-center lg:justify-between">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Financial Dashboard</h1>
        <p className="text-sm text-slate-500">
          Monitor income, expenses, and OCR queue health in real time.
        </p>
      </div>
      <div className="flex flex-1 items-center justify-end gap-3">
        <div className="relative hidden max-w-md flex-1 lg:block">
          <MagnifyingGlassIcon className="absolute left-3 top-2.5 h-5 w-5 text-slate-400" />
          <input
            type="search"
            placeholder="Search transactions or receipts"
            className="w-full rounded-xl border border-slate-200 bg-slate-50 py-2 pl-10 pr-4 text-sm text-slate-700 outline-none focus:border-primary focus:bg-white"
          />
        </div>
        <button className="rounded-full border border-slate-200 p-2 text-slate-500 hover:text-primary">
          <BellIcon className="h-5 w-5" />
        </button>
        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/15 text-sm font-semibold text-primary">
          JD
        </div>
      </div>
    </header>
  )
}

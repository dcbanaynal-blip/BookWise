const mockTransactions = [
  {
    reference: 'TRX-1001',
    description: 'Stripe payout',
    date: '2025-11-14',
    amount: 6420,
    type: 'Revenue',
  },
  {
    reference: 'TRX-1002',
    description: 'AWS invoice',
    date: '2025-11-13',
    amount: -1820,
    type: 'Expense',
  },
]

export function TransactionsPage() {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm shadow-slate-100">
      <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-900">Transactions</h2>
          <p className="text-sm text-slate-500">
            Manage double-entry records synced from the accounting core.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <input
            type="text"
            placeholder="Search reference"
            className="rounded-xl border border-slate-200 px-3 py-1.5 text-sm text-slate-600"
          />
          <button className="rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-white shadow-lg shadow-primary/30">
            New Transaction
          </button>
        </div>
      </div>
      <div className="mt-5 overflow-x-auto">
        <table className="min-w-full divide-y divide-slate-100 text-sm">
          <thead>
            <tr className="text-left text-xs uppercase tracking-widest text-slate-500">
              <th className="px-4 py-2">Reference</th>
              <th className="px-4 py-2">Description</th>
              <th className="px-4 py-2">Date</th>
              <th className="px-4 py-2">Type</th>
              <th className="px-4 py-2 text-right">Amount</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {mockTransactions.map((trx) => (
              <tr key={trx.reference}>
                <td className="px-4 py-3 font-semibold text-slate-900">{trx.reference}</td>
                <td className="px-4 py-3 text-slate-600">{trx.description}</td>
                <td className="px-4 py-3 text-slate-500">{trx.date}</td>
                <td className="px-4 py-3">
                  <span
                    className={`rounded-full px-3 py-1 text-xs font-semibold ${
                      trx.type === 'Revenue'
                        ? 'bg-emerald-100 text-emerald-700'
                        : 'bg-rose-100 text-rose-700'
                    }`}
                  >
                    {trx.type}
                  </span>
                </td>
                <td className="px-4 py-3 text-right font-semibold text-slate-900">
                  {trx.amount > 0 ? '+' : '-'}${Math.abs(trx.amount).toLocaleString()}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

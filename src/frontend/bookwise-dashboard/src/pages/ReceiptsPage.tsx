const mockReceipts = [
  { id: 1, vendor: 'Uber', total: 42.5, status: 'Completed', uploadedAt: '2025-11-14' },
  { id: 2, vendor: 'AWS', total: 1200, status: 'Processing', uploadedAt: '2025-11-14' },
  { id: 3, vendor: 'Local Cafe', total: 18.75, status: 'Pending', uploadedAt: '2025-11-13' },
]

const statusColor: Record<string, string> = {
  Pending: 'text-amber-600 bg-amber-100',
  Processing: 'text-blue-600 bg-blue-100',
  Completed: 'text-emerald-600 bg-emerald-100',
  Failed: 'text-rose-600 bg-rose-100',
}

export function ReceiptsPage() {
  return (
    <div className="grid gap-6 xl:grid-cols-2">
      <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm shadow-slate-100">
        <h2 className="text-lg font-semibold text-slate-900">Receipt Inbox</h2>
        <p className="text-sm text-slate-500">
          Each upload persists binary data to SQL and streams OCR to AWS Textract.
        </p>
        <div className="mt-5 divide-y divide-slate-100">
          {mockReceipts.map((receipt) => (
            <div key={receipt.id} className="flex items-center justify-between py-3">
              <div>
                <p className="font-semibold text-slate-900">{receipt.vendor}</p>
                <p className="text-xs text-slate-500">{receipt.uploadedAt}</p>
              </div>
              <div className="text-right">
                <p className="font-semibold text-slate-900">
                  ${receipt.total.toFixed(2)}
                </p>
                <span
                  className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${
                    statusColor[receipt.status] ?? 'bg-slate-100 text-slate-600'
                  }`}
                >
                  {receipt.status}
                </span>
              </div>
            </div>
          ))}
        </div>
      </div>
      <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm shadow-slate-100">
        <h2 className="text-lg font-semibold text-slate-900">Auto Categorization</h2>
        <p className="text-sm text-slate-500">
          NLP and ML heuristics suggest accounts before entries post to the GL.
        </p>
        <div className="mt-6 space-y-4">
          {mockReceipts.map((receipt) => (
            <div
              key={`suggestion-${receipt.id}`}
              className="rounded-xl border border-slate-200 p-4 hover:border-primary"
            >
              <p className="text-sm font-semibold text-slate-900">
                {receipt.vendor}{' '}
                <span className="text-xs font-normal text-slate-500">
                  {receipt.uploadedAt}
                </span>
              </p>
              <p className="text-sm text-slate-500">
                Suggested Account:{' '}
                <span className="font-semibold text-primary">Travel &amp; Meals</span>
              </p>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

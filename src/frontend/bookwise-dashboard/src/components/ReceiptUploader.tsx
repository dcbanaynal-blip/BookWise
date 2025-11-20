import { CloudArrowUpIcon } from '@heroicons/react/24/outline'
import { useState } from 'react'

export function ReceiptUploader() {
  const [fileName, setFileName] = useState<string>()

  const handleFile = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (file) {
      setFileName(file.name)
    }
  }

  return (
    <div className="rounded-2xl border border-dashed border-slate-300 bg-white p-6 text-center">
      <CloudArrowUpIcon className="mx-auto h-12 w-12 text-slate-400" />
      <p className="mt-3 text-lg font-semibold text-slate-900">
        Upload receipt or invoice
      </p>
      <p className="text-sm text-slate-500">PNG, JPG, or PDF up to 10MB</p>
      <label className="mt-5 inline-flex cursor-pointer items-center justify-center rounded-full bg-primary px-5 py-2 text-sm font-semibold text-white shadow-lg shadow-primary/30">
        Select File
        <input type="file" className="hidden" onChange={handleFile} />
      </label>
      {fileName && (
        <p className="mt-2 text-xs text-slate-500">Ready to upload: {fileName}</p>
      )}
    </div>
  )
}

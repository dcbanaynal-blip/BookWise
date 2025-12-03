# Receipt Upload & OCR Load Test (Phase 5.1)

## Scenario
- **Objective:** Validate throughput for concurrent uploads of large receipts and the downstream OCR pipeline.
- **Tooling:** `tools/ReceiptLoadTester` harness (in-memory SQL + simulated OCR pipeline delays).
- **Parameters:** `200` receipts, `32` parallel uploads, `2 MB` payload per receipt, OCR batch size `25`, artificial delays `8 ms` (preprocess) / `12 ms` (extract).

## Results (2025-12-03)
| Metric | Value |
| --- | --- |
| Upload duration | `00:00:01.65` |
| Upload throughput | `≈121 receipts/sec` |
| OCR duration | `00:00:13.32` |
| OCR throughput | `≈15 receipts/sec` |
| End-to-end duration | `00:00:14.98` |
| Max queue depth | `200` |
| Failures | `0` |

Interpretation:
- Upload path (API + queue) comfortably handles 2 MB receipts with >100 req/s on a dev workstation.
- The OCR stage dominates runtime; even with modest simulated CPU waits (8/12 ms) the worker processes ~15 receipts/sec. Real OCR costs will be higher, so this is a lower bound for capacity planning.

## How to rerun
```bash
dotnet run --project tools/ReceiptLoadTester/ReceiptLoadTester.csproj -- \
  --receipts 200 \
  --parallel 32 \
  --size-mb 2 \
  --batch 25 \
  --preprocess-delay-ms 8 \
  --extract-delay-ms 12
```

Flags:
- `--receipts` total upload count.
- `--parallel` concurrent upload workers.
- `--size-mb` simulated payload size per receipt.
- `--batch` records processed per OCR pass.
- `--preprocess-delay-ms` / `--extract-delay-ms` mimic compute cost per job.

## Follow-ups
- Run the harness against real SQL + Magick/Tesseract to capture production-grade timings.
- Capture worker CPU/memory using `dotnet-counters` during the run for full telemetry.

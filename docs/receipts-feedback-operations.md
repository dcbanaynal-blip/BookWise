# Receipt Feedback Workflow & Operations

This note captures how receipts flow from upload through human review and how to keep that loop healthy in production.

## Workflow overview
1. **Upload (API / UI)** – Receipts arrive through `ReceiptsService.CreateReceiptAsync`, land in SQL, and enqueue a `ReceiptProcessingJob` with `Status = Pending`.
2. **OCR worker – preprocessing** – `ReceiptOcrPipeline.PreprocessPendingReceiptsAsync` normalizes the image, updates the receipt to `Status = Processing`, and stamps `ReceiptProcessingJob.StartedAt`.
3. **OCR worker – extraction** – `ReceiptOcrPipeline.ExtractReceiptContentAsync` produces the OCR text + confidence, flips the receipt to `Status = Completed`, and exposes the document for review.
4. **Manual review + approval** – Accountants review Completed receipts, capture overrides through `ReceiptsService.ApproveReceiptAsync`, and those decisions become the feedback artifacts (`ReceiptDecisions`).
5. **Auto-categorization refresher** – `AutoCategorizationRuleRefresher` periodically aggregates consistent decisions into `AccountSuggestionRules`, which power future recommendations.

When any step is interrupted, receipts sit in `ReceiptProcessingJobs` or `Receipt` tables until operators intervene.

## Monitoring & dashboards
The OCR worker now emits structured backlog telemetry via `ReceiptBacklogMonitor`:

- Configuration lives under `BookWise.OcrWorker/appsettings*.json` → `BacklogMonitoring`.
- Every `SnapshotIntervalMinutes` (default: 5) the worker logs either `Receipt backlog snapshot {...}` or `Receipt backlog alert {...}` with the following payload:
  - `Snapshot.PendingReceipts`, `Snapshot.ProcessingReceipts`, `Snapshot.AwaitingReviewReceipts`, `Snapshot.FailedReceipts`
  - `Snapshot.PendingJobs`, `Snapshot.ProcessingJobs`, `Snapshot.FailedJobs`
  - `Snapshot.OldestPendingReceiptAge`, `Snapshot.OldestPendingJobAge`, `Snapshot.OldestProcessingJobAge` (ISO 8601 duration)
  - `PendingReceiptAlert`, `PendingJobAlert`, `ProcessingJobAlert`, `HasAlert`
- Alert thresholds are driven by the same config (e.g., `PendingReceiptAlertMinutes = 30` will fire if any receipt has been Pending for ≥30 minutes).

### App Insights workbook recipe
1. Create a new **Logs** query pinned to a workbook tile:

    ```kusto
    traces
    | where message startswith "Receipt backlog"
    | extend pending = toint(customDimensions["Snapshot.PendingReceipts"])
           , processing = toint(customDimensions["Snapshot.ProcessingReceipts"])
           , awaitingReview = toint(customDimensions["Snapshot.AwaitingReviewReceipts"])
           , pendingAge = todouble(customDimensions["Snapshot.OldestPendingReceiptAge"])
           , pendingAlert = tobool(customDimensions["PendingReceiptAlert"])
           , jobAlert = tobool(customDimensions["PendingJobAlert"]) or tobool(customDimensions["ProcessingJobAlert"])
    | summarize
        pending = avg(pending),
        processing = avg(processing),
        awaitingReview = avg(awaitingReview),
        pendingAgeMinutes = avg(pendingAge) / 600000000
      by bin(timestamp, 5m)
    ```

2. Plot `pending` / `processing` as stacked columns and overlay `pendingAgeMinutes` as a line to visualize backlog drift.
3. Convert the same query into an alert rule filtered by `PendingReceiptAlert == true or PendingJobAlert == true`, so operators receive an email/Teams ping when receipts stall.

## Runbook for stuck receipts
1. **Confirm the alert** – open the alert instance and copy the `ReceiptBacklogStatus` payload (it gives counts + ages).
2. **Drill into SQL** – check `ReceiptProcessingJobs` where `Status IN (0,1)` ordered by `CreatedAt`. If jobs are old but counts are small, a single row may contain a bad payload – inspect `ErrorMessage`.
3. **Requeue or retry** – for transient OCR failures, set `Receipt.Status = Pending`, delete/insert the queue row, and allow the worker to pick it up. For consistent errors, download the binary to reproduce locally.
4. **Escalate to dev** – if the backlog alert coincides with OCR worker crashes or deployment events, capture logs plus the alert payload and open an incident ticket.
5. **Close the loop** – after the backlog clears, document the root cause in this file (new section) and adjust `BacklogMonitoring` thresholds if alerts were noisy.

Keeping these steps documented plus the dashboards in place satisfies Phase 4.5 of the Receipts Capture & OCR plan.

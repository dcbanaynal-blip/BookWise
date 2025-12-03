# Receipts Capture & OCR Plan

Roadmap for delivering end-to-end receipt ingestion (upload → OCR → review → transaction link). Each phase contains scoped, checkbox tasks so we can track progress incrementally.

---

## Phase 1 – API Foundations
- [x] **1.1** Define receipt DTOs (upload request/response, listing, detail, line items).
- [x] **1.2** Add `ReceiptsController` with endpoints:
  - [x] **1.2.1** `POST /api/receipts` (multipart upload, persist metadata + binary, enqueue OCR job).
  - [x] **1.2.2** `GET /api/receipts` with filters/pagination.
  - [x] **1.2.3** `GET /api/receipts/{id}` for detailed view (includes OCR text, line items, flags).
- [x] **1.3** Implement file storage abstraction (SQL varbinary vs. blob storage) and wire to controller, ensuring invoice images are transmitted/stored as raw binary to preserve fidelity for OCR.
- [x] **1.4** Add FluentValidation + size/MIME vetting; enforce role access (Bookkeeper upload, Accountant review).
- [x] **1.5** Seed initial OCR job queue integration (DB table or background command) consumed by worker.

## Phase 2 – OCR Worker Enhancements
- [x] **2.1** Extend worker to poll queued receipts (new status column or queue table).
- [x] **2.2** Normalize images (orientation, grayscale) before OCR run.
- [x] **2.3** Extract header/line items, tax/VAT indicators, and populate `ReceiptLineItem` entities.
- [x] **2.4** Capture confidence scores + raw OCR text for auditing.
- [x] **2.5** Update worker logging/telemetry for success/failure tracking; add retry policy.

- [x] **3.5** Provide controls to link receipt to existing transaction or start a new draft.
- [x] **3.6** Surface suggested accounts from auto-categorization (read-only for now) with manual override inputs.

## Phase 4 – Transaction Integration & Feedback Loop
- [ ] **4.1** When a receipt is approved, generate or update the related transaction (pre-populated entry stubs).
- [ ] **4.2** Persist accept/override data (selected account, VAT flag, totals) to feed auto-categorization logic.
- [ ] **4.3** Expose APIs for fetching “unlinked receipts” when drafting transactions.
- [ ] **4.4** Add background job (or use worker) to analyze overrides and promote recurring mappings into rule table.
- [ ] **4.5** Document the feedback workflow and provide operational dashboards/alerts for stuck receipts.

## Phase 5 – Hardening & Deployment
- [ ] **5.1** Load/perf test upload/OCR flow (large receipts, concurrent uploads).
- [ ] **5.2** Add automated tests (API integration tests, worker unit tests, front-end e2e for upload/review).
- [ ] **5.3** Update deployment scripts/docs for IIS + Windows Service + static frontend hosting.
- [ ] **5.4** Create runbooks for reprocessing failed OCR jobs and rotating storage secrets.
- [ ] **5.5** Finalize monitoring (App Insights dashboards, alerts on OCR backlog, receipt-to-transaction SLA).

# Receipts Capture & OCR Plan

Roadmap for delivering end-to-end receipt ingestion (upload -> OCR -> review -> transaction link). Each phase contains scoped, checkbox tasks so we can track progress incrementally.

## Gap Analysis - Bookkeeper Posting Workflow
- **Database layer:** Core tables (Receipts, ReceiptDecisions, AccountSuggestionRules) exist, but we lack seeded data sets that mimic high-volume uploads or complicated posting edge cases (multi-line invoices, tax overrides). Need richer seed scripts/anonymized fixtures.
- **Data ingress:** API accepts one file per request and enqueues OCR jobs, yet there is no bulk-upload endpoint nor status/ retry APIs for the UI to poll. Client-side upload progress/errors are missing.
- **Business logic:** `ReceiptsService` can approve and spawn `FinancialTransaction` rows, but there is no orchestrated queue for bookkeepers vs. accountants (e.g., "Ready to Post", "Awaiting Approval"). Approvals must still be triggered manually.
- **OCR/worker:** Pipeline completes OCR and refreshes suggestion rules but does not emit events or APIs for "ready for review" notifications. Backlog metrics exist only in logs.
- **UI:** The dashboard shows mock data only. No authenticated calls to the API, no real upload form, no review drawer tied to OCR output, and no posting action to `/api/receipts/{id}/approve`.
- **Concurrency:** Multiple bookkeepers may upload/review simultaneously, yet there is no notion of claim/ownership, per-user progress tracking, or collision avoidance when two users edit the same receipt.
- **Bulk handoffs:** Bookkeepers often upload hundreds of receipts before reassigning posting duties; there is no explicit, audited bulk handoff workflow to move large batches between users.
- **Ownership classification:** Receipts need tagging/grouping by ownership type (Sole Prop vs Partnership vs Corporation) for compliance, but no schema/UI/API currently exposes that dimension.
- **End-to-end posting goal:** After OCR completes, bookkeepers must route each receipt to the correct accounts and hand off to accountants. Today this requires manual API calls with JSON; there is no guided UI, batch-posting experience, role-specific queue, or audit surface.

---

## Phase 1 - API Foundations
- [x] **1.1** Define receipt DTOs (upload request/response, listing, detail, line items).
- [x] **1.2** Add `ReceiptsController` with endpoints:
  - [x] **1.2.1** `POST /api/receipts` (multipart upload, persist metadata + binary, enqueue OCR job).
  - [x] **1.2.2** `GET /api/receipts` with filters/pagination.
  - [x] **1.2.3** `GET /api/receipts/{id}` for detailed view (includes OCR text, line items, flags).
- [x] **1.3** Implement file storage abstraction (SQL varbinary vs. blob storage) and wire to controller.
- [x] **1.4** Add FluentValidation + size/MIME vetting; enforce role access (Bookkeeper upload, Accountant review).
- [x] **1.5** Seed initial OCR job queue integration (DB table or background command) consumed by worker.

## Phase 2 - OCR Worker Enhancements
- [x] **2.1** Extend worker to poll queued receipts (new status column or queue table).
- [x] **2.2** Normalize images (orientation, grayscale) before OCR run.
- [x] **2.3** Extract header/line items, tax/VAT indicators, and populate `ReceiptLineItem` entities.
- [x] **2.4** Capture confidence scores + raw OCR text for auditing.
- [x] **2.5** Update worker logging/telemetry for success/failure tracking; add retry policy.

## Phase 3 - Dashboard UI (Capture & Review)
- [x] **3.1** Build "Receipts" page (sidebar entry, route guard).
- [x] **3.2** Implement upload dialog with drag/drop, metadata entry (date, vendor, VAT flag override).
- [x] **3.3** Add list/table view showing status (Pending, Processing, Needs Review, Completed) with filters/search.
- [x] **3.4** Create review drawer/page to display OCR output, allow manual corrections, and mark ready for posting.
- [x] **3.5** Provide controls to link receipt to existing transaction or start a new draft.
- [x] **3.6** Surface suggested accounts from auto-categorization (read-only for now) with manual override inputs.

## Phase 4 - Transaction Integration & Feedback Loop
- [x] **4.1** When a receipt is approved, generate or update the related transaction (pre-populated entry stubs).
- [x] **4.2** Persist accept/override data (selected account, VAT flag, totals) to feed auto-categorization logic.
- [x] **4.3** Expose APIs for fetching "unlinked receipts" when drafting transactions.
- [x] **4.4** Add background job (or use worker) to analyze overrides and promote recurring mappings into rule table.
- [x] **4.5** Document the feedback workflow and provide operational dashboards/alerts for stuck receipts (`docs/receipts-feedback-operations.md`).

## Phase 5 - Hardening & Deployment
- [x] **5.1** Load/perf test upload/OCR flow (large receipts, concurrent uploads) via `tools/ReceiptLoadTester` (see `docs/receipts-load-test-report.md`).
- [ ] **5.2** Add automated tests (API integration tests, worker unit tests, front-end e2e for upload/review).
- [ ] **5.3** Update deployment scripts/docs for IIS + Windows Service + static frontend hosting.
- [ ] **5.4** Create runbooks for reprocessing failed OCR jobs and rotating storage secrets.
- [ ] **5.5** Finalize monitoring (App Insights dashboards, alerts on OCR backlog, receipt-to-transaction SLA).

## Phase 6 - Frontend Receipt Capture Integration
- [ ] **6.1** Implement receipt upload form with Firebase-authenticated multipart POST to `/api/receipts` (show progress/errors).
- [ ] **6.2** Add receipts list view backed by API filters (status chips, pagination) showing only in-system records (no "pending upload" stack).
- [ ] **6.3** Build review drawer that fetches receipt details/OCR text and allows manual corrections before approval.
- [ ] **6.4** Wire approval actions to `/api/receipts/{id}/approve`, including decision inputs and optimistic updates.
- [ ] **6.5** Integrate suggested account hints from backend and display backlog status indicators sourced from new monitoring logs.
- [ ] **6.6** Implement role-aware UI states: bookkeepers see only receipts they own (upload -> post), accountants see review/approval queues; no unassigned records exist because the uploader is the default owner.
- [ ] **6.7** Support multi-select uploads (hundreds at a time) with per-file progress, retry, and ownership indicators so concurrent bookkeepers can ingest large batches without conflicts.
- [ ] **6.8** Provide UI to bulk reassign receipts (with audit notes) so bookkeepers explicitly hand off capture/posting responsibility for 100+ receipt batches when needed.
- [ ] **6.9** Add ownership-type facets (Sole Prop, Partnership, Corporation, etc.) to receipts list/review UI so bookkeepers can filter and group work by entity classification.

## Phase 7 - Posting Workflow & Transaction Finalization
- [ ] **7.1** Expose "Ready to Post" queue endpoints filtering receipts with completed OCR but pending decisions, including assignment metadata/pagination (since every receipt has an owner from upload).
- [ ] **7.2** Extend approval payloads to support multi-line edits, tax overrides, and account search, persisting audit metadata per field change.
- [ ] **7.3** Update the frontend to guide bookkeepers through posting: bulk-select receipts, edit account mappings, preview transactions, and submit approvals with validation against the chart of accounts.
- [ ] **7.4** Enhance transaction creation to support split postings and enforce double-entry balancing before committing to `FinancialTransaction`/`Entry` tables.
- [ ] **7.5** Add confirmation dashboards showing posted vs. pending receipts per client/period plus exportable audit logs for controllers.
- [ ] **7.6** Enforce bookkeeper->accountant workflow: add status transitions (Posted, Awaiting Approval, Approved), role-based permissions, and notification hooks for approvals/rejections.
- [ ] **7.7** Update schema (Receipts/ReceiptProcessingJobs) with ownership + audit columns (AssignedBookkeeperId, ClaimedAt, LockedBy, etc.) and expose them via EF migrations.
- [ ] **7.8** Build receipt assignment & locking flows (bulk claim/release APIs, activity logs, conflict warnings) so multiple bookkeepers/accountants can coordinate safely even when hundreds of receipts change hands.
- [ ] **7.9** Enforce posting authorization rules: only the assigned bookkeeper (or elevated role) may post unless a tracked reassignment occurs; surface audit logs for uploads/postings/reassignments.
- [ ] **7.10** Extend receipt/transaction metadata + seed data to capture ownership type (Sole Prop/Partnership/Corporation) with validation, reporting filters, and defaulting per client.
- [ ] **7.11** Externalize ownership types (reference table + admin API/UI) so new classifications can be added without code changes, and update enums/validation accordingly.

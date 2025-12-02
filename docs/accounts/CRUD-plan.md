# Accounts CRUD Plan & Tracker

## Overview
BookWise models accounts as a multi-level chart of accounts where each segment (e.g., `1000-200-30`) is its own node linked via `ParentAccountId`. Only the lowest-level (leaf) accounts can receive journal entries, while higher-level nodes serve as summaries. `ExternalAccountNumber` stays immutable to preserve the legacy identifiers, `SegmentCode`/`Level` encode the hierarchy, and filtered indexes plus triggers enforce uniqueness and leaf-only posting. This plan outlines how to deliver a full CRUD experience (API + UI) that respects those invariants, enabling admins/accountants to build and manage the hierarchy entirely inside the app.

## Phase 1 – API Foundation
- [x] Define request/response DTOs for accounts (list, detail, create, update, delete, lookup)
- [x] Implement `IAccountsService` (EF Core) enforcing hierarchy rules, immutability, and leaf-only deletes
- [x] Add `AccountsController` endpoints: `GET /api/accounts` (flat + tree), `GET /api/accounts/{id}`, `POST`, `PUT`, `DELETE`, and search support
- [x] Cover new service/controller behavior with unit/integration tests

## Phase 2 – Front-End Data Layer
- [x] Create API client module (Axios/Fetch) for `/api/accounts` endpoints
- [x] Build React Query hooks (`useAccounts`, `useAccount`, `useCreateAccount`, etc.) with cache invalidation and error normalization
- [x] Ensure hooks expose metadata needed by the UI (children counts, leaf flag, validation errors)

## Phase 3 – UI/UX Implementation
- [ ] Implement `AccountsPage` with tree/table visualization, expand/collapse, and search/filter controls
- [ ] Add reusable `AccountForm` modal/drawer for create/update (pre-fill parent/level, lock external number on edit)
- [ ] Wire row-level actions (add child, edit, delete) with guard rails for non-leaf nodes and confirmation dialogs
- [ ] Register navigation entry and route (e.g., `/accounts`) secured to Admin/Accountant roles

## Phase 4 – Validation, Docs, QA
- [ ] Add client-side validation mirroring backend rules (segment uniqueness, required parent for non-root, level increments)
- [ ] Document setup steps in README/onboarding (migrations, feature flags, environment variables)
- [ ] Produce QA checklist covering hierarchy rendering, CRUD flows, and error scenarios (immutability violations, non-leaf deletes)

# BookWise Roles & Access

## Purpose
BookWise ships with four built-in roles that drive API authorization, UI routing, and seed data. This document centralizes the role descriptions and enumerates the capabilities attached to each area of the product so engineers and stakeholders share the same expectations when building new features.

- Canonical role constants live in `src/backend/BookWise.Domain/Authorization/UserRoles.cs`.
- API enforcement relies on ASP.NET Core `[Authorize]` attributes (see `AccountsController`, `AdminUsersController`, etc.).
- The React client mirrors those checks via `<RequireRole>` wrappers and sidebar metadata (`src/frontend/bookwise-dashboard/src/routes.jsx` and `sidenav.jsx`).

## Role Overview
### Admin
- Full-control tenant administrator.
- Can invite/deactivate users, assign roles, and view every screen.
- Owns high-risk bookkeeping actions (approving adjustments, unblocking hierarchy rule violations, toggling background jobs).

### Bookkeeper
- Preparer role responsible for capturing operational activity.
- Scans/uploads invoices, reviews account coding, and posts transactions that move to the ledger for Accountant approval.
- No access to configuration (chart-of-accounts edits, user management, system toggles).

### Accountant
- Supervisory controller focused on review and administration.
- Reviews/approves Bookkeeper postings, maintains the chart of accounts, and manages user access.
- Can still perform higher-risk adjustments or emergency postings but typically focuses on oversight tasks.

### Viewer
- Read-only analyst/auditor.
- Can browse dashboards, reports, transactions, and receipts but cannot mutate financial or configuration data.

## Feature Access Matrix
| Feature / Area | Description | Admin | Accountant | Bookkeeper | Viewer | Notes / Source |
| --- | --- | --- | --- | --- | --- | --- |
| Core dashboard pages | `/dashboard/home`, profile, tables, notifications | ✅ | ✅ | ✅ | ✅ | Default `[Authorize]` on controllers and layout. |
| Chart of Accounts – read | `GET /api/accounts` plus dashboard table | ✅ | ✅ | ✅ | ✅ | Controller scoped to `[Authorize]` only; UI search allowed for admins/accountants by default but API permits all roles for read access. |
| Chart of Accounts – manage | Create/update/delete hierarchy + “Chart of Accounts” page | ✅ | ✅ | ❌ | ❌ | `[Authorize(Roles = Admin,Accountant)]` on POST/PUT/DELETE + `<RequireRole>` guard in `routes.jsx`. |
| User management | Invite users, change roles, deactivate | ✅ | ✅ | ❌ | ❌ | Requirement: accountants co-own user administration with admins. Update `[Authorize]` policies and UI route guards accordingly. |
| Transactions & journal entries – create/edit* | Prepare postings and submit for approval | ✅ | ✅ | ✅ | ❌ | Bookkeepers perform operational posting; accountants/admins retain capability for corrections. |
| Transactions & journal entries – approve/review* | Second-level review of postings before finalizing | ✅ | ✅ | ❌ | ❌ | Accountant reviews Bookkeeper submissions; Admin can override. |
| Transaction/receipt review* | View transactions with linked OCR receipts | ✅ | ✅ | ✅ | ✅ | View access for all roles; mutations limited per workflow above. |
| Receipt uploads & OCR re-runs* | Upload invoices, trigger OCR parse | ✅ | ✅ | ✅ | ❌ | Bookkeeper handles ingestion; Accountant/Admin can re-run OCR while auditing. |
| Reporting & exports | Income statement, summary widgets, data export | ✅ | ✅ | ✅ | ✅ | Reports are read-only; all authenticated users can pull data. |
| System configuration / safeguards | Tuning triggers, enabling background services, managing integrations | ✅ | ❌ | ❌ | ❌ | Reserved for administrators because failures impact every tenant. |

\*Denotes upcoming work captured in `docs/bookkeeping-system-plan.md`; the matrix records the intended access so backlogs stay aligned as those features ship.

## Using This Document
- New features should call out their required roles up front and update the matrix when behavior changes.
- When adjusting authorization in code, mirror the change in the React route guards and note it here to keep PMs, QA, and support teams informed.
- If tenants require additional personas later (e.g., “Auditor” or “Department Manager”), extend `UserRoles.cs`, add corresponding UI gating, and append new columns/rows in this file.

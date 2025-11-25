# Automated Bookkeeping System Plan

## 1. Project Overview

### Purpose and Goals
- Deliver an automated bookkeeping platform that centralizes revenue/expense capture, receipt digitization, and financial reporting.
- Provide near real-time insights via income statements and transaction dashboards to support small-business decision making.
- Offer a modular architecture so OCR, data ingestion, and reporting can evolve independently.

### Key Features
- Double-entry transaction tracking with audit-ready history.
- Receipt scanning (Tesseract.js locally or AWS Textract remotely) with auto-linking to transactions.
- Interactive dashboards and drill-down reporting using Creative Tim React templates.
- Role-aware Web API with validation, logging, and background OCR jobs.

## 2. Database Schema (Microsoft SQL Server)

### Core Tables
```sql
CREATE TABLE Accounts (
    AccountId INT IDENTITY PRIMARY KEY,
    ExternalAccountNumber NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(120) NOT NULL,
    Type NVARCHAR(40) NOT NULL, -- Asset, Liability, Equity, Revenue, Expense
    ParentAccountId INT NULL REFERENCES Accounts(AccountId)
);

CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FirstName NVARCHAR(255) NOT NULL,
    LastName NVARCHAR(255) NOT NULL,
    Role NVARCHAR(40) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL REFERENCES Users(UserId)
);

CREATE TABLE UserEmails (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(UserId),
    Email NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL REFERENCES Users(UserId)
);

CREATE TABLE Transactions (
    TransactionId INT IDENTITY PRIMARY KEY,
    ReferenceNumber NVARCHAR(64) UNIQUE,
    Description NVARCHAR(255),
    TransactionDate DATE NOT NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL REFERENCES Users(UserId),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Entries (
    EntryId INT IDENTITY PRIMARY KEY,
    TransactionId INT NOT NULL REFERENCES Transactions(TransactionId),
    AccountId INT NOT NULL REFERENCES Accounts(AccountId),
    Debit DECIMAL(18,2) NOT NULL DEFAULT 0,
    Credit DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT CK_Entries_DebitCredit CHECK ((Debit = 0 AND Credit > 0) OR (Credit = 0 AND Debit > 0))
);

CREATE TABLE Receipts (
    ReceiptId INT IDENTITY PRIMARY KEY,
    TransactionId INT NULL REFERENCES Transactions(TransactionId),
    Type NVARCHAR(20) NOT NULL DEFAULT 'Unknown',
    DocumentDate DATETIME2 NULL,
    SellerName NVARCHAR(255),
    SellerTaxId NVARCHAR(50),
    SellerAddress NVARCHAR(512),
    CustomerName NVARCHAR(255),
    CustomerTaxId NVARCHAR(50),
    CustomerAddress NVARCHAR(512),
    Terms NVARCHAR(120),
    PurchaseOrderNumber NVARCHAR(120),
    BusinessStyle NVARCHAR(120),
    Notes NVARCHAR(1024),
    ImageData VARBINARY(MAX) NOT NULL,
    MimeType NVARCHAR(80),
    UploadedBy UNIQUEIDENTIFIER NOT NULL REFERENCES Users(UserId),
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    SubtotalAmount DECIMAL(18,2),
    TaxAmount DECIMAL(18,2),
    DiscountAmount DECIMAL(18,2),
    WithholdingTaxAmount DECIMAL(18,2),
    TotalAmount DECIMAL(18,2),
    CurrencyCode NVARCHAR(3),
    OcrText NVARCHAR(MAX),
    Status NVARCHAR(30) NOT NULL DEFAULT 'Pending'
);

CREATE TABLE ReceiptLineItems (
    ReceiptLineItemId INT IDENTITY PRIMARY KEY,
    ReceiptId INT NOT NULL REFERENCES Receipts(ReceiptId),
    Quantity DECIMAL(18,2) NOT NULL,
    Unit NVARCHAR(50),
    Description NVARCHAR(512) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL
);
```

### Relationships and Constraints
- `Accounts` self-references for chart-of-accounts hierarchy; enforce summary vs leaf accounts via triggers or computed flags. `ExternalAccountNumber` stays immutable/unique to mirror the legacy accounting system for synchronization.
- `Transactions` aggregate multiple `Entries`; enforce balanced transactions via trigger ensuring `SUM(Debit) = SUM(Credit)`.
- `Receipts` optionally link to `Transactions`; allow orphan receipts until categorized.
- `Users` drive audit fields (CreatedBy/UploadedBy) and role-based security; seed with the pre-authorized email allowlist referenced by Firebase Authentication.
- Consider filtered indexes on `TransactionDate`, `Status`, and `AccountId` for reporting workloads.

## 3. Entity Framework Core Models

### Class Definitions & Navigation Properties
```csharp
public class Transaction
{
    public int TransactionId { get; set; }
    public string ReferenceNumber { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid CreatedBy { get; set; }

    public User Creator { get; set; } = default!;
    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    public Receipt? Receipt { get; set; }
}
```

- Use data annotations (`[Required]`, `[MaxLength]`, `[Index]` via Fluent API) to enforce schema rules.
- Apply Fluent API in `OnModelCreating` for:
  - Composite unique constraints (e.g., `ReferenceNumber` scoped per year if needed).
  - Enum-to-string conversions for account type and receipt status.
  - Shadow properties (`CreatedAt`, `UpdatedAt`) with automatic timestamps.
- Surface `ExternalAccountNumber` on the `Account` entity with `[Required]` and alternate key configuration so imports from the existing accounting app stay stable; expose API filters/selectors by this code for cross-system reconciliation.
- Enable lazy-loading proxies or explicit loading depending on performance profile; expose DTOs to the API to avoid over-posting.

## 4. Web API Endpoints (ASP.NET Core)

- Requests include Firebase ID tokens in the `Authorization: Bearer` header; middleware uses Firebase Admin SDK to validate tokens, confirm the email exists in the pre-authorized allowlist (`Users` table), and populate HttpContext with role claims.

### Accounts & Transactions
- `GET /api/accounts` - list accounts with balances (optionally include aggregates via query params).
- `POST /api/accounts` - create account; validate parent/child type constraints.
- Support lookup endpoints/query params using `externalAccountNumber` to keep data synchronized with the legacy accounting app.
- `GET /api/transactions?dateRange=` — filterable list; include entries and linked receipts.
- `POST /api/transactions` — create transaction with entries; service enforces double-entry rule.
- `PUT /api/transactions/{id}` / `DELETE` — updates with optimistic concurrency tokens.

### Receipts & OCR
- `POST /api/receipts` - multipart upload; stores metadata, queues OCR job (Tesseract.js worker or AWS Textract).
- `GET /api/receipts/{id}` - fetch OCR text, status, linked transaction.
- `POST /api/receipts/{id}/parse` - manual re-run of OCR or classification.

### Reporting
- `GET /api/reports/income-statement?period=` — aggregates revenue/expense accounts by month or YTD.
- `GET /api/reports/summary` — quick ratios, burn rate, outstanding receipts.
- Consider SignalR hub for pushing OCR completion events to the UI.

## 5. React Front-End (Creative Tim)

### Component Tree (excerpt)
```
App
 ├─ AuthProvider
 ├─ Layout (Creative Tim dashboard shell)
 │   ├─ Sidebar
 │   └─ TopBar
 └─ Routes
     ├─ DashboardPage
     ├─ TransactionsPage
     │   ├─ TransactionTable
     │   └─ TransactionDrawer
     ├─ ReceiptsPage
     │   ├─ ReceiptUploader
     │   └─ ReceiptViewer
     └─ ReportsPage
```

### Pages & Data Access
- **Authentication**: React Context hooks Firebase Authentication (pre-authorized emails only) to sign users in, obtains ID tokens, and injects them into Axios/React Query requests.
- **Dashboard**: KPIs, charts from `/api/reports/summary`.
- **Transactions**: paginated ledger, inline forms; use React Query to cache `/api/transactions`.
- **Receipts**: upload widget (Dropzone + progress), OCR status indicators, manual categorization modal.
- **Reports**: income statement filters, charts (Recharts/Victory) fed by `/api/reports/income-statement`.
- Integrate Axios or React Query with interceptors for auth tokens, retry logic, and error toasts.
- Utilize Creative Tim components for consistent styling; wrap in form hooks (React Hook Form) for validation.

## 6. OCR Pipeline

### Image Preprocessing
- Client compresses/resizes before upload; server normalizes (deskew, grayscale) via Magick.NET for better OCR accuracy.
- Store originals plus processed versions for audit.
- Flow: user scans or uploads a receipt, server persists the original image bytes in the `Receipts.ImageData` VARBINARY column for proofing, then triggers preprocessing, AWS Textract extraction, and finally stores structured data/OCR text back in SQL Server.

### Text Extraction
- Local mode: Node worker with Tesseract.js; leverage multiple languages and `tessedit_char_whitelist`.
- Cloud mode: AWS Textract (DetectDocumentText or Expense API) invoked via AWS SDK; stream bytes directly from SQL VARBINARY column to Textract `Bytes` payload (or stage in S3 if file size exceeds inline limits), assume IAM role via temporary credentials, and handle async job completions through SNS/SQS callbacks.
- Persist OCR text and confidence metrics on `Receipts`.

### Auto-Categorization Logic
- NLP rules map extracted merchant, total, date to probable accounts.
- Use regex patterns for tax, tips, invoice IDs; fallback to ML classifier trained on historical receipts.
- Present suggested transaction mapping in UI; allow user override to feed feedback loop.

## 7. Security and Validation

- **Input Validation**: FluentValidation or .NET data annotations on DTOs; server-side checks for amount balance, valid dates, allowed MIME types.
- **Authentication**: Firebase Authentication handles user sign-in; only pre-authorized emails seeded in the `Users` table may complete onboarding. The API verifies Firebase ID tokens via Firebase Admin SDK and maps the token UID/email to internal user records.
- **Role-Based Access Control**: Custom authorization policies leverage Users/roles once Firebase tokens are validated; enforce roles like `Admin`, `Accountant`, `Viewer` for posting entries, running reports, or viewing receipts.
- **Data Integrity**: Database constraints (FKs, CHECKs), transaction scopes for multi-table mutations, background job retries with idempotency keys.
- **Secrets & Compliance**: Store connection strings/keys in Azure Key Vault or user secrets; enforce TLS and encrypt receipt binaries at rest through SQL Server Transparent Data Encryption or column-level encryption.

## 8. Deployment Plan

- **Local Development**:
  - VSCode + Codex extensions; Docker containers for SQL Server, Web API, React dev server.
  - `dotnet ef database update` for schema migrations; `npm start` for front-end with proxy to API.
  - Seed data scripts for demo accounts and transactions.
- **Production Hosting**:
  - API + OCR workers on Azure App Service or Azure Kubernetes Service with managed identity.
  - SQL Server on Azure SQL Database with geo-replication storing receipt VARBINARY data (TDE enabled).
  - CI/CD via GitHub Actions or Azure DevOps: lint/test, EF migrations, automated front-end build.
  - Monitoring: Application Insights, Azure Monitor alerts for OCR failures or unbalanced transactions.

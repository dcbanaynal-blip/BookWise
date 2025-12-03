# Accounts Setup Guide

## Prerequisites
- SQL Server instance reachable from the API
- BookWise API appsettings configured with a valid `ConnectionStrings:SqlServer`
- Firebase project for authentication plus `.env.local` values for the dashboard

## Backend
1. Restore EF tools once per environment:
   ```powershell
   dotnet tool restore
   ```
2. Apply migrations to create/update the schema:
   ```powershell
   dotnet ef database update -p src/backend/BookWise.Infrastructure -s src/backend/BookWise.Api
   ```
3. Configure the API:
   - `Firebase:ServiceAccountKeyPath` in `appsettings.Development.json`
   - `Cors:AllowedOrigins` to include the dashboard origin (e.g., `http://localhost:5173`)
   - Run the API via `dotnet run --project src/backend/BookWise.Api`

## Frontend
1. Create `src/frontend/bookwise-dashboard/.env.local` with:
   ```env
   VITE_FIREBASE_API_KEY=...
   VITE_FIREBASE_AUTH_DOMAIN=...
   VITE_FIREBASE_PROJECT_ID=...
   VITE_FIREBASE_APP_ID=...
   VITE_API_BASE_URL=https://localhost:7043
   ```
2. Install dependencies and start the dev server:
   ```powershell
   cd src/frontend/bookwise-dashboard
   npm install
   npm run dev
   ```

## Roles & Access
- All authenticated roles can view `/dashboard/accounts`, but only `Admin` and `Accountant` can create, edit, or delete accounts.
- Use the User Management page (Admin or Accountant) to promote/demote roles
- Accounts obey hierarchy rules enforced in the database; any imported data must provide `ExternalAccountNumber`, `SegmentCode`, and `Level`

## Verification Checklist
- Visit `/dashboard/accounts` and confirm the tree renders (empty state is acceptable)
- Create a root account, then add a child to confirm levels/segment codes update
- Attempt invalid inputs (overly long names, duplicate segment codes) to see client-side validation and API error messaging

# User Management Module Notes

## Running Automated Tests

Backend service tests:

```bash
cd src/backend
dotnet test BookWise.sln
```

Frontend component tests:

```bash
cd src/frontend/bookwise-dashboard
npm run test
```

## Manual Verification

See `docs/user-management/qa-checklist.md` for end-to-end manual steps (invite, role change, email management, unauthorized access).

## Admin UI Usage

1. Ensure `VITE_API_BASE_URL` points at the running BookWise API and that your account is allowlisted as `Admin`.
2. Sign in via Google, navigate to **Dashboard → User Management**.
3. Use the **Invite User** button to add teammates; select the role from the dropdown.
4. For existing entries, update the role inline and manage email addresses via the **Manage emails** action.
5. Non-admin users will not see the navigation link and attempting to visit `/dashboard/users` redirects back to the dashboard.

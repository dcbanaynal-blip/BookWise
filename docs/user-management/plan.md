# User & Role Management Implementation Plan

1. **API & Domain Enhancements**
   - [x] 1.1 Define DTOs (request/response) for listing users, inviting new users, editing roles, and managing email addresses.
   - [x] 1.2 Add services/repositories in the backend to encapsulate allowlist operations (fetch, add, update, delete) with validation (unique email, existing roles).
   - [x] 1.3 Expose privileged endpoints under `/api/admin/users` with `[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Accountant}")]`.
   - [x] 1.4 Introduce audit logging for user changes (creator/updater ids, timestamps) and return structured error messages for the UI.

2. **Authentication & Authorization Integration**
   - [x] 2.1 Extend `/api/users/me` response to include role metadata already present in the BookWise user record.
   - [x] 2.2 In the React auth context, expose the BookWise role and helper hooks such as `useHasRole`.
   - [x] 2.3 Create a `<RequireRole>` component that blocks non-admins from reaching the new screens.

3. **Front-End API Client Layer**
   - [x] 3.1 Extend `src/config/api.ts` (or a new module) with typed functions for the new admin endpoints, automatically attaching the Firebase ID token.
   - [x] 3.2 Wrap the fetchers with React Query hook factories (`useUsersQuery`, `useInviteUserMutation`, etc.) for caching and optimistic updates.

4. **UI/UX Surfaces**
   - [x] 4.1 Add a "User Management" route under the dashboard layout, visible to admins and accountants (update sidebar nav).
   - [x] 4.2 Build the main grid/table:
       - [x] Columns: Name, Primary Email, Additional Emails, Role, Created/Updated info, Status/Actions.
       - [x] Row actions for editing role, managing emails, deactivating accounts.
   - [x] 4.3 Modals/forms:
       - [x] Invite user modal (first/last name, primary email, role).
       - [x] Manage emails modal (list/add/remove emails).
       - [x] Change role confirmation dialog referencing `UserRoles`.
   - [x] 4.4 Provide inline notifications/snackbars for success/failure and loading states (skeleton rows or spinners).

5. **Testing & Validation**
   - [x] 5.1 Backend: add unit/integration tests covering the new services (unique email constraint, role transitions, unauthorized access).
   - [x] 5.2 Frontend: add component/unit tests for hooks and pages (e.g., verifying admin guard behavior) plus manual QA checklist (invite flow, role change, unauthorized access).
   - [x] 5.3 Update documentation (README/plan) with instructions on managing users via UI, including required environment variables and permissions.

# User & Role Management Implementation Plan

1. **API & Domain Enhancements**
   - [x] 1.1 Define DTOs (request/response) for listing users, inviting new users, editing roles, and managing email addresses.
   - [x] 1.2 Add services/repositories in the backend to encapsulate allowlist operations (fetch, add, update, delete) with validation (unique email, existing roles).
   - [x] 1.3 Expose admin-only endpoints under `/api/admin/users` with `[Authorize(Roles = UserRoles.Admin)]`.
   - [x] 1.4 Introduce audit logging for user changes (creator/updater ids, timestamps) and return structured error messages for the UI.

2. **Authentication & Authorization Integration**
   - [ ] 2.1 Extend `/api/users/me` response to include role metadata already present in the BookWise user record.
   - [ ] 2.2 In the React auth context, expose the BookWise role and helper hooks such as `useHasRole`.
   - [ ] 2.3 Create a `<RequireRole>` component that blocks non-admins from reaching the new screens.

3. **Front-End API Client Layer**
   - [ ] 3.1 Extend `src/config/api.ts` (or a new module) with typed functions for the new admin endpoints, automatically attaching the Firebase ID token.
   - [ ] 3.2 Wrap the fetchers with React Query hook factories (`useUsersQuery`, `useInviteUserMutation`, etc.) for caching and optimistic updates.

4. **UI/UX Surfaces**
   - [ ] 4.1 Add a “User Management” route under the dashboard layout, visible only to admins (update sidebar nav).
   - [ ] 4.2 Build the main grid/table:
       - [ ] Columns: Name, Primary Email, Additional Emails, Role, Created/Updated info, Status/Actions.
       - [ ] Row actions for editing role, managing emails, deactivating accounts.
   - [ ] 4.3 Modals/forms:
       - [ ] Invite user modal (first/last name, primary email, role).
       - [ ] Manage emails modal (list/add/remove emails).
       - [ ] Change role confirmation dialog referencing `UserRoles`.
   - [ ] 4.4 Provide inline notifications/snackbars for success/failure and loading states (skeleton rows or spinners).

5. **Testing & Validation**
   - [ ] 5.1 Backend: add unit/integration tests covering the new services (unique email constraint, role transitions, unauthorized access).
   - [ ] 5.2 Frontend: add component/unit tests for hooks and pages (e.g., verifying admin guard behavior) plus manual QA checklist (invite flow, role change, unauthorized access).
   - [ ] 5.3 Update documentation (README/plan) with instructions on managing users via UI, including required environment variables and permissions.

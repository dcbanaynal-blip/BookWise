# User Management QA Checklist

Manual validation steps to accompany the automated tests:

1. **Allowlist Enforcement**
   - Sign in with a non-allowlisted Google account -> confirm the dashboard shows the "not authorized" message and the user is signed out.
   - Sign in with an allowlisted account -> confirm the dashboard loads and the sidebar shows the “User Management” link.

2. **User Management Grid**
   - Navigate to `/dashboard/users` as an Admin.
   - Verify that existing users display with the correct primary/secondary emails and roles.
   - Change a user’s role via the dropdown -> observe success toast/logs and confirm the table refreshes.

3. **Invite Flow**
   - Click “Invite User”, complete the form, and submit.
   - Assert that the modal closes, the grid refreshes with the new user, and the API receives the `POST /api/admin/users` call.

4. **Email Management**
   - Open “Manage emails” for a user with multiple emails and remove one -> ensure at least one email remains and backend receives `DELETE /emails/{id}`.
   - Add a new email -> confirm the chip list updates and duplicate addresses produce a readable error.

5. **Unauthorized Access**
   - As a Viewer/Accountant, attempt to hit `/dashboard/users` -> confirm redirect back to the dashboard home and no nav link is visible.

Record results (Pass/Fail) for each step before each release.

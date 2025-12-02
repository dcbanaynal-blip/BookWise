# Accounts QA Checklist

## Pre-checks
- [ ] Backend running with latest migrations applied
- [ ] Dashboard `.env.local` points to the running API
- [ ] Test user has `Admin` or `Accountant` role

## CRUD Flow
- [ ] Create a root account (no parent). Verify it appears at Level 1 and the external number is immutable on edit.
- [ ] Add a child account via the row actions. Confirm it is indented one level deeper and inherits `parentAccountId`.
- [ ] Edit an account’s name/segment/type and confirm the changes persist.
- [ ] Attempt to edit the external account number and verify the backend rejects the request (immutability).
- [ ] Delete a leaf account successfully. Verify deleting an account with children is blocked with an error/toast.

## Validation & Search
- [ ] Try to submit the form with missing fields—client-side validation should flag each required value.
- [ ] Enter overly long values (>50 chars for segment/external, >120 for name) and confirm client-side errors.
- [ ] Use the search box to filter accounts by name, external number, or segment.
- [ ] Verify expand/collapse toggles work and retain state during navigation.

## Permissions
- [ ] Log in as a Viewer—Accounts nav item should be hidden and direct URL should redirect/fail authorization.
- [ ] Log in as an Accountant—Accounts nav item visible and CRUD actions accessible.

## Error Handling
- [ ] Simulate API failure (stop backend) and confirm the Accounts page shows the retry alert.
- [ ] Attempt to delete an account with ledger entries (if data exists) and confirm API error surfaces in the UI.

import { useEffect, useMemo, useState } from "react";
import {
  Card,
  CardHeader,
  CardBody,
  Typography,
  Button,
  Chip,
  Dialog,
  DialogHeader,
  DialogBody,
  DialogFooter,
  Input,
  Select,
  Option,
  Spinner,
  IconButton,
  Alert,
} from "@material-tailwind/react";
import { XMarkIcon } from "@heroicons/react/24/solid";
import {
  useAdminUsers,
  useAddUserEmailMutation,
  useInviteUserMutation,
  useRemoveUserEmailMutation,
  useUpdateUserRoleMutation,
} from "@/hooks/useAdminUsers";

const USER_ROLES = ["Admin", "Accountant", "Viewer"];

export function UserManagement() {
  const { data, isLoading, isError, refetch } = useAdminUsers();
  const [inviteOpen, setInviteOpen] = useState(false);
  const [emailDialogUser, setEmailDialogUser] = useState(null);
  const updateRoleMutation = useUpdateUserRoleMutation();

  useEffect(() => {
    if (!emailDialogUser || !data) {
      return;
    }
    const refreshedUser = data.find(u => u.userId === emailDialogUser.userId);
    if (refreshedUser && refreshedUser !== emailDialogUser) {
      setEmailDialogUser(refreshedUser);
    }
  }, [data, emailDialogUser]);

  const rows = useMemo(() => data ?? [], [data]);

  return (
    <div className="mt-12 mb-8 flex flex-col gap-6">
      <Card>
        <CardHeader floated={false} shadow={false} className="rounded-none flex justify-between items-center">
          <div>
            <Typography variant="h5" color="blue-gray">
              User Management
            </Typography>
            <Typography variant="small" color="gray" className="mt-1 font-normal">
              Invite teammates and manage their BookWise roles.
            </Typography>
          </div>
          <Button color="blue" onClick={() => setInviteOpen(true)}>
            Invite User
          </Button>
        </CardHeader>
        <CardBody className="px-0">
          {isLoading && (
            <div className="py-16 flex justify-center">
              <Spinner className="h-6 w-6" />
            </div>
          )}
          {isError && (
            <div className="px-6 py-8">
              <Alert color="red" className="mb-4">
                Unable to load users. <Button variant="text" size="sm" onClick={() => refetch()}>Try again</Button>
              </Alert>
            </div>
          )}
          {!isLoading && !isError && (
            <div className="overflow-x-auto px-6 pb-6">
              <table className="w-full table-auto text-left min-w-[960px]">
                <thead>
                  <tr>
                    {["Name", "Primary Email", "Additional Emails", "Role", "Actions"].map(header => (
                      <th key={header} className="border-b border-blue-gray-50 py-3">
                        <Typography variant="small" className="font-semibold uppercase text-blue-gray-400">
                          {header}
                        </Typography>
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {rows.map((user, idx) => {
                    const emails = user.emails ?? [];
                    const primaryEmail = emails[0]?.email ?? "—";
                    const otherEmails = emails.slice(1);
                    return (
                      <tr key={user.userId}>
                        <td className={rowClass(idx, rows.length)}>
                          <Typography variant="small" color="blue-gray" className="font-semibold">
                            {user.firstName} {user.lastName}
                          </Typography>
                        </td>
                        <td className={rowClass(idx, rows.length)}>
                          <Typography variant="small">{primaryEmail}</Typography>
                        </td>
                        <td className={rowClass(idx, rows.length)}>
                          <div className="flex flex-wrap gap-2">
                            {otherEmails.length === 0 && (
                              <Typography variant="small" color="gray">
                                —
                              </Typography>
                            )}
                            {otherEmails.map(email => (
                              <Chip key={email.id} value={email.email} color="blue-gray" size="sm" />
                            ))}
                          </div>
                        </td>
                        <td className={`${rowClass(idx, rows.length)} max-w-xs`}>
                          <Select
                            value={user.role}
                            label="Role"
                            onChange={value => {
                              if (value && value !== user.role) {
                                updateRoleMutation.mutate({ userId: user.userId, role: value });
                              }
                            }}
                            disabled={updateRoleMutation.isPending}
                          >
                            {USER_ROLES.map(role => (
                              <Option key={role} value={role}>
                                {role}
                              </Option>
                            ))}
                          </Select>
                        </td>
                        <td className={rowClass(idx, rows.length)}>
                          <Button
                            variant="text"
                            size="sm"
                            color="blue"
                            onClick={() => setEmailDialogUser(user)}
                          >
                            Manage emails
                          </Button>
                        </td>
                      </tr>
                    );
                  })}
                  {rows.length === 0 && (
                    <tr>
                      <td colSpan={5} className="py-8 text-center text-blue-gray-400">
                        No users found.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          )}
        </CardBody>
      </Card>

      <InviteUserDialog open={inviteOpen} onClose={() => setInviteOpen(false)} />
      <ManageEmailsDialog user={emailDialogUser} onClose={() => setEmailDialogUser(null)} />
    </div>
  );
}

function rowClass(index, length) {
  return `py-4 px-2 ${index === length - 1 ? "" : "border-b border-blue-gray-50"}`;
}

function InviteUserDialog({ open, onClose }) {
  const [form, setForm] = useState({ firstName: "", lastName: "", email: "", role: USER_ROLES[1] });
  const [error, setError] = useState("");
  const inviteMutation = useInviteUserMutation();

  const handleChange = e => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleSubmit = async event => {
    event.preventDefault();
    setError("");
    try {
      await inviteMutation.mutateAsync({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        role: form.role,
        emails: [form.email.trim()],
      });
      setForm({ firstName: "", lastName: "", email: "", role: USER_ROLES[1] });
      onClose();
    } catch (err) {
      setError(err.message || "Unable to invite user.");
    }
  };

  return (
    <Dialog open={open} handler={onClose} size="sm">
      <DialogHeader>Invite User</DialogHeader>
      <form onSubmit={handleSubmit}>
        <DialogBody divider className="space-y-4">
          <Input label="First Name" name="firstName" value={form.firstName} onChange={handleChange} required />
          <Input label="Last Name" name="lastName" value={form.lastName} onChange={handleChange} required />
          <Input label="Primary Email" name="email" type="email" value={form.email} onChange={handleChange} required />
          <Select label="Role" value={form.role} onChange={value => setForm(prev => ({ ...prev, role: value || prev.role }))}>
            {USER_ROLES.map(role => (
              <Option key={role} value={role}>
                {role}
              </Option>
            ))}
          </Select>
          {error && (
            <Alert color="red" className="mt-2">
              {error}
            </Alert>
          )}
        </DialogBody>
        <DialogFooter>
          <Button variant="text" color="gray" onClick={onClose} className="mr-2">
            Cancel
          </Button>
          <Button color="blue" type="submit" disabled={inviteMutation.isPending}>
            {inviteMutation.isPending ? "Inviting..." : "Invite"}
          </Button>
        </DialogFooter>
      </form>
    </Dialog>
  );
}

function ManageEmailsDialog({ user, onClose }) {
  const addEmailMutation = useAddUserEmailMutation();
  const removeEmailMutation = useRemoveUserEmailMutation();
  const [newEmail, setNewEmail] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    if (user) {
      setNewEmail("");
      setError("");
    }
  }, [user]);

  if (!user) {
    return null;
  }

  const handleAddEmail = async event => {
    event.preventDefault();
    setError("");
    try {
      await addEmailMutation.mutateAsync({ userId: user.userId, email: newEmail.trim() });
      setNewEmail("");
    } catch (err) {
      setError(err.message || "Unable to add email.");
    }
  };

  const handleRemoveEmail = async emailId => {
    setError("");
    try {
      await removeEmailMutation.mutateAsync({ userId: user.userId, emailId });
    } catch (err) {
      setError(err.message || "Unable to remove email.");
    }
  };

  return (
    <Dialog open={Boolean(user)} handler={onClose} size="md">
      <DialogHeader>
        Manage emails for {user.firstName} {user.lastName}
      </DialogHeader>
      <DialogBody divider className="space-y-4 max-h-[60vh] overflow-y-auto">
        <Typography variant="small" color="blue-gray">
          BookWise requires at least one email per user. Removing the last email is not allowed.
        </Typography>
        <div className="space-y-2">
          {user.emails.map(email => (
            <div key={email.id} className="flex items-center justify-between rounded-md border border-blue-gray-50 px-3 py-2">
              <div>
                <Typography variant="small">{email.email}</Typography>
                <Typography variant="small" color="gray">
                  Added {new Date(email.createdAt).toLocaleDateString()}
                </Typography>
              </div>
              <IconButton
                variant="text"
                color="blue-gray"
                onClick={() => handleRemoveEmail(email.id)}
                disabled={user.emails.length <= 1 || removeEmailMutation.isPending}
              >
                <XMarkIcon className="h-4 w-4" />
              </IconButton>
            </div>
          ))}
        </div>
        <form onSubmit={handleAddEmail} className="flex flex-col gap-3">
          <Input label="Add email" type="email" value={newEmail} onChange={e => setNewEmail(e.target.value)} />
          <Button type="submit" disabled={addEmailMutation.isPending}>
            {addEmailMutation.isPending ? "Adding..." : "Add Email"}
          </Button>
          {error && (
            <Alert color="red" className="mt-2">
              {error}
            </Alert>
          )}
        </form>
      </DialogBody>
      <DialogFooter>
        <Button variant="text" color="gray" onClick={onClose}>
          Close
        </Button>
      </DialogFooter>
    </Dialog>
  );
}

export default UserManagement;

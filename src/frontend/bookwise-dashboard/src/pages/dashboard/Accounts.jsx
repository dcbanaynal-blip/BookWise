import { useEffect, useMemo, useState } from "react";
import {
  Card,
  CardHeader,
  CardBody,
  Typography,
  Button,
  Input,
  Spinner,
  Alert,
  IconButton,
  Dialog,
  DialogHeader,
  DialogBody,
  DialogFooter,
  Select,
  Option,
} from "@material-tailwind/react";
import {
  ChevronDownIcon,
  ChevronRightIcon,
  PencilSquareIcon,
  PlusIcon,
  TrashIcon,
} from "@heroicons/react/24/solid";
import {
  useAccountsQuery,
  useCreateAccountMutation,
  useDeleteAccountMutation,
  useUpdateAccountMutation,
} from "@/hooks/useAccounts";
import { useAuth } from "@/auth";

const ACCOUNT_TYPE_OPTIONS = [
  { label: "Asset", value: 0 },
  { label: "Liability", value: 1 },
  { label: "Equity", value: 2 },
  { label: "Revenue", value: 3 },
  { label: "Expense", value: 4 },
];

const DEFAULT_ACCOUNT_TYPE_LABEL = ACCOUNT_TYPE_OPTIONS[0].label;
const ROOT_PARENT_LABEL = "No parent (root)";
const LEVEL_LABELS = ["Category", "Department", "Account Purpose", "Posting Entity"];

export function Accounts() {
  const [searchInput, setSearchInput] = useState("");
  const search = searchInput.trim();
  const isSearching = search.length > 0;
  const treeQuery = useAccountsQuery({ includeTree: true });
  const searchQuery = useAccountsQuery({ search, enabled: isSearching });
  const activeQuery = isSearching ? searchQuery : treeQuery;
  const data = activeQuery.data;
  const isLoading = activeQuery.isLoading || (!isSearching && treeQuery.isLoading);
  const isError = activeQuery.isError;
  const refetch = activeQuery.refetch;
  const { hasRole } = useAuth();
  const canManageAccounts = hasRole("Admin", "Accountant");

  const [collapsedMap, setCollapsedMap] = useState({});
  const collapsedKey = JSON.stringify(collapsedMap);

  const rows = useMemo(() => {
    if (!data) {
      return [];
    }

    if (isSearching) {
      return data.map(account => ({
        ...account,
        depth: 0,
        isCollapsed: false,
      }));
    }

    return flattenTree(data, collapsedMap);
  }, [data, isSearching, collapsedKey]);

  const createAccountMutation = useCreateAccountMutation();
  const updateAccountMutation = useUpdateAccountMutation();
  const deleteAccountMutation = useDeleteAccountMutation();

  const [formDialog, setFormDialog] = useState({
    open: false,
    mode: "create",
    account: null,
    parentAccountId: null,
    parentAccountLabel: "",
    parentAccountType: DEFAULT_ACCOUNT_TYPE_LABEL,
  });
  const [deleteTarget, setDeleteTarget] = useState(null);

  const toggleCollapse = accountId => {
    setCollapsedMap(prev => ({
      ...prev,
      [accountId]: !prev[accountId],
    }));
  };

  const openCreateDialog = parentAccount => {
    setFormDialog({
      open: true,
      mode: "create",
      account: null,
      parentAccountId: parentAccount?.accountId ?? null,
      parentAccountLabel: parentAccount
        ? `${parentAccount.name} (${parentAccount.externalAccountNumber})`
        : ROOT_PARENT_LABEL,
      parentAccountType: parentAccount?.type ?? DEFAULT_ACCOUNT_TYPE_LABEL,
    });
  };

  const openEditDialog = account => {
    setFormDialog({
      open: true,
      mode: "edit",
      account,
      parentAccountId: null,
    });
  };

  const closeFormDialog = () => {
    setFormDialog(prev => ({ ...prev, open: false }));
  };

  const handleCreateAccount = payload => createAccountMutation.mutateAsync(payload);
  const handleUpdateAccount = (accountId, payload) => updateAccountMutation.mutateAsync({ accountId, payload });
  const handleDeleteAccount = accountId => deleteAccountMutation.mutateAsync(accountId);

  return (
    <div className="mt-12 mb-8 flex flex-col gap-6">
      <Card>
        <CardHeader floated={false} shadow={false} className="rounded-none flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <Typography variant="h5" color="blue-gray">
              Chart of Accounts
            </Typography>
            <Typography variant="small" color="gray" className="mt-1 font-normal">
              Manage the hierarchical chart of accounts used across BookWise.
            </Typography>
          </div>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
            <Input
              label="Search accounts"
              value={searchInput}
              crossOrigin={undefined}
              onChange={event => setSearchInput(event.target.value)}
            />
            {canManageAccounts && (
              <Button color="blue" onClick={() => openCreateDialog(null)}>
                <PlusIcon className="h-4 w-4 mr-2 inline" />
                Add Root Account
              </Button>
            )}
          </div>
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
                Unable to load accounts.{" "}
                <Button variant="text" size="sm" onClick={() => refetch()}>
                  Try again
                </Button>
              </Alert>
            </div>
          )}
          {!isLoading && !isError && (
            <div className="overflow-x-auto px-6 pb-6">
              {rows.length === 0 ? (
                <Typography variant="small" color="blue-gray" className="py-8 text-center">
                  {isSearching ? "No accounts match your search." : "No accounts found. Start by adding a root account."}
                </Typography>
              ) : (
                <table className="w-full table-auto min-w-[960px] text-left">
                  <thead>
                    <tr>
                      <th className="border-b border-blue-gray-50 py-3 w-16"></th>
                      {["Account", "Full Code", "Segment", "Level", "Type", "Actions"].map(header => (
                        <th key={header} className="border-b border-blue-gray-50 py-3">
                          <Typography variant="small" className="font-semibold uppercase text-blue-gray-400">
                            {header}
                          </Typography>
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {rows.map((row, idx) => (
                      <tr key={row.accountId}>
                        <td className={`${rowClass(idx, rows.length)} w-16`}>
                          <div className="flex items-center" style={{ paddingLeft: `${row.depth * 16}px` }}>
                            {!isSearching && row.hasChildren ? (
                              <IconButton
                                variant="text"
                                color="blue-gray"
                                size="sm"
                                onClick={() => toggleCollapse(row.accountId)}
                              >
                                {collapsedMap[row.accountId] ? (
                                  <ChevronRightIcon className="h-4 w-4" />
                                ) : (
                                  <ChevronDownIcon className="h-4 w-4" />
                                )}
                              </IconButton>
                            ) : (
                              <span className="inline-block w-4 h-4" />
                            )}
                          </div>
                        </td>
                        <td className={rowClass(idx, rows.length)}>
                          <div style={{ paddingLeft: `${row.depth * 12}px` }}>
                            <Typography variant="small" className="font-semibold text-blue-gray-800">
                              {row.name}
                            </Typography>
                            <Typography variant="small" color="gray">
                              {LEVEL_LABELS[row.level - 1] ?? "Account"}
                            </Typography>
                          </div>
                        </td>
                        <td className={rowClass(idx, rows.length)}>
                          <Typography variant="small">{row.fullSegmentCode}</Typography>
                        </td>
                        <td className={rowClass(idx, rows.length)}>
                          <Typography variant="small">{row.segmentCode}</Typography>
                        </td>
                        <td className={rowClass(idx, rows.length)}>
                          <Typography variant="small">{row.level}</Typography>
                        </td>
                        <td className={rowClass(idx, rows.length)}>
                          <Typography variant="small">{row.type}</Typography>
                        </td>
                        <td className={`${rowClass(idx, rows.length)} whitespace-nowrap`}>
                          {canManageAccounts ? (
                            <div className="flex flex-wrap gap-2">
                              <Button
                                variant="text"
                                size="sm"
                                color="green"
                                onClick={() => openCreateDialog(row)}
                                disabled={isSearching}
                                className="flex items-center gap-1"
                              >
                                <PlusIcon className="h-4 w-4" />
                                Child
                              </Button>
                              <Button
                                variant="text"
                                size="sm"
                                color="blue"
                                onClick={() => openEditDialog(row)}
                                className="flex items-center gap-1"
                              >
                                <PencilSquareIcon className="h-4 w-4" />
                                Edit
                              </Button>
                              <Button
                                variant="text"
                                size="sm"
                                color="red"
                                onClick={() => setDeleteTarget(row)}
                                disabled={row.hasChildren}
                                className="flex items-center gap-1"
                              >
                                <TrashIcon className="h-4 w-4" />
                                Delete
                              </Button>
                            </div>
                          ) : (
                            <Typography variant="small" color="gray">
                              View only
                            </Typography>
                          )}
                          {row.hasChildren && (
                            <Typography variant="small" color="gray">
                              Contains sub-accounts
                            </Typography>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          )}
        </CardBody>
      </Card>

      {canManageAccounts && (
        <AccountFormDialog
          open={formDialog.open}
          mode={formDialog.mode}
          account={formDialog.account}
          parentAccountId={formDialog.parentAccountId}
          parentAccountLabel={formDialog.parentAccountLabel}
          parentAccountType={formDialog.parentAccountType}
          onClose={closeFormDialog}
          onCreate={handleCreateAccount}
          onUpdate={handleUpdateAccount}
        />
      )}

      {canManageAccounts && (
        <DeleteAccountDialog
          account={deleteTarget}
          onClose={() => setDeleteTarget(null)}
          onDelete={handleDeleteAccount}
          mutation={deleteAccountMutation}
        />
      )}
    </div>
  );
}

function rowClass(index, length) {
  return `py-4 px-2 ${index === length - 1 ? "" : "border-b border-blue-gray-50"}`;
}

function flattenTree(nodes, collapsedMap, depth = 0, ancestorCollapsed = false, list = []) {
  nodes.forEach(node => {
    if (ancestorCollapsed) {
      return;
    }

    const isCollapsed = Boolean(collapsedMap[node.accountId]);
    list.push({ ...node, depth, isCollapsed });

    if (node.children?.length) {
      flattenTree(node.children, collapsedMap, depth + 1, isCollapsed, list);
    }
  });

  return list;
}

function AccountFormDialog({
  open,
  mode,
  account,
  parentAccountId,
  parentAccountLabel,
  parentAccountType,
  onClose,
  onCreate,
  onUpdate,
}) {
  const isEdit = mode === "edit";
  const [form, setForm] = useState({
    externalAccountNumber: account?.externalAccountNumber ?? "",
    name: account?.name ?? "",
    segmentCode: account?.segmentCode ?? "",
    type:
      account?.type && ACCOUNT_TYPE_OPTIONS.some(option => option.label === account.type)
        ? account.type
        : parentAccountType ?? DEFAULT_ACCOUNT_TYPE_LABEL,
    parentAccountId: parentAccountId != null ? String(parentAccountId) : "",
    parentAccountLabel: parentAccountLabel ?? ROOT_PARENT_LABEL,
  });
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!open) {
      return;
    }
    setForm({
      externalAccountNumber: account?.externalAccountNumber ?? "",
      name: account?.name ?? "",
      segmentCode: account?.segmentCode ?? "",
      type:
        account?.type && ACCOUNT_TYPE_OPTIONS.some(option => option.label === account.type)
          ? account.type
          : parentAccountType ??
            (parentAccountId != null
              ? accountOptions.find(option => String(option.value) === String(parentAccountId))?.type ??
                DEFAULT_ACCOUNT_TYPE_LABEL
              : DEFAULT_ACCOUNT_TYPE_LABEL),
      parentAccountId: parentAccountId != null ? String(parentAccountId) : "",
      parentAccountLabel: parentAccountLabel ?? ROOT_PARENT_LABEL,
    });
  }, [account, parentAccountId, parentAccountLabel, parentAccountType, open, mode]);

  const handleClose = () => {
    setError("");
    onClose();
  };

  const handleSubmit = async event => {
    event.preventDefault();
    setError("");
    setSubmitting(true);

    const selectedType = ACCOUNT_TYPE_OPTIONS.find(option => option.label === form.type)?.value;

    if (selectedType === undefined) {
      setError("Select a valid account type.");
      setSubmitting(false);
      return;
    }

    const trimmedExternal = form.externalAccountNumber?.trim() || "";
    const payload = {
      externalAccountNumber: trimmedExternal ? trimmedExternal : null,
      name: form.name.trim(),
      segmentCode: form.segmentCode.trim(),
      type: selectedType,
      parentAccountId: form.parentAccountId ? Number(form.parentAccountId) : null,
    };

    try {
      const validationMessage = validateAccountPayload(payload, isEdit);
      if (validationMessage) {
        setSubmitting(false);
        setError(validationMessage);
        return;
      }

      if (isEdit && account) {
        const updatePayload = {
          name: payload.name,
          segmentCode: payload.segmentCode,
          type: payload.type,
        };
        await onUpdate(account.accountId, updatePayload);
      } else {
        const createPayload = {
          externalAccountNumber: payload.externalAccountNumber,
          name: payload.name,
          segmentCode: payload.segmentCode,
          type: payload.type,
          parentAccountId: payload.parentAccountId,
        };
        await onCreate(createPayload);
      }
      setSubmitting(false);
      onClose();
    } catch (err) {
      setSubmitting(false);
      setError(err.message || "Something went wrong. Please try again.");
    }
  };

  return (
    <Dialog open={open} handler={handleClose} size="md">
      <DialogHeader>{isEdit ? "Edit Account" : "Create Account"}</DialogHeader>
      <form onSubmit={handleSubmit}>
        <DialogBody divider className="space-y-4">
          {!isEdit && (
            <Input
              label="External Account Number"
              value={form.externalAccountNumber}
              crossOrigin={undefined}
              onChange={event => setForm(prev => ({ ...prev, externalAccountNumber: event.target.value }))}
            />
          )}
          <Input
            label="Account Name"
            value={form.name}
            crossOrigin={undefined}
            onChange={event => setForm(prev => ({ ...prev, name: event.target.value }))}
            required
          />
          <Input
            label="Segment Code"
            value={form.segmentCode}
            crossOrigin={undefined}
            onChange={event => setForm(prev => ({ ...prev, segmentCode: event.target.value }))}
            required
          />
          <Select
            label="Account Type"
            value={form.type}
            disabled={Boolean(form.parentAccountId)}
            onChange={value => value && setForm(prev => ({ ...prev, type: value }))}
          >
            {ACCOUNT_TYPE_OPTIONS.map(option => (
              <Option key={option.label} value={option.label}>
                {option.label}
              </Option>
            ))}
          </Select>
          {!isEdit && (
            <Input
              label="Parent Account"
              value={form.parentAccountLabel}
              crossOrigin={undefined}
              readOnly
            />
          )}
          {error && (
            <Alert color="red">
              {error}
            </Alert>
          )}
        </DialogBody>
        <DialogFooter>
          <Button variant="text" color="gray" onClick={handleClose} className="mr-2">
            Cancel
          </Button>
          <Button color="blue" type="submit" disabled={submitting}>
            {submitting ? "Saving..." : isEdit ? "Save changes" : "Create account"}
          </Button>
        </DialogFooter>
      </form>
    </Dialog>
  );
}

function DeleteAccountDialog({ account, onClose, onDelete, mutation }) {
  const [error, setError] = useState("");

  useEffect(() => {
    setError("");
  }, [account]);

  if (!account) {
    return null;
  }

  const handleConfirm = async () => {
    setError("");
    try {
      await onDelete(account.accountId);
      onClose();
    } catch (err) {
      setError(err.message || "Unable to delete account.");
    }
  };

  return (
    <Dialog open={Boolean(account)} handler={onClose} size="sm">
      <DialogHeader>Delete account</DialogHeader>
      <DialogBody divider className="space-y-3">
        <Typography color="blue-gray">
          Are you sure you want to delete <span className="font-semibold">{account.name}</span>? This action cannot be undone.
        </Typography>
        {account.hasChildren && (
          <Alert color="amber">
            This account has child nodes and cannot be deleted until all children are removed.
          </Alert>
        )}
        {error && (
          <Alert color="red">
            {error}
          </Alert>
        )}
      </DialogBody>
      <DialogFooter>
        <Button variant="text" color="gray" onClick={onClose} className="mr-2">
          Cancel
        </Button>
        <Button color="red" onClick={handleConfirm} disabled={mutation.isPending || account.hasChildren}>
          {mutation.isPending ? "Deleting..." : "Delete"}
        </Button>
      </DialogFooter>
    </Dialog>
  );
}

function validateAccountPayload(payload, isEdit) {
  if (payload.name.length > 120) {
    return "Account name must be 120 characters or fewer.";
  }

  if (!payload.name) {
    return "Account name is required.";
  }

  if (payload.externalAccountNumber && payload.externalAccountNumber.length > 50) {
    return "External account number must be 50 characters or fewer.";
  }

  if (!payload.segmentCode) {
    return "Segment code is required.";
  }

  if (payload.segmentCode.length > 50) {
    return "Segment code must be 50 characters or fewer.";
  }

  if (payload.type === undefined || payload.type === null) {
    return "Select a valid account type.";
  }

  return "";
}

export default Accounts;

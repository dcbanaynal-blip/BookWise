import { useState } from "react";
import {
  Card,
  CardBody,
  CardHeader,
  Typography,
  Button,
  Dialog,
  DialogHeader,
  DialogBody,
  DialogFooter,
  Input,
  Textarea,
  Select,
  Option,
  Alert,
  Tabs,
  TabsHeader,
  Tab,
} from "@material-tailwind/react";

export function Receipts() {
  const [uploadOpen, setUploadOpen] = useState(false);
  const [form, setForm] = useState({
    file: null,
    documentDate: "",
    sellerName: "",
    vatFlag: "unknown",
    notes: "",
  });
  const [error, setError] = useState("");
  const [reviewTarget, setReviewTarget] = useState(null);
  const [tab, setTab] = useState("all");
  const sampleReceipt = {
    receiptId: 1,
    notes: "Sample OCR extract...\nTotal: 123.45",
  };

  const handleToggle = () => {
    setUploadOpen(prev => !prev);
    setError("");
  };

  const handleFileChange = event => {
    const selected = event.target.files?.[0];
    if (selected && !["image/jpeg", "image/png", "application/pdf"].includes(selected.type)) {
      setError("Only JPEG, PNG, or PDF files are supported.");
      return;
    }
    if (selected && selected.size > 25 * 1024 * 1024) {
      setError("File exceeds the 25MB limit.");
      return;
    }
    setForm(prev => ({ ...prev, file: selected || null }));
    setError("");
  };

  const handleSubmit = event => {
    event.preventDefault();
    if (!form.file) {
      setError("Please select a receipt file.");
      return;
    }
    setUploadOpen(false);
  };

  return (
    <div className="mt-12 mb-8 flex flex-col gap-6">
      <Card>
        <CardHeader floated={false} shadow={false} className="rounded-none flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <Typography variant="h5" color="blue-gray">
            Receipts (Preview)
          </Typography>
          <Button color="blue" onClick={handleToggle}>
            Upload Receipt
          </Button>
        </CardHeader>
        <CardBody className="px-6 py-8">
          <Tabs value={tab} className="w-full mb-4">
            <TabsHeader>
              <Tab value="all" onClick={() => setTab("all")}>
                All receipts
              </Tab>
              <Tab value="unlinked" onClick={() => setTab("unlinked")}>
                Unlinked only
              </Tab>
            </TabsHeader>
          </Tabs>
          <Typography variant="small" color="blue-gray">
            Showing {tab === "all" ? "all receipts" : "only receipts not yet linked to a transaction"} (mock data).
          </Typography>
          <Button variant="text" color="blue" onClick={() => setReviewTarget(sampleReceipt)} className="mt-4">
            Preview Review Drawer
          </Button>
        </CardBody>
      </Card>

      <Dialog open={uploadOpen} handler={handleToggle} size="md">
        <DialogHeader>Upload Receipt</DialogHeader>
        <form onSubmit={handleSubmit}>
          <DialogBody divider className="space-y-4">
            <input type="file" accept="image/jpeg,image/png,application/pdf" onChange={handleFileChange} className="block w-full text-sm text-gray-600 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100" />
            <Input type="date" label="Document Date" value={form.documentDate} crossOrigin={undefined} onChange={event => setForm(prev => ({ ...prev, documentDate: event.target.value }))} />
            <Input label="Seller Name" value={form.sellerName} crossOrigin={undefined} onChange={event => setForm(prev => ({ ...prev, sellerName: event.target.value }))} />
            <Select label="VAT Applicable" value={form.vatFlag} onChange={value => value && setForm(prev => ({ ...prev, vatFlag: value }))}>
              <Option value="unknown">Unknown</Option>
              <Option value="yes">Yes</Option>
              <Option value="no">No</Option>
            </Select>
            <Textarea label="Notes" value={form.notes} onChange={event => setForm(prev => ({ ...prev, notes: event.target.value }))} />
            {error && (
              <Alert color="red">
                {error}
              </Alert>
            )}
          </DialogBody>
          <DialogFooter>
            <Button variant="text" color="gray" className="mr-2" onClick={handleToggle}>
              Cancel
            </Button>
            <Button color="blue" type="submit">
              Upload
            </Button>
          </DialogFooter>
        </form>
      </Dialog>

      <Dialog open={Boolean(reviewTarget)} handler={() => setReviewTarget(null)} size="lg">
        <DialogHeader>Review Receipt</DialogHeader>
        <DialogBody divider className="space-y-4">
          <Typography variant="small" color="blue-gray">
            OCR Text
          </Typography>
          <Textarea value={reviewTarget?.notes ?? ""} onChange={() => {}} />
          <div className="flex flex-col gap-3">
            <Typography variant="small" color="gray">Link this receipt to a transaction:</Typography>
            <div className="flex flex-col gap-2 sm:flex-row">
              <Button color="blue" variant="outlined" className="flex-1">
                Start New Draft Transaction
              </Button>
              <Button color="green" className="flex-1">
                Link to Existing Transaction
              </Button>
            </div>
          </div>
          <div className="space-y-2">
            <Typography variant="small" color="gray">Suggested Account Purpose</Typography>
            <Select label="Account Purpose" value="Travel">
              <Option value="Travel">Travel · Dept Support</Option>
              <Option value="Meals">Meals · Dept Support</Option>
              <Option value="Supplies">Supplies · HQ</Option>
            </Select>
            <Typography variant="small" color="gray">Suggested Posting Entity</Typography>
            <Select label="Posting Entity" value="Dexter">
              <Option value="Dexter">Dexter C. Banaynal</Option>
              <Option value="Finance-Team">Finance - Shared</Option>
              <Option value="IT-Team">IT - Shared</Option>
            </Select>
          </div>
        </DialogBody>
        <DialogFooter>
          <Button variant="text" color="gray" onClick={() => setReviewTarget(null)} className="mr-2">
            Close
          </Button>
          <Button color="blue" onClick={() => setReviewTarget(null)}>
            Mark Ready
          </Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
}

export default Receipts;

import { Card, CardBody, CardHeader, Typography } from "@material-tailwind/react";

export function Receipts() {
  return (
    <div className="mt-12 mb-8 flex flex-col gap-6">
      <Card>
        <CardHeader floated={false} shadow={false} className="rounded-none">
          <Typography variant="h5" color="blue-gray">
            Receipts (Preview)
          </Typography>
          <Typography variant="small" color="gray" className="mt-1 font-normal">
            Receipt capture & OCR review UI is coming soon. This placeholder ensures routing and access controls are in place.
          </Typography>
        </CardHeader>
        <CardBody className="px-6 py-8">
          <Typography variant="small" color="blue-gray">
            Phase 3 will add upload controls, processing indicators, and review tools here. For now, this page verifies the navigation flow for Admins, Accountants, and Bookkeepers.
          </Typography>
        </CardBody>
      </Card>
    </div>
  );
}

export default Receipts;

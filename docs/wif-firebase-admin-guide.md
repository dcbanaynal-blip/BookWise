# Workload Identity Federation for BookWise (Google Cloud ↔ AWS)

This guide explains how to configure Google Cloud and AWS so the **BookWise** backend can run the Firebase Admin SDK without storing a long-lived service-account key. The BookWise workloads running on AWS (Lambda, ECS, EC2, etc.) will exchange their AWS identity for short-lived Google credentials via Workload Identity Federation (WIF). Afterward, BookWise can initialize Firebase Admin with Application Default Credentials (ADC) and verify Firebase ID tokens or call other Admin APIs securely.

---

## Prerequisites

1. **Google Cloud**
   - Access to the Google Cloud project that backs the BookWise Firebase app.
   - Roles: `Service Account Admin` and `Workload Identity Pool Admin`.
2. **AWS**
   - Admin rights in the AWS account where BookWise runs.
   - An IAM role that BookWise workloads use (e.g., `bookwise-server-role`).
3. **Tooling**
   - `gcloud` CLI (version 420 or newer).
   - `aws` CLI (optional but helpful).

---

## Step 1 – Create or reuse the Google service account

1. In **Google Cloud Console → IAM & Admin → Service Accounts**, click **Create service account**.
2. Name it `bookwise-firebase-admin` (or similar) and add a description.
3. Grant only required roles, e.g.:
   - `Firebase Admin` (broad) or `Firebase Authentication Admin`.
4. Finish creation; **do not** generate a key.

Record the service account email, e.g. `bookwise-firebase-admin@bookwise-project.iam.gserviceaccount.com`.

---

## Step 2 – Create a Workload Identity Pool and AWS provider

1. Go to **IAM & Admin → Workload Identity Federation**.
2. Click **Create pool**:
   - Name: `bookwise-aws-pool`
   - ID: `bookwise_aws_pool`
   - Location: `Global`
3. After the pool exists, click **Add provider**:
   - Provider type: **AWS**
   - Name: `bookwise-aws`
   - AWS account: enter the AWS account ID that runs BookWise.
   - Keep default attribute mappings unless you need custom ones.
4. Save the provider.

---

## Step 3 – Allow the AWS role to impersonate the service account

1. Open the `bookwise-firebase-admin` service account → **Permissions** tab → **Grant access**.
2. Add this principal (replace placeholders):
   ```
   principalSet://iam.googleapis.com/projects/<PROJECT_NUMBER>/locations/global/workloadIdentityPools/<POOL_ID>/attribute.aws_role/<AWS_ROLE_ARN>
   ```
3. Assign the role **Workload Identity User**.

This lets the specified AWS IAM role impersonate the BookWise service account through the pool.

---

## Step 4 – Update the AWS IAM role trust policy

1. In the AWS Console, open **IAM → Roles** and select the role your BookWise workload uses (e.g., `bookwise-server-role`).
2. Edit the **Trust relationship** and add a statement similar to:
   ```json
   {
     "Effect": "Allow",
     "Principal": {
       "Federated": "arn:aws:iam::<AWS_ACCOUNT_ID>:oidc-provider/iam.googleapis.com"
     },
     "Action": "sts:AssumeRoleWithWebIdentity",
     "Condition": {
       "StringEquals": {
         "iam.googleapis.com:sub": "projects/<PROJECT_NUMBER>/locations/global/workloadIdentityPools/<POOL_ID>/attribute.aws_role/<AWS_ROLE_ARN>"
       }
     }
   }
   ```
3. Save the policy.

Now, any BookWise workload using that IAM role can request Google credentials via WIF.

---

## Step 5 – Generate the credential configuration file

On a secure workstation:

```bash
gcloud iam workload-identity-pools create-cred-config \
  projects/<PROJECT_NUMBER>/locations/global/workloadIdentityPools/<POOL_ID>/providers/<PROVIDER_ID> \
  --service-account=<SERVICE_ACCOUNT_EMAIL> \
  --aws \
  --output-file=bookwise-wif-credentials.json
```

This JSON does not contain any private key; it only documents how to exchange AWS web-identity tokens for Google STS tokens. Store it securely (Secrets Manager, encrypted S3, etc.).

---

## Step 6 – Deploy credentials with the BookWise workload

1. Make the `bookwise-wif-credentials.json` file available to the container/instance that hosts BookWise.
2. Set:
   ```
   GOOGLE_APPLICATION_CREDENTIALS=/path/to/bookwise-wif-credentials.json
   ```
3. Ensure the workload runs with the IAM role from Step 4.

---

## Step 7 – Initialize Firebase Admin SDK (ADC)

Example (Node.js):

```js
const admin = require('firebase-admin');
admin.initializeApp(); // ADC picks up the WIF credentials

// Middleware to verify BookWise tokens
app.use(async (req, res, next) => {
  const token = req.headers.authorization?.split('Bearer ')[1];
  if (!token) return res.status(401).send('Missing token');

  try {
    req.firebaseUser = await admin.auth().verifyIdToken(token);
    next();
  } catch (err) {
    res.status(401).send('Invalid token');
  }
});
```

Because ADC references the WIF config, Firebase Admin uses short-lived tokens without any JSON key.

---

## Step 8 – Monitor and maintain

1. **Google Cloud:** enable Audit Logs for IAM Credentials and Workload Identity Federation to monitor access from BookWise.
2. **AWS:** monitor CloudTrail for `AssumeRoleWithWebIdentity` events on `bookwise-server-role`.
3. To revoke access, remove the `Workload Identity User` binding or update the AWS trust policy—no key rotation required.

---

## Summary

After these steps:

- BookWise workloads on AWS can impersonate the Google service account via WIF.
- Firebase Admin runs with Application Default Credentials (no long-lived keys).
- BookWise can verify Firebase ID tokens and call Admin APIs using secure, short-lived credentials.

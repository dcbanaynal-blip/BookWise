# Creating a Firebase Service Account Key for Server Verification

Follow these steps to generate and safely use a Firebase service-account key for verifying Firebase Authentication ID tokens on your server with the Firebase Admin SDK.

## 1. Confirm prerequisites (prefer Workload Identity Federation)
1. Identify the Google Cloud project that backs your Firebase app. You can find it in Firebase Console -> Project Settings -> General -> "Project ID".
2. Decide which runtime will call the Firebase Admin SDK.
   - **If you run on Google Cloud (Cloud Run, GKE, GCE, App Engine)**, you already get Application Default Credentials without any key file. Simply attach the right service account to that workload.
   - **If you run outside Google Cloud**, Google now recommends using **Workload Identity Federation (WIF)** so your platform issues its own OIDC/SAML identity token and Google exchanges it for short-lived credentials. Only fall back to a JSON key if WIF is impossible.
3. Ensure your user account (or automation identity) has `Service Account Admin` plus `Workload Identity Pool Admin` (when configuring WIF) in the project.
4. If you still need to store a JSON key (last resort), prepare the target location (for example Secret Manager, AWS Secrets Manager, or Vault).

## 2. (Preferred) Configure Workload Identity Federation
If your backend is outside Google Cloud:
1. In Google Cloud Console go to **IAM & Admin -> Workload Identity Federation** and create a pool (e.g., `bookwise-pool`) and provider (GitHub, AWS, generic OIDC, etc.) that trusts your runtime.
2. Grant the service account below the role `Workload Identity User` on that pool.
3. In your runtime, configure the `GOOGLE_APPLICATION_CREDENTIALS` environment variable to point to a JSON file that references the pool/provider (use `gcloud iam workload-identity-pools create-cred-config ...`). The file contains *no* private key, just the WIF metadata.
4. With ADC pointing to that file, the Firebase Admin SDK can call Google APIs (manage users, custom claims) without downloading a long-lived key.

If you run on GCP, attach the service account to the compute resource and skip WIF—the Admin SDK will use the metadata server automatically.

## 3. (Fallback) Download a service account key
Only do this when WIF/ADC is unavailable.

1. Open https://console.cloud.google.com/ and ensure the project is selected.
2. From the left-hand navigation menu, choose **IAM & Admin -> Service Accounts**. You can also reach this page directly at https://console.cloud.google.com/iam-admin/serviceaccounts.

The remaining steps are identical whether you use WIF or a key file: you still need a service account identity that Firebase Admin will impersonate.

## 4. Create or select a service account
1. Go to https://console.cloud.google.com/.
2. Make sure the correct project is selected in the top navigation bar. Switch projects if needed.
3. If you already have a dedicated service account for backend authentication tasks, click its email to manage it. Otherwise, click **+ Create Service Account**.
2. Provide a descriptive name (for example `firebase-admin-verifier`) and optionally a description (for example "Verifies Firebase ID tokens on BookWise backend"). Click **Create and Continue**.
3. Assign a role that grants the minimum required permissions:
   - Recommended: `Firebase Admin` or `Firebase Authentication Admin`.
   - For tighter control, choose `Service Account Token Creator` plus any other roles your backend needs.
4. Click **Continue**, then **Done** to finish creation without granting user access.

## 5. (Optional) Generate a JSON key
Only if WIF/ADC is not possible:
1. In the service accounts table, locate the account you created or selected.
2. Click the three-dot menu at the far right and choose **Manage keys**.
3. In the Keys tab, click **Add key -> Create new key**.
4. Select the **JSON** key type and click **Create**.
5. A JSON key file downloads to your computer; this is the only time Google shows it. Treat it as a secret and store it securely. Plan to rotate or delete it once you can move to WIF.

## 6. Configure the BookWise API
1. For **local development**, place the JSON key on your workstation (outside source control) and set `Firebase:ServiceAccountKeyPath` in `src/backend/BookWise.Api/appsettings.Development.json` to that file path. The API now loads the key automatically when `ASPNETCORE_ENVIRONMENT=Development`.
2. For **production on AWS**, deploy the WIF credential config JSON (created via `gcloud iam workload-identity-pools create-cred-config`) and set the `GOOGLE_APPLICATION_CREDENTIALS` environment variable on the IIS host to point at it. The BookWise API will call `GoogleCredential.GetApplicationDefault()` and use WIF/ADC without any explicit code changes.

## 7. Store and reference the credential securely
1. Move the JSON file to a secure, access-controlled location (never commit it to git).
2. Choose how your server will read the credentials:
   - **Environment variable:** base64-encode the JSON and store it in `FIREBASE_ADMIN_KEY_JSON`.
   - **Secrets manager file:** upload the JSON to your secrets manager and load it at runtime.
   - **Local secrets path:** keep it on disk and reference the absolute path (ensure file permissions restrict access).
3. Example of loading the key from an environment variable in Node.js:
```js
const admin = require('firebase-admin');

const firebaseAdminKey = JSON.parse(process.env.FIREBASE_ADMIN_KEY_JSON);

admin.initializeApp({
  credential: admin.credential.cert(firebaseAdminKey),
});
```

## 8. Verify Firebase ID tokens on the server
1. Obtain the `Authorization: Bearer <idToken>` header from your clients.
2. Use the Admin SDK `verifyIdToken` API. Example in Node.js:
```js
const authenticate = async (req, res, next) => {
  try {
    const idToken = req.headers.authorization?.split('Bearer ')[1];
    if (!idToken) throw new Error('Missing ID token');

    const decoded = await admin.auth().verifyIdToken(idToken, true); // true checks for revoked tokens
    req.firebaseUser = decoded;
    return next();
  } catch (err) {
    return res.status(401).json({ error: 'Invalid or expired Firebase token' });
  }
};
```
3. Handle common errors (expired token, revoked token, clock skew) and log them for debugging.

## 9. Rotate and monitor credentials
1. If you use WIF/ADC, rotate upstream credentials according to your identity provider’s policy and review WIF audit logs.
2. If you still rely on JSON keys, repeat the "Create new key" process whenever you need rotation; download the new JSON, deploy it, and then delete the old key via **Remove key** in the Keys tab.
2. Keep an inventory of where each key is deployed so you can update services quickly when rotating or revoking.
3. Enable Cloud Audit Logs for "Admin Read" and "Admin Write" to track key creation and deletion events.
4. Consider alerting on key downloads, deletions, or IAM permission changes.

## 10. Optional hardening
- Restrict service account IAM permissions to only the functionality your backend requires. Prefer WIF or attached service accounts over key files.
- If running on Google Cloud, attach the service account directly to compute resources so you can avoid distributing long-lived keys entirely.
- Require MFA for admins who can create or download keys.
- Document the rotation cadence (for example quarterly) and store keys using envelope encryption where possible.

With this process—preferably using Workload Identity Federation or other keyless ADC—your backend can trust Firebase ID tokens via the Firebase Admin SDK without relying on long-lived private keys.

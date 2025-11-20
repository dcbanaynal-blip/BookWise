## BookWise IaC

1. Authenticate: `az login` and select the target subscription.
2. Deploy RG + resources:

```bash
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters resourceGroupName=rg-bookwise-dev sqlAdminLogin=bookwiseadmin sqlAdminPassword=<secret> environment=dev
```

3. Capture outputs (SQL connection string) and store them as secrets in your CI/CD pipeline or Azure Key Vault.

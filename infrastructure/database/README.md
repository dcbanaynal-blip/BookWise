## Database Operations

- Apply EF Core migrations locally: `cd src/backend && dotnet tool run dotnet-ef database update -s BookWise.Api/BookWise.Api.csproj -p BookWise.Infrastructure/BookWise.Infrastructure.csproj`.
- Generate deployment script for DBA review:

```bash
cd src/backend
dotnet tool run dotnet-ef migrations script \
  -p BookWise.Infrastructure/BookWise.Infrastructure.csproj \
  -s BookWise.Api/BookWise.Api.csproj \
  -o ../../infrastructure/database/InitialCreate.sql
```

- Seed reference data by running the stored procedure template under `seeds/seed-accounts.sql` after migrations complete.

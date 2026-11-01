# ApexPost Counter Platform
## Week 03 — SQL Server, EF Core 10 and relational persistence

**Assessment target:** 98–100/100  
**Solution:** .NET 10 · C# 14 · ASP.NET Core MVC · EF Core 10 · SQL Server  
**Database:** `ApexPostDb`  
**Status:** Source and documentation package prepared; machine-specific build, SQL Server tests and performance evidence must be completed before final sign-off.

---

## 1. Project overview

ApexPost is a branch-based postal counter platform. Week 03 replaces the previous JSON persistence adapter with SQL Server as the system of record while retaining the established domain rules, MVC screens, authentication, authorization, middleware and filters.

The application supports branch and staff administration, product catalogue and stock, counter transactions, voids/refunds, shipping, money orders, identity verification, audit history and operational reports.

### Solution structure

| Project | Responsibility |
|---|---|
| `src/ApexPost.Domain` | Entities, value objects, lifecycle rules and domain invariants. No infrastructure dependency. |
| `src/ApexPost.Application` | Use cases, commands, DTOs, service contracts, repository contracts and Unit of Work abstraction. |
| `src/ApexPost.Infrastructure` | EF Core 10 DbContext/configurations, SQL Server repositories, migrations, stored procedures, query services, audit persistence, seeding and outbox/inbox. |
| `src/ApexPost.Web` | MVC controllers/views, authentication, authorization, middleware, filters and dependency-injection composition root. |
| `tests/ApexPost.UnitTests` | Domain and application unit tests; no SQL Server dependency. |
| `tests/ApexPost.IntegrationTests` | WebApplicationFactory, SQL Server persistence, middleware/security and end-to-end integration tests. |

See [DESIGN.md](DESIGN.md) for architecture and dependency diagrams, [SQL-DESIGN-AND-FLOW.md](SQL-DESIGN-AND-FLOW.md) for the relational design and execution paths, and [docs/adr](docs/adr/) for architecture decision records.

---

## 2. Prerequisites

- .NET 10 SDK (`10.0.100` or a compatible later feature band).
- Visual Studio 2026 or VS Code.
- SQL Server Developer, Express or LocalDB.
- SQL Server Management Studio (SSMS) for schema inspection, stored procedures, actual execution plans and `STATISTICS IO/TIME`.
- EF Core CLI:
  ```powershell
  dotnet tool install --global dotnet-ef --version 10.0.0
  ```

The repository pins the SDK in `global.json`. SQL Server is the only supported database provider for this assessment. Do not use the shared training database.

---

## 3. Configure local secrets

Secrets are not stored in source control. Run these commands from the solution root and replace the connection string with the SQL Server instance available on your machine.

```powershell
dotnet user-secrets set "ConnectionStrings:ApexPost" "Server=(localdb)\MSSQLLocalDB;Database=ApexPostDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" --project src/ApexPost.Web

dotnet user-secrets set "DemoPasswords:CounterClerk" "<your-long-unique-demo-password>" --project src/ApexPost.Web
dotnet user-secrets set "DemoPasswords:BranchManager" "<your-long-unique-demo-password>" --project src/ApexPost.Web
dotnet user-secrets set "DemoPasswords:Auditor" "<your-long-unique-demo-password>" --project src/ApexPost.Web
dotnet user-secrets set "DemoPasswords:Administrator" "<your-long-unique-demo-password>" --project src/ApexPost.Web

dotnet user-secrets set "Integration:ApiKey" "<generate-a-long-random-partner-key>" --project src/ApexPost.Web
```

The Infrastructure design-time factory also accepts `APEXPOST_CONNECTION_STRING` for EF CLI operations. Integration tests accept `APEXPOST_TEST_CONNECTION` (a SQL Server connection with permission to create and drop isolated test databases).

Do not commit user-secrets, `.env` files, real identity documents, payment data, passwords, API keys or production connection strings.

---

## 4. Create and migrate the database

1. Start your local SQL Server.
2. In SSMS, connect to `master` and execute `database/Create-ApexPostDb.sql`. This creates the empty dedicated database only.
3. Confirm the `ConnectionStrings:ApexPost` secret points to `ApexPostDb`.
4. From the solution root, restore and apply EF Core migrations:

```powershell
dotnet restore ApexPost.sln

dotnet ef migrations list `
  --project src/ApexPost.Infrastructure `
  --startup-project src/ApexPost.Web

dotnet ef database update `
  --project src/ApexPost.Infrastructure `
  --startup-project src/ApexPost.Web
```

### Migration sequence

| Migration | Purpose |
|---|---|
| `202611010001_InitialCreate` | Normalized tables, keys, relationships, constraints, defaults, audit columns and rowversion columns. |
| `202611010002_AddWorkloadIndexes` | Composite, covering and filtered indexes for documented access patterns. |
| `202611010003_StoredProcedures` | Reporting and stock-adjustment stored procedures. |
| `202611010004_OfflineSyncOutbox` | Transactional outbox and idempotent sync inbox tables. |

Generate the authoritative idempotent script using the EF CLI on the target machine:

```powershell
dotnet ef migrations script --idempotent `
  --project src/ApexPost.Infrastructure `
  --startup-project src/ApexPost.Web `
  --output database/migrations/ApexPostDb-ef-generated-idempotent.sql
```

`database/migrations/ApexPostDb-idempotent.sql` is a hand-maintained deployment fallback. It is not represented as EF CLI-generated output. Review the generated script and migration operations before deployment. Do not run `sql/001_CreateSchema.sql` before EF migrations: that file is a standalone schema reference, while the EF migrations are the application migration history. `EnsureCreated()` is not used.

Automatic migrations at application startup are disabled by default. The integration-test factory controls schema creation for its disposable test database.

---

## 5. Seed data and demo accounts

The Infrastructure seeder creates fictional demonstration data on a clean database, including:

- 5 branches and 20 staff members;
- 30 products and branch stock rows;
- shipping zones and service rates;
- at least 200 historical paid counter transactions and payment records;
- a closed branch (`CHN01`) and an inactive staff example for business-rule demonstrations.

Example seeded identities:

| Role | Example login |
|---|---|
| Counter Clerk | `counterclerk1@apexpost.local` |
| Branch Manager | `branchmanager2@apexpost.local` |
| Auditor | `auditor3@apexpost.local` |
| Administrator | `administrator4@apexpost.local` |

Set the role-specific demo passwords using user-secrets before login. If a role password is not configured, its demo account remains unconfigured and cannot authenticate. No default password is published in this repository. Existing databases are not overwritten by the clean-database seed path.

---

## 6. Run the application

```powershell
dotnet build ApexPost.sln -c Release
dotnet run --project src/ApexPost.Web
```

Open the HTTPS URL printed by ASP.NET Core. Use the seeded account corresponding to the role being demonstrated.

In Development, EF command logging and query diagnostics emit SQL samples for low-stock, daily-sales and shipment-search LINQ queries. The outbox dispatcher is a simulated publisher; a real message broker/carrier transport is outside this assessment slice.

---

## 7. Test strategy

### Unit tests — no database

```powershell
dotnet test tests/ApexPost.UnitTests/ApexPost.UnitTests.csproj -c Release
```

These tests cover domain rules and application service transaction boundaries with fakes/spies.

### SQL Server and MVC integration tests

The `WebApplicationFactory` test fixture creates a uniquely named disposable SQL Server database and attempts cleanup when the fixture is disposed. By default, the fixture targets LocalDB. For another SQL Server instance, provide a connection string with permission to create and drop databases:

```powershell
$env:APEXPOST_TEST_CONNECTION = "Server=localhost;Database=master;Integrated Security=True;TrustServerCertificate=True"
dotnet test tests/ApexPost.IntegrationTests/ApexPost.IntegrationTests.csproj -c Release
```

Run the complete suite:

```powershell
dotnet test ApexPost.sln -c Release --logger "console;verbosity=normal"
```

Do not point integration tests at a development, shared training or production database. Do not remove tests or bypass migrations to make the suite pass.

---

## 8. SQL artefacts

| Path | Purpose |
|---|---|
| `database/Create-ApexPostDb.sql` | Creates the empty dedicated database. |
| `database/migrations/` | EF migration deployment scripts and hand-maintained idempotent fallback. |
| `sql/001_CreateSchema.sql` | Standalone normalized schema reference. |
| `sql/002_ReportingQueries.sql` | Reporting joins, grouping, aggregation and date-range examples. |
| `sql/003_StoredProcedures.sql` | `CREATE OR ALTER` procedures for reports and stock adjustment. |
| `sql/004_SaleTransactionDemo.sql` | Atomic multi-table sale and rollback demonstration. |
| `sql/005_BlockingDemo.sql` | Two-session blocking and isolation exercise. |
| `sql/006_CandidateIndexes.sql` | Candidate workload indexes. |
| `sql/007_OfflineSync.sql` | Outbox/inbox SQL reference. |
| `sql/008_PerformanceCapture.sql` | Actual-plan, IO/TIME, Query Store and index-storage capture. |
| `sql/009_LeastPrivilege.sql` | Least-privilege database role and grants. |

Read [SQL-DESIGN-AND-FLOW.md](SQL-DESIGN-AND-FLOW.md) before running the scripts. Some scripts are demonstrations or standalone references; follow their headers and do not apply both standalone schema creation and EF migrations to the same empty database.

---

## 9. Architecture decisions and assessment evidence

The design uses explicit decisions rather than relying on undocumented conventions. Week 03 adds ADR-009 through ADR-016:

1. Relational schema and delete behaviour.
2. Workload-driven index selection.
3. Stored procedure boundaries.
4. Repository and Unit of Work boundaries.
5. Domain/persistence model separation.
6. Stock concurrency and offline conflict handling.
7. Audit and outbox transaction boundaries.
8. Use-case atomicity and projected read DTOs.

See [DESIGN.md](DESIGN.md) and the individual records in [docs/adr](docs/adr/).

The assessment evidence map is [ASSESSMENT-TRACEABILITY.md](ASSESSMENT-TRACEABILITY.md). The actionable submission gate is [CHECKLIST.md](CHECKLIST.md). Performance methodology and evidence status are in [PERFORMANCE.md](PERFORMANCE.md).

---

## 10. Performance evidence — do not fabricate measurements

Candidate indexes and capture scripts are included. Actual `.sqlplan` files, logical reads, CPU/elapsed times, index storage/write cost and estimated-versus-actual row analysis must be collected on the target SQL Server. The preparation environment did not provide the target SQL Server measurements, so the report deliberately labels them **Not measured**.

Follow `PERFORMANCE.md` and save evidence under `database/evidence/`. A source-code index definition is not proof of a faster execution plan.

---

## 11. Quality gates and known limitations

Run these commands on the assessment machine and retain the output:

```powershell
dotnet restore ApexPost.sln
dotnet build ApexPost.sln -c Release
dotnet test ApexPost.sln -c Release
dotnet ef migrations list --project src/ApexPost.Infrastructure --startup-project src/ApexPost.Web
```

Before submission, confirm:

- Release build succeeds with zero warnings and zero errors.
- All unit and SQL Server integration tests pass.
- The generated idempotent migration script is reviewed.
- Stored procedures return expected results against the seeded SQL Server database.
- Actual execution plans and `STATISTICS IO/TIME` evidence are captured.
- Secrets are absent from tracked files.
- Git branches, PRs and meaningful commits are visible in the actual GitHub repository.

The ZIP archive does not include `.git` history, so it cannot prove feature branches, pull requests or commit hygiene. The courier/outbox publisher remains simulated and must not be described as a production carrier or broker integration.

**Assessment target is 98–100 marks. A mark is not guaranteed by documentation alone; the runtime and measured evidence gates above are mandatory.**

# ApexPost Week 03 assessment traceability

| Requirement | Implementation / evidence |
|---|---|
| Dedicated SQL Server `ApexPostDb` | `sql/001_CreateSchema.sql`, migrations, README user-secrets setup |
| 3NF tables, PK/FK/delete behaviour, unique/check/default/audit columns | `sql/001_CreateSchema.sql`, `ApexPostDbContext`, Fluent configurations, ADR-009 |
| BranchCode/Sku/staff email uniqueness | `UQ_Branches_Code`, `UQ_Products_Sku`, `UQ_Staff_Email` |
| Rowversion concurrency | Shadow `RowVersion` on BranchStock and CounterTransactions; integration test for stale stock update |
| Masked identity document only | `IdentityVerificationRecord`, explicit mapper, `varchar(8)` and SQL check constraint |
| ER diagram and normalization anomalies | `DESIGN.md` Week 03 section |
| Reporting joins, LEFT JOIN, GROUP BY/HAVING, top 10 | `sql/002_ReportingQueries.sql` |
| Multi-table T-SQL transaction | `sql/004_SaleTransactionDemo.sql` |
| Blocking / isolation demonstration | `sql/005_BlockingDemo.sql` and `PERFORMANCE.md` |
| Four workload indexes and SARGability | migration 002, `sql/006_CandidateIndexes.sql`, `sql/008_PerformanceCapture.sql`, `PERFORMANCE.md` |
| Actual plan, IO/TIME and storage evidence | Capture scripts are present; target SQL Server measurements and `.sqlplan` files remain to be captured (not fabricated) |
| Three required stored procedures | `sql/003_StoredProcedures.sql`, migration 003 |
| Parameterized EF procedure calls | `EfReportService`, `EfStockAdjustmentService` use `Database.SqlQuery` / `ExecuteSqlInterpolatedAsync` |
| EF Core 10 DbContext and Fluent mappings | `Data/ApexPostDbContext.cs`, `Data/Configurations` |
| Initial + follow-up migrations | Migrations 001–004 |
| Idempotent schema deployment | `database/migrations/ApexPostDb-idempotent.sql`; regenerate EF CLI output with README command |
| 5 branches, 20 staff, 30 products, 10 zones/rates, 200+ transactions | `AppSeeder`; SQL integration test checks branches/staff/products/transactions/payments |
| AsNoTracking and no N+1 | `EfPlatformQueryService` projects product stock, transactions, transaction details/lines, money orders, staff, branches and verification summaries; `EfReportService` uses projections/procedures; Development EF command logging is enabled |
| ExecuteUpdateAsync | `EfProductPriceRevisionService` plus SQL integration test |
| One SaveChanges for paid retail sale, stock and payment | `CounterService.CompleteAsync`; `rowversion` conflict test |
| Explicit transaction + execution strategy | `EfUnitOfWork.ExecuteInTransactionAsync`, stock adjustment and price revision services |
| ToQueryString samples | `EfQueryDiagnostics.Capture`, logged in Development; exact output must be saved from local run |
| Application-owned repositories and UoW | Application interfaces; EF repositories stage changes; `IUnitOfWork` commits |
| No IQueryable from repositories | Repository contracts return entity collections/DTOs only |
| Domain/persistence separation | Identity verification persistence record + mapper |
| Request DTO / over-posting | MVC request/view models, application commands and response DTOs are separate; screen response DTOs are projected in `EfPlatformQueryService`; sensitive/identity server fields are not accepted in money-order request |
| Audit table and separate audit transaction | `EfAuditStore` with independent DbContext factory; ADR-015 |
| Offline outbox/inbox and idempotency | `ChangeTrackingOutboxInterceptor`, `OutboxDispatcher`, `SyncInbox`, `OfflineSyncService`, migration 004 |
| Unit tests and SQL integration tests | 12 unit-test facts and 27 `WebApplicationFactory`/SQL Server integration-test facts, including seed states, rowversion, rollback, procedures and void/refund stock restoration; execution requires .NET 10 and SQL Server |
| 8 new ADRs | ADR-009 through ADR-016 |
| Secrets not committed | User-secrets instructions; no development connection string or real API credential in source. The GitHub Actions workflow contains a test-only SQL Server password used only by its disposable service container. |

## Verification status

Source files, SQL scripts, project references and test definitions are included. Build/test execution was not possible in the preparation environment because the .NET SDK and SQL Server were unavailable. Therefore no claim is made that the Release build or SQL Server integration suite passed. The actual EF CLI-generated idempotent script and SQL performance evidence also remain to be captured on the target environment. Run the commands in `README.md` before submission.


## Package policy review

The central package file pins the EF Core SQL Server/Design/Tools packages and the xUnit/MVC test packages. It also contains a `System.Security.Cryptography.Pkcs` transitive-version pin. The assessment explicitly requires trainer approval and an ADR for anything outside its allowed package families. Confirm with the trainer whether this transitive security pin and the standard `Microsoft.NET.Test.Sdk` / `xunit.runner.visualstudio` test infrastructure are accepted. Do not claim strict package-policy compliance until this review is complete.

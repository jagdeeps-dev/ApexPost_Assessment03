# Assessment Checklist and Submission Gate
## ApexPost Week 03 — SQL Server / EF Core 10

**Target:** 98–100/100  
**Status key:** `[x]` = source/artifact exists in this archive; `[ ]` = execution, review or measured evidence is still required. A checked source item does not mean the runtime test has passed.

---

## A. Relational schema, constraints and migrations — 15 marks

- [x] Dedicated `ApexPostDb` bootstrap script.
- [x] EF Core code-first migrations and model snapshot.
- [x] Normalized organization, catalogue, stock, transaction, payment, shipping, money-order, identity, audit and sync tables.
- [x] Primary keys, foreign keys, unique constraints, check constraints and defaults are defined in migration/schema artefacts.
- [x] Historical delete behavior is documented; financial history is not cascade-deleted.
- [x] Stock and counter transaction rowversion concurrency mapping is present.
- [x] Identity document persistence is masked and constrained.
- [x] ER diagram and normalization rationale are documented in `DESIGN.md` and `SQL-DESIGN-AND-FLOW.md`.
- [ ] Apply all migrations to a fresh disposable SQL Server database.
- [ ] Review the generated EF idempotent script against the model snapshot and live schema.
- [ ] Verify FK delete actions and constraint definitions from SQL Server catalog views.

## B. SQL queries, joins, transactions and integrity — 10 marks

- [x] Reporting SQL includes joins, left joins, grouping, aggregates and branch/date filters.
- [x] Multi-table sale transaction sample uses explicit transaction handling, `TRY/CATCH`, `XACT_ABORT`, `XACT_STATE()` and `THROW`.
- [x] Stored procedures use parameter validation and `CREATE OR ALTER`.
- [x] Blocking/isolation demonstration is provided.
- [ ] Execute report queries and verify expected totals against seeded records.
- [ ] Execute the sale rollback demonstration in a disposable database.
- [ ] Complete the two-session blocking exercise and record the observed isolation behavior.

## C. Index optimization and execution-plan evidence — 15 marks

- [x] Workload-specific composite, covering and filtered index candidates are documented.
- [x] SARGable half-open date-range example is documented.
- [x] Query Store, statistics, actual-plan and index-storage capture scripts are present.
- [ ] Capture baseline and after `.sqlplan` files with actual row counts for at least four workloads.
- [ ] Capture `SET STATISTICS IO/TIME` output and record logical reads, CPU and elapsed time.
- [ ] Record index storage and write overhead.
- [ ] Investigate at least one estimated-versus-actual row mismatch.
- [ ] Populate `PERFORMANCE.md` with measured values only; do not estimate or fabricate results.

## D. Stored procedures and safe EF use — 10 marks

- [x] Daily sales, low stock, stock adjustment and additional branch reporting procedures are included.
- [x] Procedure definitions use `CREATE OR ALTER`.
- [x] EF procedure calls use parameterized APIs.
- [x] Least-privilege role script is included.
- [ ] Execute every procedure against SQL Server and verify result shape and totals.
- [ ] Verify the application database principal has only the documented permissions.
- [ ] Retain procedure execution evidence.

## E. EF Core mapping, querying, change tracking and transactions — 15 marks

- [x] `ApexPostDbContext` and Fluent entity configurations are included.
- [x] Owned/value-object mappings and persistence-specific identity mapping are included.
- [x] No `EnsureCreated()` schema shortcut is used.
- [x] Read paths use no-tracking DTO projections.
- [x] Repository contracts do not expose `IQueryable`.
- [x] Sale write path stages stock, transaction and payment in one use-case commit.
- [x] Rowversion concurrency and HTTP 409 mapping are implemented in source.
- [x] Set-based price revision uses `ExecuteUpdateAsync` with explicit transaction/execution strategy.
- [ ] Run persistence mapping and transaction rollback integration tests on SQL Server.
- [ ] Prove stale stock update produces the expected 409 response.
- [ ] Verify all database operations propagate `CancellationToken`.
- [ ] Capture three actual `ToQueryString()` outputs from the SQL Server provider.

## F. Repository, Unit of Work, DTOs and model boundaries — 20 marks

- [x] Domain/Application/Infrastructure/Web project dependency direction is documented.
- [x] Application owns repository and Unit of Work contracts.
- [x] Infrastructure repositories stage changes; Unit of Work owns commit.
- [x] Request models, application commands and response DTOs are separated.
- [x] Direct `DbContext` projections are restricted to Infrastructure read/query services.
- [x] Identity verification has a persistence record and explicit mapper.
- [x] Repository methods do not return EF queryables.
- [ ] Run fake/spy Unit of Work tests proving one commit on success and no commit on validation failure.
- [ ] Review every MVC POST/PUT model for over-posting of IDs, status, branch, role and audit fields.
- [ ] Run an over-posting integration test with forged server-owned values.

## G. Testing, documentation, ADRs and Git hygiene — 15 marks

- [x] Unit test project and SQL Server/WebApplicationFactory integration test project are included in the solution.
- [x] Integration factory is designed to use an isolated disposable SQL Server database.
- [x] CI workflow provisions SQL Server for automated validation.
- [x] README includes prerequisites, user-secrets, migration, run and test commands.
- [x] DESIGN includes architecture, dependency direction, ER model and SOLID mapping.
- [x] SQL-DESIGN-AND-FLOW documents relational and transaction flows.
- [x] PERFORMANCE documents methodology and explicitly unmeasured evidence.
- [x] Eight Week 03 ADRs (ADR-009 through ADR-016) cover the required decision areas.
- [x] Assessment traceability matrix maps requirements to source and evidence.
- [ ] Run Release build and retain zero-warning/zero-error output.
- [ ] Run all unit and SQL Server integration tests and retain the complete test summary.
- [ ] Review package allow-list compliance and obtain trainer approval for any non-allowed direct dependency.
- [ ] Verify no credentials, real personal data or secrets are tracked.
- [ ] Complete feature branches, pull requests and meaningful commits in the actual GitHub repository. ZIP files do not contain Git history.

---

## Final release sign-off

- [ ] Fresh database migration succeeds.
- [ ] Release build succeeds with **0 warnings and 0 errors**.
- [ ] All unit tests pass.
- [ ] All SQL Server integration tests pass.
- [ ] Required MVC/API security and Problem Details scenarios pass.
- [ ] Stored procedure results are verified.
- [ ] Actual execution plans, IO/TIME, index storage and generated SQL evidence are saved.
- [ ] README, DESIGN, SQL design, ADRs, PERFORMANCE and traceability agree with the final implementation.
- [ ] GitHub branch/PR/commit evidence is available.

**Marking principle:** The target is 98–100, but a score cannot be guaranteed by source files or documentation alone. The assessor must be able to reproduce the build, database behavior, tests and measured performance evidence.

# Project Structure

```text
ApexPost/
├── ApexPost.sln
├── README.md / DESIGN.md / CHECKLIST.md / ASSESSMENT-TRACEABILITY.md
├── docs/adr/
├── src/
│   ├── ApexPost.Domain/
│   │   ├── Primitives/
│   │   └── Rules/
│   ├── ApexPost.Application/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   ├── DTOs/
│   │   ├── Reports/
│   │   └── Validation/
│   ├── ApexPost.Infrastructure/
│   │   ├── Persistence/
│   │   ├── Repositories/
│   │   ├── Audit/
│   │   ├── Caching/
│   │   ├── Notifications/
│   │   └── SeedData/
│   └── ApexPost.Web/
│       ├── Controllers/ Views/ Models/ wwwroot/
│       ├── Middleware/ Filters/ Security/
│       └── Program.cs
└── tests/
    ├── ApexPost.UnitTests/Domain/Services/Rules/
    └── ApexPost.IntegrationTests/Middleware/Security/Authentication/Filters/
```

## Dependency direction
- Domain has no project references.
- Application references Domain.
- Infrastructure implements Application contracts and references Application + Domain.
- Web references Application, Domain, and Infrastructure only for composition at startup.
- Tests reference the projects they verify.

Folders organize responsibilities; namespaces remain `ApexPost.*` so this structural refactor does not unnecessarily change public type names. Empty folders are intentionally placeholders for the relevant assessment concerns; add files only when implementing a real requirement. Do not add a database/ORM.

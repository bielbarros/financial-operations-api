# Financial Operations API

English · [Português](README.pt-BR.md)

Fictional API for customer, account, ledger, and transfer operations. It is a portfolio project for practicing Clean Architecture on .NET, from an empty solution through persistence, HTTP, authentication, and tests.

## Stack

.NET 9 · C# · ASP.NET Core · EF Core · SQL Server · JWT · OpenAPI · Docker

EF Core, SQL Server, JWT, and Docker are introduced in later steps. They are not in this repository yet.

## Status

Part 2 of 12: solution skeleton.

| Project | Role today |
| --- | --- |
| `FinancialOperations.Domain` | Empty class library. No project references. |
| `FinancialOperations.Application` | Empty class library. References Domain. |
| `FinancialOperations.Infrastructure` | Empty class library. References Application and Domain. |
| `FinancialOperations.Api` | ASP.NET Core host. No business endpoints yet. |
| `FinancialOperations.UnitTests` | xUnit. References Domain and Application. |
| `FinancialOperations.IntegrationTests` | xUnit. References the API. |

`dotnet build` and `dotnet test` succeed. The two tests are the empty template facts and will be replaced when domain rules exist.

## Build

From this folder:

```powershell
dotnet build
dotnet test
```

## Documentation

Every document in `docs/` exists in Portuguese and English. English files use the `.en.md` suffix.

- [Roadmap](docs/ROADMAP.en.md) — the twelve parts and the order they are built · [Português](docs/ROADMAP.md)
- [Part 2 — solution skeleton](docs/02-esqueleto-da-solution.en.md) — commands, references, and what the template left behind · [Português](docs/02-esqueleto-da-solution.md)

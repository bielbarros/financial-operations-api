# Financial Operations API

[English](README.md) · Português

API fictícia de clientes, contas, lançamentos e transferências. É um projeto de portfólio para praticar Clean Architecture em .NET, da solution vazia até persistência, HTTP, autenticação e testes.

## Stack

.NET 9 · C# · ASP.NET Core · EF Core · SQL Server · JWT · OpenAPI · Docker

EF Core, SQL Server, JWT e Docker entram em partes posteriores. Ainda não estão neste repositório.

## Estado

Parte 2 de 12: esqueleto da solution.

| Projeto | Papel hoje |
| --- | --- |
| `FinancialOperations.Domain` | Biblioteca vazia. Nenhuma referência de projeto. |
| `FinancialOperations.Application` | Biblioteca vazia. Referencia o Domain. |
| `FinancialOperations.Infrastructure` | Biblioteca vazia. Referencia Application e Domain. |
| `FinancialOperations.Api` | Host ASP.NET Core. Ainda sem endpoint de negócio. |
| `FinancialOperations.UnitTests` | xUnit. Referencia Domain e Application. |
| `FinancialOperations.IntegrationTests` | xUnit. Referencia a Api. |

`dotnet build` e `dotnet test` passam. Os dois testes são o exemplo vazio do template e serão substituídos quando existirem regras de domínio.

## Build

Nesta pasta:

```powershell
dotnet build
dotnet test
```

## Documentação

Cada documento em `docs/` existe em português e em inglês. O inglês usa o sufixo `.en.md`.

- [Roteiro](docs/ROADMAP.md) — as doze partes e a ordem em que são construídas · [English](docs/ROADMAP.en.md)
- [Parte 2 — esqueleto da solution](docs/02-esqueleto-da-solution.md) — comandos, referências e o que saiu do template · [English](docs/02-esqueleto-da-solution.en.md)

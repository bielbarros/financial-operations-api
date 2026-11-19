# Financial Operations API — build roadmap

[Português](ROADMAP.md) · English

Fictional financial-operations API, built as a portfolio project.

Target stack: **.NET 9** · C# · ASP.NET Core · EF Core · SQL Server · JWT · Swagger · Docker.

SDK already installed on this machine: .NET 9.0.311 (.NET 8.0.428 is also present). The solution targets `net9.0`.

This file is the map. Code for a part lands only after the previous part is clear.

## Languages

Every document in `docs/` has two versions, written together. The Portuguese file is the base name. English uses the same name with an `.en.md` suffix.

| Português | English |
| --- | --- |
| [ROADMAP.md](ROADMAP.md) | [ROADMAP.en.md](ROADMAP.en.md) |
| [02-esqueleto-da-solution.md](02-esqueleto-da-solution.md) | [02-esqueleto-da-solution.en.md](02-esqueleto-da-solution.en.md) |

GitHub renders the root `README.md` in English. The Portuguese pair is [README.pt-BR.md](../README.pt-BR.md).

---

## How the work proceeds

1. One part at a time.
2. Read the goal and the "clear when" criterion.
3. Explain it in your own words. A short explanation is enough.
4. Only then is the next part implemented.

Each part ends with a done criterion: something that exists on disk, or a sentence you can say without looking at the text.

---

## The right order

In a legacy system, the screen, the rule, and the table usually appear together. Here the business rule comes first, and the database follows from it.

| Order | Part | What appears | Does the database enter? |
| --- | --- | --- | --- |
| 1 | Business map | Vocabulary and rules, still without C# | No |
| 2 | Solution skeleton | Empty projects and the dependency rule | No |
| 3 | Domain | Entities, values, and rules in C# | No |
| 4 | Application | Use cases and persistence contracts | No |
| 5 | Infrastructure | EF Core, SQL Server, migrations | Yes, here |
| 6 | API | First endpoint working end to end | Already there |
| 7 | API edge | Validation, errors, pagination, logs, Swagger | — |
| 8 | Consistency | Atomic transfer in the database | — |
| 9 | Security | JWT and authorization | — |
| 10 | Audit | Who did what, and when | — |
| 11 | Tests | Domain rules and one API flow | — |
| 12 | Delivery | Docker, README, and a repository a recruiter can follow | — |

Reason for the order: in Clean Architecture the dependency arrows point inward. Domain does not know EF Core, SQL Server, JWT, or controllers. A database-first model would let the tables dictate the rules. The portfolio needs the opposite.

---

## Target structure

```text
financial-operations-api/
├── FinancialOperations.sln
├── src/
│   ├── FinancialOperations.Domain/
│   ├── FinancialOperations.Application/
│   ├── FinancialOperations.Infrastructure/
│   └── FinancialOperations.Api/
├── tests/
│   ├── FinancialOperations.UnitTests/
│   └── FinancialOperations.IntegrationTests/
├── docs/
│   ├── ROADMAP.md
│   ├── ROADMAP.en.md
│   ├── 02-esqueleto-da-solution.md
│   └── 02-esqueleto-da-solution.en.md
├── README.md
└── README.pt-BR.md
```

The short README already exists in both languages. Part 12 expands it with Docker, authentication, and a transfer example.

`tests/` sits outside `src/` because tests are not the product. Test projects reference the product. The product does not reference tests.

### Who may reference whom

```text
Api  --------------→  Application  →  Domain
 |                         ↑
 Infrastructure  ----------┘
         |
         └→ Domain
```

- **Domain**: rules and vocabulary. No web or database packages.
- **Application**: use cases ("open an account", "transfer"). Depends only on Domain. Declares the interfaces it needs (`IAccountRepository`) and does not implement SQL.
- **Infrastructure**: implements those interfaces with EF Core and SQL Server. Depends on Application and Domain.
- **Api**: HTTP, Swagger, authentication, and composition in `Program.cs`. Depends on Application and Infrastructure, which is where the implementations are wired. Balance rules stay out of controllers.

---

## Part 1 — Business map (done)

Goal: agree on the vocabulary and the rules before any class or table exists.

Bounded context: **account operations**. Customer registration, account opening, ledger entries, and transfers between accounts in this API. Outside this context: cards, boletos, real PIX, tax, internet banking.

Money here is fictional. The design imitates a simple ledger to demonstrate consistency. It does not move real value.

### Vocabulary

| Business term | Name in code | What it is |
| --- | --- | --- |
| Customer | `Customer` | The account holder. May exist with no account. |
| Account | `Account` | One customer's account. Owns the balance and the entries. |
| Ledger entry | `LedgerEntry` | An immutable credit or debit. The statement is the list of these entries. |
| Transfer | `Transfer` | A debit on one account and a credit on another, stored together or not stored at all. |
| Statement | query | A paged, filterable history of one account's entries. |

`LedgerEntry` says the record is not edited after it is created. In EF Core, "transaction" already means a database transaction. A second type with that name would collide.

### Aggregates

An aggregate is the group of objects that changes only through its root, so a stray `UPDATE` cannot bypass the rule.

- **Customer** is a root. It holds the customer's identity and status (active or inactive).
- **Account** is a root. Balance and entries live here. A new entry appears only when the account credits or debits.
- **Transfer** is an operation across two accounts. It is not a loose table that updates balances from outside the aggregate. The application asks one account to `Debit` and the other to `Credit`, and Infrastructure stores both changes in the same database transaction.

### Rules the Domain will protect

1. An entry amount is always positive. The direction (credit or debit) carries the sign.
2. A debit that would make the balance negative is rejected. Accounts in this system do not go overdrawn.
3. An account that is not active accepts no credit, debit, or transfer.
4. An inactive customer cannot open a new account.
5. A transfer requires two active, distinct accounts in the same currency.
6. An entry, once created, is not changed or deleted. A future reversal is a new entry in the opposite direction.
7. Both sides of a transfer are persisted together. If one fails, neither remains.

Initial currency: `BRL` only, represented as a value object rather than a string copied through the code. Money uses `decimal`.

### Why money is `decimal`

`decimal` is the base-10 numeric type in C#. A value such as `10.50` fits in it exactly. SQL Server maps it to `decimal` / `numeric`, so the balance the rule computed is the balance the database stores.

`float` and `double` are binary floating point (IEEE 754). Base 2 represents powers of two well and everyday decimal fractions poorly. `0.10` in `double` becomes an approximation. Adding many entries accumulates that difference, and the balance stops matching the statement.

The literal suffix selects the type: `0.1` is `double`, `0.1f` is `float`, `0.1m` is `decimal`.

| Type | Size | Base | Typical use |
| --- | --- | --- | --- |
| `float` | 32 bits | 2 | Graphics, sensors, fast approximation. About 6 to 9 digits. |
| `double` | 64 bits | 2 | Science, measurements, math APIs. About 15 to 17 digits. Very large range. |
| `decimal` | 128 bits | 10 | Money, tax, commercial quantity. 28 to 29 significant digits. |
| `Half` | 16 bits | 2 | Tight memory (ML). No role in a balance. |

`int`, `long`, and `BigInteger` are integers: they do not store cents on their own. A professional design stores `long` as cents (`1050` = R$ 10.50). The arithmetic stays exact because there is no fraction. That design carries the scale (2 places) through every operation. This project uses `decimal` so the value stays readable (`10.50m`) and still exact for the real's places. Division (`1m / 3m`) still rounds, so monetary values enter and leave with 2 places and an explicit rounding mode when the operation needs it.

`decimal` is slower than `double`. For balances and entries that cost does not matter. Exactness is the requirement.

Identifiers: `Guid`. Opening balance: zero. Account number: generated by the application, unique, and human-readable. It is not the `Guid`.

### What this part does not do

This part has no `.sln`, class, table, Docker, or endpoint. The design above is the contract for parts 3 through 8.

### Clear when you can say

- An **account** is the root that protects the balance and the entries.
- A **transfer** is two entries that count only if both are stored.
- **SQL Server appears in part 5**, after the Domain already exists in C#.

---

## Part 2 — Solution skeleton (done)

Goal: create the solution, the four projects under `src/`, the two under `tests/`, and the references in the diagram. The projects compile empty.

Record of what landed on disk, including the commands and the reference graph: [02-esqueleto-da-solution.en.md](02-esqueleto-da-solution.en.md) · [Português](02-esqueleto-da-solution.md).

`dotnet build` finished without errors and `dotnet test` passed the 2 sample tests from the template. Domain references nothing.

---

## Part 3 — Domain

Goal: translate part 1 into C#, still without a database.

These arrive in this order:

1. Enums (`AccountStatus`, `CustomerStatus`, `EntryDirection`, `Currency`).
2. Value objects (`Money`, `AccountNumber`, `Document`).
3. Domain exceptions (insufficient balance, inactive account, same account on both sides of a transfer).
4. Entities `Customer`, `Account`, `LedgerEntry`.
5. Methods that carry the rules: `Activate` / `Deactivate`, `Open`, `Credit`, `Debit`.

`Credit` and `Debit` update the balance and append the entry. Callers do not assign `Balance` directly.

Done when you can read `Debit` and point at the line where the rule lives. Unit tests for insufficient balance and an inactive account are part of this trail's part 11. The rule is already callable in part 3.

---

## Part 4 — Application

Goal: the use cases the API will call. One use case per operation, with an explicit input and output.

First slice, the happy path already designed:

- Register a customer
- Open an account
- Credit
- Debit
- Transfer
- Read the statement (paged, filtered by period and direction)

Application defines the interfaces (`ICustomerRepository`, `IAccountRepository`, `IUnitOfWork`) and the DTOs. It does not reference `DbContext`.

Done when each use case fits in one sentence ("given an active account and an amount, the account credits and the use case asks to save").

---

## Part 5 — Infrastructure and the database

Goal: persist the Domain. SQL Server, EF Core, and migrations appear here.

- Map entities to tables (EF configuration, with no database attributes scattered through Domain).
- `DbContext` and repositories.
- `IUnitOfWork` opening a database transaction.
- Initial migration.
- Connection string from configuration, never hardcoded.

The schema comes from the classes in part 3. Index adjustments (unique account number, unique document) are born in the mapping.

Done when the migration applies to a local SQL Server (or a container) and the tables reflect Customer, Account, and LedgerEntry.

---

## Part 6 — API, first vertical slice

Goal: one small HTTP flow, complete.

`POST /api/customers` and `POST /api/accounts` (or the pair chosen at the time), with `Program.cs` wiring Infrastructure and Application.

A thin controller receives the request, calls the use case, and returns the HTTP status. The balance rule stays in Domain.

Done when a customer and an account are created in SQL Server through HTTP, and the row is visible in the database.

---

## Part 7 — API edge

Goal: the API behaves like a product, past a controller demo.

- Input validation (FluentValidation) before the use case.
- Global exception handling: a domain error becomes 422 or 409 with a stable body; an unexpected error becomes 500 without leaking a stack trace in production.
- Pagination and filters on the statement.
- Structured request logging, without logging a full document or a secret.
- Swagger / OpenAPI with the contracts visible.

Done when an invalid debit returns a predictable error, and the statement accepts page, size, and a filter.

---

## Part 8 — Transfer consistency

Goal: the transfer `POST` stores the debit and the credit in the same database transaction.

If the destination account fails, the origin debit does not remain. That uses the `IUnitOfWork` from part 5, rather than two saves with no transaction around them.

Done when a forced failure on the second side rolls back the first (an integration test, or a check in the database).

---

## Part 9 — Security

Goal: JWT and authorization.

- Issue a token for an API user. That access user is distinct from the business `Customer`, so login is not mixed with the account holder.
- Operation endpoints require a token.
- A simple authorization policy: an authenticated user may operate. An administrative role stays reserved for audit if it fits.

The JWT secret comes from configuration or user secrets, never from the repository.

Done when a missing token is rejected, and a valid token still completes the part 6 flow.

---

## Part 10 — Audit

Goal: record who ran the operation, when, and which aggregate was affected.

Creation fields (`CreatedAt`, `CreatedBy`) are filled in one place (an EF interceptor or an audit service), not copied into every controller.

Done when a credit leaves a readable trail of author and instant.

---

## Part 11 — Tests

Goal: a safety net for the rules the portfolio claims to have.

- Domain unit tests: insufficient balance, inactive account, transfer to the same account, zero amount.
- Application unit tests with fake repositories: the use case orchestrates and does not compute the balance itself.
- Integration: one flow boots the API (or the `DbContext`) and checks that the transfer is atomic.

Done when `dotnet test` passes and the test names describe the rule, not the method.

---

## Part 12 — Delivery

Goal: a repository a recruiter can open and understand in a few minutes.

- `Dockerfile` and `docker-compose` with the API and SQL Server.
- README, in both languages: what it is, how to run it, how to authenticate, a transfer example, the dependency diagram, and the decisions (why `LedgerEntry`, why the balance lives on the account).
- A .NET `.gitignore`, with no `bin/`, `obj/`, user secrets, or a real connection string.
- Small commits, one subject each, along the trail, rather than a single commit at the end.

Done when another person follows the README and completes a transfer.

---

## Decisions already closed

- Solution: `FinancialOperations`
- Target framework: `net9.0`
- Architecture: Clean Architecture, with Domain at the center
- Database modeled after the Domain, through EF Core migrations
- Initial currency: `BRL`
- Money: `decimal`
- Entries are immutable; a future reversal is a new entry
- The balance lives on the account and changes only inside `Credit` and `Debit`

Decisions left for the part that uses them: exact route names, the account-number format, the log provider, and whether local SQL Server is an install or a container. The container is part of part 12 either way. Part 5 uses whichever is simpler on this machine.

---

## Where we are

Repository: https://github.com/bielbarros/financial-operations-api

- [x] Part 1 written in this file
- [x] Part 1 confirmed
- [x] Part 2 — solution skeleton ([record](02-esqueleto-da-solution.en.md))
- [ ] Parts 3 to 12

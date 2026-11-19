# Part 2 — Solution skeleton

[Português](02-esqueleto-da-solution.md) · English

Record of what was created on 19 November 2026.

The solution compiles, the projects reference each other in the Clean Architecture direction, and there is still no business rule, database, or endpoint. This file is the snapshot of that cut.

Trail context: [ROADMAP.en.md](ROADMAP.en.md).

## Result

| Check | Result |
| --- | --- |
| `dotnet build` | Succeeded in 7.2 s. Six projects compiled. |
| `dotnet test` | 2 tests, 0 failures, duration 2.6 s. |
| Framework | `net9.0` on every project. |
| SDK on this machine | .NET 9.0.311. |

The two tests are the empty xUnit template sample (`Test1`). A `[Fact]` that throws nothing passes. They exist to prove that `tests/` runs. Balance and transfer tests arrive in part 11.

## Tree

```text
financial-operations-api/
├── FinancialOperations.sln
├── .gitignore
├── README.md
├── README.pt-BR.md
├── docs/
│   ├── ROADMAP.md
│   ├── ROADMAP.en.md
│   ├── 02-esqueleto-da-solution.md
│   └── 02-esqueleto-da-solution.en.md
├── src/
│   ├── FinancialOperations.Domain/
│   ├── FinancialOperations.Application/
│   ├── FinancialOperations.Infrastructure/
│   └── FinancialOperations.Api/
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Properties/launchSettings.json
└── tests/
    ├── FinancialOperations.UnitTests/
    └── FinancialOperations.IntegrationTests/
```

`bin/` and `obj/` appear after a build. The `.gitignore` from `dotnet new gitignore` keeps those folders out of Git.

Inside the solution, projects are grouped in the virtual folders `src` and `tests` (`--solution-folder`). That organizes Solution Explorer. Each project's real folder was set by `-o` at creation.

## Role of each project

| Project | SDK | Responsibility in this cut |
| --- | --- | --- |
| `FinancialOperations.Domain` | `Microsoft.NET.Sdk` | Empty library. Home of the rules from part 3. No `ProjectReference`. |
| `FinancialOperations.Application` | `Microsoft.NET.Sdk` | Empty library. Use cases from part 4. References Domain only. |
| `FinancialOperations.Infrastructure` | `Microsoft.NET.Sdk` | Empty library. EF Core and SQL Server from part 5. References Application and Domain. |
| `FinancialOperations.Api` | `Microsoft.NET.Sdk.Web` | HTTP host. Starts, registers controllers, and exposes the OpenAPI document in Development. No controller yet. |
| `FinancialOperations.UnitTests` | `Microsoft.NET.Sdk` | xUnit. References Domain and Application. |
| `FinancialOperations.IntegrationTests` | `Microsoft.NET.Sdk` | xUnit. References the Api, which is how the HTTP flow will be exercised. |

Domain uses `Microsoft.NET.Sdk` (a library). The Api uses `Microsoft.NET.Sdk.Web` (Kestrel, configuration, the HTTP pipeline). That SDK difference is what allows `dotnet run` on the Api and keeps Domain from becoming a website by accident.

Every `.csproj` has `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`. A null reference is a compiler warning, and the common usings (`System`, and ASP.NET Core on the Api) arrive without a manual declaration.

## References

`dotnet add A reference B` makes A depend on B. A can see B's public types. The direction below is the rule the next parts keep.

| Project | References |
| --- | --- |
| Domain | nothing |
| Application | Domain |
| Infrastructure | Application, Domain |
| Api | Application, Infrastructure |
| UnitTests | Domain, Application |
| IntegrationTests | Api |

```text
Api  --------------→  Application  →  Domain
 |                         ↑
 Infrastructure  ----------┘
         |
         └→ Domain
```

Domain's `.csproj` confirms the empty center: only `TargetFramework`, `ImplicitUsings`, and `Nullable`. There is no `ProjectReference` and no `PackageReference`.

A test project may point at the product. Tests stay outside the layer diagram.

## Commands that were run

From the `financial-operations-api` folder:

```powershell
dotnet new sln -n FinancialOperations

dotnet new classlib -n FinancialOperations.Domain -o src/FinancialOperations.Domain -f net9.0
dotnet new classlib -n FinancialOperations.Application -o src/FinancialOperations.Application -f net9.0
dotnet new classlib -n FinancialOperations.Infrastructure -o src/FinancialOperations.Infrastructure -f net9.0
dotnet new webapi -n FinancialOperations.Api -o src/FinancialOperations.Api -f net9.0 --use-controllers

dotnet new xunit -n FinancialOperations.UnitTests -o tests/FinancialOperations.UnitTests -f net9.0
dotnet new xunit -n FinancialOperations.IntegrationTests -o tests/FinancialOperations.IntegrationTests -f net9.0
```

```powershell
dotnet sln add src/FinancialOperations.Domain/FinancialOperations.Domain.csproj --solution-folder src
dotnet sln add src/FinancialOperations.Application/FinancialOperations.Application.csproj --solution-folder src
dotnet sln add src/FinancialOperations.Infrastructure/FinancialOperations.Infrastructure.csproj --solution-folder src
dotnet sln add src/FinancialOperations.Api/FinancialOperations.Api.csproj --solution-folder src
dotnet sln add tests/FinancialOperations.UnitTests/FinancialOperations.UnitTests.csproj --solution-folder tests
dotnet sln add tests/FinancialOperations.IntegrationTests/FinancialOperations.IntegrationTests.csproj --solution-folder tests
```

```powershell
dotnet add src/FinancialOperations.Application/FinancialOperations.Application.csproj reference src/FinancialOperations.Domain/FinancialOperations.Domain.csproj

dotnet add src/FinancialOperations.Infrastructure/FinancialOperations.Infrastructure.csproj reference src/FinancialOperations.Application/FinancialOperations.Application.csproj
dotnet add src/FinancialOperations.Infrastructure/FinancialOperations.Infrastructure.csproj reference src/FinancialOperations.Domain/FinancialOperations.Domain.csproj

dotnet add src/FinancialOperations.Api/FinancialOperations.Api.csproj reference src/FinancialOperations.Application/FinancialOperations.Application.csproj
dotnet add src/FinancialOperations.Api/FinancialOperations.Api.csproj reference src/FinancialOperations.Infrastructure/FinancialOperations.Infrastructure.csproj

dotnet add tests/FinancialOperations.UnitTests/FinancialOperations.UnitTests.csproj reference src/FinancialOperations.Domain/FinancialOperations.Domain.csproj
dotnet add tests/FinancialOperations.UnitTests/FinancialOperations.UnitTests.csproj reference src/FinancialOperations.Application/FinancialOperations.Application.csproj

dotnet add tests/FinancialOperations.IntegrationTests/FinancialOperations.IntegrationTests.csproj reference src/FinancialOperations.Api/FinancialOperations.Api.csproj
```

```powershell
dotnet new gitignore
dotnet build
dotnet test
```

`--use-controllers` selects the controller style (`CustomersController`, `AccountsController` in later parts). The template's authentication was left off. JWT arrives in part 9, written in this project.

## What left the template

`classlib` creates a `Class1.cs`. `webapi` creates a weather-forecast sample. These files were removed:

- `src/FinancialOperations.Domain/Class1.cs`
- `src/FinancialOperations.Application/Class1.cs`
- `src/FinancialOperations.Infrastructure/Class1.cs`
- `src/FinancialOperations.Api/Controllers/WeatherForecastController.cs`
- `src/FinancialOperations.Api/WeatherForecast.cs`

`Program.cs` stayed. With zero controllers, `MapControllers()` publishes no business route. The Api starts and, in Development, publishes the OpenAPI document.

## State of the Api

Package brought in by the template: `Microsoft.AspNetCore.OpenApi` 9.0.13.

```csharp
builder.Services.AddControllers();
builder.Services.AddOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

`AddOpenApi` / `MapOpenApi` produce the OpenAPI document (the JSON contract). The Swagger UI, the page where a request is sent from the browser, is not in this project yet. It arrives in part 7, together with validation, errors, and pagination.

`appsettings.json` is the template default: log level and `AllowedHosts`. There is no connection string. SQL Server arrives in part 5.

## What this part deliberately leaves out

- Entities, value objects, and rules (part 3).
- Use cases (part 4).
- EF Core, migrations, and SQL Server (part 5).
- A controller and a customer or account route (part 6).
- FluentValidation, global error handling, pagination, business logging, and the Swagger UI (part 7).
- JWT (part 9).
- Dockerfile and the presentation README (part 12).

## How to repeat the check

From the `financial-operations-api` folder:

```powershell
dotnet build
dotnet test
```

Both commands finish without errors. Any new `ProjectReference` has to respect the graph in this part: Domain still references nothing.

# Parte 2 — Esqueleto da solution

Português · [English](02-esqueleto-da-solution.en.md)

Registro do que foi criado em 19 de novembro de 2026.

A solution compila, os projetos se referenciam na direção da Clean Architecture e ainda não existe regra de negócio, banco nem endpoint. Este arquivo é o retrato desse corte.

Contexto da trilha: [ROADMAP.md](ROADMAP.md).

## Resultado

| Verificação | Resultado |
| --- | --- |
| `dotnet build` | Sucesso em 7,2 s. Seis projetos compilados. |
| `dotnet test` | 2 testes, 0 falhas, duração 2,6 s. |
| Framework | `net9.0` em todos os projetos. |
| SDK usado na máquina | .NET 9.0.311. |

Os dois testes são o exemplo vazio do template xUnit (`Test1`). Um `[Fact]` sem exceção passa. Eles existem para provar que a pasta `tests/` executa. Os testes de saldo e de transferência entram na parte 11.

## Árvore

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

`bin/` e `obj/` aparecem depois do build. O `.gitignore` gerado por `dotnet new gitignore` deixa essas pastas fora do Git.

Na solution, os projetos estão agrupados nas pastas virtuais `src` e `tests` (`--solution-folder`). Isso organiza o Solution Explorer. A pasta real de cada projeto foi definida pelo `-o` na criação.

## Papel de cada projeto

| Projeto | SDK | Responsabilidade neste corte |
| --- | --- | --- |
| `FinancialOperations.Domain` | `Microsoft.NET.Sdk` | Biblioteca vazia. Centro das regras a partir da parte 3. Nenhuma `ProjectReference`. |
| `FinancialOperations.Application` | `Microsoft.NET.Sdk` | Biblioteca vazia. Casos de uso a partir da parte 4. Referencia só o Domain. |
| `FinancialOperations.Infrastructure` | `Microsoft.NET.Sdk` | Biblioteca vazia. EF Core e SQL Server a partir da parte 5. Referencia Application e Domain. |
| `FinancialOperations.Api` | `Microsoft.NET.Sdk.Web` | Host HTTP. Sobe, registra controllers e expõe o documento OpenAPI em Development. Ainda sem controller. |
| `FinancialOperations.UnitTests` | `Microsoft.NET.Sdk` | xUnit. Referencia Domain e Application. |
| `FinancialOperations.IntegrationTests` | `Microsoft.NET.Sdk` | xUnit. Referencia a Api, por onde o fluxo HTTP será exercitado. |

O Domain usa `Microsoft.NET.Sdk` (biblioteca). A Api usa `Microsoft.NET.Sdk.Web` (Kestrel, configuração, pipeline HTTP). Essa diferença de SDK é o que permite `dotnet run` no projeto da Api e impede o Domain de virar um site por acidente.

Todos os `.csproj` estão com `<Nullable>enable</Nullable>` e `<ImplicitUsings>enable</ImplicitUsings>`. Referência nula é aviso de compilação, e os usings mais comuns (`System`, e na Api também o de ASP.NET Core) entram sem declaração manual.

## Referências

`dotnet add A reference B` faz A depender de B. A passa a enxergar os tipos públicos de B. A direção abaixo é a regra que as próximas partes preservam.

| Projeto | Referencia |
| --- | --- |
| Domain | ninguém |
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

O `.csproj` do Domain confirma o centro vazio de dependências: só `TargetFramework`, `ImplicitUsings` e `Nullable`. Não há `ProjectReference` nem `PackageReference`.

Projeto de teste pode apontar para o produto. Teste não entra no diagrama das camadas.

## Comandos executados

Na pasta `financial-operations-api`:

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

`--use-controllers` escolhe o modelo de controller (`CustomersController`, `AccountsController` nas partes seguintes). A autenticação do template ficou desligada. JWT entra na parte 9, escrito neste projeto.

## O que saiu do template

O `classlib` cria um `Class1.cs`. A `webapi` cria um exemplo de previsão do tempo. Esses arquivos foram removidos:

- `src/FinancialOperations.Domain/Class1.cs`
- `src/FinancialOperations.Application/Class1.cs`
- `src/FinancialOperations.Infrastructure/Class1.cs`
- `src/FinancialOperations.Api/Controllers/WeatherForecastController.cs`
- `src/FinancialOperations.Api/WeatherForecast.cs`

`Program.cs` permaneceu. Com zero controllers, `MapControllers()` não publica rota de negócio. A Api sobe e, em Development, publica o documento OpenAPI.

## Estado da Api

Pacote trazido pelo template: `Microsoft.AspNetCore.OpenApi` 9.0.13.

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

`AddOpenApi` / `MapOpenApi` geram o documento OpenAPI (o contrato JSON). A interface Swagger, aquela página em que se executa a requisição no navegador, ainda não está neste projeto. Ela entra na parte 7, junto com validação, erros e paginação.

`appsettings.json` está no padrão do template: nível de log e `AllowedHosts`. Não há connection string. SQL Server entra na parte 5.

## O que esta parte de propósito não tem

- Entidades, value objects e regras (parte 3).
- Casos de uso (parte 4).
- EF Core, migration e SQL Server (parte 5).
- Controller e rota de cliente ou conta (parte 6).
- FluentValidation, tratamento global de erro, paginação, log de negócio e UI do Swagger (parte 7).
- JWT (parte 9).
- Dockerfile e README de apresentação (parte 12).

## Como repetir a verificação

Na pasta `financial-operations-api`:

```powershell
dotnet build
dotnet test
```

Os dois comandos fecham sem erro. Qualquer `ProjectReference` nova precisa respeitar o grafo desta parte: o Domain continua sem referência.

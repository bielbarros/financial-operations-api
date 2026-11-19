# Financial Operations API — roteiro de construção

Português · [English](ROADMAP.en.md)

API fictícia de operações financeiras, feita para portfólio.

Stack alvo: **.NET 9** · C# · ASP.NET Core · EF Core · SQL Server · JWT · Swagger · Docker.

SDK já instalado nesta máquina: .NET 9.0.311 (também há o 8.0.428). A solution nasce em `net9.0`.

Este arquivo é o mapa. O código de cada parte só entra quando a parte anterior estiver clara.

## Idiomas

Cada documento em `docs/` tem duas versões, nascidas juntas. O nome em português é o arquivo base. O inglês usa o mesmo nome com o sufixo `.en.md`.

| Português | English |
| --- | --- |
| [ROADMAP.md](ROADMAP.md) | [ROADMAP.en.md](ROADMAP.en.md) |
| [02-esqueleto-da-solution.md](02-esqueleto-da-solution.md) | [02-esqueleto-da-solution.en.md](02-esqueleto-da-solution.en.md) |

Na raiz, o GitHub abre o `README.md` em inglês. O par em português é o [README.pt-BR.md](../README.pt-BR.md).

---

## Como vamos trabalhar

1. Uma parte por vez.
2. Você lê o objetivo e o "o que precisa ficar claro".
3. Você explica com as suas palavras (pode ser curto).
4. Só então a próxima parte é implementada.

Cada parte termina com um critério de pronto: algo que existe no disco ou uma frase que você consegue dizer sem olhar o texto.

---

## A ordem certa

Num sistema legado, tela, regra e tabela costumam nascer juntas. Aqui a regra de negócio nasce primeiro e o banco é uma consequência dela.

| Ordem | Parte | O que nasce | O banco entra? |
| --- | --- | --- | --- |
| 1 | Mapa do negócio | Vocabulário e regras, ainda sem C# | Não |
| 2 | Esqueleto da solution | Projetos vazios e a regra de dependência | Não |
| 3 | Domain | Entidades, valores e regras em C# | Não |
| 4 | Application | Casos de uso e contratos de persistência | Não |
| 5 | Infrastructure | EF Core, SQL Server, migrations | Sim, aqui |
| 6 | API | Primeiro endpoint funcionando de ponta a ponta | Já existe |
| 7 | Borda da API | Validação, erros, paginação, logs, Swagger | — |
| 8 | Consistência | Transferência atômica no banco | — |
| 9 | Segurança | JWT e autorização | — |
| 10 | Auditoria | Quem fez o quê, e quando | — |
| 11 | Testes | Regras de domínio e um fluxo de API | — |
| 12 | Entrega | Docker, README e repositório apresentável | — |

Motivo da ordem: em Clean Architecture as setas de dependência apontam para dentro. Domain não conhece EF Core, SQL Server, JWT nem controllers. Se o banco viesse primeiro, o modelo de tabelas mandaria nas regras. O inverso é o que o portfólio precisa mostrar.

---

## Estrutura alvo

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

O README curto, nas duas línguas, já está na raiz. A parte 12 amplia esse texto com Docker, autenticação e o exemplo de transferência.

`tests/` fica fora de `src/` porque teste não é o produto. Os projetos de teste referenciam o produto; o produto não referencia teste.

### Quem pode referenciar quem

```text
Api  --------------→  Application  →  Domain
 |                         ↑
 Infrastructure  ----------┘
         |
         └→ Domain
```

- **Domain**: regras e vocabulário. Zero pacotes de web ou de banco.
- **Application**: casos de uso ("abrir conta", "transferir"). Depende só do Domain. Define interfaces (`IAccountRepository`) que ela precisa; não implementa SQL.
- **Infrastructure**: implementa essas interfaces com EF Core e SQL Server. Depende de Application e Domain.
- **Api**: HTTP, Swagger, autenticação, composição no `Program.cs`. Depende de Application e Infrastructure (para ligar as implementações). Não coloca regra de saldo dentro de controller.

---

## Parte 1 — Mapa do negócio (concluída)

Objetivo: combinar o vocabulário e as regras antes de existir classe ou tabela.

Contexto limitado (bounded context): **operações de conta**. Cadastro de cliente, abertura de conta, lançamentos e transferência entre contas desta API. Fora deste contexto: cartão, boleto, PIX real, impostos, internet banking.

Dinheiro aqui é fictício. O desenho imita um razão contábil simples para demonstrar consistência, não para operar valor real.

### Vocabulário

| Termo no negócio | Nome no código | O que é |
| --- | --- | --- |
| Cliente | `Customer` | Pessoa titular. Pode existir sem conta. |
| Conta | `Account` | Conta de um cliente. Dono do saldo e dos lançamentos. |
| Lançamento | `LedgerEntry` | Crédito ou débito imutável. O extrato é a lista desses lançamentos. |
| Transferência | `Transfer` | Um débito numa conta e um crédito noutra, gravados juntos ou nem gravados. |
| Extrato | consulta | Histórico paginado e filtrável dos lançamentos de uma conta. |

`LedgerEntry` (lançamento de razão) deixa explícito que o registro não se edita depois de criado. "Transaction" no EF Core já significa transação de banco; outro tipo com o mesmo nome confunde.

### Agregados

Agregado é o grupo de objetos que só muda passando pela raiz, para a regra não ser furada por um `UPDATE` solto.

- **Customer** é raiz. Guarda identidade e status do cliente (ativo ou inativo).
- **Account** é raiz. Saldo e lançamentos moram aqui. Um lançamento novo só aparece quando a conta credita ou debita.
- **Transfer** é uma operação entre duas contas. Ela não é uma tabela solta que "atualiza saldo" por fora do agregado. A aplicação pede `Debit` numa conta e `Credit` na outra, e a Infrastructure grava as duas mudanças na mesma transação de banco.

### Regras que o Domain vai proteger

1. Valor de lançamento é sempre positivo. A direção (crédito ou débito) diz o sinal.
2. Débito que deixaria o saldo negativo é recusado. A conta não fica devedora neste sistema.
3. Conta que não está ativa não recebe crédito, débito nem transferência.
4. Cliente inativo não abre conta nova.
5. Transferência exige duas contas ativas, distintas, na mesma moeda.
6. Lançamento criado não é alterado nem apagado. Estorno, quando existir, é um lançamento novo no sentido contrário.
7. Os dois lados da transferência persistem juntos. Se um falha, nenhum dos dois permanece.

Moeda inicial: apenas `BRL`, representada como valor (value object), não como string solta espalhada pelo código. Dinheiro usa `decimal`.

### Por que dinheiro é `decimal`

`decimal` é o tipo numérico do C# em base 10. Um valor como `10.50` cabe nele de forma exata. No SQL Server ele mapeia para `decimal` / `numeric`, então o saldo que a regra calculou é o saldo que o banco guarda.

`float` e `double` são ponto flutuante binário (IEEE 754). A base 2 representa bem potências de dois e mal frações decimais do dia a dia. `0.10` em `double` vira uma aproximação. Somar muitos lançamentos acumula essa diferença, e o saldo deixa de bater com o extrato.

O sufixo do literal diz o tipo: `0.1` é `double`, `0.1f` é `float`, `0.1m` é `decimal`.

| Tipo | Tamanho | Base | Uso típico |
| --- | --- | --- | --- |
| `float` | 32 bits | 2 | Gráfico, sensor, aproximação rápida. Cerca de 6 a 9 dígitos. |
| `double` | 64 bits | 2 | Ciência, medidas, APIs de matemática. Cerca de 15 a 17 dígitos. Alcance enorme. |
| `decimal` | 128 bits | 10 | Dinheiro, imposto, quantidade comercial. 28 a 29 dígitos significativos. |
| `Half` | 16 bits | 2 | Memória apertada (ML). Sem papel em saldo. |

`int`, `long` e `BigInteger` são inteiros: não guardam centavos sozinhos. Há um desenho profissional que usa `long` em centavos (`1050` = R$ 10,50). A conta fica exata porque não existe fração. Quem adota isso carrega a escala (2 casas) em todo lugar. Neste projeto o `decimal` deixa o valor legível (`10.50m`) e ainda é exato para as casas do real. Divisão (`1m / 3m`) ainda arredonda; por isso valor monetário entra e sai com 2 casas, com arredondamento explícito quando a operação exigir.

`decimal` é mais lento que `double`. Para saldo e lançamento isso é irrelevante. A exatidão é o requisito.

Identificadores: `Guid`. Saldo inicial na abertura: zero. Número da conta: gerado pela aplicação, único, legível (não é o `Guid`).

### O que esta parte não faz

Nesta parte não há `.sln`, classe, tabela, Docker nem endpoint. O desenho acima é o contrato das partes 3 a 8.

### Ficou claro quando você consegue dizer

- Uma **Conta** é a raiz que protege saldo e lançamentos.
- Uma **transferência** são dois lançamentos que só valem se os dois forem gravados.
- O **SQL Server aparece na parte 5**, quando o Domain já existir em C#.

---

## Parte 2 — Esqueleto da solution (concluída)

Objetivo: criar a solution, os quatro projetos de `src/`, os dois de `tests/`, e as referências do diagrama. Os projetos compilam vazios.

Registro do que ficou no disco, com os comandos e o grafo de referências: [02-esqueleto-da-solution.md](02-esqueleto-da-solution.md) · [English](02-esqueleto-da-solution.en.md).

`dotnet build` concluiu sem erro e `dotnet test` passou nos 2 testes de exemplo do template. O Domain não referencia ninguém.

---

## Parte 3 — Domain

Objetivo: traduzir a parte 1 para C#, ainda sem banco.

Entram, nesta ordem:

1. Enums (`AccountStatus`, `CustomerStatus`, `EntryDirection`, `Currency`).
2. Value objects (`Money`, `AccountNumber`, `Document`).
3. Exceções de domínio (saldo insuficiente, conta inativa, contas iguais na transferência).
4. Entidades `Customer`, `Account`, `LedgerEntry`.
5. Métodos com regra: `Activate` / `Deactivate`, `Open`, `Credit`, `Debit`.

`Credit` e `Debit` atualizam o saldo e acrescentam o lançamento. Quem chama esses métodos não mexe em `Balance` diretamente.

Pronto quando: um teste de unidade (pode esperar a parte 11 se você preferir ver o teste junto, mas a regra já fica chamável) cobre saldo insuficiente e conta inativa. Na prática desta trilha, o teste desses dois casos entra na parte 11; na parte 3 o critério é você ler `Debit` e apontar onde a regra mora.

---

## Parte 4 — Application

Objetivo: casos de uso que a API vai chamar. Um caso de uso por operação, com entrada e saída explícitas.

Primeira leva, só o fluxo feliz desenhado:

- Cadastrar cliente
- Abrir conta
- Creditar
- Debitar
- Transferir
- Consultar extrato (paginado, com filtro por período e direção)

A Application define interfaces (`ICustomerRepository`, `IAccountRepository`, `IUnitOfWork`) e DTOs. Ela não referencia `DbContext`.

Pronto quando: cada caso de uso cabe numa frase ("dado um cliente ativo e um valor, a conta credita e pede para salvar").

---

## Parte 5 — Infrastructure e banco

Objetivo: persistir o Domain. Aqui sim nascem SQL Server, EF Core e migrations.

- Mapeamento das entidades para tabelas (configuração do EF, sem atributo de banco espalhado no Domain).
- `DbContext` e repositórios.
- `IUnitOfWork` abrindo uma transação de banco.
- Migration inicial.
- Connection string via configuração, nunca fixa no código.

O esquema sai das classes da parte 3. Ajustes de índice (número da conta único, documento único) nascem no mapeamento.

Pronto quando: a migration aplica num SQL Server local (ou container) e as tabelas refletem Cliente, Conta e Lançamento.

---

## Parte 6 — API, primeiro corte vertical

Objetivo: um fluxo HTTP completo, pequeno.

`POST /api/customers` e `POST /api/accounts` (ou o par que escolhermos na hora), com `Program.cs` ligando Infrastructure e Application.

Controller fino: recebe o pedido, chama o caso de uso, devolve o status HTTP. Regra de saldo continua no Domain.

Pronto quando: um cliente e uma conta são criados de verdade no SQL Server via HTTP, e você consegue ler o registro no banco.

---

## Parte 7 — Borda da API

Objetivo: a API se comportar como produto, não como demo de controller.

- Validação de entrada (FluentValidation) antes do caso de uso.
- Tratamento global de exceções: erro de domínio vira 422 ou 409 com corpo estável; erro inesperado vira 500 sem vazar stack em produção.
- Paginação e filtros no extrato.
- Log estruturado de pedido (sem logar documento completo nem segredo).
- Swagger / OpenAPI com os contratos visíveis.

Pronto quando: um débito inválido volta erro previsível, e o extrato aceita página, tamanho e filtro.

---

## Parte 8 — Consistência da transferência

Objetivo: `POST` de transferência grava débito e crédito na mesma transação de banco.

Se a conta de destino falhar, o débito da origem não permanece. Isso usa o `IUnitOfWork` da parte 5, não um "salvar, depois salvar de novo" sem transação.

Pronto quando: uma falha forçada no segundo lado desfaz o primeiro (teste de integração ou verificação no banco).

---

## Parte 9 — Segurança

Objetivo: JWT e autorização.

- Emissão de token para um usuário da API (usuário de acesso, distinto do `Customer` de negócio, para não misturar login com titular da conta).
- Endpoints de operação exigem token.
- Política simples de autorização (autenticado pode operar; um papel administrativo fica reservado para auditoria se couber).

Segredo do JWT vem de configuração / user secrets, nunca do repositório.

Pronto quando: sem token o endpoint recusa; com token válido, o fluxo da parte 6 continua funcionando.

---

## Parte 10 — Auditoria

Objetivo: registrar quem executou a operação, quando, e qual agregado foi afetado.

Campos de criação (`CreatedAt`, `CreatedBy`) preenchidos num único lugar (interceptor do EF ou serviço de auditoria), não copiados em cada controller.

Pronto quando: um crédito deixa rastro consultável de autor e instante.

---

## Parte 11 — Testes

Objetivo: rede de segurança para as regras que o portfólio afirma ter.

- Unidade no Domain: saldo insuficiente, conta inativa, transferência para a mesma conta, valor zero.
- Unidade na Application com repositórios falsos: o caso de uso orquestra e não calcula saldo ele mesmo.
- Integração: um fluxo sobe a API (ou o `DbContext`) e verifica a transferência atômica.

Pronto quando: `dotnet test` passa e os nomes dos testes descrevem a regra, não o método.

---

## Parte 12 — Entrega

Objetivo: um repositório que um recrutador abre e entende em poucos minutos.

- `Dockerfile` e `docker-compose` com API + SQL Server.
- README, nas duas línguas: o que é, como subir, como autenticar, exemplo de transferência, diagrama das dependências, decisões (por que `LedgerEntry`, por que saldo na conta).
- `.gitignore` de .NET, sem `bin/`, `obj/`, user secrets ou connection string real.
- Commits pequenos, um assunto por commit, ao longo da trilha (não um commit único no final).

Pronto quando: outra pessoa segue o README e conclui uma transferência.

---

## Decisões já fechadas

- Solution: `FinancialOperations`
- Target framework: `net9.0`
- Arquitetura: Clean Architecture, com Domain no centro
- Banco modelado depois do Domain, via EF Core migrations
- Moeda inicial: `BRL`
- Dinheiro: `decimal`
- Lançamento imutável; estorno futuro é lançamento novo
- Saldo vive na conta e muda só dentro de `Credit` e `Debit`

Decisões que ficam para a parte em que forem usadas: nomes exatos das rotas, formato do número da conta, provedor de log, e se o SQL Server local será instalação ou container. O container entra de qualquer forma na parte 12; na parte 5 usamos o que estiver mais simples na sua máquina.

---

## Onde estamos

Repositório: https://github.com/bielbarros/financial-operations-api

- [x] Parte 1 escrita neste arquivo
- [x] Parte 1 confirmada por você
- [x] Parte 2 — esqueleto da solution ([registro](02-esqueleto-da-solution.md))
- [ ] Partes 3 a 12

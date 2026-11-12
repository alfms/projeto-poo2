# Sistema de Locação de Veículos (POO2 · UVV)

Base do projeto em **C# / .NET 8**, com interface gráfica **WinForms**, arquitetura **MVC em camadas**,
**EF Core Code First** sobre **SQLite** e uma funcionalidade completa em **SQL puro** (a Locação).

Telas (seguem os wireframes do grupo):

1. **Login** do funcionário
2. **Dashboard** com os números do dia
3. **Veículos e Categorias** (duas abas)
4. **Locação**: aba *Nova locação* e aba *Consultar locações*

Clientes **não têm tela de cadastro**: já vêm cadastrados no banco (seed em `DadosIniciais.cs`).

---

## Estrutura (MVC em 3 projetos)

```
LocacaoVeiculos.sln
├── LocacaoVeiculos.Core   → MODEL + CONTROLLER (não conhece EF nem WinForms)
│   ├── Models/            Cliente, PessoaFisica, PessoaJuridica, Funcionario, Credencial,
│   │                      Categoria, Veiculo, Carro, Moto, ITarifa, Locacao, LocacaoVeiculo
│   ├── Repositories/      Interfaces (contratos) que os controllers usam
│   ├── Controllers/       Auth, Dashboard, Veiculo, Categoria, Locacao
│   └── Utils/             SenhaHasher
├── LocacaoVeiculos.Data   → PERSISTÊNCIA (implementa as interfaces do Core)
│   ├── AppDbContext.cs    Mapeamento Code First (herança, 1x1, 1xN, NxN)
│   ├── DadosIniciais.cs   Seed: funcionários, categorias, veículos e CLIENTES
│   ├── BancoDeDados.cs    Cria o contexto e aplica as migrations
│   └── Repositories/      Repositórios EF + LocacaoSqlRepository (SQL puro)
└── LocacaoVeiculos.App    → VIEW (WinForms)
    ├── Program.cs         Monta tudo (composition root) e abre o Login
    └── Views/             LoginForm, MainForm, DashboardView, VeiculosView, LocacaoView, Tema
```

Fluxo: **View** chama **Controller** → Controller valida regra de negócio e chama a **interface do repositório** →
a implementação em **Data** fala com o banco.

---

## Como rodar (Windows + Visual Studio 2022)

1. Abrir `LocacaoVeiculos.sln` e definir `LocacaoVeiculos.App` como projeto de inicialização.
2. Instalar a ferramenta do EF (uma vez só, no terminal):
   ```
   dotnet tool install --global dotnet-ef
   ```
3. Gerar a migration inicial (Code First). Na pasta da solução:
   ```
   dotnet ef migrations add Inicial --project LocacaoVeiculos.Data
   ```
   Isso cria a pasta `LocacaoVeiculos.Data/Migrations`. **Commitem essa pasta.**
4. Rodar (F5). Na primeira execução o banco `locacao.db` é criado sozinho com os dados de teste.

Logins de teste (seed):

| Login | Senha | Perfil |
|---|---|---|
| `admin` | `admin123` | Gerente |
| `atendente` | `atend123` | Atendente |

Mudou alguma classe do Model? Gere uma nova migration (`dotnet ef migrations add NomeDaMudanca --project LocacaoVeiculos.Data`)
e apague o `locacao.db` se quiser recomeçar do zero.

### No Mac

WinForms só **executa** no Windows. No Mac dá para:

- Programar e compilar tudo com o .NET 8 SDK + Rider ou VS Code (C# Dev Kit). O projeto App já tem `EnableWindowsTargeting`.
- Gerar migrations normalmente (o comando acima usa só o projeto Data).
- Para **rodar as telas**: usar uma VM com Windows (Parallels/UTM) ou rodar no computador de um colega com Windows.

As regras de negócio (Core) não dependem de WinForms, então dá para testar no Mac.

---

## Onde está cada requisito do enunciado

### Relacionamentos e herança

| Requisito | Onde |
|---|---|
| Herança | `Cliente` → `PessoaFisica` / `PessoaJuridica` e `Veiculo` → `Carro` / `Moto` (TPH no `AppDbContext`) |
| Interface + polimorfismo | `ITarifa.CalcularDiaria()`: Carro e Moto calculam a diária de jeitos diferentes |
| 1x1 | `Funcionario` ↔ `Credencial` (login e senha) |
| 1xN | Cliente → Locações, Funcionário → Locações, Categoria → Veículos |
| NxN | `Locacao` ↔ `Veiculo` via classe associativa `LocacaoVeiculo` (uma locação pode ter vários veículos) |
| Composição | `Locacao` ◆ `LocacaoVeiculo` (cascade) e `Funcionario` ◆ `Credencial` |
| Agregação | `Categoria` ◇ `Veiculo` (excluir categoria não apaga veículo: `DeleteBehavior.Restrict`) |

### Banco

- **Code First**: modelo em `AppDbContext.cs`, banco criado por migration (`BancoDeDados.AplicarMigrations`).
- **EF como ORM**: veículos, categorias, clientes e login (`Data/Repositories/EfRepositories.cs`).
- **SQL puro (INSERT, UPDATE e SELECT)**: toda a Locação em `Data/Repositories/LocacaoSqlRepository.cs`
  - INSERT da locação e dos itens, com transação
  - UPDATE de status do veículo (alugado/disponível) e de finalizar/cancelar
  - SELECT da consulta com JOIN + GROUP BY e dos números do Dashboard
  - Sempre com **parâmetros** (sem concatenar texto), evitando SQL Injection

### SOLID

| Princípio | Onde |
|---|---|
| **S** Responsabilidade única | Cada controller cuida de um assunto (`VeiculoController`, `LocacaoController`...). `Tema` só cuida de visual. `SenhaHasher` só de hash. |
| **O** Aberto/fechado | Novo tipo de veículo = nova subclasse de `Veiculo` com seu `CalcularDiaria()`, sem mexer no `LocacaoController`. |
| **L** Substituição de Liskov | `LocacaoController` trata qualquer `Veiculo` pelo tipo base; Carro e Moto funcionam no lugar dele. Idem `Cliente.PodeLocar`. |
| **I** Segregação de interfaces | `IClienteRepository` só tem leitura (clientes são pré-cadastrados). `ILocacaoRepository` não herda o CRUD genérico. |
| **D** Inversão de dependência | Controllers recebem **interfaces** no construtor. Só o `Program.cs` conhece as classes concretas. O Core nem referencia EF. |

### Clean Code (exemplos para citar)

- Nomes que dizem o que fazem: `PodeLocar`, `MarcarVeiculoComoAlugado`, `CalcularOrcamento`, `PlacaJaCadastrada`.
- Sem números mágicos: `AcrescimoCambioAutomatico`, `LimiteBaixaCilindrada`, `AnoMinimo`, `Tema.Espaco`.
- Métodos curtos com um objetivo (ex.: `LocacaoSqlRepository.Registrar` delega para `InserirLocacao`, `InserirItem`...).
- Sem duplicação: `Tema.cs` centraliza componentes visuais; `Repository<T>` centraliza o CRUD.
- Retornos antecipados (guard clauses) nas validações dos controllers.
- Erros tratados e mostrados ao usuário via `ResultadoOperacao`, sem exception estourando na tela.

---

## O que o grupo ainda precisa fazer

- [ ] Gerar a migration inicial e rodar no Windows (passo 3 acima)
- [ ] Testar todas as telas e ajustar layout se precisar
- [ ] Atualizar diagrama de casos de uso e de classes para este escopo (4 telas, 1x1 em Funcionario ↔ Credencial)
- [ ] Trocar os dados fictícios do seed se quiserem
- [ ] **Registrar o uso de IA** na documentação (ver abaixo)

## Uso de IA (obrigatório documentar)

Esta base foi gerada com auxílio de IA (Claude) a partir dos wireframes e da modelagem do grupo.
Pelo enunciado (nível IA-2), o grupo precisa:

- registrar a ferramenta, os prompts e o resultado;
- escrever no método: *"a IA auxiliou na geração inicial de código com revisão humana completa"*;
- revisar e entender cada arquivo, porque a nota é individual e o professor faz perguntas sobre o código;
- montar a seção comparando um trecho **com IA** e **sem IA** (sugestão: reescrever na mão um método do
  `LocacaoController` ou uma das telas e comparar).

# Consigned Credit

API para inclusão e processamento de propostas de crédito consignado para aposentados.

O projeto foi desenvolvido como desafio técnico, com foco em arquitetura, regras de domínio, processamento assíncrono, resiliência e consistência dos dados.

## Arquitetura

A solução foi organizada utilizando princípios de Clean Architecture, separando responsabilidades entre domínio, aplicação, infraestrutura e interfaces de entrada.

```text
ConsignedCredit.Domain
        ↑
ConsignedCredit.Application
        ↑
ConsignedCredit.Infrastructure
        ↑
 ┌──────┴──────┐
 API          Worker
```

### Projetos

- **ConsignedCredit.Domain**  
  Entidades, Value Objects, regras e transições de estado do domínio.

- **ConsignedCredit.Application**  
  Casos de uso e abstrações necessárias para execução das regras da aplicação.

- **ConsignedCredit.Infrastructure**  
  Persistência com Entity Framework Core, SQL Server, RabbitMQ, Outbox e implementações das integrações externas simuladas.

- **ConsignedCredit.Api**  
  Endpoints HTTP para criação e consulta das propostas.

- **ConsignedCredit.Worker**  
  Consumidor responsável pelo processamento assíncrono das propostas.

- **ConsignedCredit.UnitTests**  
  Testes das principais regras de domínio e fluxos da aplicação.

## Fluxo de criação da proposta

Ao receber uma nova proposta, são realizadas as principais validações de elegibilidade:

1. agente deve estar ativo;
2. proponente não pode possuir outra proposta em aberto;
3. CPF não pode constar na lista de fraude;
4. restrições de valor específicas do estado são verificadas;
5. dados obrigatórios do proponente são validados;
6. simulação deve respeitar as regras de crédito;
7. última parcela não pode ultrapassar os 80 anos do proponente.

Após as validações, a proposta e uma mensagem de Outbox são persistidas na mesma transação.

```text
POST /api/proposals
       │
       ▼
CreateProposalUseCase
       │
       ├── Validações
       │
       ├── Proposal
       │
       └── OutboxMessage
              │
              ▼
          SQL Server
```

## Simulação

Foram implementadas as seguintes regras:

- máximo de 60 parcelas;
- valor da parcela limitado a 30% da renda;
- taxa de juros de 12% ao ano;
- última parcela limitada à idade de 80 anos.

Como o desafio define a taxa anual, mas não especifica o sistema de amortização, foi adotado o **Sistema Price**, utilizando a taxa mensal efetiva equivalente aos 12% ao ano.

## Processamento assíncrono

Depois da criação da proposta, o **Outbox Processor** publica o evento no RabbitMQ.

O Worker consome o evento e executa sequencialmente:

```text
Simulation Validation
        ↓
Risk Analysis
        ↓
INSS Registration
        ↓
Contract Generation
        ↓
Digital Signature
        ↓
Payment
        ↓
Approved
```

As etapas de validação da simulação e análise de risco retornam um score entre 0 e 10.

Caso qualquer um desses scores seja inferior a **7**, a proposta é rejeitada.

As integrações externas foram abstraídas através de interfaces e possuem implementações simuladas na camada Infrastructure.

## Confiabilidade e resiliência

### Transactional Outbox

A criação da proposta e a mensagem que inicia seu processamento são persistidas na mesma transação.

Isso evita o cenário em que a proposta seja salva no banco, mas a aplicação falhe antes da publicação da mensagem no broker.

```text
Database Transaction
┌────────────────────────────┐
│ Proposal                   │
│ OutboxMessage              │
└────────────────────────────┘
              │
              ▼
       Outbox Processor
              │
              ▼
           RabbitMQ
```

A entrega trabalha com semântica **at-least-once**.

Uma mensagem pode eventualmente ser entregue novamente, mas o fluxo foi desenvolvido para permitir a retomada do processamento.

### Processamento retomável

O estado atual do processamento é persistido na própria proposta através de `ProcessingStep`.

Exemplo:

```text
Status: Processing
ProcessingStep: ContractGeneration
```

Se o Worker for reiniciado ou a mensagem for entregue novamente, etapas já concluídas não são executadas novamente.

O estado é persistido após cada etapa concluída.

### Retry e Dead Letter Queue

Falhas técnicas durante o processamento não rejeitam a proposta.

Nesses casos, a mensagem é encaminhada para uma fila de retry e processada novamente após um intervalo.

Após atingir o limite configurado de tentativas, a mensagem é enviada para uma **Dead Letter Queue (DLQ)** para inspeção ou reprocessamento posterior.

```text
Main Queue
    │
    ├── Success ──► ACK
    │
    └── Failure
          │
          ▼
      Retry Queue
          │
          ▼
      Main Queue
          │
          └── Max retries ──► DLQ
```

## Idempotência

O processamento persistido reduz a possibilidade de repetição das etapas já concluídas.

Existe, entretanto, uma janela entre a conclusão de uma integração externa e a persistência do novo estado.

Por exemplo:

```text
INSS processa com sucesso
        ↓
Aplicação falha antes de persistir o novo ProcessingStep
        ↓
Mensagem é processada novamente
        ↓
Chamada ao INSS pode ser repetida
```

Em um ambiente produtivo, as integrações externas também deveriam suportar idempotência, utilizando uma chave como:

```text
ProposalId + ProcessingStep
```

Essa decisão permitiria manter a semântica at-least-once sem duplicar efeitos externos.

## Restrições estaduais

As restrições de valor por estado foram modeladas em banco através de `StateLoanRestriction`.

Isso permite alterar limites sem necessidade de nova publicação da aplicação.

O desafio informa que alguns estados possuem restrições, mas não define os valores específicos. Portanto, eventuais valores cadastrados durante o desenvolvimento são apenas dados demonstrativos.

## Tecnologias

- .NET 9
- ASP.NET Core
- Entity Framework Core
- SQL Server
- RabbitMQ
- xUnit
- Moq
- Swagger / OpenAPI

## Executando o projeto

### Pré-requisitos

- .NET 9 SDK
- SQL Server
- RabbitMQ

Durante o desenvolvimento, SQL Server e RabbitMQ foram executados através de containers Docker.

Configure a connection string em:

```text
ConsignedCredit.Api/appsettings.json
ConsignedCredit.Worker/appsettings.json
```

Depois aplique as migrations:

```bash
dotnet ef database update \
  --project ConsignedCredit.Infrastructure \
  --startup-project ConsignedCredit.Api
```

Execute a API:

```bash
dotnet run --project ConsignedCredit.Api
```

E, em outro terminal, execute o Worker:

```bash
dotnet run --project ConsignedCredit.Worker
```

O Swagger estará disponível no endereço exibido pela API durante a inicialização.

## Endpoints

### Criar proposta

```http
POST /api/proposals
```

Exemplo:

```json
{
  "agentId": "4ec98254-b842-4cc4-b687-1a2fb5e716b9",
  "storeId": "8cfe61a5-85f8-4eb6-8bd3-5478de52651d",
  "cpf": "12345678901",
  "inssNumber": "123456789",
  "retirementIncome": 5000,
  "birthDate": "1960-01-01",
  "email": "customer@example.com",
  "phone": "54999999999",
  "street": "Rua Teste",
  "number": "123",
  "city": "Caxias do Sul",
  "state": "RS",
  "zipCode": "95000-000",
  "requestedAmount": 10000,
  "installments": 48
}
```

### Consultar proposta

```http
GET /api/proposals/{id}
```

A consulta permite acompanhar o status e a etapa atual do processamento da proposta.

## Testes

Para executar todos os testes:

```bash
dotnet test
```

Os testes cobrem principalmente:

- regras de criação da proposta;
- regras da simulação;
- restrições estaduais;
- rejeição por score;
- fluxo completo de processamento;
- retomada a partir de uma etapa intermediária;
- propostas já finalizadas.

## Decisões e trade-offs

O objetivo foi manter a solução simples, mas preparada para os principais problemas de consistência e confiabilidade do fluxo.

Algumas decisões foram intencionais:

- não foi utilizado MediatR ou um framework de CQRS, evitando abstrações sem necessidade para o tamanho atual da aplicação;
- o workflow permanece explícito no `ProcessProposalUseCase`, pois existem poucas etapas fixas e sua leitura sequencial facilita o entendimento do processo;
- caso o número de etapas ou variações do workflow aumentasse, cada etapa poderia ser extraída para uma estratégia/handler independente;
- a homologação da loja foi considerada uma pré-condição do contexto de originação da proposta; o `StoreId` identifica sua origem;
- autenticação e autorização não foram implementadas por não fazerem parte dos requisitos apresentados;
- as integrações externas foram simuladas e isoladas através de abstrações, permitindo substituição por integrações reais;
- não foram criados CRUDs para entidades auxiliares que não são necessários para demonstrar o fluxo principal.

## Possíveis evoluções

Em um cenário produtivo, alguns pontos poderiam ser evoluídos:

- idempotência nas integrações externas;
- controle de concorrência do processamento do Outbox em múltiplas instâncias;
- observabilidade e métricas distribuídas;
- autenticação e autorização conforme os requisitos de acesso;
- integrações reais com os serviços externos;
- estratégia automatizada de reprocessamento da DLQ.

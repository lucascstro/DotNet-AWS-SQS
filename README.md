# DotNet AWS SQS — Publisher/Consumer

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![AWS SQS](https://img.shields.io/badge/AWS-SQS-FF9900?logo=amazonsqs&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-blue)

Projeto de estudo que implementa um fluxo de mensageria assíncrona na AWS usando **Amazon SQS (FIFO)**, com dois serviços .NET independentes:

- **`publisher`** — Web API (ASP.NET Core) responsável por publicar mensagens na fila.
- **`consumer`** — Worker Service (`BackgroundService`) que faz *long polling* na fila, processa e remove cada mensagem.

O objetivo é demonstrar, de forma enxuta, o desacoplamento produtor/consumidor tipicamente usado em arquiteturas orientadas a eventos.

## Arquitetura

```
┌─────────────┐        SendMessage        ┌───────────────┐        ReceiveMessage        ┌─────────────┐
│  publisher  │   ────────────────────▶   │  Amazon SQS   │   ◀────────────────────────  │  consumer   │
│ (Web API)   │                           │  (FIFO queue) │                              │ (Worker)    │
└─────────────┘                           └───────────────┘        DeleteMessage         └─────────────┘
```

1. Um `POST` na API `publisher` envia o corpo da requisição como mensagem para a fila FIFO.
2. O `consumer` roda em segundo plano fazendo long polling (`WaitTimeSeconds: 20`), processa cada mensagem recebida e a remove da fila após o processamento (`DeleteMessage`).

## Tecnologias

- **.NET 10**
- **AWSSDK.SQS** / **AWSSDK.Extensions.NETCore.Setup** — integração com AWS SQS
- **ASP.NET Core Web API** — camada de publicação
- **.NET Generic Host + `BackgroundService`** — worker de consumo
- **Scalar.AspNetCore** — documentação/UI interativa da API (alternativa ao Swagger)

## Estrutura do projeto

```
src/
├── publisher/                  # API que publica mensagens na fila SQS
│   ├── Controllers/
│   │   └── PublishController.cs
│   ├── Services/
│   │   └── SqsService.cs
│   └── appsettings.json
└── consumer/                    # Worker que consome e processa mensagens
    ├── Services/
    │   └── ConsumerWorker.cs
    ├── Program.cs
    └── appsettings.json
```

## Como executar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Uma fila **SQS do tipo FIFO** criada na AWS (o nome deve terminar em `.fifo`)
- Credenciais AWS configuradas localmente.

### 1. Configurar a URL da fila

Defina a URL da fila em `src/publisher/appsettings.json` e `src/consumer/appsettings.json`:

```json
{
  "AWS": {
    "Region": "us-east-1",
    "UrlQueue": "https://sqs.us-east-1.amazonaws.com/<account-id>/<queue-name>.fifo"
  }
}
```

> Dica: use `appsettings.Development.json` ou variáveis de ambiente para não versionar a URL real da fila.

### 2. Rodar a API publisher

```bash
cd src/publisher
dotnet run
```

Com o ambiente de desenvolvimento ativo, a documentação interativa (Scalar) fica disponível na raiz da API.

Publicando uma mensagem:

```bash
curl -X POST http://localhost:5219/api/Publish \
  -H "Content-Type: application/json" \
  -d "\"Minha primeira mensagem\""
```

### 3. Rodar o consumer

Em outro terminal:

```bash
cd src/consumer
dotnet run
```

O worker inicia o long polling e passa a logar no console as mensagens recebidas, processadas e removidas da fila.

## Pontos interessantes

- **Long polling** (`WaitTimeSeconds: 20`) para reduzir requisições vazias e custo de chamadas à AWS.
- **FIFO + deduplicação**: cada mensagem publicada recebe um `MessageGroupId` e um `MessageDeduplicationId` (`Guid`) únicos, garantindo ordem e evitando duplicidade.
- **Visibility timeout** (`40s`) configurado para dar margem de processamento antes que a mensagem volte a ficar visível para outros consumidores.
- **Remoção explícita** da mensagem (`DeleteMessage`) apenas após o processamento ser concluído com sucesso.

## Próximas tarefas

- Adicionar Dead Letter Queue (DLQ) para mensagens com falha recorrente.
- Publicar as imagens em containers (Docker) para facilitar o deploy.
- Testes de integração com [LocalStack](https://www.localstack.cloud/) para simular o SQS localmente.
- Mover a `UrlQueue` para *AWS Secrets Manager* / *Systems Manager Parameter Store*.

## Autor

**Lucas Castro** — [github.com/lucascstro](https://github.com/lucascstro)

## Licença

Este projeto está sob a licença [MIT](LICENSE).
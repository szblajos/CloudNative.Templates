# MicroserviceTemplate.Aspire

A modern, production-ready .NET microservice template built with Clean Architecture principles, advanced messaging, orchestration, and extensibility in mind.

---

## ✨ Features

- **Clean Architecture**: Clear separation of concerns between Application, Domain, Infrastructure, and API layers for maintainability and testability.
- **Outbox Pattern**: Reliable message delivery and eventual consistency using the Outbox pattern for integration events.
- **Aspire Orchestration**: Ready for .NET Aspire orchestration and cloud-native deployments.
- **Flexible Response Cache**: Easily switch between Redis and Azure Cache for Redis for response caching.
- **Pluggable Message Broker**: Choose between RabbitMQ and Azure Service Bus for message-based communication.
- **PostgreSQL**: Uses PostgreSQL as the default relational database with Entity Framework Core.
- **Mediator Pattern**: CQRS and request/response handling via [martinothamar/Mediator](https://github.com/martinothamar/Mediator).
- **FluentValidation**: Robust input validation for commands and queries.
- **Mapperly**: Fast, compile-time mapping between DTOs and domain models.

---

## 🏗️ Project Structure

``` txt
MicroserviceTemplate.Aspire.sln
src/
  MyService.Api/            # Minimal API, endpoints, DI setup
  MyService.Application/    # CQRS, Mediator handlers, validation, behaviors
  MyService.Domain/         # Entities, domain events, interfaces
  MyService.Infrastructure/ # EF Core, cache, messaging, outbox, DI extensions
  MyService.Orchestration/  # Aspire orchestration project

tests/
  MyService.Tests/          # Unit and integration tests
```

---

## 🚀 Getting Started

1. **Clone the template parent repository**

    ```sh
    git clone https://github.com/szblajos/CloudNative.Templates.git
    cd templates/MicroserviceTemplate.Aspire
    ```

2. **Restore dependencies**

    ```sh
    dotnet restore
    ```

3. **Build the solution**

    ```sh
    dotnet build
    ```

4. **Run the Aspire Orchestration**

    ```sh
    dotnet run --project src/MyService.Orchestration
    ```

---

## ⚙️ Configuration

- **Cache**: Configure `Cache:PreferredCache` in `appsettings.json` to `Redis` or `AzureCacheForRedis`.
- **Message Broker**: Set `MessageBroker:PreferredTransport` to `RabbitMQ` or `AzureServiceBus`.
- **Database**: Set your PostgreSQL connection string in `appsettings.json`.

---

## 🧩 Extensibility

- Add new features by following Clean Architecture boundaries.
- Swap infrastructure (cache, broker, db) by changing configuration only.
- Add new endpoints using Mediator and FluentValidation for consistency.

---

## 🧪 Testing

- Unit and integration tests included (with Testcontainers for Redis, RabbitMQ, PostgreSQL).
- Run all tests:

  ```sh
  dotnet test
  ```

---

## 📦 Main Dependencies

- [martinothamar/Mediator](https://github.com/martinothamar/Mediator)
- [FluentValidation](https://fluentvalidation.net/)
- [Mapperly](https://mapperly.riok.app/)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [RabbitMQ.Client](https://www.rabbitmq.com/dotnet-api-guide.html)
- [Azure.Messaging.ServiceBus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.npgsql.org/efcore/index.html)
- [Testcontainers](https://dotnet.testcontainers.org/)

---

## 🤝 Contributing

Contributions are welcome! Please open issues or pull requests for improvements, bugfixes, or new features.

---

## 📄 License

This project is licensed under the [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0.html)

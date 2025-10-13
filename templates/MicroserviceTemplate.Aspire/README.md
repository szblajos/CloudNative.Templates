# Project details

This project is created by using [MicroserviceTemplate.Aspire](#microservicetemplateaspire) template from the package [Szblajos.CloudNative.Templates](https://www.nuget.org/packages/Szblajos.CloudNative.Templates) hosted on NuGet.org.

**Template created by:** [Lajos Szabó](https://github.com/szblajos)  
**Source repository:** [https://github.com/szblajos/CloudNative.Templates](https://github.com/szblajos/CloudNative.Templates)  
**License:** [Apache 2.0 License](https://github.com/szblajos/CloudNative.Templates/blob/main/LICENSE)  
**Please see:** [NOTICE.txt](https://github.com/szblajos/CloudNative.Templates/blob/main/NOTICE.txt) for detailed attribution and third-party notices  

---

## MicroserviceTemplate.Aspire

A modern, production-ready .NET microservice template built with Clean Architecture principles, advanced messaging, orchestration, and extensibility in mind.

The template project was created by [Lajos Szabó](https://github.com/szblajos) to help others build robust, scalable, and maintainable microservices with .NET. Its goal is to accelerate development by providing a solid foundation that follows best practices, enabling teams to focus on delivering business value rather than boilerplate setup.

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
*.sln
src/
  *.Api/            # Minimal API, endpoints, DI setup
  *.Application/    # CQRS, Mediator handlers, validation, behaviors
  *.Domain/         # Entities, domain events, interfaces
  *.Infrastructure/ # EF Core, cache, messaging, outbox, DI extensions
  *.Orchestration/  # Aspire orchestration project

tests/
  *.Tests/          # Unit, integration and Arcitecture tests
```

---

## 🛠️ Prerequisites

Before running this project, ensure you have the following installed:

- **[Docker Desktop](https://www.docker.com/products/docker-desktop/):** Required for running infrastructure dependencies (e.g., PostgreSQL, Redis, RabbitMQ) in containers.
- **.NET Aspire Workload:** Install the Aspire workload for orchestrating and running the solution locally.

  ```sh
  dotnet workload install aspire
  ```

Make sure Docker Desktop is running and the Aspire workload is installed before starting the project.

---

## 🚀 Getting Started

To restore dependencies, run the project, and execute tests, follow these steps:

### 1. Restore Dependencies

Restore all NuGet packages for the solution:

```sh
dotnet restore
```

### 2. Run the Solution

Start the Aspire orchestration project to launch all services and dependencies:

```sh
dotnet run --project src/YourProject.Orchestration
```

Replace `YourProject.Orchestration` with the actual orchestration project folder name if different.

### 3. Run Tests

Execute all unit and integration tests:

```sh
dotnet test
```

---

After these steps, your microservice and its dependencies should be running locally, and you can verify functionality by running the test suite.

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

We welcome contributions from the community! If you'd like to improve this template or add new features to the template:

- 🐛 **Report bugs** or request features by [opening an issue](https://github.com/szblajos/CloudNative.Templates/issues)
- 🔧 **Submit improvements** via [pull requests](https://github.com/szblajos/CloudNative.Templates/pulls)
- 💡 **Share ideas** and feedback in the [repository discussions](https://github.com/szblajos/CloudNative.Templates/discussions)

Your contributions help make this template better for everyone!

---

## 📄 License & Attribution

This template is licensed under the **Apache License 2.0**.

**Template Author:** [Lajos Szabó](https://github.com/szblajos)  
**Source Repository:** [https://github.com/szblajos/CloudNative.Templates](https://github.com/szblajos/CloudNative.Templates)  

⚠️ **Important:** Please review the [NOTICE.txt](NOTICE.txt) file for detailed attribution information and third-party component licenses used in this project.

For the full license text, see [LICENSE](https://github.com/szblajos/CloudNative.Templates/blob/main/LICENSE) or visit [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0.html).

---

## ☕ Support

If you find the [MicroserviceTemplate.Aspire](https://www.nuget.org/packages/Szblajos.CloudNative.Templates) useful and would like to support my future open-source work, your contribution is much appreciated!

- [Buy Me a Coffee](https://buymeacoffee.com/szblajos)
- [PayPal Donation](https://www.paypal.com/donate/?hosted_button_id=X6GCG6ZTK84GQ)
  - €3 – Coffee-level appreciation ☕
  - €10 – Time-saver recognition 🔧
  - €25 – Production-grade support 🚀

Thank you for your support!

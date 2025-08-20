# CloudNative.Templates

>A collection of templates for building cloud native .NET applications.

---

## ⚠️ Project Status

**This template collection is under active development.**

- The included microservice template (powered by .NET Aspire) is not finished yet.
- More templates and features will be added in the future.
- Breaking changes may occur until the first stable release.

---

## 📦 Installation

The templates are published on [NuGet.org](https://www.nuget.org/packages?q=szblajos) under the user account: **szblajos**.

Install the template package globally using the following command:

```sh
dotnet new install Szblajos.CloudNative.Templates
```

---

## 🗑️ Uninstallation

To remove the installed template package from your machine, run:

```sh
dotnet new uninstall Szblajos.CloudNative.Templates
```

This will uninstall all templates provided by this package.

---

## 🚀 Usage

After installation, you can create a new solution using the included microservice template. For example:

```sh
dotnet new clamsaspire -n StockService
```

This will scaffold a new solution named `StockService` using the Aspire-powered microservice template.

---

## 📚 Templates

- **clamsaspire**: Modern microservice template with Clean Architecture, Aspire orchestration, and cloud-native best practices.
  
  **Features:**

  - Clean separation of concerns (Application, Domain, Infrastructure, API)
  - Outbox pattern for reliable messaging
  - Pluggable message broker (RabbitMQ or Azure Service Bus)
  - Flexible response cache (Redis or Azure Cache for Redis)
  - PostgreSQL with Entity Framework Core
  - CQRS and Mediator pattern
  - FluentValidation and Mapperly integration
  - Ready for Aspire orchestration and cloud-native deployments

More templates will be added in the future.

---

## 📝 Notes

- Please report issues or suggestions via GitHub.
- Contributions are welcome!

---

## 📄 License

This project is licensed under the [Apache License 2.0](LICENSE).

See the [NOTICE.txt](NOTICE.txt) file for additional information.

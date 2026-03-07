# Shortix

> High-throughput URL shortener built for internal use — 1,000 redirects/second, Base62 encoding, fully isolated on Azure.

---

If you're new to this project, start with the **[Architecture doc](/docs/architecture/overview.md)**.

It covers how the system handles 1k req/s on the redirect path, why there are two CosmosDB containers doing different jobs, and how URL Shortener instances generate short codes without ever talking to a database for ID generation.

---

## Docs

| Document | What it covers |
|---|---|
| [Architecture](/docs/architecture/overview.md) | System design, services, data flows and security model |
| [Bootstrap](/docs/getting-started.md) | How to provision the Azure environment and configure GitHub Actions from scratch |
| [IaC Reference](/docs/infra/main.md) | Module-by-module reference for all Bicep templates |

### IaC Reference

| Module | |
|---|---|
| [main.bicep](/docs/infra/main.md) | Entry point — full resource topology and dependency graph |
| [compute/appservice](/docs/infra/modules/compute/compute-appservice.md) | App Service with VNet integration, Key Vault and App Insights |
| [compute/function](/docs/infra/modules/compute/compute-function.md) | Azure Function App (dotnet-isolated) |
| [networking/virtual-networking](/docs/infra/modules/networking/networking-virtual-networking.md) | VNet and subnet layout |
| [networking/front-door](/docs/infra/modules/networking/networking-front-door.md) | Front Door profile, WAF and custom domain |
| [networking/front-door-routes](/docs/infra/modules/networking/networking-front-door-routes.md) | Origin groups and routing rules |
| [identity/entra-app](/docs/infra/modules/identity/identity-entra-app.md) | Entra App Registration and OAuth2 scopes |
| [secrets/keyvault](/docs/infra/modules/secrets/secrets-keyvault.md) | Key Vault and RBAC role assignments |
| [storage](/docs/infra/modules/storage/storage.md) | CosmosDB, PostgreSQL, Redis and Storage Account |
| [telemetry & web](/docs/infra/modules/telemetry/telemetry-and-web.md) | App Insights, Log Analytics and Static Web App |
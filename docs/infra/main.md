# main.bicep

Infrastructure entry point. Orchestrates all modules and defines the complete topology of the URL Shortener environment on Azure.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `pgSqlPassword` | `string` (secure) | ✅ | PostgreSQL administrator password |
| `env` | `string` | ✅ | Environment identifier (e.g. `dev`, `prd`) |
| `customDomainName` | `string` | ✅ | Custom domain name associated with Azure Front Door |
| `location` | `string` | ❌ | Azure region (default: Resource Group region) |

## Variables

All resource names are generated from `uniqueString(subscriptionId, resourceGroupName)` combined with the `env` parameter, ensuring uniqueness across environments and avoiding global name collisions.

```
uniqueId = uniqueString(subscription().subscriptionId, resourceGroup().name)
```

## Provisioned Resources

### Networking
- **Virtual Network** with 6 dedicated subnets (one per service)
- **Azure Front Door** with endpoint, custom domain and WAF
- **Front Door Routes** for traffic routing across the three origins

### Identity
- **Entra App Registration** for SPA authentication (environment-conditional redirect URIs)

### Secrets
- **Key Vault** with VNet-restricted access
- **Key Vault Role Assignments** (RBAC) for all compute services

### Telemetry
- **Log Analytics Workspace** (centralized)

### Compute
- **URL Shortener API** — App Service restricted to Front Door traffic only
- **Token Range API** — App Service restricted to URL Shortener subnet traffic only
- **Redirect API** — App Service restricted to Front Door traffic only
- **Cosmos Trigger Function** — Azure Function (dotnet-isolated) triggered by CosmosDB change feed

### Web
- **Static Web App** — frontend SPA

### Storage
- **Storage Account** — used by the Azure Functions runtime
- **CosmosDB** — primary database with `items` and `byUser` containers
- **PostgreSQL Flexible Server** — token range management
- **Redis Cache** — distributed cache

## Routing Architecture (Front Door)

```
/api/r/*  →  Redirect API
/api/*    →  URL Shortener API
/*        →  Static Web App (SPA)
```

## Dependency Diagram

```
vnet ──────────────────────────────────────────────────────┐
keyVault ──────────────────────────────────────────────────┤
logAnalyticsWorkspace ─────────────────────────────────────┤
staticWebApp ──────────────────────────────────────────────┤
entraApp ──────────────────────────────────────────────────┤
                                                           │
tokenRangeApi ────────────── (depends on: vnet, kv) ───────┤
urlShortenerApi ─────────── (depends on: vnet, kv) ────────┤
redirectApi ─────────────── (depends on: vnet, kv) ────────┤
cosmosDb ────────────────── (depends on: vnet, kv) ────────┤
storageAccount ──────────── (depends on: kv) ──────────────┤
postgres ────────────────── (depends on: vnet, kv) ────────┤
redisCache ──────────────── (depends on: kv) ──────────────┤
                                                           │
cosmosTriggerFunction ────── (depends on: kv, vnet, cosmos)┤
keyVaultRoleAssignment ───── (depends on: kv + all compute)┤
frontDoor ──────────────────────────────────────────────────┤
frontDoorRoutes ─────────── (depends on: frontDoor + apis) ┘
```

## Notes

- The `env` parameter controls conditional behaviors, such as Entra App redirect URIs (`dev` includes `http://localhost:3000`).
- The `uniqueId` is stable for the same subscription + resource group pair, ensuring idempotent deployments.

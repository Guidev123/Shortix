# modules/compute/function.bicep

Module for provisioning an **Azure Function App** (Linux, dotnet-isolated) with VNet Integration, Key Vault, Application Insights and a Storage Account for the runtime.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | ✅ | Function App name |
| `appServicePlanName` | `string` | ✅ | App Service Plan name |
| `location` | `string` | ✅ | Azure region |
| `keyVaultName` | `string` | ✅ | Key Vault name to inject as an app setting |
| `logAnalyticsWorkspaceId` | `string` | ✅ | Workspace ID for Application Insights |
| `storageAccountConnectionString` | `string` (secure) | ✅ | Storage Account connection string for the runtime |
| `subnetId` | `string` | ✅ | Subnet resource ID for VNet Integration |
| `appSettings` | `array` | ❌ | Additional app settings (default: `[]`) |

## Provisioned Resources

### App Service Plan
- **SKU:** `B1` (Basic, Linux)
- **OS:** Linux (`reserved: true`)

### Function App
- **Kind:** `functionapp,linux`
- **Runtime:** `DOTNET-ISOLATED|8.0`
- **HTTPS Only:** enabled
- **Always On:** enabled
- **FTPS State:** `FtpsOnly`
- **Min TLS Version:** `1.2`
- **Health Check:** `/api/healthz`
- **Managed Identity:** System Assigned
- **Default IP restriction action:** Deny

### VNet Integration
Configured via a separate `Microsoft.Web/sites/networkConfig` resource:

```bicep
resource functionVirtualNetwork 'Microsoft.Web/sites/networkConfig@2025-03-01' = {
  name: 'virtualNetwork'
  properties: {
    subnetResourceId: subnetId
  }
}
```

> **Note:** Unlike `appservice.bicep` which uses `virtualNetworkSubnetId` directly on the site properties, this module uses a separate `networkConfig` resource for VNet integration.

### Automatically Injected App Settings

| Setting | Value |
|---|---|
| `KeyVaultName` | Key Vault name |
| `AzureWebJobsStorage` | Storage Account connection string |
| `WEBSITE_CONTENTAZUREFILECONNECTIONSTRING` | Storage Account connection string |
| `WEBSITE_CONTENTSHARE` | Content share name (lowercase of the function name) |
| `FUNCTIONS_WORKER_RUNTIME` | `dotnet-isolated` |
| `FUNCTIONS_EXTENSION_VERSION` | `~4` |
| `WEBSITE_RUN_FROM_PACKAGE` | `1` |
| `APPINSIGHTS_INSTRUMENTATIONKEY` | App Insights Instrumentation Key |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | App Insights connection string |

Additional `appSettings` are appended after the defaults via `concat()`.

### Application Insights
Instantiated via the `../telemetry/app-insights.bicep` module, named `appinsights-{name}`.

### SCM Type
Set to `GitHub` via `Microsoft.Web/sites/config` resource.

### SCM Access Restrictions
Same as `appservice.bicep`: SCM traffic allowed only from the `AzureCloud` service tag.

## Outputs

| Output | Type | Description |
|---|---|---|
| `id` | `string` | Function App resource ID |
| `principalId` | `string` | Managed Identity object ID (used for Key Vault role assignments) |

## Usage in main.bicep

Instantiated once as `cosmosTriggerFunction`, responsible for propagating data from the `items` container to the `byUser` container via the Cosmos DB Change Feed.

Specific app settings injected:

| Setting | Value |
|---|---|
| `CosmosDbConnection` | Key Vault reference (`@Microsoft.KeyVault(SecretUri=...)`) |
| `TargetDatabaseName` | `urls` |
| `TargetContainerName` | `byUser` |

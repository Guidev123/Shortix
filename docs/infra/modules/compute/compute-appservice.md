# modules/compute/appservice.bicep

Reusable module for provisioning an **Azure App Service** (Linux, .NET) with VNet integration, Key Vault, Application Insights and configurable IP restrictions.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `appName` | `string` | ✅ | App Service name |
| `appServicePlanName` | `string` | ✅ | App Service Plan name |
| `location` | `string` | ✅ | Azure region |
| `keyVaultName` | `string` | ✅ | Key Vault name to inject as an app setting |
| `logAnalyticsWorkspaceId` | `string` | ✅ | Workspace ID for Application Insights |
| `vnetId` | `string` | ✅ | Subnet resource ID for VNet Integration |
| `ipSecurityRestrictions` | `array` | ❌ | IP restriction rules (default: `[]`) |
| `appSettings` | `array` | ❌ | Additional app settings (default: `[]`) |

## Provisioned Resources

### App Service Plan
- **SKU:** `B1` (Basic, Linux)
- **OS:** Linux (`reserved: true`)

### App Service (Web App)
- **Runtime:** `DOTNETCORE|10.0`
- **HTTPS Only:** enabled
- **VNet Integration:** via `virtualNetworkSubnetId`
- **Health Check:** `/healthz`
- **Managed Identity:** System Assigned
- **Public Network Access:** Enabled (access controlled via `ipSecurityRestrictions`)
- **Default IP restriction action:** Deny

### SCM Access Restrictions
The SCM endpoint (Kudu / deploy) allows traffic only from the `AzureCloud` service tag, enabling GitHub Actions deployments.

```bicep
scmIpSecurityRestrictions: [
  {
    name: 'AllowGHDeploy'
    action: 'Allow'
    priority: 100
    tag: 'ServiceTag'
    ipAddress: 'AzureCloud'
  }
]
scmIpSecurityRestrictionsDefaultAction: 'Deny'
```

### Automatically Injected App Settings

The following settings are always included, regardless of the `appSettings` parameter:

| Setting | Value |
|---|---|
| `KeyVaultName` | Key Vault name |
| `APPINSIGHTS_INSTRUMENTATIONKEY` | App Insights Instrumentation Key |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | App Insights connection string |

Additional `appSettings` are appended after the defaults via `concat()`.

### Application Insights
Instantiated via the `../telemetry/app-insights.bicep` module, named `appinsights-{appName}`, linked to the provided Log Analytics Workspace.

### SCM Type
Set to `GitHub` via `Microsoft.Web/sites/config` resource.

## Outputs

| Output | Type | Description |
|---|---|---|
| `appServiceId` | `string` | App Service resource ID |
| `principalId` | `string` | Managed Identity object ID (used for Key Vault role assignments) |
| `url` | `string` | App Service public URL (`https://...`) |
| `hostname` | `string` | Hostname without protocol (used in Front Door routes) |

## Usage in main.bicep

This module is instantiated three times with distinct `ipSecurityRestrictions` configurations:

- **URL Shortener API** — allows only `AzureFrontDoor.Backend`
- **Redirect API** — allows only `AzureFrontDoor.Backend`
- **Token Range API** — allows only the URL Shortener subnet (internal traffic)

# modules/telemetry/log-analytics.bicep

Module for provisioning a **Log Analytics Workspace**, used as a centralized backend for all Application Insights resources in the infrastructure.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | ✅ | Workspace name |
| `location` | `string` | ✅ | Azure region |

## Provisioned Resources

### Log Analytics Workspace
- **SKU:** `PerGB2018` — pay-per-ingested-data billing model

## Outputs

| Output | Type | Description |
|---|---|---|
| `id` | `string` | Workspace resource ID — passed to all Application Insights modules |

---

# modules/telemetry/app-insights.bicep

Module for provisioning a workspace-based **Application Insights** instance, instantiated individually per compute service.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | ✅ | Application Insights resource name |
| `location` | `string` | ✅ | Azure region |
| `logAnalyticsWorkspaceId` | `string` | ✅ | Target Log Analytics Workspace resource ID |

## Provisioned Resources

### Application Insights
- **Kind:** `web`
- **Application Type:** `web`
- **Flow Type:** `rest`
- **Workspace:** linked to the provided Log Analytics workspace (workspace-based)
- **Public Network Access:** Enabled for both ingestion and query

## Outputs

| Output | Type | Description |
|---|---|---|
| `id` | `string` | Application Insights resource ID |
| `instrumentationKey` | `string` | Instrumentation Key (injected as `APPINSIGHTS_INSTRUMENTATIONKEY`) |
| `connectionString` | `string` | Full connection string (injected as `APPLICATIONINSIGHTS_CONNECTION_STRING`) |

## Usage Pattern

This module is not called directly by `main.bicep`. It is instantiated internally by `compute/appservice.bicep` and `compute/function.bicep`, creating a dedicated App Insights instance per service.

---

# modules/web/static-web-app.bicep

Module for provisioning an **Azure Static Web App** to host the frontend SPA (Single Page Application).

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | ✅ | Static Web App name |

## Provisioned Resources

### Static Web App
- **SKU:** Standard
- **Region:** `eastus2` (hardcoded)

> **Note:** The region is fixed to `eastus2` and does not receive the `location` parameter from `main.bicep`. This is intentional given the limited regional availability of Static Web Apps, but can be parameterized if needed.

## Outputs

| Output | Type | Description |
|---|---|---|
| `id` | `string` | Static Web App resource ID |
| `url` | `string` | Public URL with protocol (`https://...`) — used as a redirect URI in the Entra App |
| `hostname` | `string` | Hostname without protocol — used in Front Door routes |

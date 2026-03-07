# modules/identity/entra-app.bicep

Module for provisioning a **Microsoft Entra App Registration** via the Bicep Microsoft Graph extension. Configures the application for SPA authentication with OAuth2 and custom scope exposure.

## Prerequisite

Requires the `microsoftGraph` extension enabled in `bicepconfig.json`:

```json
{
  "extensions": {
    "microsoftGraph": "br:mcr.microsoft.com/bicep/extensions/microsoftgraph/v1.0:1.0.0"
  },
  "experimentalFeatures": {
    "enabled": ["extensibility"]
  }
}
```

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `applicationName` | `string` | ✅ | App Registration display name and `uniqueName` |
| `signInAudience` | `string` | ❌ | Sign-in audience (default: `AzureADandPersonalMicrosoftAccount`) |
| `spaRedirectUris` | `array` | ❌ | List of redirect URIs for the SPA flow (default: `[]`) |

### Allowed values for `signInAudience`

- `AzureADMyOrg`
- `AzureADMultipleOrgs`
- `AzureADandPersonalMicrosoftAccount`

## Provisioned Resources

### Application (initial creation)
Creates the App Registration with `displayName` and `uniqueName`. Required as a separate step to obtain the `appId` before configuring properties that depend on it (such as `identifierUris`).

### Application (update with settings)
A second resource of the same type to apply configurations that depend on the `appId` generated in the previous step:

**OAuth2 Permission Scopes (exposed API):**

| Scope | ID | Type | Description |
|---|---|---|---|
| `Urls.Read` | `9d0c290c-3ddf-40b9-a153-59fbff143ac3` | User | Read access to URLs |

**Identifier URI:**
```
api://{appId}
```

**SPA Redirect URIs:** Received via parameter. In `main.bicep`, conditionally include `http://localhost:3000` only in the `dev` environment.

**Implicit Grant Settings:**
- `enableAccessTokenIssuance: true`
- `enableIdTokenIssuance: true`

## Outputs

| Output | Type | Description |
|---|---|---|
| `applicationId` | `string` | App Registration Client ID (appId) — used as `AzureAd__ClientId` in App Services |

## Usage in main.bicep

```bicep
// Environment-conditional redirect URIs
spaRedirectUris: env == 'dev'
  ? ['http://localhost:3000', staticWebApp.outputs.url, ...]
  : [staticWebApp.outputs.url, ...]
```

# modules/networking/front-door.bicep

Module for provisioning **Azure Front Door** (Standard tier) with an endpoint, custom domain with managed TLS and a Web Application Firewall (WAF).

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `profileName` | `string` | ✅ | Front Door profile name |
| `endpointName` | `string` | ✅ | AFD Endpoint name |
| `wafPolicyName` | `string` | ✅ | WAF Policy name |
| `customDomainHostName` | `string` | ✅ | Custom domain hostname (e.g. `short.example.com`) |

## Provisioned Resources

### Front Door Profile
- **SKU:** `Standard_AzureFrontDoor`
- **Scope:** Global

### AFD Endpoint
- **State:** Enabled

### Custom Domain
- **TLS:** Azure-managed certificate (`ManagedCertificate`)
- **Minimum TLS:** `TLS12`
- The resource name replaces `.` with `-` in the hostname to comply with the API naming convention

### WAF Policy
- **SKU:** `Standard_AzureFrontDoor`
- **Mode:** Detection/Block via custom rules

#### Rate Limiting Rule

```
Name: RateLimitRule
Type: RateLimitRule
Action: Block
Threshold: 1000 requests / minute
Condition: RemoteAddr NOT IN 10.10.10.0/24
```

Blocks IPs exceeding 1000 requests/minute, with the exception of the `10.10.10.0/24` range.

### Security Policy
Associates the WAF Policy with both the endpoint and the custom domain, covering the `/*` pattern.

## Outputs

| Output | Type | Description |
|---|---|---|
| `endpointHostName` | `string` | AFD Endpoint generated hostname (used in routes) |
| `customDomainId` | `string` | Custom domain resource ID (used for route association) |

# modules/networking/front-door-routes.bicep

Module for configuring **Origin Groups, Origins and Routes** in Azure Front Door. Defines traffic routing across the three backend services.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `profileName` | `string` | ✅ | Existing Front Door profile name |
| `endpointName` | `string` | ✅ | Existing AFD Endpoint name |
| `originRedirectGroupName` | `string` | ✅ | Origin group name for the Redirect API |
| `originUrlShortenerGroupName` | `string` | ✅ | Origin group name for the URL Shortener API |
| `originWebApplicationGroupName` | `string` | ✅ | Origin group name for the Static Web App |
| `redirectApiHostName` | `string` | ✅ | Redirect API hostname |
| `urlShortenerApiHostName` | `string` | ✅ | URL Shortener API hostname |
| `webAppHostName` | `string` | ✅ | Static Web App hostname |
| `customDomainId` | `string` | ✅ | Custom domain resource ID to associate with routes |

## Provisioned Resources

For each of the three services, an **Origin Group**, an **Origin** and a **Route** are provisioned.

---

### Redirect API

**Origin Group** — health probe via `HEAD /healthz` every 120s.

**Route:**

| Property | Value |
|---|---|
| Pattern | `/api/r/*` |
| Origin Path | `/api/r/` |
| Forwarding Protocol | MatchRequest |
| HTTPS Redirect | Enabled |
| Cache | Disabled |

---

### URL Shortener API

**Origin Group** — health probe via `HEAD /healthz` every 120s.

**Route:**

| Property | Value |
|---|---|
| Pattern | `/api/*` |
| Origin Path | `/api/` |
| Forwarding Protocol | MatchRequest |
| HTTPS Redirect | Enabled |
| Cache | Disabled |

> **Precedence note:** The `/api/r/*` route takes precedence over `/api/*` due to Front Door's most-specific-match ordering.

---

### Static Web App (SPA)

**Origin Group** — health probe via `HEAD /` every 120s.

**Route:**

| Property | Value |
|---|---|
| Pattern | `/*` |
| Origin Path | `/` |
| Forwarding Protocol | MatchRequest |
| HTTPS Redirect | Enabled |
| Cache | Enabled (see below) |

**Cache Configuration:**

| Property | Value |
|---|---|
| Compression | Enabled |
| Content Types | HTML, CSS, JS, JSON, SVG, WOFF, WOFF2, TTF |
| Query String Behavior | IgnoreQueryString |

---

## Routing Topology

```
Front Door Endpoint
├── /api/r/*  ──→  Redirect API       (no cache)
├── /api/*    ──→  URL Shortener API  (no cache)
└── /*        ──→  Static Web App     (cache + compression)
```

All origins have `priority: 1` and `weight: 1000`, configured for single-origin with no automatic failover.

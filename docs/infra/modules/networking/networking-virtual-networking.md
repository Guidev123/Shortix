# modules/networking/virtual-networking.bicep

Module for provisioning a **Virtual Network** with configurable subnets, supporting delegations and service endpoints.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | ✅ | VNet name |
| `location` | `string` | ✅ | Azure region |
| `subnets` | `array` | ✅ | List of subnets to provision |

### Subnet object structure

```bicep
{
  name: string
  addressPrefix: string       // e.g. '10.0.1.0/24'
  delegations: [              // e.g. Microsoft.Web/serverfarms
    {
      name: string
      properties: {
        serviceName: string
      }
    }
  ]
  serviceEndpoints: [         // e.g. Microsoft.KeyVault, Microsoft.AzureCosmosDB
    { service: string }
  ]
}
```

## Provisioned Resources

- **Virtual Network** with the address space derived from the provided subnets
- **Subnets** created via loop, with per-subnet configurable delegations and service endpoints

## Subnets in main.bicep

| Subnet | CIDR | Delegation | Service Endpoints |
|---|---|---|---|
| `urlShortenerApi` | `10.0.1.0/24` | `Microsoft.Web/serverfarms` | KeyVault, CosmosDB, Web |
| `redirectApi` | `10.0.2.0/24` | `Microsoft.Web/serverfarms` | KeyVault, CosmosDB |
| `tokenRangeApi` | `10.0.3.0/24` | `Microsoft.Web/serverfarms` | KeyVault, SQL |
| `cosmosTriggerFunction` | `10.0.4.0/24` | `Microsoft.Web/serverfarms` | KeyVault, CosmosDB, Storage |
| `redis` | `10.0.5.0/24` | — | — |
| `postgres` | `10.0.6.0/24` | — | — |

The Redis and PostgreSQL subnets have no delegations since they are used exclusively for Private Endpoints.

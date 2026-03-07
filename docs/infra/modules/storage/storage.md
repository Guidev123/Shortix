# modules/storage/cosmosdb.bicep

Module for provisioning an **Azure Cosmos DB** account (SQL API), database and containers, with VNet-restricted access and the connection string stored in Key Vault.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | ✅ | Cosmos DB account name |
| `location` | `string` | ✅ | Azure region for the account |
| `kind` | `string` | ✅ | Account kind (e.g. `GlobalDocumentDB`) |
| `databaseName` | `string` | ✅ | Name of the SQL database to create |
| `locationName` | `string` | ✅ | Replication region name (e.g. `BrazilSouth`) |
| `keyVaultName` | `string` | ✅ | Key Vault name to store the connection string |
| `subnets` | `array` | ✅ | Resource IDs of subnets with permitted access |
| `containers` | `array` | ❌ | Containers to create (default: `items` and `byUser`) |

### Default containers structure

```bicep
containers = [
  { name: 'items',  partitionKey: '/PartitionKey' }
  { name: 'byUser', partitionKey: '/PartitionKey' }
]
```

## Provisioned Resources

### Cosmos DB Account
- **Offer Type:** Standard
- **VNet Filter:** Enabled (`isVirtualNetworkFilterEnabled: true`)
- **Virtual Network Rules:** one rule per subnet via loop
- **Zone Redundancy:** Disabled
- **Failover Priority:** 0 (single primary region)

### Database
- `urls` database (or the name provided via `databaseName`)

### Containers
Created via loop with the following configuration:
- **Partition Key:** Configurable per container
- **Indexing Mode:** `consistent` (automatic)
- **Included Paths:** `/*`
- **Excluded Paths:** `/"_etag"/?`
- **Default TTL:** `-1` (no automatic expiration)

### Key Vault Secret
- **Name:** `CosmosDb--ConnectionString`
- **Value:** Primary connection string via `listConnectionStrings()`

## Outputs

| Output | Type | Description |
|---|---|---|
| `cosmosDbId` | `string` | Cosmos DB account resource ID |

---

# modules/storage/postgresql.bicep

Module for provisioning **Azure Database for PostgreSQL Flexible Server** with a Private Endpoint, Private DNS Zone and the connection string stored in Key Vault.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | ✅ | PostgreSQL server name |
| `location` | `string` | ✅ | Azure region |
| `administratorLogin` | `string` | ✅ | Administrator login |
| `administratorLoginPassword` | `string` (secure) | ✅ | Administrator password |
| `keyVaultName` | `string` | ✅ | Key Vault name to store the connection string |
| `subnetId` | `string` | ✅ | Subnet resource ID for the Private Endpoint |
| `vnetId` | `string` | ✅ | VNet resource ID for the Private DNS Zone link |

## Provisioned Resources

### PostgreSQL Flexible Server
- **Version:** 16
- **SKU:** `Standard_B1ms` (Burstable)
- **Storage:** 32 GB
- **Backup Retention:** 7 days
- **Geo-Redundant Backup:** Disabled
- **Public Network Access:** Disabled
- **Database created:** `ranges`

### Private Endpoint
- Connected to the provided subnet
- Custom NIC: `nic-{name}`

### Private DNS Zone
- **Name:** `privatelink.postgres.database.azure.com`
- Linked to the VNet via `virtualNetworkLinks`

### Private DNS Zone Group
Associates the Private DNS Zone with the Private Endpoint automatically.

### Key Vault Secret
- **Name:** `Postgres--ConnectionString`
- **Value:** Full connection string with Server, Database, Port, User, Password and SSL Mode

## Outputs

| Output | Type | Description |
|---|---|---|
| `serverId` | `string` | PostgreSQL server resource ID |

---

# modules/storage/redis-cache.bicep

Module for provisioning **Azure Cache for Redis** with a Private Endpoint, Private DNS Zone and the connection string stored in Key Vault.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | ✅ | Redis cache name |
| `location` | `string` | ✅ | Azure region |
| `keyVaultName` | `string` | ✅ | Key Vault name to store the connection string |
| `subnetId` | `string` | ✅ | Subnet resource ID for the Private Endpoint |
| `vnetId` | `string` | ✅ | VNet resource ID for the Private DNS Zone link |

## Provisioned Resources

### Redis Cache
- **SKU:** Basic C0
- **Version:** 6.0
- **Public Network Access:** Disabled
- **AAD Authentication:** Enabled (`aad-enabled: 'True'`)

### Private Endpoint
- Name: `privateendpoint-{name}`
- Group ID: `redisCache`

### Private DNS Zone
- **Name:** `privatelink.redis.cache.windows.net`
- Linked to the VNet via `virtualNetworkLinks`

### A Record in the Private DNS Zone
- **Name:** same as the Redis cache name
- **IP:** extracted from `redisCachePrivateEndpoint.properties.customDnsConfigs[0].ipAddresses[0]`
- **TTL:** 3600

### Key Vault Secret
- **Name:** `Redis--ConnectionString`
- **Value:** Connection string with host, port 6380, primary key, SSL enabled and abortConnect=False

## Outputs

| Output | Type | Description |
|---|---|---|
| `id` | `string` | Redis Cache resource ID |

---

# modules/storage/storage-account.bicep

Module for provisioning an **Azure Storage Account** used by the Azure Functions runtime.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | ✅ | Storage Account name |
| `location` | `string` | ✅ | Azure region |

## Provisioned Resources

### Storage Account
- **Kind:** `Storage`
- **SKU:** `Standard_LRS`
- **HTTPS Only:** Enabled
- **Default OAuth Authentication:** Enabled

## Outputs

| Output | Type | Description |
|---|---|---|
| `storageAccountId` | `string` | Storage Account resource ID |
| `storageAccountName` | `string` | Storage Account name |
| `storageConnectionString` | `string` | Full connection string (passed to the Function App via `AzureWebJobsStorage`) |

# modules/secrets/keyvault.bicep

Module for provisioning an **Azure Key Vault** with RBAC authorization enabled and network access restricted to specific subnets.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `vaultName` | `string` | ✅ | Key Vault name |
| `location` | `string` | ❌ | Azure region (default: Resource Group region) |
| `subnets` | `array` | ✅ | List of subnet resource IDs with permitted access |

## Provisioned Resources

### Key Vault
- **SKU:** Standard
- **RBAC Authorization:** Enabled (`enableRbacAuthorization: true`)
- **Tenant ID:** Inferred from current subscription
- **Network ACLs:**
  - Default Action: `Deny`
  - Virtual Network Rules: one rule per subnet via loop

## Access Behavior

Only the subnets listed in the `subnets` parameter have network access to the Key Vault. All traffic from other sources is blocked by the network ACL.

In `main.bicep`, the authorized subnets are:
- `urlShortenerApi`
- `cosmosTriggerFunction`
- `redirectApi`
- `tokenRangeApi`

## Outputs

| Output | Type | Description |
|---|---|---|
| `id` | `string` | Key Vault resource ID |
| `name` | `string` | Key Vault name |

---

# modules/secrets/key-vault-role-assignment.bicep

Module for assigning the **Key Vault Secrets User** role to multiple principals via loop, using RBAC.

## Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `keyVaultName` | `string` | ✅ | Existing Key Vault name |
| `principalIds` | `array` | ✅ | List of Object IDs (Managed Identities) to receive the role |
| `principalType` | `string` | ❌ | Principal type (default: `ServicePrincipal`) |
| `roleDefinitionId` | `string` | ❌ | Role definition ID (default: `4633458b-17de-408a-b874-0445c86b69e6`) |

## Assigned Role

The default role is **Key Vault Secrets User** (`4633458b-17de-408a-b874-0445c86b69e6`), which grants secret read permission — sufficient for services to consume connection strings and configurations stored in the vault.

## Provisioned Resources

One `Microsoft.Authorization/roleAssignments` per principal, with the name generated via `guid(keyVault.id, principalId, roleDefinitionId)` to ensure idempotency.

## Usage in main.bicep

The four principals receiving the role are the Managed Identities of the compute services:
- `urlShortenerApi`
- `tokenRangeApi`
- `redirectApi`
- `cosmosTriggerFunction`

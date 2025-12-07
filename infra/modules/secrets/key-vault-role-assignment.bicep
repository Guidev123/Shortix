param keyVaultname string
param principalIds array
param principalType string = 'ServicePrincipal'
param roleDefinitionId string = '4f606d9d-0b45-4eb5-876c-7557afb73bb9'

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: keyVaultname
}

resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for principalId in principalIds: {
    name: guid(keyVault.id, principalId, roleDefinitionId)
    scope: keyVault
    properties: {
      roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
      principalId: principalId
      principalType: principalType
    }
  }
]

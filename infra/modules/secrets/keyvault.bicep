param location string = resourceGroup().location
param vaultName string

resource keyvault 'Microsoft.KeyVault/vaults@2025-05-01' = {
  name: vaultName
  location: location
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    enableRbacAuthorization: true
    tenantId: subscription().tenantId
    enabledForTemplateDeployment: true
  }
}

output id string = keyvault.id
output name string = keyvault.name

param location string = resourceGroup().location
param environment string
param uniqueSuffix string = uniqueString(resourceGroup().id)

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    vaultName: 'kv-${uniqueSuffix}'
    location: location
  }
}

module urlShortenerApiService 'modules/compute/appservice.bicep' = {
  name: 'urlShortenerApiDeployment'
  params: {
    appName: 'urlShortenerApi-${environment}'
    appServicePlanName: 'plan-urlShortenerApi-${environment}'
    location: location
    keyVaultName: keyVault.outputs.name
  }
}

module keyVaultRoleAssignment 'modules/secrets/key-vault-role-assignment.bicep' = {
  name: 'keyVaultRoleAssignmentDeployment'
  params: {
    keyVaultname: keyVault.outputs.name
    principalIds: [
      urlShortenerApiService.outputs.principalId
    ]
  }
}

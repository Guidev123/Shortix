param location string = resourceGroup().location
param environment string

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    vaultName: 'kv-${environment}'
    location: location
  }
}

module urlShortenerApiService 'modules/compute/appservice.bicep' = {
  name: 'urlShortenerApiDeployment'
  params: {
    appName: 'urlShortenerApi-${environment}'
    appServicePlanName: 'plan-urlShortenerApi-${environment}'
    location: location
  }
}

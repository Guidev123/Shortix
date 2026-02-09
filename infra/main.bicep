@secure()
param pgSqlPassword string
param environment string
param location string = resourceGroup().location
var uniqueId = uniqueString(subscription().subscriptionId, resourceGroup().name)
var keyVaultName = 'kv-${uniqueId}-${environment}'

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    vaultName: 'kv-${uniqueId}-${environment}'
    location: location
  }
}

module postgres 'modules/storage/postgresql.bicep' = {
  name: 'postgresDeployment'
  params: {
    name: 'postgresql-${uniqueId}-${environment}'
    location: location
    administratorLogin: 'adminuser'
    administratorLoginPassword: pgSqlPassword
    keyVaultName: keyVaultName
  }
}

module keyVaultRoleAssignment 'modules/secrets/key-vault-role-assignment.bicep' = {
  name: 'keyVaultRoleAssignmentDeployment'
  params: {
    keyVaultName: keyVaultName
    principalIds: [
      urlShortenerApi.outputs.principalId
      tokenRangeApi.outputs.principalId
    ]
  }
}

module urlShortenerApi 'modules/compute/appservice.bicep' = {
  name: 'urlShortenerApiDeployment'
  params: {
    appName: 'urlShortenerApi-${environment}'
    appServicePlanName: 'plan-urlShortenerApi-${environment}'
    location: location
    keyVaultName: keyVault.outputs.name
    appSettings: [
      {
        name: 'DatabaseName'
        value: 'urls'
      }
      {
        name: 'ContainerName'
        value: 'items'
      }
      {
        name: 'TokenRangeService__BaseUrl'
        value: tokenRangeApi.outputs.url
      }
    ]
  }
}

module tokenRangeApi 'modules/compute/appservice.bicep' = {
  name: 'tokenRangeApiDeployment'
  params: {
    appName: 'tokenRangeApi-${environment}'
    appServicePlanName: 'plan-tokenRangeApi-${environment}'
    location: location
    keyVaultName: keyVault.outputs.name
  }
}

module cosmosDb 'modules/storage/cosmosdb.bicep' = {
  name: 'cosmosDbDeployment'
  params: {
    name: 'cosmos-db-${uniqueId}-${environment}'
    location: location
    kind: 'GlobalDocumentDB'
    databaseName: 'urls'
    locationName: 'BrazilSouth'
    keyVaultName: keyVaultName
  }
}

module entraApp 'modules/identity/entra-app.bicep' = {
  name: 'entraAppDeployment'
  params: {
    applicationName: 'web-${uniqueId}-${environment}'
  }
}

param environment string
param location string = resourceGroup().location
var uniqueId = uniqueString(resourceGroup().id)
@secure()
param pgSqlPassword string

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    vaultName: 'kv-${uniqueId}-${environment}'
    location: location
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

module postgres 'modules/storage/postgresql.bicep' = {
  name: 'postgresDeployment'
  params: {
    name: 'postgresql-${uniqueId}-${environment}'
    location: location
    administratorLogin: 'adminuser'
    administratorLoginPassword: pgSqlPassword
    keyVaultName: keyVault.outputs.name
  }
}

module cosmosDb 'modules/storage/cosmosdb.bicep' = {
  name: 'cosmosDbDeployment'
  params: {
    name: 'cosmos-db-${uniqueId}'
    location: location
    kind: 'GlobalDocumentDB'
    databaseName: 'urls'
    locationName: 'BrazilSouth'
    keyVaultName: keyVault.outputs.name
  }
}

module keyVaultRoleAssignment 'modules/secrets/key-vault-role-assignment.bicep' = {
  name: 'keyVaultRoleAssignmentDeployment'
  params: {
    keyVaultName: keyVault.outputs.name
    principalIds: [
      urlShortenerApi.outputs.principalId
      tokenRangeApi.outputs.principalId
    ]
  }
}

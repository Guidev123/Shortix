param location string = resourceGroup().location
param environment string
param uniqueSuffix string = uniqueString(resourceGroup().id)
@secure()
param pgSqlPassword string

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    vaultName: 'kv-${uniqueSuffix}-${environment}'
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
    appSettings: [
      {
        name: 'CosmosDb--DatabaseName'
        value: 'cosmos-db-${environment}'
      }
      {
        name: 'CosmosDb--ContainerName'
        value: 'items'
      }
    ]
  }
}

module postgresDb 'modules/storage/postgresql.bicep' = {
  name: 'postgresDeployment'
  params: {
    name: 'postgres-db-${environment}'
    location: location
    administratorLogin: 'adminuser'
    administratorLoginPassword: pgSqlPassword
    keyVaultName: keyVault.outputs.name
  }
}

module cosmosDb 'modules/storage/cosmosdb.bicep' = {
  name: 'cosmosDbDeployment'
  params: {
    name: 'cosmos-db-${environment}'
    location: location
    kind: 'GlobalDocumentDB'
    databaseName: 'UrlShortenerDb'
    locationName: 'East US'
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

module tokenRangeApiService 'modules/compute/appservice.bicep' = {
  name: 'tokenRangeApiDeployment'
  params: {
    appName: 'tokenRangeApi-${environment}'
    appServicePlanName: 'plan-tokenRangeApi-${environment}'
    location: location
    keyVaultName: keyVault.outputs.name
  }
}

@secure()
param pgSqlPassword string
param env string
param location string = resourceGroup().location
var uniqueId = uniqueString(subscription().subscriptionId, resourceGroup().name)
var keyVaultName = 'kv-${uniqueId}-${env}'

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    vaultName: 'kv-${uniqueId}-${env}'
    location: location
  }
}

// module postgres 'modules/storage/postgresql.bicep' = {
//   name: 'postgresDeployment'
//   params: {
//     name: 'postgresql-${uniqueId}-${env}'
//     location: location
//     administratorLogin: 'adminuser'
//     administratorLoginPassword: pgSqlPassword
//     keyVaultName: keyVaultName
//   }
// }

module keyVaultRoleAssignment 'modules/secrets/key-vault-role-assignment.bicep' = {
  name: 'keyVaultRoleAssignmentDeployment'
  params: {
    keyVaultName: keyVaultName
    principalIds: [
      urlShortenerApi.outputs.principalId
      // tokenRangeApi.outputs.principalId
    ]
  }
}

module urlShortenerApi 'modules/compute/appservice.bicep' = {
  name: 'urlShortenerApiDeployment'
  params: {
    appName: 'urlShortenerApi-${env}'
    appServicePlanName: 'plan-urlShortenerApi-${env}'
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
      // {
      //   name: 'TokenRangeService__BaseUrl'
      //   value: tokenRangeApi.outputs.url
      // }
      {
        name: 'AzureAd__Instance'
        value: environment().authentication.loginEndpoint
      }
      {
        name: 'AzureAd__TenantId'
        value: tenant().tenantId
      }
      {
        name: 'AzureAd__ClientId'
        value: entraApp.outputs.applicationId
      }
      {
        name: 'AzureAd__Scopes'
        value: 'Urls.Read'
      }
    ]
  }
}

// module tokenRangeApi 'modules/compute/appservice.bicep' = {
//   name: 'tokenRangeApiDeployment'
//   params: {
//     appName: 'tokenRangeApi-${env}'
//     appServicePlanName: 'plan-tokenRangeApi-${env}'
//     location: location
//     keyVaultName: keyVault.outputs.name
//   }
// }

// module cosmosDb 'modules/storage/cosmosdb.bicep' = {
//   name: 'cosmosDbDeployment'
//   params: {
//     name: 'cosmos-db-${uniqueId}-${env}'
//     location: location
//     kind: 'GlobalDocumentDB'
//     databaseName: 'urls'
//     locationName: 'BrazilSouth'
//     keyVaultName: keyVaultName
//   }
// }

module entraApp 'modules/identity/entra-app.bicep' = {
  name: 'entraAppDeployment'
  params: {
    applicationName: 'web-${uniqueId}-${env}'
  }
}

@secure()
param pgSqlPassword string
param env string
param location string = resourceGroup().location
var uniqueId = uniqueString(subscription().subscriptionId, resourceGroup().name)
var keyVaultName = 'kv-${uniqueId}-${env}'

// ================== Identity resources ==================

module entraApp 'modules/identity/entra-app.bicep' = {
  name: 'entraAppDeployment'
  params: {
    applicationName: 'web-${uniqueId}-${env}'
    spaRedirectUris: env == 'dev'
      ? [
          'http://localhost:3000'
          staticWebApp.outputs.url
        ]
      : [
          staticWebApp.outputs.url
        ]
  }
}

// ================== Secrets resources ==================

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    vaultName: 'kv-${uniqueId}-${env}'
    location: location
  }
}

module keyVaultRoleAssignment 'modules/secrets/key-vault-role-assignment.bicep' = {
  name: 'keyVaultRoleAssignmentDeployment'
  params: {
    keyVaultName: keyVaultName
    principalIds: [
      urlShortenerApi.outputs.principalId
      tokenRangeApi.outputs.principalId
      redirectApi.outputs.principalId
      cosmosTriggerFunction.outputs.principalId
    ]
  }
  dependsOn: [
    keyVault
  ]
}

// ================== Telemetry resources ==================

module logAnalyticsWorkspace 'modules/telemetry/log-analytics.bicep' = {
  name: 'logAnalyticsWorkspaceDeployment'
  params: {
    name: 'log-analytics-ws-${uniqueId}-${env}'
    location: location
  }
}

// ================== Compute resources ==================

module urlShortenerApi 'modules/compute/appservice.bicep' = {
  name: 'urlShortenerApiDeployment'
  params: {
    appName: 'urlShortenerApi-${uniqueId}-${env}'
    appServicePlanName: 'plan-urlShortenerApi-${uniqueId}-${env}'
    location: location
    keyVaultName: keyVaultName
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.outputs.id
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
        name: 'ByUserDatabaseName'
        value: 'urls'
      }
      {
        name: 'ByUserContainerName'
        value: 'byUser'
      }
      {
        name: 'TokenRangeService__BaseUrl'
        value: tokenRangeApi.outputs.url
      }
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
      {
        name: 'WebAppEndpoints'
        value: staticWebApp.outputs.url
      }
    ]
  }
  dependsOn: [
    keyVault
  ]
}

module tokenRangeApi 'modules/compute/appservice.bicep' = {
  name: 'tokenRangeApiDeployment'
  params: {
    appName: 'tokenRangeApi-${uniqueId}-${env}'
    appServicePlanName: 'plan-tokenRangeApi-${uniqueId}-${env}'
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.outputs.id
    location: location
    keyVaultName: keyVaultName
  }
  dependsOn: [
    keyVault
  ]
}

module redirectApi 'modules/compute/appservice.bicep' = {
  name: 'redirectApiDeployment'
  params: {
    appName: 'redirectApi-${uniqueId}-${env}'
    appServicePlanName: 'plan-redirectApi-${uniqueId}-${env}'
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.outputs.id
    location: location
    keyVaultName: keyVaultName
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
  dependsOn: [
    keyVault
  ]
}

module cosmosTriggerFunction 'modules/compute/function.bicep' = {
  name: 'cosmosTriggerFunctionDeployment'
  params: {
    name: 'func-cosmosTriggerPropagation-${uniqueId}-${env}'
    appServicePlanName: 'plan-cosmosTriggerFunction-${uniqueId}-${env}'
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.outputs.id
    location: location
    keyVaultName: keyVaultName
    storageAccountConnectionString: storageAccount.outputs.storageConnectionString
    appSettings: [
      {
        name: 'CosmosDbConnection'
        value: '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/CosmosDb--ConnectionString/)'
      }
      {
        name: 'TargetDatabaseName'
        value: 'urls'
      }
      {
        name: 'TargetContainerName'
        value: 'byUser'
      }
    ]
  }
  dependsOn: [
    keyVault
    cosmosDb
  ]
}

module staticWebApp 'modules/web/static-web-app.bicep' = {
  name: 'staticWebAppDeployment'
  params: {
    name: 'url-shortener-${uniqueId}-${env}'
  }
}

// ================== Storage resources ==================

module storageAccount 'modules/storage/storage-account.bicep' = {
  name: 'storageAccountDeployment'
  params: {
    name: 'storage${uniqueId}${env}'
    location: location
  }
  dependsOn: [
    keyVault
  ]
}

module cosmosDb 'modules/storage/cosmosdb.bicep' = {
  name: 'cosmosDbDeployment'
  params: {
    name: 'cosmos-db-${uniqueId}-${env}'
    location: location
    kind: 'GlobalDocumentDB'
    databaseName: 'urls'
    locationName: 'BrazilSouth'
    keyVaultName: keyVaultName
  }
  dependsOn: [
    keyVault
  ]
}

module postgres 'modules/storage/postgresql.bicep' = {
  name: 'postgresDeployment'
  params: {
    name: 'postgresql-${uniqueId}-${env}'
    location: location
    administratorLogin: 'adminuser'
    administratorLoginPassword: pgSqlPassword
    keyVaultName: keyVaultName
  }
  dependsOn: [
    keyVault
  ]
}

module redisCache 'modules/storage/redis-cache.bicep' = {
  name: 'redisCacheDeployment'
  params: {
    name: 'redis-cache-${uniqueId}-${env}'
    location: location
    keyVaultName: keyVaultName
  }
  dependsOn: [
    keyVault
  ]
}

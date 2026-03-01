@secure()
param pgSqlPassword string
param env string
param customDomainName string
param location string = resourceGroup().location

var uniqueId = uniqueString(subscription().subscriptionId, resourceGroup().name)
var keyVaultName = 'kv-${uniqueId}-${env}'
var vnetName = 'vnet-${uniqueId}-${env}'
var urlShortenerApiSubnetName = 'subnet-url-shortener-api-${uniqueId}-${env}'
var tokenRangeApiSubnetName = 'subnet-token-range-api-${uniqueId}-${env}'
var redirectApiSubnetName = 'subnet-redirect-api-${uniqueId}-${env}'
var cosmosTriggerFunctionSubnetName = 'subnet-cosmos-trigger-${uniqueId}-${env}'
var redisSubnetName = 'subnet-redis-${uniqueId}-${env}'
var postgresSubnetName = 'subnet-postgres-${uniqueId}-${env}'

// ================== Networking resources ==================

module vnet 'modules/networking/virtual-networking.bicep' = {
  name: 'vnetDeployment'
  params: {
    name: vnetName
    location: location
    subnets: [
      {
        name: urlShortenerApiSubnetName
        addressPrefix: '10.0.1.0/24'
        delegations: [
          {
            name: 'Microsoft.Web/serverfarms'
            properties: {
              serviceName: 'Microsoft.Web/serverfarms'
            }
          }
        ]
        serviceEndpoints: [
          { service: 'Microsoft.KeyVault' }
          { service: 'Microsoft.AzureCosmosDB' }
          { service: 'Microsoft.Web' }
        ]
      }
      {
        name: redirectApiSubnetName
        addressPrefix: '10.0.2.0/24'
        delegations: [
          {
            name: 'Microsoft.Web/serverfarms'
            properties: {
              serviceName: 'Microsoft.Web/serverfarms'
            }
          }
        ]
        serviceEndpoints: [
          { service: 'Microsoft.KeyVault' }
          { service: 'Microsoft.AzureCosmosDB' }
        ]
      }
      {
        name: tokenRangeApiSubnetName
        addressPrefix: '10.0.3.0/24'
        delegations: [
          {
            name: 'Microsoft.Web/serverfarms'
            properties: {
              serviceName: 'Microsoft.Web/serverfarms'
            }
          }
        ]
        serviceEndpoints: [
          { service: 'Microsoft.KeyVault' }
          { service: 'Microsoft.SQL' }
        ]
      }
      {
        name: cosmosTriggerFunctionSubnetName
        addressPrefix: '10.0.4.0/24'
        delegations: [
          {
            name: 'Microsoft.Web/serverfarms'
            properties: {
              serviceName: 'Microsoft.Web/serverfarms'
            }
          }
        ]
        serviceEndpoints: [
          { service: 'Microsoft.KeyVault' }
          { service: 'Microsoft.AzureCosmosDB' }
          { service: 'Microsoft.Storage' }
        ]
      }
      {
        name: redisSubnetName
        addressPrefix: '10.0.5.0/24'
        delegations: []
        serviceEndpoints: []
      }
      {
        name: postgresSubnetName
        addressPrefix: '10.0.6.0/24'
        delegations: []
        serviceEndpoints: []
      }
    ]
  }
}

module frontDoor 'modules/networking/front-door.bicep' = {
  name: 'frontDoorDeployment'
  params: {
    endpointName: 'endpoint-${uniqueId}-${env}'
    profileName: 'front-door-${uniqueId}-${env}'
    wafPolicyName: 'wafPolicy${uniqueId}-${env}'
    customDomainHostName: customDomainName
  }
}

module frontDoorRoutes 'modules/networking/front-door-routes.bicep' = {
  name: 'frontDoorRoutesDeployment'
  params: {
    endpointName: 'endpoint-${uniqueId}-${env}'
    profileName: 'front-door-${uniqueId}-${env}'
    originRedirectGroupName: 'origin-group-redirect-${uniqueId}-${env}'
    originUrlShortenerGroupName: 'origin-group-url-shortener-${uniqueId}-${env}'
    originWebApplicationGroupName: 'origin-group-web-application-${uniqueId}-${env}'
    redirectApiHostName: redirectApi.outputs.hostname
    urlShortenerApiHostName: urlShortenerApi.outputs.hostname
    webAppHostName: staticWebApp.outputs.hostname
    customDomainId: frontDoor.outputs.customDomainId
  }
}

// ================== Identity resources ==================

module entraApp 'modules/identity/entra-app.bicep' = {
  name: 'entraAppDeployment'
  params: {
    applicationName: 'web-${uniqueId}-${env}'
    spaRedirectUris: env == 'dev'
      ? [
          'http://localhost:3000'
          staticWebApp.outputs.url
          'https://${frontDoor.outputs.endpointHostName}'
          'https://${customDomainName}'
        ]
      : [
          staticWebApp.outputs.url
          'https://${frontDoor.outputs.endpointHostName}'
          'https://${customDomainName}'
        ]
  }
}

// ================== Secrets resources ==================

module keyVault 'modules/secrets/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    vaultName: 'kv-${uniqueId}-${env}'
    location: location
    subnets: [
      resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, urlShortenerApiSubnetName)
      resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, cosmosTriggerFunctionSubnetName)
      resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, redirectApiSubnetName)
      resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, tokenRangeApiSubnetName)
    ]
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
    vnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, urlShortenerApiSubnetName)
    ipSecurityRestrictions: [
      {
        name: 'AllowFrontDoor'
        action: 'Allow'
        priority: 100
        tag: 'ServiceTag'
        ipAddress: 'AzureFrontDoor.Backend'
      }
    ]
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
    vnet
  ]
}

module tokenRangeApi 'modules/compute/appservice.bicep' = {
  name: 'tokenRangeApiDeployment'
  params: {
    appName: 'tokenRangeApi-${uniqueId}-${env}'
    appServicePlanName: 'plan-tokenRangeApi-${uniqueId}-${env}'
    vnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, tokenRangeApiSubnetName)
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.outputs.id
    ipSecurityRestrictions: [
      {
        tag: 'Default'
        action: 'Allow'
        priority: 100
        name: 'AllowUrlShortenerApiSubnet'
        vnetSubnetResourceId: resourceId(
          'Microsoft.Network/virtualNetworks/subnets',
          vnetName,
          urlShortenerApiSubnetName
        )
      }
    ]
    location: location
    keyVaultName: keyVaultName
  }
  dependsOn: [
    vnet
    keyVault
  ]
}

module redirectApi 'modules/compute/appservice.bicep' = {
  name: 'redirectApiDeployment'
  params: {
    appName: 'redirectApi-${uniqueId}-${env}'
    appServicePlanName: 'plan-redirectApi-${uniqueId}-${env}'
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.outputs.id
    vnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, redirectApiSubnetName)
    location: location
    keyVaultName: keyVaultName
    ipSecurityRestrictions: [
      {
        name: 'AllowFrontDoor'
        action: 'Allow'
        priority: 100
        tag: 'ServiceTag'
        ipAddress: 'AzureFrontDoor.Backend'
      }
    ]
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
    vnet
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
    subnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, cosmosTriggerFunctionSubnetName)
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
    vnet
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
    subnets: [
      resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, urlShortenerApiSubnetName)
      resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, cosmosTriggerFunctionSubnetName)
      resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, redirectApiSubnetName)
    ]
  }
  dependsOn: [
    keyVault
    vnet
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
    vnetId: resourceId('Microsoft.Network/virtualNetworks', vnetName)
    subnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, postgresSubnetName)
  }
  dependsOn: [
    keyVault
    vnet
  ]
}

module redisCache 'modules/storage/redis-cache.bicep' = {
  name: 'redisCacheDeployment'
  params: {
    name: 'redis-cache-${uniqueId}-${env}'
    location: location
    keyVaultName: keyVaultName
    vnetId: resourceId('Microsoft.Network/virtualNetworks', vnetName)
    subnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, redisSubnetName)
  }
  dependsOn: [
    keyVault
  ]
}

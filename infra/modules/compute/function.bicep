param appServicePlanName string
param location string
param name string
param keyVaultName string
param appSettings array = []
@secure()
param storageAccountConnectionString string
param logAnalyticsWorkspaceId string
param subnetId string

module appInsights '../telemetry/app-insights.bicep' = {
  name: '${name}-AppInsightsDeployment'
  params: {
    location: location
    name: 'appinsights-${name}'
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2025-03-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: 'B1'
  }
}

resource function 'Microsoft.Web/sites@2025-03-01' = {
  name: name
  location: location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      healthCheckPath: '/api/healthz'
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      alwaysOn: true
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
      publicNetworkAccess: 'Enabled'
      ipSecurityRestrictionsDefaultAction: 'Deny'
      scmIpSecurityRestrictions: [
        {
          name: 'AllowGHDeploy'
          action: 'Allow'
          priority: 100
          tag: 'ServiceTag'
          ipAddress: 'AzureCloud'
        }
      ]
      scmIpSecurityRestrictionsDefaultAction: 'Deny'
      appSettings: concat(
        [
          {
            name: 'KeyVaultName'
            value: keyVaultName
          }
          {
            name: 'AzureWebJobsStorage'
            value: storageAccountConnectionString
          }
          {
            name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
            value: storageAccountConnectionString
          }
          {
            name: 'WEBSITE_CONTENTSHARE'
            value: toLower(name)
          }
          {
            name: 'FUNCTIONS_WORKER_RUNTIME'
            value: 'dotnet-isolated'
          }
          {
            name: 'FUNCTIONS_EXTENSION_VERSION'
            value: '~4'
          }
          {
            name: 'WEBSITE_RUN_FROM_PACKAGE'
            value: '1'
          }
          {
            name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
            value: appInsights.outputs.instrumentationKey
          }
          {
            name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
            value: appInsights.outputs.connectionString
          }
        ],
        appSettings
      )
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource webAppConfig 'Microsoft.Web/sites/config@2025-03-01' = {
  parent: function
  name: 'web'
  properties: {
    scmType: 'GitHub'
  }
}

resource functionVirtualNetwork 'Microsoft.Web/sites/networkConfig@2025-03-01' = {
  parent: function
  name: 'virtualNetwork'
  properties: {
    subnetResourceId: subnetId
  }
}

output id string = function.id
output principalId string = function.identity.principalId

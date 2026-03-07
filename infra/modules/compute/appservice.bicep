param location string = resourceGroup().location
param appServicePlanName string
param appName string
param keyVaultName string
param appSettings array = []
param logAnalyticsWorkspaceId string
param vnetId string
param ipSecurityRestrictions array = []

module appInsights '../telemetry/app-insights.bicep' = {
  name: '${appName}-AppInsightsDeployment'
  params: {
    location: location
    name: 'appinsights-${appName}'
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2025-03-01' = {
  kind: 'linux'
  location: location
  name: appServicePlanName
  properties: {
    reserved: true
  }
  sku: {
    name: 'B1'
  }
}

resource webApp 'Microsoft.Web/sites@2025-03-01' = {
  name: appName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    virtualNetworkSubnetId: vnetId
    siteConfig: {
      healthCheckPath: '/healthz'
      linuxFxVersion: 'DOTNETCORE|10.0'
      publicNetworkAccess: 'Enabled'
      ipSecurityRestrictionsDefaultAction: 'Deny'
      ipSecurityRestrictions: ipSecurityRestrictions
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
  parent: webApp
  name: 'web'
  properties: {
    scmType: 'GitHub'
  }
}

output appServiceId string = webApp.id
output principalId string = webApp.identity.principalId
output url string = 'https://${webApp.properties.defaultHostName}'
output hostname string = webApp.properties.defaultHostName

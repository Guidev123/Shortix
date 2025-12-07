param location string = resourceGroup().location
param appServicePlanName string
param appName string
param keyVaultName string

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
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      appSettings: [
        { name: 'KeyVaultName', value: keyVaultName }
      ]
    }
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
output appServicePrincipalId string = webApp.identity.principalId

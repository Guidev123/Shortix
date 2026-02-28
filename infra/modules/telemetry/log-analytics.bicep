param name string
param location string

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2025-07-01' = {
  name: name
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

output id string = logAnalyticsWorkspace.id

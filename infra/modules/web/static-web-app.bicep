param name string

resource staticWebApp 'Microsoft.Web/staticSites@2025-03-01' = {
  location: 'eastus2'
  name: name
  sku: {
    tier: 'Standard'
    name: 'Standard'
  }
  properties: {}
}

output id string = staticWebApp.id
output url string = 'https://${staticWebApp.properties.defaultHostname}'
output hostname string = staticWebApp.properties.defaultHostname

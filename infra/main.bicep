param location string = resourceGroup().location

var uniqueId = uniqueString(resourceGroup().id)

module urlShortenerApiService 'modules/compute/appservice.bicep' = {
  name: 'urlShortenerApiDeployment'
  params: {
    appName: 'urlShortenerApi-${uniqueId}'
    appServicePlanName: 'plan-urlShortenerApi-${uniqueId}'
    location: location
  }
}

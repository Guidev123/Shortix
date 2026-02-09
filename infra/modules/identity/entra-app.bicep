extension microsoftGraph

param applicationName string
@allowed([
  'AzureADMyOrg'
  'AzureADMultipleOrgs'
  'AzureADandPersonalMicrosoftAccount'
])
param signInAudience string = 'AzureADandPersonalMicrosoftAccount'

resource application 'Microsoft.Graph/applications@v1.0' = {
  displayName: applicationName
  uniqueName: applicationName
  signInAudience: signInAudience
}

output applicationId string = application.appId

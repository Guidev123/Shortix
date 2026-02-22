extension microsoftGraph

param applicationName string
@allowed([
  'AzureADMyOrg'
  'AzureADMultipleOrgs'
  'AzureADandPersonalMicrosoftAccount'
])
param signInAudience string = 'AzureADandPersonalMicrosoftAccount'
param spaRedirectUris array = []

resource application 'Microsoft.Graph/applications@v1.0' = {
  displayName: applicationName
  uniqueName: applicationName
  signInAudience: signInAudience
}

resource updateApplicationWithSettings 'Microsoft.Graph/applications@v1.0' = {
  displayName: applicationName
  uniqueName: applicationName
  signInAudience: signInAudience
  api: {
    oauth2PermissionScopes: [
      {
        id: '9d0c290c-3ddf-40b9-a153-59fbff143ac3'
        isEnabled: true
        value: 'Urls.Read'
        type: 'User'
        adminConsentDescription: 'URLs Read'
        adminConsentDisplayName: 'URLs Read'
        userConsentDescription: null
        userConsentDisplayName: 'Read Access to Urls'
      }
    ]
  }
  identifierUris: [
    'api://${application.appId}'
  ]
  spa: {
    redirectUris: spaRedirectUris
  }
  web: {
    implicitGrantSettings: {
      enableAccessTokenIssuance: true
      enableIdTokenIssuance: true
    }
    redirectUris: []
  }
}

output applicationId string = application.appId

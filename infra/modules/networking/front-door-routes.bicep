param profileName string
param endpointName string
param originRedirectGroupName string
param redirectApiHostName string
param urlShortenerApiHostName string
param originUrlShortenerGroupName string
param webAppHostName string
param originWebApplicationGroupName string
param customDomainId string

resource profile 'Microsoft.Cdn/profiles@2025-06-01' existing = {
  name: profileName
}

resource endpoint 'Microsoft.Cdn/profiles/afdEndpoints@2025-06-01' existing = {
  name: endpointName
  parent: profile
}

// Redirect API 

resource originRedirectGroup 'Microsoft.Cdn/profiles/originGroups@2025-06-01' = {
  parent: profile
  name: originRedirectGroupName
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
    }
    healthProbeSettings: {
      probePath: '/healthz'
      probeRequestType: 'HEAD'
      probeProtocol: 'Http'
      probeIntervalInSeconds: 120
    }
  }
}

resource originRedirect 'Microsoft.Cdn/profiles/originGroups/origins@2025-06-01' = {
  parent: originRedirectGroup
  name: 'origin-redirect'
  properties: {
    hostName: redirectApiHostName
    httpPort: 80
    httpsPort: 443
    originHostHeader: redirectApiHostName
    priority: 1
    weight: 1000
  }
}

resource routeRedirect 'Microsoft.Cdn/profiles/afdEndpoints/routes@2025-06-01' = {
  parent: endpoint
  name: 'route-redirect'
  properties: {
    originGroup: {
      id: originRedirectGroup.id
    }
    supportedProtocols: [
      'Http'
      'Https'
    ]
    patternsToMatch: [
      '/api/r/*'
    ]
    originPath: '/api/r/'
    forwardingProtocol: 'MatchRequest'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
    customDomains: [
      {
        id: customDomainId
      }
    ]
  }
  dependsOn: [
    originRedirect
  ]
}

// Url shortener API 

resource originUrlShortenerGroup 'Microsoft.Cdn/profiles/originGroups@2025-06-01' = {
  parent: profile
  name: originUrlShortenerGroupName
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
    }
    healthProbeSettings: {
      probePath: '/healthz'
      probeRequestType: 'HEAD'
      probeProtocol: 'Http'
      probeIntervalInSeconds: 120
    }
  }
}

resource originUrlShortener 'Microsoft.Cdn/profiles/originGroups/origins@2025-06-01' = {
  parent: originUrlShortenerGroup
  name: 'origin-url-shortener'
  properties: {
    hostName: urlShortenerApiHostName
    httpPort: 80
    httpsPort: 443
    originHostHeader: urlShortenerApiHostName
    priority: 1
    weight: 1000
  }
}

resource routeUrlShortener 'Microsoft.Cdn/profiles/afdEndpoints/routes@2025-06-01' = {
  parent: endpoint
  name: 'route-url-shortener'
  properties: {
    originGroup: {
      id: originUrlShortenerGroup.id
    }
    supportedProtocols: [
      'Http'
      'Https'
    ]
    patternsToMatch: [
      '/api/*'
    ]
    originPath: '/api/'
    forwardingProtocol: 'MatchRequest'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
    customDomains: [
      {
        id: customDomainId
      }
    ]
  }
  dependsOn: [
    originUrlShortener
  ]
}

// Web Application

resource originWebApplicationGroup 'Microsoft.Cdn/profiles/originGroups@2025-06-01' = {
  parent: profile
  name: originWebApplicationGroupName
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
    }
    healthProbeSettings: {
      probePath: '/'
      probeRequestType: 'HEAD'
      probeProtocol: 'Http'
      probeIntervalInSeconds: 120
    }
  }
}

resource originWebApplication 'Microsoft.Cdn/profiles/originGroups/origins@2025-06-01' = {
  parent: originWebApplicationGroup
  name: 'origin-web-application'
  properties: {
    hostName: webAppHostName
    httpPort: 80
    httpsPort: 443
    originHostHeader: webAppHostName
    priority: 1
    weight: 1000
  }
}

resource routeWebApplication 'Microsoft.Cdn/profiles/afdEndpoints/routes@2025-06-01' = {
  parent: endpoint
  name: 'route-web-application'
  properties: {
    originGroup: {
      id: originWebApplicationGroup.id
    }
    supportedProtocols: [
      'Http'
      'Https'
    ]
    patternsToMatch: [
      '/*'
    ]
    originPath: '/'
    forwardingProtocol: 'MatchRequest'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
    customDomains: [
      {
        id: customDomainId
      }
    ]
    cacheConfiguration: {
      compressionSettings: {
        isCompressionEnabled: true
        contentTypesToCompress: [
          'text/html'
          'text/css'
          'application/javascript'
          'application/json'
          'image/svg+xml'
          'font/woff'
          'font/woff2'
          'font/ttf'
        ]
      }
      queryStringCachingBehavior: 'IgnoreQueryString'
    }
  }
  dependsOn: [
    originWebApplication
  ]
}

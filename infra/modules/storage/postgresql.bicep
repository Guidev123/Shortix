param name string
param location string
param administratorLogin string
@secure()
param administratorLoginPassword string

resource postgresqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2025-08-01' = {
  name: name
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
  }
  resource database 'databases' = {
    name: 'Ranges'
  }
  resource firewallRules 'firewallRules' = {
    name: 'AllowAllAzureServices'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '255.255.255.255'
    }
  }
}

output serverId string = postgresqlServer.id
output serverName string = postgresqlServer.name
output databaseName string = 'Ranges'

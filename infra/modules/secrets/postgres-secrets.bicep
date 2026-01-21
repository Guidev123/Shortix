param keyVaultName string
param serverName string
param databaseName string
param administratorLogin string
@secure()
param administratorLoginPassword string

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: keyVaultName
  scope: resourceGroup()
}

resource postgresDbConnectionString 'Microsoft.KeyVault/vaults/secrets@2025-05-01' = {
  parent: keyVault
  name: 'Postgres__ConnectionString'
  properties: {
    value: 'Server=${serverName}.postgres.database.azure.com;Database=${databaseName};Port=5432;User Id=${administratorLogin};Password=${administratorLoginPassword};Ssl Mode=Require;'
  }
}

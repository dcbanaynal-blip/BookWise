param location string
param environment string
param adminLogin string
@secure()
param adminPassword string

var sqlServerName = 'bookwise-sql-${environment}-${uniqueString(resourceGroup().id)}'
var sqlDbName = 'bookwise-db-${environment}'

resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: adminLogin
    administratorLoginPassword: adminPassword
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  name: '${sqlServer.name}/${sqlDbName}'
  location: location
  sku: {
    name: 'S0'
    tier: 'Standard'
  }
  properties: {
    zoneRedundant: false
  }
}

resource firewall 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = {
  name: '${sqlServer.name}/AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

output sqlConnectionString string = 'Server=tcp:${sqlServer.name}.database.windows.net,1433;Initial Catalog=${sqlDbName};User ID=${adminLogin};Password=${adminPassword};Encrypt=True;Connection Timeout=30;'

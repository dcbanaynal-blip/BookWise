targetScope = 'subscription'

param location string = 'eastus'
param resourceGroupName string = 'rg-bookwise'
param sqlAdminLogin string
@secure()
param sqlAdminPassword string
param environment string = 'dev'

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: resourceGroupName
  location: location
  tags: {
    'application': 'BookWise'
    'environment': environment
  }
}

module sql './modules/sql.bicep' = {
  name: 'sql'
  scope: rg
  params: {
    location: location
    environment: environment
    adminLogin: sqlAdminLogin
    adminPassword: sqlAdminPassword
  }
}

module app './modules/appservice.bicep' = {
  name: 'appService'
  scope: rg
  params: {
    location: location
    environment: environment
    sqlConnectionString: sql.outputs.sqlConnectionString
  }
}

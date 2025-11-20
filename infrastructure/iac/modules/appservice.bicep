param location string
param environment string
param sqlConnectionString string

@description('Name prefix for all application workloads.')
var namePrefix = 'bookwise-${environment}'

resource plan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: '${namePrefix}-plan'
  location: location
  sku: {
    name: 'P1v3'
    capacity: 1
    tier: 'PremiumV3'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource api 'Microsoft.Web/sites@2022-09-01' = {
  name: '${namePrefix}-api'
  location: location
  kind: 'app,linux,container'
  properties: {
    serverFarmId: plan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment
        }
        {
          name: 'ConnectionStrings__SqlServer'
          value: sqlConnectionString
        }
      ]
    }
  }
}

resource worker 'Microsoft.Web/sites@2022-09-01' = {
  name: '${namePrefix}-ocr-worker'
  location: location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: plan.id
    siteConfig: {
      appSettings: [
        {
          name: 'ConnectionStrings__SqlServer'
          value: sqlConnectionString
        }
      ]
    }
  }
}

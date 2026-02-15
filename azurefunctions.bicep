param prefix string
param serviceTag string
param environment string
param regionTag string

param tags object = {}
param location string = resourceGroup().location

param appInsightsRetention int = 30

param numberOfWorkers int = 1


var resourcePrefix = '${prefix}-${serviceTag}-${environment}-${regionTag}'
var appInsightsName = '${resourcePrefix}-AI-1'
var storageAccountName = replace(toLower('${resourcePrefix}-SA-1'), '-', '')
var functionAppName = '${resourcePrefix}-FA-1'
var serverFarmName = '${resourcePrefix}-ASP-1'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  kind: 'StorageV2'
  location: location
  tags: tags
  name: storageAccountName
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
  }

  resource blobContainer 'blobServices@2023-01-01' = {
    name: 'default'
    properties: {}
  }

  resource queue 'queueServices@2023-01-01' = {
    name: 'default'
    properties: {}
  }
}

// App Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  kind: 'web'
  location: location
  tags: tags
  name: appInsightsName
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
    RetentionInDays: appInsightsRetention
  }
}

// App Service Plan (Consumption)
resource serverFarm 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: serverFarmName
  location: location
  tags: tags
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

// Function App
resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  properties: {
    enabled: true
    serverFarmId: serverFarm.id
    siteConfig: {
      autoHealEnabled: true
      autoHealRules: {
        triggers: {
          privateBytesInKB: 0
          statusCodes: [
            {
              status: 500
              subStatus: 0
              win32Status: 0
              count: 25
              timeInterval: '00:05:00'
            }
          ]
        }
        actions: {
          actionType: 'Recycle'
          minProcessExecutionTime: '00:01:00'
        }
      }
      numberOfWorkers: numberOfWorkers
      netFrameworkVersion: 'v6.0'
    }
    clientAffinityEnabled: false
    httpsOnly: true
    redundancyMode: 'None'
  }

  resource functionAppConfig 'config@2022-09-01' = {
    name: 'appsettings'
    properties: {
      AzureWebJobsStorage: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
      FUNCTIONS_EXTENSION_VERSION: '~4'
      FUNCTIONS_WORKER_RUNTIME: 'dotnet'
      WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
      WEBSITE_CONTENTSHARE: replace(toLower(functionApp.name), '-', '')
      APPINSIGHTS_INSTRUMENTATIONKEY: appInsights.properties.InstrumentationKey
      APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
    }
  }
}

output functionAppName string = functionApp.name
output storageAccountName string = storageAccount.name
output appInsightsName string = appInsights.name

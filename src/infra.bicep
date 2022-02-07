param subscriptionId string
param resourceGroup string
param adminEmail string
param organizationName string
param nameSuffix string
param location string
param locationName string
param skuStaticApp string
param skuCodeStaticApp string
param staticAppDomain string
param cosmosDefaultExperience string
param platformFunctionUse32BitWorkerProcess bool
param platformFunctionSku string
param platformFunctionSkuCode string
param platformFunctionWorkerSize string
param platformFunctionWorkerSizeId string
param platformFunctionNumberOfWorkers string
param platformFunctionStorageAccountName string
param platformFunctionLinuxFxVersion string
param apimTier string

var appInsightsPlanName = 'moosta-insights${nameSuffix}'
var functionsHostingPlanName = 'moosta-functions-hosting${nameSuffix}'
var appInsightsWorkspaceName = 'MoostaAppInsights${nameSuffix}'

//Mosta.Web
resource moostaWeb 'Microsoft.Web/staticSites@2019-12-01-preview' = {
  name: 'moosta-web${nameSuffix}'
  location: location
  tags: {}
  properties: {}
  sku: {
    Tier: skuStaticApp
    Name: skuCodeStaticApp
  }
  resource moostaWebDomains 'customDomains@2021-01-15' = {
    name: staticAppDomain
    properties: {
      validationMethod: 'cname-delegation'
    }    
  }
}

//Moosta Database
resource moostaCosmosDb 'Microsoft.DocumentDb/databaseAccounts@2021-10-15-preview' = {
  kind: 'GlobalDocumentDB'
  name: 'moosta-cosmos${nameSuffix}'
  location: location
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        failoverPriority: 0
        locationName: locationName
      }
    ]
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Geo'
      }
    }
    isVirtualNetworkFilterEnabled: false
    virtualNetworkRules: []
    ipRules: []
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    enableFreeTier: true
  }
  tags: {
    defaultExperience: cosmosDefaultExperience
    'hidden-cosmos-mmspecial': ''
  }
}

//Moosta.Functions.Platform
resource moostaPlatformFunction 'Microsoft.Web/sites@2018-11-01' = {
  name: 'moosta-functions-platform${nameSuffix}'
  kind: 'functionapp,linux'
  location: location
  tags: {}
  properties: {
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: reference('microsoft.insights/components/${appInsightsPlanName}', '2015-05-01').InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: reference('microsoft.insights/components/${appInsightsPlanName}', '2015-05-01').ConnectionString
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${platformFunctionStorageAccountName};AccountKey=${listKeys(moostaPlatformFunctionStorageAccount.id, '2019-06-01').keys[0].value};EndpointSuffix=core.windows.net'
        }
      ]
      cors: {
        allowedOrigins: [
          'https://portal.azure.com'
        ]
      }
      use32BitWorkerProcess: platformFunctionUse32BitWorkerProcess
      linuxFxVersion: platformFunctionLinuxFxVersion
    }
    serverFarmId: '/subscriptions/${subscriptionId}/resourcegroups/${resourceGroup}/providers/Microsoft.Web/serverfarms/${functionsHostingPlanName}'
    clientAffinityEnabled: false
  }
  dependsOn: [
    moostaPlatformFunctionAppInsights
    moostaPlatformFunction_HostingPlan
  ]
}

resource moostaPlatformFunction_HostingPlan 'Microsoft.Web/serverfarms@2018-11-01' = {
  name: functionsHostingPlanName
  location: location
  kind: 'linux'
  tags: {}
  properties: {
    name: functionsHostingPlanName
    workerSize: platformFunctionWorkerSize
    workerSizeId: platformFunctionWorkerSizeId
    numberOfWorkers: platformFunctionNumberOfWorkers
    reserved: true
  }
  sku: {
    Tier: platformFunctionSku
    Name: platformFunctionSkuCode
  }
  dependsOn: []
}

resource moostaPlatformFunctionAppInsights 'microsoft.insights/components@2020-02-02-preview' = {
  name: appInsightsPlanName
  location: location
  kind: 'web'
  tags: {}
  properties: {
    Request_Source: 'rest'
    Flow_Type: 'Bluefield'
    Application_Type: 'web'
    WorkspaceResourceId: '/subscriptions/84346150-8539-4bfa-9045-b1b8e7cbbb45/resourceGroups/${resourceGroup}/providers/Microsoft.OperationalInsights/workspaces/${appInsightsWorkspaceName}'
  }
  dependsOn: [
    appInsightsWorkspace
  ]
}

resource moostaPlatformFunctionStorageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: platformFunctionStorageAccountName
  location: location
  kind: 'StorageV2'
  tags: {}
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

resource appInsightsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-08-01' = {
  name: appInsightsWorkspaceName
  location: locationName
  properties: {}
}

//Moosta API Management
resource apimMoostaAPI 'Microsoft.ApiManagement/service@2019-01-01' = {
  name: 'moosta-apim${nameSuffix}'
  location: location
  tags: {}
  sku: {
    name: apimTier
  }
  identity:{
    type: 'SystemAssigned'
  } 
  properties: {
    publisherEmail: adminEmail
    publisherName: organizationName
    virtualNetworkType: 'None'
  }
  dependsOn: [
  ]
}

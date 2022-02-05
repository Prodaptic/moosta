param nameSuffix string
param location string
param locationName string
param skuStaticApp string
param skuCodeStaticApp string
param staticAppDomain string
param cosmosDefaultExperience string

resource staticWebApp 'Microsoft.Web/staticSites@2019-12-01-preview' = {
  name: 'moosta-web${nameSuffix}'
  location: location
  tags: {}
  properties: {}
  sku: {
    Tier: skuStaticApp
    Name: skuCodeStaticApp
  }
  resource staticWebAppDomains 'customDomains@2021-01-15' = {
    name: staticAppDomain
    properties: {
      validationMethod: 'cname-delegation'
    }    
  }
}

resource cosmosDb 'Microsoft.DocumentDb/databaseAccounts@2021-10-15-preview' = {
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
    dependsOn: []
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

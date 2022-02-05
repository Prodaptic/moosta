param nameSuffix string
param location string
param skuStaticApp string
param skuCodeStaticApp string
param staticAppDomain string

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

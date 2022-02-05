resource symbolicname 'Microsoft.Web/staticSites@2021-02-01' = {
  name: 'moosta-web-dev'
  location: 'eastus2'
  sku: {
    capabilities: [
      {
        name: 'free'
      }
    ]
  }
}

param projectName string
param location string = resourceGroup().location
param storageAccountName string
param containerRegistryName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2026-04-01' = {
  name: storageAccountName
  location: location
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
  }
}

resource certificateContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2026-04-01' = {
  name: '${storageAccount.name}/default/certificates'
  properties: { publicAccess: 'None' }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-11-01' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: { adminUserEnabled: false }
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2026-03-01' = {
  name: 'law-${projectName}'
  location: location
  properties: {
    retentionInDays: 30
    sku: { name: 'PerGB2018' }
  }
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2026-01-01' = {
  name: 'cae-${projectName}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

output storageAccountName string = storageAccount.name
output storageAccountUrl string = 'https://${storageAccount.name}.${environment().suffixes.storage}'
output containerRegistryLoginServer string = containerRegistry.properties.loginServer

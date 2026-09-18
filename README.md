# Certify AB

## How to run

```bash
RG="" # Name of your resource group

# Sign in to Azure
az login

# Check resources
az deployment group what-if \
  --resource-group $RG \
  --parameters infra/main.bicepparam

# Deploy
az deployment group create \
  --resource-group $RG \
  --parameters infra/main.bicepparam
```

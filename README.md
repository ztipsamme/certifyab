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

## Build Docker Locally

```bash
NEW_VERSION="" # Set new version. Ex: v1
PROJECT_NAME="certifyab"
Container_Registry_Name="acrcertifyabemm"

find . \( -name "*.csproj" -o -name "*.sln" \)
docker build --platform linux/amd64 -f Dockerfile -t $PROJECT_NAME . # Platform has to be specified

# Test run. Visit http://localhost:8080/swagger or http://localhost:8080/health
docker run --rm -p 8080:8080 $PROJECT_NAME


# Sign in to ACR
az acr login --name $Container_Registry_Name

# Tag & Push new container to ACR
docker tag $PROJECT_NAME $Container_Registry_Name.azurecr.io/$PROJECT_NAME:$NEW_VERSION
docker push $Container_Registry_Name.azurecr.io/$PROJECT_NAME:$NEW_VERSION

# Verify that the new version exists in ARC
az acr repository show-tags --name $Container_Registry_Name --repository $PROJECT_NAME --output table

```

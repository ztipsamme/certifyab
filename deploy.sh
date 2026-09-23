#!/bin/bash

set -e

if [ -z "$1" ]; then
  echo "Usage: ./deploy.sh <resource-group>"
  exit 1
fi

RG="$1"
NEW_VERSION="v1"
PROJECT_NAME="certifyab"
CONTAINER_REGISTRY_NAME="acrcertifyabemm"

ACR_IMAGE="$CONTAINER_REGISTRY_NAME.azurecr.io/$PROJECT_NAME:$NEW_VERSION" PLACEHOLDER_IMAGE="mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
echo

echo "Resource group: $RG"
echo "Project:        $PROJECT_NAME"
echo "Version:        $NEW_VERSION"
echo "ACR:            $CONTAINER_REGISTRY_NAME"
echo

# What-if
echo "Running Bicep what-if..."
echo

az deployment group what-if \
  --resource-group $RG \
  --parameters infra/main.bicepparam \
  --parameters deployContainerApp=false

echo


read -r -p "Continue with deployment? [y/N] " CONFIRM

if [ "$CONFIRM" != "y" ] && [ "$CONFIRM" != "Y" ]; then
  echo "Deployment cancelled."
  exit 0
fi


# Deploy Azure infrastructure EXCEPT Container App
echo
echo "Deploying Azure infrastructure..."

az deployment group create \
  --resource-group $RG \
  --parameters infra/main.bicepparam \
  --parameters deployContainerApp=false \
  --parameters containerImage="$PLACEHOLDER_IMAGE"


# Build Docker image
echo
echo "Building Docker image..."

docker build \
  --platform linux/amd64 \
  -f Dockerfile \
  -t $PROJECT_NAME .


# Login to ACR
echo
echo "Logging in to ACR..."

az acr login --name $CONTAINER_REGISTRY_NAME


# Tag image
echo
echo "Tagging Docker image..."

docker tag $PROJECT_NAME $CONTAINER_REGISTRY_NAME.azurecr.io/$PROJECT_NAME:$NEW_VERSION


# Push image
echo
echo "Pushing Docker image..."

docker push $CONTAINER_REGISTRY_NAME.azurecr.io/$PROJECT_NAME:$NEW_VERSION


# Create Container App
echo
echo "Creating Container App..."

az deployment group create \
 --resource-group "$RG" \
 --parameters infra/main.bicepparam \
 --parameters deployContainerApp=true \
 --parameters containerImage="$ACR_IMAGE"


echo echo "Checking Container App..."

az containerapp show \
 --name "ca-$PROJECT_NAME" \
 --resource-group "$RG" \
 --query "{status:properties.runningStatus, provisioning:properties.provisioningState, revision:properties.latestReadyRevisionName, fqdn:properties.latestRevisionFqdn}" \
 -o table

echo
echo "Deployment complete."
echo


echo "Testing /health..."

FQDN=$(az containerapp show \
  --name "ca-$PROJECT_NAME" \
  --resource-group $RG\
  --query "properties.latestRevisionFqdn" \
  -o tsv)

echo
echo "Container App URL:"
echo "https://$FQDN"

echo
echo "Health check:"
curl "https://$FQDN/health"
echo

echo
echo "Deployment complete."
# Certify AB

API for creating and verifying digital certificates. .NET 10 minimal API in Docker, deployed to Azure Container Apps with Blob Storage, Bicep and an Azure DevOps pipeline.

## Endpoints

| Endpoint                 | Description                                         | Auth    |
| ------------------------ | --------------------------------------------------- | ------- |
| `POST /certificates`     | Create a certificate, returns ID + verification URL | API key |
| `GET /certificates/{id}` | Get a certificate                                   | API key |
| `GET /certificates`      | List all certificates                               | API key |
| `GET /verify/{uuid}`     | Public verification                                 | None    |
| `GET /health`            | Health check                                        | None    |

The API key is sent in the `X-API-Key` header. Swagger UI is available at `/swagger`.

## Running locally

```bash
dotnet user-secrets set "ApiKey" "<any-key>"
dotnet run
```

In Development, in-memory mock data is used, so no Azure connection is needed.

## Infrastructure and deployment

The infrastructure is defined in `Infra/main.bicep`. The environments differ in their parameter files:

|                    | dev                   | prod                   |
| ------------------ | --------------------- | ---------------------- |
| File               | `main.dev.bicepparam` | `main.prod.bicepparam` |
| Replicas (min/max) | 1 / 2                 | 2 / 3                  |

See what a deployment would change before running it:

```bash
RG="<your-resource-group>"
API_KEY="<your-api-key>"

az deployment group what-if \
  -g $RG \
  -p Infra/main.prod.bicepparam \
  -p apiKey="$API_KEY"
```

`apiKey` must be a non-empty value, otherwise Azure rejects the deployment (`ContainerAppSecretInvalid`).

Regular deployments are done through the pipeline (see below), not manually.

## CI/CD

`azure-pipelines.yml` is triggered on push to `main`:
Build & test → Azure connection test → Docker build & push to ACR → Bicep deploy to Container Apps.

The API key comes from the pipeline variable `API_KEY` (secret).

## Other documentation

- `ARCHITECTURE.md` – technical decisions, security and cost
- `RAPPORT.md` – customer report

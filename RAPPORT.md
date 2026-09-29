# Teknisk leveransrapport

**Uppdrag:** CertifyAB

**Konsultteam:** Emma Spitz

**Datum:** 2027.10.01

**Version:** 1.0

---

## Sammanfattning

En molnbaserad plattform för digitala certifikat med publik verifiering. Kunden kan nu skapa certifikat via ett säkert API där varje certifikat får en unik, offentligt verifierbar länk.Mottagaren eller vem som helst kan besöka länken och omedelbart se att certifikatet är äkta utan att behöva kontakta Certify AB manuellt. Plattformen är driftsatt i Microsoft Azure, körs i container och är byggd för att skala när fler kunder tillkommer.

---

## Vad som levereras

### Inkluderat i leveransen

| Komponent                   | Teknisk lösning                                                     | Status       |
| --------------------------- | ------------------------------------------------------------------- | ------------ |
| REST API                    | .NET 10 WebAPI, 5 endpoints                                         | ✅ Levererat |
| Certifikatdata              | JSON istället för genererad PDF (se motivering nedan)               | ✅ Levererat |
| Containerisering            | Docker, multi-stage build                                           | ✅ Levererat |
| Driftsättning               | Azure Container Apps                                                | ✅ Levererat |
| Autoskalning                | HTTP-baserad scale rule (>10 samtidiga requests), belastningstestad | ✅ Levererat |
| Bildarkiv                   | Azure Container Registry                                            | ✅ Levererat |
| Fillagring                  | Azure Blob Storage (ett JSON-dokument per certifikat)               | ✅ Levererat |
| Infrastruktur som kod       | Bicep, parametriserad för dev/prod                                  | ✅ Levererat |
| Övervakning                 | Application Insights kopplat, alert vid felfrekvens > 5 %           | ✅ Levererat |
| Automatiserad driftsättning | Azure DevOps YAML-pipeline                                          | ✅ Levererat |
| API-dokumentation           | Swagger UI (/swagger)                                               | ✅ Levererat |
| Åtkomstskydd                | API-nyckel på admin-endpoints, Managed Identity mot Azure-resurser  | ✅ Levererat |

### Utanför leveransens scope

Följande punkter identifierades under uppdraget men ingår inte i denna leverans. De rekommenderas som nästa steg.

| Punkt                                              | Motivering                                                                                                                                                      |
| -------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| PDF-generering av certifikat                       | Kräver externa bibliotek i .NET och bedömdes inte nödvändigt för att bevisa lösningen. Certifikatdata returneras som JSON, vilket räcker för verifieringsflödet |
| Produktionsgodkänd autentisering för slutanvändare | Nuvarande skydd är en delad API-nyckel för admin-endpoints. Kräver Entra ID- eller nyckel-per-kund-integration inför kundlansering                              |
| Rate limiting på den publika `/verify`-endpointen  | Inte implementerat ännu. Relevant risk givet att certifikatlänkar är tänkta att delas publikt                                                                   |

---

## Arkitektur

### Systemdiagram

```
[Klient / Mottagare av certifikat]
        │
        ▼
[Azure Container Apps — REST API]
        │
        ├──► [Azure Blob Storage — ett JSON-dokument per certifikat]
        │
        └──► [Azure Application Insights — loggning & alerting]

[Publik verifiering]
        │
        ▼
GET /verify/{uuid} ──► [Azure Container Apps API] ──► [Azure Blob Storage]
```

### Motiverade arkitekturval

**Varför Azure Container Apps och inte AKS?**
Container Apps ger serverless drift på Consumption-planen där vi slipper hantera noder, kluster och patchning, och betalar per faktisk förbrukning i stället för för ett helt kluster som står och går. Det passar en enskild API-tjänst med låg och oförutsägbar trafik i en tidig SaaS-fas bättre än AKS, som hade inneburit betydligt mer operativ komplexitet för ett problem som inte kräver mikrotjänster eller avancerad nätverkskontroll.

**Varför Bicep och inte manuell konfiguration?**
Hela infrastrukturen d.v.s lagring, register, Container Apps-miljö, appen själv och rolltilldelningarna ligger som kod i git där den är spårbar och granskningsbar. Med az deployment group what-if kan varje ändring förhandsgranskas innan den appliceras i den delade resursgruppen och mallen är idempotent. Vilket betyder att samma deployment kan köras flera gånger utan att skapa dubbletter.

**Varför Azure Blob Storage för fillagring?**
Blob Storage är billigt för den här typen av data (ett litet JSON-dokument per certifikat) och skalar utan att vi behöver hantera en databasserver. Det integreras direkt med Managed Identity så att ingen lagringsnyckel behöver delas eller roteras manuellt.

---

## Säkerhetsarkitektur

### Identitet och åtkomst

| Resurs                                                                                | Åtkomstkontroll                                           |
| ------------------------------------------------------------------------------------- | --------------------------------------------------------- |
| Azure Container Apps                                                                  | Managed Identity — ingen hårdkodad nyckel                 |
| Azure Blob Storage                                                                    | RBAC via Managed Identity (Storage Blob Data Contributor) |
| Admin-endpoints (`POST /certificates`, `GET /certificates/{id}`, `GET /certificates`) | Statisk API-nyckel i header (X-API-Key)                   |
| `GET /verify/{uuid`                                                                   | Avsiktligt publik — ingen autentisering                   |
| Pipeline-credentials                                                                  | Azure DevOps service connection + pipeline-secrets        |

### Hemlighetshantering

Inga credentials lagras i källkod eller git-historik. `ApiKey` är `@secure()`-parametrar i Bicep och hanteras via Azure DevOps pipeline-secrets och refereras som miljövariabler i Container App.

### Kvarvarande risker

| Risk                                                                               | Sannolikhet | Åtgärd                                                                          |
| ---------------------------------------------------------------------------------- | ----------- | ------------------------------------------------------------------------------- |
| `/verify/{uuid}` saknar rate limiting och kan träffas hårt om en länk delas viralt | Medel–hög   | Lägg till API Management eller throttling-regler på endpointen                  |
| En delad API-nyckel för alla admin-anrop, ingen nyckel per kund                    | Medel       | Implementera Entra ID Easy Auth eller nyckel per kund innan offentlig lansering |

---

## Kostnadskalkyl

### Månadskostnad vid lansering

### Skalningspunkt

---

## Rekommendationer inför produktionssättning

1. **Autentisering för slutanvändare** — Implementera Entra ID Easy Auth eller API-nyckel per kund innan offentlig lansering.
2. **Kostnadslarm** — Sätt budget-alert i Azure Cost Management vid 80 % av månadsbudget.
3. **Rate limiting på `/verify`** — Skydda den publika endpointen mot att bli nedtyngd vid en trafiktopp.

---

## Överlämning

| Leverabel           | Plats                                                                                  |
| ------------------- | -------------------------------------------------------------------------------------- |
| Källkod             | https://github.com/ztipsamme/certifyab                                                 |
| Bicep-mallar        | `Infra/` i repot                                                                       |
| Pipeline-definition | `azure-pipelines.yml` i repots rot                                                     |
| API-dokumentation   | https://ca-certifyab.salmonflower-d77996d7.swedencentral.azurecontainerapps.io/swagger |
| Denna rapport       | `RAPPORT.md` i repots rot                                                              |

---

_Rapporten är upprättad av konsultteamet som ett avslutande leveransdokument. Frågor hänvisas till teamet via Azure DevOps._

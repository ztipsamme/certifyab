# Teknisk leveransrapport

**Uppdrag:** CertifyAB

**Konsultteam:** Emma Spitz

**Datum:** 2027.10.01

**Version:** 1.0

---

## Sammanfattning

En molnbaserad plattform för digitala certifikat med publik verifiering. Kunden kan nu skapa certifikat via ett säkert API där varje certifikat får en unik, offentligt verifierbar länk. Mottagaren eller vem som helst kan besöka länken och omedelbart se att certifikatet är äkta utan att behöva kontakta Certify AB manuellt. Plattformen är driftsatt i Microsoft Azure, körs i container och är byggd för att skala när fler kunder tillkommer.

---

## Vad som levereras

### Inkluderat i leveransen

| Komponent                   | Teknisk lösning                                                                | Status       |
| --------------------------- | ------------------------------------------------------------------------------ | ------------ |
| REST API                    | .NET 10 WebAPI, 5 endpoints                                                    | ✅ Levererat |
| Certifikatdata              | JSON istället för genererad PDF (se motivering nedan)                          | ✅ Levererat |
| Containerisering            | Docker, multi-stage build                                                      | ✅ Levererat |
| Driftsättning               | Azure Container Apps                                                           | ✅ Levererat |
| Autoskalning                | HTTP-baserad scale rule (10 samtidiga requests), belastningstestad             | ✅ Levererat |
| Bildarkiv                   | Azure Container Registry                                                       | ✅ Levererat |
| Fillagring                  | Azure Blob Storage (ett JSON-dokument per certifikat)                          | ✅ Levererat |
| Infrastruktur som kod       | Bicep, parametriserad för dev/prod                                             | ✅ Levererat |
| Övervakning                 | Application Insights kopplat, alert vid fler än 5 misslyckade anrop per 15 min | ✅ Levererat |
| Automatiserad driftsättning | Azure DevOps YAML-pipeline                                                     | ✅ Levererat |
| API-dokumentation           | Swagger UI (/swagger)                                                          | ✅ Levererat |
| Åtkomstskydd                | API-nyckel på admin-endpoints, Managed Identity mot Azure-resurser             | ✅ Levererat |

### Utanför leveransens scope

Följande punkter identifierades under uppdraget men ingår inte i denna leverans. De rekommenderas som nästa steg.

| Punkt                                              | Motivering                                                                                                                                                            |
| -------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| PDF-generering av certifikat                       | Kräver externa bibliotek i .NET och bedömdes inte nödvändigt för att bevisa lösningen. Certifikatdata returneras som JSON, vilket räcker för verifieringsflödet       |
| Produktionsgodkänd autentisering för slutanvändare | Nuvarande skydd är en delad API-nyckel för admin-endpoints. Kräver Entra ID- eller nyckel-per-kund-integration inför kundlansering                                    |
| Rate limiting på den publika `/verify`-endpointen  | Inte implementerat ännu. Relevant risk givet att certifikatlänkar är tänkta att delas publikt                                                                         |
| Larmnotifiering                                    | Alerten på felfrekvens är aktiv men saknar mottagare, eftersom kontopolicyn i labbmiljön inte tillåter action groups. Bör kopplas till e-post eller sms i produktion. |

---

## Arkitektur

### Systemdiagram

```
[Klient / Mottagare av certifikat]
        │
        ▼
[Azure Container Apps — REST API] ◄── [Azure Container Registry — container-image]
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
Vi deployar till Container Apps eftersom det ger serverless drift där Microsoft sköter kluster, noder och skalning, vilket passar en enskild API-tjänst med låg och oförutsägbar trafik bättre än AKS. Begränsningen är att vi saknar direkt åtkomst till Kubernetes-API:et. Container Apps kan skalas ned till noll, men vi kör alltid minst 2 replicas så att tjänsten klarar att en instans startas om, och det är kostnaden för tillförlitlighet. AKS skulle passa Certify längre fram när systemet består av flera samverkande tjänster eller får specialkrav, men då tar vi också över ansvaret för nodpooler och uppgraderingar (och i labben är AKS dessutom blockerat av kontopolicyn).

**Varför Bicep och inte manuell konfiguration?**
Hela infrastrukturen (lagring, register, Container Apps-miljö, övervakning och appen själv) ligger som kod i git där den är spårbar och granskningsbar. Med az deployment group what-if kan varje ändring förhandsgranskas innan den appliceras i den delade resursgruppen och mallen är idempotent. Vilket betyder att samma deployment kan köras flera gånger utan att skapa dubbletter.

**Varför Azure Blob Storage för fillagring?**
Blob Storage är billigt för den här typen av data (ett litet JSON-dokument per certifikat) och skalar utan att vi behöver hantera en databasserver. Det integreras direkt med Managed Identity så att ingen lagringsnyckel behöver delas eller roteras manuellt.

---

## Säkerhetsarkitektur

### Identitet och åtkomst

| Resurs                                                                                | Åtkomstkontroll                                             |
| ------------------------------------------------------------------------------------- | ----------------------------------------------------------- |
| Azure Container Apps                                                                  | Managed Identity — ingen hårdkodad nyckel                   |
| Azure Container Registry                                                              | RBAC via Managed Identity (AcrPull) — ingen admin-användare |
| Azure Blob Storage                                                                    | RBAC via Managed Identity (Storage Blob Data Contributor)   |
| Admin-endpoints (`POST /certificates`, `GET /certificates/{id}`, `GET /certificates`) | Statisk API-nyckel i header (X-API-Key)                     |
| `GET /verify/{uuid}` och `GET /health`                                                | Avsiktligt publik — ingen autentisering                     |
| Pipeline-credentials                                                                  | Azure DevOps service connection + pipeline-secrets          |

### Hemlighetshantering

Inga credentials lagras i källkod eller git-historik. `ApiKey` är `@secure()`-parametrar i Bicep och hanteras via Azure DevOps pipeline-secrets och refereras som miljövariabler i Container App.

### Kvarvarande risker

| Risk                                                                               | Sannolikhet | Åtgärd                                                                          |
| ---------------------------------------------------------------------------------- | ----------- | ------------------------------------------------------------------------------- |
| `/verify/{uuid}` saknar rate limiting och kan träffas hårt om en länk delas viralt | Medel–hög   | Lägg till ASP.NET Core rate limiting och caching på endpointen                  |
| En delad API-nyckel för alla admin-anrop, ingen nyckel per kund                    | Medel       | Implementera Entra ID Easy Auth eller nyckel per kund innan offentlig lansering |
| Felalerten har ingen mottagare, så ingen notifieras när den utlöses                | Medel       | Koppla en action group (e-post/sms) i en miljö där policyn tillåter det         |

---

## Kostnadskalkyl

### Månadskostnad vid lansering

| Resurs                                                   | SKU                 | Uppskattad kr/mån | 3x kundbasen kr/mån | 4x kundbasen kr/mån |
| -------------------------------------------------------- | ------------------- | ----------------- | ------------------- | ------------------- |
| Container Apps compute (2 idle-replikor, 1 vCPU / 2 GiB) | Consumption         | ~450 kr           | ~450 kr             | ~450 kr             |
| Azure Container Registry (ACR)                           | Basic               | ~48 kr            | ~48 kr              | ~48 kr              |
| Azure Blob Storage                                       | Standard LRS        | ~5 kr             | ~16 kr              | ~21 kr              |
| Azure Monitor (Application Insights + Logging & alerts)  | Pay-as-you-go       | ~0 kr             | ~0 kr               | ~0 kr               |
| Azure DevOps                                             | Basic (5 användare) | ~0 kr             | ~0 kr               | ~0 kr               |
| **Totalt**                                               |                     | **~503 kr/mån**   | **~514 kr/mån**     | **~519 kr/mån**     |

> Beräknat med 40 kunder, ~200 certifikat/mån vardera = totalt 8 000 certifikat/mån.
> Betalningsmetod Pay-as-you-go. Källa: Azure Pricing Calculator.
> 40 kunder x 99 kr = 3 960 kr/ mån. Kostnaden är ca 13 kr per kund mot 99 kr i intäkt. Vid 3x kundbas (120 kunder, 11 880 kr/mån) är kostnaden ca 4 kr per kund.

### Skalningspunkt

Container Apps är dyrast eftersom vi alltid har igång två replicas för att tjänsten ska vara pålitlig. Lagringen är billigast, filerna som lagras är väldigt små.

Container Apps inkluderar 2 miljoner requests i månaden, och blob-transaktionerna kostar någon krona. Vid ökad kundbas är inte trafiken problemet, utan att vi betalar för de replicas som körs. Med `maxReplicas: 3` och 10 samtidiga anrop per replica skalar tjänsten till som mest ~30 samtidiga anrop, och därefter köas anropen. Fyrdubblas trafiken är flaskhalsen antalet samtidiga anrop och replicas vi tillåter, det är lätt att höja.

Skulle appen få 10 000 anrop på en dag är det i snitt ca 0,12 anrop per sekund, så merkostnaden är ~0 kr. Verifieringen slår upp blobben direkt på namnet i stället för att skanna alla, så latensen växer inte med antalet certifikat. HTTP-skalningsregeln hanterar en topp, och maxReplicas sätter ett tak för kostnaden. Rate limiting och caching på /verify skulle vara lämpliga skyddar mot missbruk.

---

## Rekommendationer inför produktionssättning

1. **Autentisering för slutanvändare** — Implementera Entra ID Easy Auth eller API-nyckel per kund innan offentlig lansering.
2. **Kostnadslarm** — Sätt budget-alert i Azure Cost Management vid 80 % av månadsbudget.
3. **Rate limiting på `/verify`** — Skydda den publika endpointen mot att bli nedtyngd vid en trafiktopp.
4. **Larmnotifiering** — Koppla felalerten till en action group så att någon faktiskt får ett meddelande när den utlöses.
5. **Egen Domän** – Sätta deras egna domän som adress.

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

# Teknisk leveransrapport

**Uppdrag:** CertifyAB
**Konsultteam:** Emma Spitz
**Datum:** 2027.10.01
**Version:** 1.0

---

## Sammanfattning

[2–4 meningar. Vad har ni byggt, för vem, och vad kan kunden göra med det nu som de inte kunde innan? Riktar sig till en icke-teknisk läsare — inga förkortningar utan förklaring.]

> **Exempel:**
> Vi har levererat en fungerande molnbaserad plattform för automatisk fakturaigenkänning. Kunden kan nu ladda upp fakturor via ett säkert API och omedelbart få tillbaka strukturerad data — leverantör, belopp och datum — utan manuell handpåläggning. Plattformen är driftsatt i Microsoft Azure och skalas automatiskt vid hög belastning.

---

## Vad som levereras

### Inkluderat i leveransen

| Komponent                   | Teknisk lösning             | Status       |
| --------------------------- | --------------------------- | ------------ |
| REST API                    | .NET 10 WebAPI, 5 endpoints | ✅ Levererat |
| Containerisering            | Docker, multi-stage build   | ✅ Levererat |
| Driftsättning               | Azure Container Apps        | ✅ Levererat |
| Bildarkiv                   | Azure Container Registry    | ✅ Levererat |
| Fillagring                  | Azure Blob Storage          | ✅ Levererat |
| Infrastruktur som kod       | Bicep                       | ✅ Levererat |
| Automatiserad driftsättning | Azure DevOps YAML-pipeline  | ✅ Levererat |
| API-dokumentation           | Swagger UI (/swagger)       | ✅ Levererat |

### Utanför leveransens scope

Följande punkter identifierades under uppdraget men ingår inte i denna leverans. De rekommenderas som nästa steg.

| Punkt                                                      | Motivering                                                                                             |
| ---------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| [T.ex. Produktionsgodkänd autentisering för slutanvändare] | [T.ex. Kräver Entra ID-integration och definierade användarroller — rekommenderas inför kundlansering] |
| [T.ex. Disaster recovery-plan]                             | [T.ex. Utanför tidsscopeför denna sprint — bör definieras innan produktionssättning]                   |
| [Lägg till egna]                                           |                                                                                                        |

---

## Arkitektur

### Systemdiagram

```
[Klient / Användare]
        │
        ▼
[Azure Container Apps — REST API]
        │
        ├──► [Azure Blob Storage — fillagring]
        │
        └──► [Azure Document Intelligence — AI-analys]
                        │
                        ▼
              [Strukturerat svar tillbaka till API]
```

> Ersätt med ett faktiskt Mermaid-diagram eller bild om ni har ett.

### Motiverade arkitekturval

**Varför Azure Container Apps och inte AKS?**
[Skriv 3–5 meningar. Förklara beslutet som ett aktivt val, inte som en begränsning. Exempel: Container Apps hanterar skalning och infrastruktur automatiskt, vilket minskar driftskostnaden och komplexiteten för ett tidigt SaaS-bolag. AKS ger mer kontroll men kräver ett dedikerat driftteam — ett omotiverat overhead i den här fasen.]

**Varför Bicep och inte manuell konfiguration?**
[Skriv 2–4 meningar om reproducerbarhet, spårbarhet och idempotens.]

**Varför Azure Blob Storage för fillagring?**
[Skriv 2–3 meningar om kostnad, skalbarhet och enkel integration med Managed Identity.]

---

## Säkerhetsarkitektur

### Identitet och åtkomst

| Resurs                      | Åtkomstkontroll                                           |
| --------------------------- | --------------------------------------------------------- |
| Azure Container Apps        | Managed Identity — ingen hårdkodad nyckel                 |
| Azure Blob Storage          | RBAC via Managed Identity (Storage Blob Data Contributor) |
| Azure Document Intelligence | Managed Identity — endpoint från pipeline-secret          |
| Pipeline-credentials        | Azure DevOps service connection — roteras vid behov       |

### Hemlighetshantering

Inga credentials lagras i källkod eller git-historik. Alla hemligheter hanteras via Azure DevOps pipeline-secrets och refereras som miljövariabler i Container App.

### Kvarvarande risker

| Risk                                               | Sannolikhet | Åtgärd                                           |
| -------------------------------------------------- | ----------- | ------------------------------------------------ |
| [T.ex. API saknar autentisering för slutanvändare] | Hög         | Implementera Entra ID Easy Auth i nästa sprint   |
| [T.ex. Ingen rate limiting på POST-endpoint]       | Medel       | Lägg till API Management eller throttling-regler |

---

## Kostnadskalkyl

### Månadskostnad vid lansering

| Resurs                      | SKU                 | Uppskattad kostnad/mån |
| --------------------------- | ------------------- | ---------------------- |
| Container Apps Environment  | Consumption         | [X kr]                 |
| Container App               | Consumption         | [X kr]                 |
| Azure Container Registry    | Basic               | [X kr]                 |
| Azure Blob Storage          | Standard LRS        | [X kr]                 |
| Azure Document Intelligence | S0                  | [X kr]                 |
| Azure DevOps                | Basic (5 användare) | Gratis                 |
| **Totalt**                  |                     | **[Summa] kr/mån**     |

> Beräknat med [antal] kunder och [antal] analyser per månad. Källa: Azure Pricing Calculator.

### Skalningspunkt

[Beskriv var flaskhalsen uppstår om trafiken ökar. Vilken resurs når sin gräns först? Vad händer då, och vad kostar det att skala upp?]

---

## Rekommendationer inför produktionssättning

1. **Autentisering för slutanvändare** — Implementera Entra ID Easy Auth eller API-nyckel per kund innan offentlig lansering.
2. **Monitoring** — Koppla Application Insights och sätt upp alert vid fel > 5 % eller svarstid > 2 sek.
3. **Parametriserad infrastruktur** — Separera dev- och prod-miljö med Bicep-parameterfiler.
4. **Kostnadslarm** — Sätt budget-alert i Azure Cost Management vid 80 % av månadsbudget.
5. [Lägg till egna utifrån ert scenario]

---

## Överlämning

| Leverabel           | Plats                              |
| ------------------- | ---------------------------------- |
| Källkod             | [Länk till repo]                   |
| Bicep-mallar        | `/infra/` i repot                  |
| Pipeline-definition | `azure-pipelines.yml` i repots rot |
| API-dokumentation   | `https://[er-app-url]/swagger`     |
| Denna rapport       | `RAPPORT.md` i repots rot          |

---

_Rapporten är upprättad av konsultteamet som ett avslutande leveransdokument. Frågor hänvisas till teamet via Azure DevOps._

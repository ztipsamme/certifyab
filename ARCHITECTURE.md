# Architecture

## Svaren ska vara skrivna av teamet med egna ord. Varje svar: 3–8 meningar.

### 1. Container Apps — Varför deployer ni till Container Apps och inte AKS? Vilka begränsningar innebär det, och när skulle ni välja AKS istället?

#### Container Apps

- Passar mikrotjänster, köbaserade jobb, enklare API:er.
- Lågt driftansvar: Serverless. Microsoft sköter kluster, noder och skalningslogik. Slipper skriva Kubernetes-manifest.
- Ingen direkt åtkomst till det underliggande Kubernetes-API:et.

- Kan skalas ned till noll repliker, vilket innebär att inaktiva appar inte kostar något i beräkningskraft. Skalar via KEDA.
- Har inbyggt stöd för mikrotjänstkommunikation via Dapr och trafiksökning/ revisioner.

#### AKS

- Passar storskaliga, komplexa system medspecialkrav.
- Högt driftansvar: Eget ansvar för nodpooler, uppgraderingar och klusterkonfiguration.
- Fullständig tillgång till Kubernetes API, vilket stödjer Custom Resource Definitions (CRDs), DaemonSets och avancerade nätverk.
- Kräver att minst en nod är igång och debiteras löpande (förutom i vissa automatiserade konfigurationer).

---

### 2. CI/CD — Beskriv ert pipeline-flöde steg för steg, från git push till live app. Vad händer om bygget misslyckas?

---

### IaC — Varför definierar ni infrastrukturen i Bicep istället för att klicka i portalen? Vad menas med idempotens och varför spelar det roll?

Infrasturkturen defineras i Bicep för att den enkelt ska kunna repliceras. Idempotens betyder att en handling eller operation ger samma resultat oavsett hur många gånger den upprepas. När infrasturkturen defineras i Bicep blir den idempotent.

---

### 3. Säkerhet — Hur hanterar ni hemligheter och credentials? Vad händer om en nyckel råkar hamna i git-historiken?

Hemligheter i git-historiken är sökbara och en säkerhetsrisk.

---

### 4. Ekonomi — Se ert scenarios ekonomifrågor. Skriv faktiska siffror med motivering.

**Generellt:**

- Uppskattad månadskostnad vid lansering (antal kunder × last enligt scenariot)
- Uppskattad kostnad om kundbasen tredubblas
- Vilken resurs är dyrast och varför?
- Var är flaskhalsen om trafiken fyradubblas?

Använd Azures priskalkylator. Skriv faktiska siffror — inte "det beror på".

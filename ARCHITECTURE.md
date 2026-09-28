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

- Passar storskaliga, komplexa system med specialkrav.
- Högt driftansvar: Eget ansvar för nodpooler, uppgraderingar och klusterkonfiguration.
- Fullständig tillgång till Kubernetes API, vilket stödjer Custom Resource Definitions (CRDs), DaemonSets och avancerade nätverk.
- Kräver att minst en nod är igång och debiteras löpande (förutom i vissa automatiserade konfigurationer).

---

### 2. CI/CD — Beskriv ert pipeline-flöde steg för steg, från git push till live app. Vad händer om bygget misslyckas?

Pipelinen triggas vid push till main-branch:en. Flödet är uppdelat i fyra stages där alla använder `dependsOn` och `condition: succeeded()`. Det innebär att en stage bara startar om föregående stage har lyckats. Om ett stage misslyckas stoppas pipelinen där och inget senare stage körs. Den nuvarande live-versionen påverkas inte eftersom den nya versionen inte har deployats färdigt.

1. Build – Installerar .NET SDK, kör dotnet restore, bygger applikationen med dotnet build och kör testerna med dotnet test.
2. AzureTest – Testar anslutningen till Azure genom vår Azure DevOps Service Connection. `az account show` kontrollerar att den kan autentisera mot rätt Azure-prenumeration.
3. Docker – Loggar in mot ACR. Sedan byggs en Docker-image av appen som taggas med `$Build.BuildId` så att varje pipeline-körning får en unik version. Imagen pushas sedan till ACR. Container Appen konfigureras så att den nya Docker-imagen används och appen går sedan att nå via sin publika Azure Container Apps-url.
4. Infrastructure – Bicep deployas till resource groupen och får Docker-imagens url samt API-nyckeln som parametrar. Bicep konfigurerar Container Appen att använda den nya imagen. När deploymenten lyckas körs den nya versionen i Azure Container Apps och blir tillgänglig via den publika URL:en.

---

### IaC — Varför definierar ni infrastrukturen i Bicep istället för att klicka i portalen? Vad menas med idempotens och varför spelar det roll?

Genom att definiera infrastrukturen i Bicep istället för att konfigurera resurser manuellt i Azure Portal blir infrastrukturen reproducerbar och versionshanterad tillsammans med koden. Det gör att samma önskade infrastruktur kan deployas flera gånger utan att skapa duplicerade resurser.

Idempotens betyder att en handling eller operation ger samma resultat oavsett hur många gånger den upprepas. Det är viktigt eftersom vi kan ändra infrastrukturen genom kod, granska ändringarna och återskapa miljön på ett mer förutsägbart sätt.

---

### 3. Säkerhet — Hur hanterar ni hemligheter och credentials? Vad händer om en nyckel råkar hamna i git-historiken?

Hemligheter sparas inte i koden utan hanteras i secrets i Azure DevOps och skickas till deployment som `environment variables`. API-nyckeln är en i pipeline-secret och exponeras inte i `azure-pipelines.yml`. Åtkomst till Azure-resurser sker även genom Azure DevOps Service Connection och Managed Identity där det är möjligt istället för hårdkodade credentials. Om en hemlighet skulle råka hamna i git-historiken är den blottad och måste bytas ut eller återkallas direkt. Att bara ta bort nyckeln från den senaste commit:en räcker inte eftersom den fortfarande kan finnas kvar i git-historiken.

---

### 4. Ekonomi — Se ert scenarios ekonomifrågor. Skriv faktiska siffror med motivering.

**Generellt:**

- Uppskattad månadskostnad vid lansering (antal kunder × last enligt scenariot)
- Uppskattad kostnad om kundbasen tredubblas
- Vilken resurs är dyrast och varför?
- Var är flaskhalsen om trafiken fyradubblas?

Använd Azures priskalkylator. Skriv faktiska siffror — inte "det beror på".

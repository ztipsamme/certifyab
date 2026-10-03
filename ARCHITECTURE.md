# Architecture

### 1. Container Apps — Varför deployer ni till Container Apps och inte AKS? Vilka begränsningar innebär det, och när skulle ni välja AKS istället?

Vi deployar till Container Apps eftersom det ger serverless drift där Microsoft sköter kluster, noder och skalning, vilket passar en enskild API-tjänst med låg och oförutsägbar trafik bättre än AKS. Begränsningen är att vi saknar direkt åtkomst till Kubernetes-API:et. Container Apps kan skalas ned till noll, men vi kör alltid minst 2 replicas så att tjänsten klarar att en instans startas om, och det är kostnaden för tillförlitlighet. AKS skulle passa Certify längre fram när systemet består av flera samverkande tjänster eller får specialkrav, men då tar vi också över ansvaret för nodpooler och uppgraderingar (och i labben är AKS dessutom blockerat av kontopolicyn).

---

### 2. CI/CD — Beskriv ert pipeline-flöde steg för steg, från git push till live app. Vad händer om bygget misslyckas?

Pipelinen triggas vid push till main och körs även på pull requests mot main, men då pushas och deployas ingenting. Build-stagen installerar .NET 10 och kör `dotnet restore`, `dotnet build` och `dotnet test`. Docker-stagen startar först när Build har lyckats, bygger en image som taggas med `$(Build.BuildId)` så att varje körning får en unik version, och pushar den till ACR bara om körningen sker på main. Infrastructure-stagen kräver att Docker har lyckats och att körningen sker på main, och kör då `az deployment group create` med vår Bicep-mall, där image-taggen och API-nyckeln (en pipeline-secret) skickas in som parametrar. Bicep uppdaterar Container Appen till den nya imagen, och när deploymenten är klar körs den nya versionen på den publika URL:en. Om bygget eller testerna misslyckas stoppas pipelinen direkt och inget senare stage körs. Eftersom bara Infrastructure-stagen ändrar den körande appen förblir den nuvarande live-versionen orörd, och en pull request kan aldrig nå varken ACR eller Azure-miljön.

---

### 3. IaC — Varför definierar ni infrastrukturen i Bicep istället för att klicka i portalen? Vad menas med idempotens och varför spelar det roll?

Genom att definiera infrastrukturen i Bicep istället för att konfigurera resurser manuellt i Azure Portal blir infrastrukturen reproducerbar och versionshanterad tillsammans med koden. Det gör att samma önskade infrastruktur kan deployas flera gånger utan att skapa duplicerade resurser.

Idempotens betyder att en handling eller operation ger samma resultat oavsett hur många gånger den upprepas. Det är viktigt eftersom vi kan ändra infrastrukturen genom kod, granska ändringarna och återskapa miljön på ett mer förutsägbart sätt. Det är det som gör en omkörning efter ett misslyckat bygge ofarlig.

---

### 4. Säkerhet — Hur hanterar ni hemligheter och credentials? Vad händer om en nyckel råkar hamna i git-historiken?

Hemligheter sparas inte i koden utan hanteras i secrets i Azure DevOps och skickas till deployment som `environment variables`. API-nyckeln är en pipeline-secret och exponeras inte i `azure-pipelines.yml`. Åtkomst till Azure-resurser sker även genom Azure DevOps Service Connection och Managed Identity där det är möjligt istället för hårdkodade credentials. Om en hemlighet skulle råka hamna i git-historiken är den blottad och måste bytas ut eller återkallas direkt. Att bara ta bort nyckeln från den senaste commit:en räcker inte eftersom den fortfarande kan finnas kvar i git-historiken.

---

### 5. Ekonomi — Se ert scenarios ekonomifrågor. Skriv faktiska siffror med motivering.

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

Dyrast är Container Apps eftersom vi alltid har igång två replicas för att tjänsten ska vara pålitlig. Lagringen är billigast, filerna som lagras är väldigt små.

Container Apps inkluderar 2 miljoner requests i månaden, och blob-transaktionerna kostar någon krona. Vid ökad kundbas är inte trafiken problemet, utan att vi betalar för de replicas som körs. Med `maxReplicas: 3` och 10 samtidiga anrop per replica skalar tjänsten till som mest ~30 samtidiga anrop, och därefter köas anropen. Fyrdubblas trafiken är flaskhalsen antalet samtidiga anrop och replicas vi tillåter, det är lätt att höja.

Skulle appen få 10 000 anrop på en dag är det i snitt ca 0,12 anrop per sekund, så merkostnaden är ~0 kr. Verifieringen slår upp blobben direkt på namnet i stället för att skanna alla, så latensen växer inte med antalet certifikat. HTTP-skalningsregeln hanterar en topp, och maxReplicas sätter ett tak för kostnaden. Rate limiting och caching på /verify skulle vara lämpliga skyddar mot missbruk.

Betalningsalternativet kan också sänka kostnaderna. När belastningen är känd rekommenderas att välja en Savings Plan istället för Pay-as-you-go för ett reducerat pris på t.ex. Container Apps.

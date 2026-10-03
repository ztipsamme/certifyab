# Individuell reflektion — Teamlabb

Längd: 400–700 ord totalt. Inga poänglösa inledningar — börja direkt med svaret.

---

## Frågorna

### 1. Din roll i teamet

Vad bidrog du konkret med under sprinten? Nämn specifika filer, kommandon, beslut eller debugg-sessioner du ägde.

Undvik: "Jag hjälpte till med allt." Det är inte ett svar.

**Svar:**
Jag ägde hela leveransen från start till slut. Detta innefattar bl.a. uppslaget på blobnamnet i `CertificateRepo.cs`, `main.bicep` med dev/prod-parameterfilerna, de tre pipeline-stagesen, skalningsregeln med hey-testet, och alerten. Vid behov användes AI som bollplank och stöd.

När jag satte upp endpointsen valde jag att sätta id och uuid som olika Guid för att det interna id:t ska vara skyddat. Jag beslutade också att kräva API-nyckel för alla endpoints (inte bara `GET /certificates`) förutom `/health` och `/verify/uuid` för att skydda eventuella känslig data.

---

### 2. Det svåraste momentet

Vad fastnade du på längst? Beskriv problemet, vad du försökte, och vad som till slut löste det.

Om du inte fastnade på något: beskriv istället det beslut som var svårast att fatta och varför.

**Svar:**

I mitt försök att sätta upp en Custom Alert vid >5% felkvot blev set-upen blockerad av skolans policy. Minns ej om det var what-if, stop i pipelinen eller bägge som avslöjade att policyn blockerar loggbaserade larmregler (Log Query Alerts) samt globalt placerade resurser. Efter att ha testat att byta location och larmtyp, samt rådfråga AI om andra potentiella lösningar landade jag i att istället implementera metrik-baserade larm (Metric Alerts) i samma EU-region som övrig infrastruktur. Lösningen larmar vid >5 misslyckade anrop inom 15 min istället för procentuell felkvot. En medveten trade-off för att hålla sig inom ramen.

---

### 3. Vad förstår du nu som du inte förstod innan?

Nämn ett specifikt tekniskt koncept som du nu faktiskt förstår — inte bara kan använda, utan förstår varför det är designat som det är.

Exempel på vad som inte räknas: "Jag lärde mig hur man använder Bicep." Det är ett verktyg. Vad förstår du om varför det fungerar som det gör?

**Svar:**

Azure Pipelines expanderar inte secret-variabler med vanlig `$(namn)`-syntax i inline scripts av säkerhetsskäl, för att värdet annars skulle kunna läcka innan maskeringen hinner slå till. Lösningen är att mappa in dem explicit via ett env:-block i YAML och referera dem som vanliga bash-miljövariabler i scriptet istället. Det gav mig förståelse för att secret-hantering i CI/CD inte bara handlar om att undvika hårdkodning i koden.

---

### 4. Vad skulle du göra annorlunda?

Om ni fick göra om sprinten från dag ett — ett konkret tekniskt beslut ni skulle fatta annorlunda, och varför.

**Svar:**

Jag hade inte börjat med att bygga `deploy.sh`. Vår klass har aldrig tidigare skrivit bicep-skript så `deploy.sh` kändes då som det självklara valet för att komma igång. Filen var dessutom ett bra underlag för att sedan konstruera `main.bicep` och `azure-pipelines.yml`, men `deploy.sh` kostade onödigt med tid. Hade jag kunnat gå tillbaka i tiden hade jag börjat med pipelinen, byggt bicep steg för steg och aldrig skrivit `deploy.sh`.

---

### 5. Arkitektur och ekonomi

Kunden frågar dig direkt (inte teamet): "Vad kostar vår lösning per månad, och var är den mest sårbar?"

Svara som om du satt i ett kundmöte. Inga "vi" — du äger svaret.

**Svar:**
Vid lansering kostar lösningen ~503kr/ mån, alltså ~13kr/ kund. Skulle kundbasen tredubblas landar ni på ~514 kr/mån och ~4 kr/ kund.

Det mest sårbara är den publika länken `/verify/uuid` utan rate limiting (en begränsning av hur många anrop en och samma avsändare får göra) samt den gemensamma API-nyckeln för alla admin-anrop. Utan rate limiting kan appen belastas mer än nödvändigt och om API-nyckeln skulle läcka kan vem som helst få tillgång till alla era endpoints och t.ex. skapa egna certifikat.
Jag rekommenderar att ni börjar med rate limiting på `/verify/uuid` eftersom det inte är särskilt komplicerat att lösa. Därefter bör ni satsa på att implementera verifiering via Entra ID för alla admin-användare. Det kan kräva lite mer arbete om inte alla admins redan har t.ex. varsitt Microsoft-konto.

---

_Reflektionen bedöms inte med betyg — den är underlag för Marcus att förstå vad varje person faktiskt lärt sig, och för dig att ha något konkret att ta med till en arbetsintervju._

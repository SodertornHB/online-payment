# Säkerhetsgranskning: online-payment

## Sammanfattning

Denna rapport **ersätter och kompletterar** den tidigare partiella granskningen av `swish-for-koha` (vars rapport finns i `organizational-specific/SECURITY_AUDIT.md`). Den tidigare granskningen kunde endast se webblagret (~7 C#-filer) eftersom projektet `OnlinePayment.Logic` (callback-service, betalningsservice, Swish-klient, dataåtkomst) saknades. Många fynd markerades då som "kräver verifiering". **Nu finns hela applikationen** och samtliga tidigare misstänkta punkter har kunnat verifieras mot faktisk källkod.

online-payment är en ASP.NET Core 8-applikation som integrerar bibliotekssystemet Koha med **Swish** (svenskt mobilbetalningssystem) för att låta låntagare betala sina biblioteksavgifter. Applikationen hanterar **riktiga betalningar** och är därmed ett högvärdesmål för betalningsbedrägeri.

Granskningen är **i allt väsentligt komplett** för den kod som finns i repot. Verifieringen bekräftar och **förvärrar** de tidigare farhågorna. De allvarligaste bekräftade fynden:

- **Callbacken (`/callback`) litas på blint** – ingen avsändarautentisering (signatur/mTLS/IP-allowlist), **inget beloppsintegritetsskydd** (Koha krediteras med beloppet *från callbacken*, inte det begärda), **ingen bekräftande statuskontroll mot Swish**, **ingen idempotens/replayskydd**. En anonym angripare kan markera avgifter som betalda utan att pengar överförs (Kritisk – betalningsbedrägeri).
- **SQL-injektion via den anonyma `/callback`** – `callbackModel.Id` sätts oescapat direkt in i en SQL-sträng i `PaymentDataAccessExtended.GetByExternalId` (Kritisk).
- **Produktionens Swish-merchantcertifikat (.p12) + lösenfras ligger i källträdet** (Kritisk).
- **Oautentiserad utlämning av PII och oautentiserad betalningsinitiering för godtyckligt låntagarnummer** (Hög).
- **Hemligheter i klartext i `appsettings.production.json`** (Hög).

Det som var **positivt** och som avfärdar tidigare farhågor: den utgående TLS-anslutningen till Swish är korrekt (TLS 1.2/1.3, servercertifikat valideras, klientcert laddas per thumbprint från Windows-certifikatlagret), och den vidöppna CORS-policyn `AllowAllOrigins` är definierad men **aktiveras aldrig** (inget `UseCors`/`[EnableCors]` finns).

## Omfattning och antaganden

- **Granskad kod:** hela `C:\Users\shsnnr19adm\code2\online-payment`:
  - `OnlinePayment.Logic` (Service, Http, DataAccess, Model, Setting)
  - `OnlinePayment.Web` (Controller, ApiController, ViewModel, Startup/Program, konfiguration)
  - `organizational-specific` (org-specifika controllers, `appsettings.production.json`, `appsettings.Development.json`, certifikatfiler)
  - `OnlinePayment.Test` (översiktligt)
- **Statisk, läsande granskning.** Ingen kod har körts, ändrats eller testats. Ingen betalning eller callback har triggats. Endast denna rapportfil har skapats.
- **Antaganden:**
  - `[NoLibraryAuth]` gör endpointen helt anonymt åtkomlig. Detta är **verifierat** i bibliotekets källkod (se nedan), inte ett antagande: `AuthAttributes.cs` definierar `NoLibraryAuth` som en tom markörattribut och `AuthenticationMiddlewareBase.ShouldBeAuthorized` returnerar `false` när attributet finns, varvid middleware släpper igenom anropet utan autentisering. Endpoints utan `[NoLibraryAuth]` skyddas dock endast om appen faktiskt registrerar `UseLibraryAuthentication()`/`UseLibraryApiAuthentication()` – detta bör kontrolleras i online-payments pipeline.
  - `appsettings.production.json` speglar skarp miljö.
  - Basklasserna `Startup`/`Program` är autogenererade men **finns nu i repot** och har granskats.
  - **Källkoden till `Sh.Library.Authentication` finns i repot** (`AuthService/AuthenticationLibrary/`, källversion 1.2.13) och har granskats – se avsnittet "Applikationsspecifika observationer". Obs: online-payment refererar NuGet-version **1.2.8**, dvs. en versionsdrift mot källans 1.2.13 som bör verifieras. `Sh.Library.MailSender` (binärt paket) kunde inte granskas.

## Övergripande riskbedömning

**Sammantagen risk: Kritisk.**

Applikationen exponerar internetnära, anonyma betalnings-endpoints utan tillräckliga förtroendegränser. Med hela betalningslogiken synlig är det nu **bekräftat** att den mest kritiska farhågan stämmer: callbacken behandlas utan någon autenticitets-, belopps-, status- eller idempotenskontroll och krediterar Koha-kontot direkt. I kombination med att betalningsinitiering och sessionsuppslag är anonyma finns en **komplett, oautentiserad bedrägerikedja** som kan nolla en godtycklig låntagares avgifter utan att betala. Utöver detta finns en oautentiserad SQL-injektion i samma callback-flöde, ett komprometterat produktionscertifikat i källträdet och klartexthemligheter. Systemet bör **inte** betraktas som produktionssäkert i nuvarande skick.

## Bekräftade fynd

### 1. Callbacken litas på blint – saknar autenticitet, beloppsintegritet, statuskontroll och idempotens

- **✅ ÅTGÄRDAT (app-lagret) 2026-07-08:** `PaymentCallbackServiceExtended.Insert` (grundfunktionalitet) hårdad: krediterar Koha endast när providern själv rapporterar status `PAID` (hämtas via mTLS-GET i `GetByExternalId`/`UpdateStatusIfChanged`) i stället för på callbackens egna `DatePaid`; krediterar det **lagrade** beloppet `payment.Amount` (aldrig callbackens `Amount`) och loggar avvikelse; avvisar callback med avvikande valuta mot `swishApiSettings.Currency`; och är idempotent (avvisar dubblettcallback per session). Detta bryter den oautentiserade bedrägerikedjan. **Kvarstår:** (1) IP-allowlist för `/callback` — org-specifikt/infrastruktur (reverse proxy eller org-overlay); (2) unik DB-constraint på callback-sessionen för starkare skydd mot genuint samtidiga dubbletter (nuvarande idempotens är check-then-act).
- **Allvarlighetsgrad:** Kritisk
- **Typ:** Webhook-/callback-verifiering, beloppsintegritet, förtroendegräns, betalningsbedrägeri
- **Berörda filer eller metoder:**
  - `OnlinePayment.Web/Controller/PaymentCallbackControllerExtended.cs` → `Callback(...)`
  - `OnlinePayment.Logic/Service/PaymentCallbackServiceExtended.cs` → `Insert(PaymentCallback, string externalId)`
  - `OnlinePayment.Logic/Service/KohaServiceExtended.cs` → `UpdateSum(...)`
  - `OnlinePayment.Logic/Service/PaymentServiceExtended.cs` → `GetByExternalId` / `UpdateStatusIfChanged`
- **Observation:** `POST /callback` är `[NoLibraryAuth]` (anonym) och tar `[FromBody] dynamic`. Nedströms i `PaymentCallbackServiceExtended.Insert`:
  - Betalningen hämtas via `paymentService.GetByExternalId(externalId)`. Om `model.DatePaid == null` avbryts – men **`DatePaid` kommer från callbackens body** (`CallbackRequestModel.DatePaid`) och styrs alltså helt av anroparen.
  - Om `DatePaid` är satt krediteras Koha: `kohaService.UpdateSum(payment.BorrowerNumber, Utils.ConvertToInt(model.Amount), payment.Session)`. **Beloppet tas från callbacken (`model.Amount`), inte från den lagrade `payment.Amount`.** Det finns alltså **ingen beloppsintegritet** – anroparen bestämmer krediterat belopp.
  - `GetByExternalId` anropar visserligen `UpdateStatusIfChanged`, som gör en GET mot Swish och uppdaterar `Payment.Status`, men **koden grindar aldrig krediteringen på det auktoritativa Swish-statusvärdet** (t.ex. `== "PAID"`). Enda villkoret är `model.DatePaid != null`.
  - Det finns **ingen kontroll av att anropet kommer från Swish** (ingen signatur/HMAC, ingen mTLS på inkommande, ingen IP-allowlist – `UseIPBlock` är en *blocklista* och är tom).
  - Det finns **ingen idempotens/replayskydd**: samma callback kan skickas flera gånger och `UpdateSum` (som postar en kredit mot Koha) körs varje gång → upprepad kreditering.
- **Risk – fullständig bedrägerikedja:** `/pay` och `/init` är anonyma (fynd 4–5) och kan initiera en betalning för en **godtycklig** `borrowerNumber`. `GET /session/{id}` (fynd 8) returnerar hela betalningsentiteten inklusive `ExternalId`. En angripare kan därmed: (1) initiera betalning för ett offers låntagarnummer, (2) läsa ut `ExternalId` via `/session`, (3) POSTa en förfalskad callback med `Id = ExternalId`, `DatePaid = <nu>` och valfritt `Amount` → offrets avgifter i Koha nollställs (krediteras) utan att någon betalning skett. Beloppet kan dessutom överstiga skulden och skapa ett tillgodohavande.
- **Sannolikhet:** Hög (alla nödvändiga steg är anonyma och kontraktet är enkelt).
- **Påverkan:** Mycket hög – direkt ekonomiskt bedrägeri och felaktig kontokreditering i Koha.
- **Rekommenderad åtgärd:**
  - Lita aldrig på callbackens status/belopp. Gör en **bekräftande GET mot Swish-API:t via mTLS** och kreditera endast om Swish-status är `PAID`.
  - Kreditera med det **lagrade, begärda beloppet** (`payment.Amount`), och verifiera att callbackens belopp/valuta matchar.
  - Inför **idempotens**: markera en betalning som slutbehandlad och avvisa dubbletter (unik `PaymentReference`/`ExternalId` får bara krediteras en gång).
  - Begränsa `/callback` till Swish publicerade IP-intervall i reverse proxy/brandvägg.
- **Åtgärdsinsats:** Medel–Stor.
- **Verifiering/test:** Skicka förfalskad callback och verifiera att den avvisas; skicka dubblett och verifiera att endast en kreditering sker; manipulera `Amount` och verifiera att det lagrade beloppet används; verifiera att kreditering endast sker efter `PAID` från Swish.

### 2. Oautentiserad SQL-injektion via /callback (GetByExternalId)

- **✅ ÅTGÄRDAT 2026-07-08:** Helt åtgärdat. `GetByExternalId`/`GetBySessionId` använder Dapper-parameter (`@externalId`/`@sessionId`), `like` är bytt mot `=` (ingen wildcardmatchning mot godtycklig rad), och `PaymentServiceExtended.GetByExternalId` validerar formatet (`^[a-fA-F0-9]{32}$`) före användning. Både SQL-injektionen och wildcard-/integritetsproblemet är stängda. (Least-privilege på databaskontot är en driftsåtgärd utanför koden.) Detaljer nedan.
- **Allvarlighetsgrad:** Kritisk
- **Typ:** SQL-injektion (oparametriserad query), extern oautentiserad angripare
- **Berörda filer eller metoder:** `OnlinePayment.Logic/DataAccess/PaymentDataAccessExtended.cs` → `GetByExternalId(string externalId)`; anropas från `PaymentCallbackServiceExtended.Insert` med `callbackModel.Id` från callback-bodyn.
- **Observation:** Metoden bygger SQL genom stränginterpolation utan parametrar och **utan någon escaping**:
  ```
  string sql = $"SELECT * FROM [{Table}] where ExternalId like '{externalId}'";
  ```
  `externalId` är `callbackModel.Id`, dvs. ett fritextfält direkt från den anonyma `POST /callback`. Till skillnad från `SqlStringBuilder` (som åtminstone dubblar enkelfnuttar vid insert/update) sker här ingen sanering alls. `GetBySessionId` interpolerar också men skyddas av en `^[a-fA-F0-9]{32}$`-regex; **`GetByExternalId` valideras inte någonstans.**
- **Risk:** En angripare kan injicera godtycklig SQL (UNION-baserad exfiltrering, subqueries, tidsbaserad blind SQLi, eventuellt `xp_cmdshell` beroende på DB-rättigheter) mot betalningsdatabasen – helt oautentiserat. Även utan full injektion tillåter `like` med `%` att en callback matchar en godtycklig betalningsrad (integritetsproblem).
- **Sannolikhet:** Hög (anonym endpoint, trivialt injektionsläge).
- **Påverkan:** Mycket hög – läsning/manipulation av hela betalningsdatabasen.
- **Rekommenderad åtgärd:** Använd parametriserad query (Dapper-parameter `@externalId`) och byt `like` mot `=`. Validera `externalId`-formatet (32 hex) innan användning. Kör databaskontot med minsta möjliga rättigheter.
- **Åtgärdsinsats:** Liten.
- **Verifiering/test:** Skicka callback med `Id` innehållande `'`-tecken/UNION-payload och verifiera att queryn parametriseras och att inga fel/data läcker.
- **✅ ÅTGÄRDAT 2026-07-02:** `GetByExternalId` och `GetBySessionId` använder nu Dapper-parameter (`@externalId`/`@sessionId`) istället för stränginterpolation. Logic- och Web-projekt bygger utan fel.
- **✅ HELT ÅTGÄRDAT 2026-07-08:** Kvarvarande rekommendationer stängda: `like` bytt mot `=` i båda metoderna (eliminerar `%`-wildcardmatchning mot godtycklig rad, även en korrekthetsbugg för `%`/`_` i legitima id:n), och `PaymentServiceExtended.GetByExternalId` validerar nu `externalId`-formatet (`^[a-fA-F0-9]{32}$`) före användning, som `GetBySessionId`. Databaskontots rättigheter (least privilege) är en driftsåtgärd utanför koden.

### 3. Produktionens Swish-merchantcertifikat och lösenfras finns i källträdet

- **Allvarlighetsgrad:** Kritisk
- **Typ:** Secrets/nyckelhantering, mTLS-identitet, betalningsbedrägeri
- **Berörda filer eller metoder:** `organizational-specific/swish_certificate.p12`, `organizational-specific/Swish_Merchant_TestCertificate_1234679304.p12`, `organizational-specific/appsettings.production.json` (sektion `CertificationAuthentication`)
- **Observation:** Två PKCS#12-certifikatfiler ligger i `organizational-specific/` och kopieras dessutom in i webbprojektet via MSBuild-target `CopyOrgSpecificFiles` i `Web.csproj`. Motsvarande lösenfras och thumbprint finns i klartext i `appsettings.production.json` (värden återges ej här). Detta är merchant-identiteten (klientcertifikatet för mTLS) mot Swish CPC-API:t. Runtime laddar certifikatet via thumbprint från `LocalMachine\My` (`SwishHttpClient`), men själva `.p12`-filen med privat nyckel finns ändå versionshanterad i trädet.
- **Risk:** Den som får tag på `.p12`-filen och lösenfrasen kan agera som merchanten mot Swish (initiera betalningar/återbetalningar, hämta transaktionsdata) i organisationens namn.
- **Sannolikhet:** Hög (materialet ligger redan i klartext/binärt i trädet).
- **Påverkan:** Mycket hög – ekonomiskt bedrägeri och förlust av merchant-förtroende.
- **Rekommenderad åtgärd:** Ta bort båda `.p12`-filerna och lösenfrasen ur källträdet omedelbart. Betrakta certifikatet som **komprometterat och rotera/återkalla det hos Swish**. Distribuera certifikatet endast till certifikatlagret på servern via säker pipeline; injicera lösenfras via secret manager.
- **Åtgärdsinsats:** Medel (rotering hos Swish), brådskande.
- **Verifiering/test:** Bekräfta att repo och byggartefakter inte längre innehåller `.p12`/lösenfras; verifiera att gammalt certifikat återkallats.

### 4. Oautentiserad utlämning av personuppgifter (IDOR) via /init och /js

- **✅ ÅTGÄRDAT 2026-07-08 (med känd residual):** Autentisering är inte möjlig här (externa Koha-OPAC-användare), så per rekommendationen används nu en **signerad, kortlivad token** (ASP.NET Core Data Protection, 15 min) i stället för ett gissningsbart heltal. `/js` myntar token och länkar `/init?token=…`; `/init` och `/pay` löser `borrowerNumber` **ur token** och accepterar aldrig ett rått nummer från klienten (härdar även Fynd 5). PII i `/init`-vyn bantad till namn + belopp (e-post/telefon borttagna). **Rate limiting** (30/min per IP) på `/js`, `/init`, `/pay`. Ny bas-tjänst `OnlinePayment.Web/Security/BorrowerTokenService.cs`; ändringen speglas i org-overlayt (behåller `[NoLibraryAuth]`). **Residual:** `/js` myntar token för valfritt `borrowernumber` (OPAC-ingången kan inte autentiseras), så en beslutsam angripare kan nå PII i två steg (`/js`→`/init`) — men nu rate-limitat, och `/js` avslöjar endast om avgifter finns, inte PII. Vidare härdning: minska/ta bort fee-existens-signalen i `/js` och överväg IP-allowlist/WAF om OPAC-ingången kan begränsas.
- **Allvarlighetsgrad:** Hög
- **Typ:** Åtkomstkontroll (saknad auktorisering), dataskydd/PII, IDOR/enumerering
- **Berörda filer eller metoder:** `OnlinePayment.Web/Controller/PaymentControllerExtended.cs` → `Init(...)`; `organizational-specific/Controller/HomeControllerExtended.cs` → `js(...)`
- **Observation:** `GET /init?borrowerNumber={n}` är `[NoLibraryAuth]` och returnerar en vy med `PatronName`, `PatronPhoneNumber`, `PatronEmail` och saldo för det angivna heltalet. Ingen kontroll av att anroparen är just den låntagaren eller ens autentiserad. `GET /js?borrowerNumber={n}` läcker indirekt om ett låntagarnummer har avgifter (>1) att betala.
- **Risk:** Trivial enumerering av sekventiella låntagarnummer ger massutlämning av namn, telefonnummer, e-post och skuldsaldon (allvarligt GDPR-/integritetsproblem).
- **Sannolikhet:** Hög.
- **Påverkan:** Hög.
- **Rekommenderad åtgärd:** Kräv autentisering och koppla begäran till den inloggade låntagaren. Om anropet måste initieras anonymt från Koha OPAC, använd en signerad, kortlivad token som identifierar låntagaren i stället för ett gissningsbart heltal.
- **Åtgärdsinsats:** Medel.
- **Verifiering/test:** Anropa `/init` med annat låntagarnummer än det inloggade och verifiera att åtkomst nekas.

### 5. Oautentiserad betalningsinitiering för godtyckligt låntagarnummer via /pay

- **Allvarlighetsgrad:** Hög
- **Typ:** Åtkomstkontroll, missbruk av betalnings-API
- **Berörda filer eller metoder:** `PaymentControllerExtended.cs` → `Pay(...)`; `PaymentServiceExtended.InitiatePayment(int)`
- **Observation:** `POST /pay` är `[NoLibraryAuth]` och anropar `InitiatePayment(viewModel.BorrowerNumber)` med ett klientstyrt låntagarnummer. Ingen autentisering eller ägarskapskontroll, ingen hastighetsbegränsning.
- **Risk:** Extern part kan initiera Swish-betalningsförfrågningar för godtyckliga låntagare (trakasserier via upprepade Swish-notiser till kopplade telefonnummer, last mot Swish-API/databas). Är dessutom steg 1 i bedrägerikedjan i fynd 1.
- **Sannolikhet:** Medel–Hög.
- **Påverkan:** Medel–Hög.
- **Rekommenderad åtgärd:** Kräv autentisering, validera att `BorrowerNumber` tillhör den inloggade användaren, inför rate limiting.
- **Åtgärdsinsats:** Medel.
- **Verifiering/test:** Försök initiera betalning för annan låntagare och verifiera att det nekas.

### 6. Hemligheter i klartext i produktionskonfigurationen

- **Allvarlighetsgrad:** Hög
- **Typ:** Secrets/config-hantering
- **Berörda filer eller metoder:** `organizational-specific/appsettings.production.json`, `organizational-specific/appsettings.Development.json`
- **Observation:** `appsettings.production.json` innehåller i klartext: databasanslutningssträng, Koha-API-användarnamn och -lösenord, `Authentication:BearerToken` (GUID) och Swish-certifikatets lösenfras/thumbprint. `appsettings.Development.json` innehåller motsvarande test-/utvecklingshemligheter. (Värden återges ej här.)
- **Risk:** Alla med läsåtkomst till repot/byggartefakter får direkt tillgång till databas, Koha-API (Basic-auth) och bearer-token mot auth-tjänsten.
- **Sannolikhet:** Hög.
- **Påverkan:** Hög – lateral åtkomst till backend-system.
- **Rekommenderad åtgärd:** Flytta samtliga hemligheter till miljövariabler/secret manager (`Program.cs` läser redan `AddEnvironmentVariables()`). Rotera alla exponerade lösenord/tokens. Ta bort hemligheterna ur versionshanterade filer.
- **Åtgärdsinsats:** Medel.
- **Verifiering/test:** Bekräfta att konfigfiler inte längre innehåller hemligheter; bekräfta rotering.

### 7. Oautentiserad, destruktiv /clean-up och informationsläckande /version före autentisering

- **Allvarlighetsgrad:** Medel
- **Typ:** Åtkomstkontroll, middleware-ordning, anti-forensik/DoS, informationsexponering
- **Berörda filer eller metoder:** `OnlinePayment.Web/Startup.cs` – middleware `CleanUp`, `Version` registreras i `RegisterMiddleware` som körs **före** `CustomConfiguration` där `UseLibraryApiAuthentication`/`UseLibraryAuthentication` läggs till. `CleanUpServiceExtended.ProcessCleanUp` (i `StartupExtended.cs`) raderar loggfiler och DB-loggposter.
- **Observation:** Varje request vars path slutar på `clean-up` fångas av middleware **innan autentiseringen** och kör `ProcessCleanUp()` som raderar loggfiler äldre än retentionsgränsen samt DB-loggposter äldre än 7 dagar. Path som slutar på `version` returnerar tjänstens namn, klient-/databasversion och laddade Sh.Library-/Logic-assemblies – anonymt.
- **Risk:** Vem som helst kan anonymt trigga radering av loggar (anti-forensik, förstör spårbarhet vid ett pågående angrepp) och upprepat belasta systemet. `/version` läcker versions-/komponentinformation som underlättar riktade attacker.
- **Sannolikhet:** Medel.
- **Påverkan:** Medel.
- **Rekommenderad åtgärd:** Kräv autentisering/behörighet för `/clean-up` (och helst `/version`), eller kör städning som schemalagt internt jobb i stället för via HTTP. Flytta dessa middleware bakom autentiseringen.
- **Åtgärdsinsats:** Liten–Medel.
- **Verifiering/test:** Anropa `/clean-up` och `/version` anonymt och verifiera att de nekas.

### 8. Oautentiserad utlämning av hela betalningsentiteten via /session/{sessionId}

- **Allvarlighetsgrad:** Medel
- **Typ:** Åtkomstkontroll, informationsexponering (överexponerad modell)
- **Berörda filer eller metoder:** `OnlinePayment.Web/ApiController/PaymentApiControllerExtended.cs` → `Get(...)`; `PaymentServiceExtended.GetBySessionId`
- **Observation:** `GET /session/{sessionId}` är `[NoLibraryAuth]` och returnerar hela `Payment`-entiteten (`return Ok(payment)`), inklusive `ExternalId`, `BorrowerNumber`, `PatronName`, `PatronEmail`, `PatronPhoneNumber` och `Amount`. `sessionId` valideras till 32 hex (128-bitars gissningsskydd), men entiteten avslöjar `ExternalId` som är den saknade pusselbiten i callback-förfalskningen (fynd 1).
- **Risk:** Röjt sessions-id (via loggar, referrer, delad länk) exponerar PII och den `ExternalId` som möjliggör betalningsbedrägeri. Returnerar intern entitet i stället för avgränsad DTO.
- **Sannolikhet:** Medel (128-bitars id svårgissat men röjs lätt i detta flöde).
- **Påverkan:** Medel–Hög (via kopplingen till fynd 1).
- **Rekommenderad åtgärd:** Returnera en minimal DTO (endast status/session), exponera aldrig `ExternalId`/PII. Överväg åtkomstkontroll knuten till användaren.
- **Åtgärdsinsats:** Liten.
- **Verifiering/test:** Kontrollera svarsfälten och att `ExternalId`/PII inte läcker.

### 9. Betalnings-/persondata loggas i klartext till fil och databas

- **✅ ÅTGÄRDAT 2026-07-09:** Loggningen sanerad: callback-payloaden (betalaralias/telefonnummer, belopp, referenser) loggas inte längre och ekas inte tillbaka i svaret (`PaymentCallbackControllerExtended`); `/init` loggar inte låntagar-id/belopp/saldo; `PaymentServiceExtended` och `PaymentCallbackServiceExtended` identifierar via slumpat sessions-id i stället för borrowernumber/belopp, och Swish-location-URL:en togs ur meddelandena. `Program.cs` höjde loggolvet Trace→Information (Debug-loggarna med hela request-/responsekroppar når aldrig målen). `nlog.config`: framåtsnedstreck i sökvägar (backslash gav filer med bokstavligt `\` i namnet i approten på Linux) samt 7 dagars retention på diskloggar (`maxArchiveFiles`). Kvarstår (drift): rensa befintliga loggfiler på staging (`logs\2026-07-0*.log` i approten) som skrevs före fixen, samt åtkomstkontroll på logg-katalogen i prod; `Log`-tabellen självsaneras av CleanUpService (7 dagar).
- **Allvarlighetsgrad:** Medel
- **Typ:** Loggning av PII/betalningsdata
- **Berörda filer eller metoder:** `PaymentCallbackControllerExtended.cs` (`logger.LogInformation($"Callback received model: {serializedModel}")`), `PaymentControllerExtended.cs` `Init` (loggar låntagar-id, belopp, saldo), `PaymentServiceExtended` (loggar borrowernumber/belopp), `nlog.config` (skriver `${message}` till både fil och `Log`-tabell).
- **Observation:** Hela den serialiserade callback-modellen loggas på Info-nivå. Swish-callbacks innehåller betalarens alias (telefonnummer), belopp, betalnings-/transaktionsreferenser. `nlog.config` speglar allt till både loggfil och databas, minlevel Info. Program.cs sätter dessutom `SetMinimumLevel(LogLevel.Trace)`.
- **Risk:** Personuppgifter och betalningsdetaljer hamnar i loggar (fil + DB) med bredare åtkomst/längre livslängd än nödvändigt; strider mot dataminimering.
- **Sannolikhet:** Medel.
- **Påverkan:** Medel.
- **Rekommenderad åtgärd:** Logga inte hela nyttolasten på Info-nivå; maskera betalaralias/PII. Höj minsta loggnivå i produktion (inte Trace). Säkerställ åtkomstkontroll/retention på loggar.
- **Åtgärdsinsats:** Liten.
- **Verifiering/test:** Granska logg-/DB-utdata och bekräfta att PII inte förekommer.

### 10. Hemmagjord SQL-strängbyggare för insert/update (enbart citattecken-dubbling)

- **Allvarlighetsgrad:** Låg (defense-in-depth; potentiell risk)
- **Typ:** Databasåtkomst, injektionshärdning, kulturberoende
- **Berörda filer eller metoder:** `OnlinePayment.Logic/DataAccess/SqlStringBuilderDataAccess.cs`, `BaseDataAccess.Insert/Update`
- **Observation:** Insert/update bygger SQL genom att interpolera värden och skydda enbart genom `str.Replace("'", "''")`. Callback-fält (`Status`, `PaymentReference`, `PayerAlias`, `Message`, `ErrorMessage` m.fl.) flödar in via denna väg. Dubblingen neutraliserar visserligen enkelfnutt-baserad injektion för strängkolumner, men mönstret är skört och avviker från parametriserade queries. Numeriska/`decimal`-värden skrivs via `.ToString()` med aktuell kultur (sv-SE använder decimalkomma), vilket kan ge felaktiga/ogiltiga SQL-värden (dataintegritet).
- **Risk:** Främst dataintegritet och framtida injektionsrisk om ett fält byter typ/kontext.
- **Sannolikhet:** Låg.
- **Påverkan:** Låg–Medel.
- **Rekommenderad åtgärd:** Migrera insert/update till parametriserade queries (Dapper stöder detta redan för select/delete). Använd `CultureInfo.InvariantCulture` för numerisk formatering.
- **Åtgärdsinsats:** Medel.
- **Verifiering/test:** Enhetstesta insert/update med värden som innehåller specialtecken och decimaltal.
- **✅ ÅTGÄRDAT 2026-07-02:** SqlStringBuilder INSERT/UPDATE parameteriserad (@-platshållare, binds av Dapper via modellen); HasIdentityColumn parameteriserad. Logic- och Web-projekt bygger utan fel. Även `PaymentDataAccessExtended.GetByExternalId` (och `GetBySessionId`) parameteriserades med Dapper-parameter (se fynd 2).

### 11. TrustServerCertificate=True i databasanslutningen

- **Allvarlighetsgrad:** Låg
- **Typ:** Transportkryptering/konfiguration
- **Berörda filer eller metoder:** `appsettings.production.json`, `appsettings.Development.json` (`ConnectionStrings:Default`)
- **Observation:** Anslutningssträngarna använder `TrustServerCertificate=True`, vilket inaktiverar validering av SQL Server-certifikatet.
- **Risk:** Möjliggör MITM mot databasanslutningen i icke betrodda nät.
- **Sannolikhet:** Låg (Trusted_Connection, sannolikt internt nät).
- **Påverkan:** Medel om exploaterat.
- **Rekommenderad åtgärd:** Använd betrott servercertifikat, sätt `TrustServerCertificate=False` och `Encrypt=True`.
- **Åtgärdsinsats:** Liten.
- **Verifiering/test:** Verifiera krypterad, validerad DB-anslutning.

### 12. Fullständigt CRUD-API under /api/v1 skyddat endast av en committad bearer-token

- **Allvarlighetsgrad:** Hög
- **Typ:** Åtkomstkontroll/API-säkerhet, exponerad endpoint, PII-läckage, dataintegritet
- **Berörda filer eller metoder:** `OnlinePayment.Web/ApiController/*.cs` (`PaymentApiController`, `PaymentCallbackApiController`, `AuditApiController`, `LogApiController`, `MigrationApiController`, `PaymentResponseApiController`, `PaymentRequestApiController`) med `[Route("api/v1/[controller]s")]`; autentiseringen sköts av `Sh.Library.Authentication.ApiAuthenticationMiddleware` (`C:\Users\shsnnr19adm\code2\AuthService\AuthenticationLibrary\ApiAuthenticationMiddleware.cs`), registrerad i `StartupExtended.CustomConfiguration` via `UseLibraryApiAuthentication()`.
- **Observation:** Varje entitet exponerar autogenererad full CRUD: `GET` (alla), `GET/{id}`, `GET/search`, `POST` (insert), `PUT/{id}`, `DELETE/{id}` – t.ex. `POST /api/v1/paymentcallbacks` (`service.Insert`), `GET /api/v1/payments` (alla betalningar inkl. PII), `DELETE /api/v1/audits/{id}`. `ApiAuthenticationMiddleware` skyddar endast sökvägar som identifieras som API (`EndpointIsApi`) och kräver en `Authorization: Bearer <token>` som valideras mot AuthService. En sådan bearer-token (`Authentication:BearerToken`, GUID) ligger **committad i klartext** i `appsettings.production.json`/`appsettings.Development.json` (fynd 6). Middleware **är** registrerad (default-deny gäller alltså för `/api`), men skyddet står och faller med tokenhemligheten.
- **Risk:** Den som läser repot/byggartefakterna får den kända bearer-token. Om den är en giltig credential mot AuthService kan angriparen läsa och mutera hela betalnings-, callback-, audit- och loggdatan direkt via CRUD-API:t – inklusive massutlämning av PII (namn/telefon/e-post/belopp), radering av audit-spår och insättning/ändring av betalningsposter.
- **Sannolikhet:** Medel (kräver att den committade token accepteras av AuthService).
- **Påverkan:** Hög.
- **Rekommenderad åtgärd:** Rotera bearer-token och flytta den ur versionshanterade filer (fynd 6). Överväg att stänga av eller kraftigt begränsa de autogenererade CRUD-endpointsen i produktion (de behövs sällan externt). Inför rollbaserad behörighet på skrivande/raderande operationer.
- **Åtgärdsinsats:** Medel.
- **Verifiering/test:** Anropa `GET /api/v1/payments` med den committade token och verifiera om åtkomst ges; bekräfta att token roterats och att CRUD-ytan begränsats.

### 13. Bibliotekets sessionscookie är förfalskningsbar (svag krypto) och saknar cookie-flaggor

- **Allvarlighetsgrad:** Medel
- **Typ:** Autentisering/identitet, behörighetseskalering, cookie-säkerhet
- **Berörda filer eller metoder:** `Sh.Library.Authentication` (`AuthenticationMiddleware.cs`, `Encryption.cs`) i `C:\Users\shsnnr19adm\code2\AuthService\AuthenticationLibrary\`; används av online-payment via `UseLibraryAuthentication()` i `StartupExtended.cs`. Berörd online-payment-endpoint: `GET /list` i `PaymentControllerExtended.cs` (enda icke-`[NoLibraryAuth]`, icke-`/api`-endpointen).
- **Observation:** Bibliotekets cookie-autentisering (`BiblAppsSession`) krypteras enligt granskning av auth-biblioteket med **hårdkodad nyckel och noll-IV** (`Encryption.cs`), och rollen (`IsStaff`/`GetRole`) läses från denna klientkontrollerade cookie. Cookien sätts dessutom med `HttpOnly=false` och `Secure=false`. Därmed kan en angripare förfalska en giltig session (och roll). online-payment använder inte `[LibraryAuthStaffOnly]` i sina controllers, men förlitar sig på cookie-autentiseringen för `GET /list` (som listar samtliga betalningar inkl. PII).
- **Risk:** En angripare kan förfalska `BiblAppsSession` och nå `/list` (och alla framtida icke-`[NoLibraryAuth]`-endpoints), samt eskalera till staff/adminroll i den bredare biblioteksplattformen. `HttpOnly=false` gör cookien åtkomlig för JavaScript (XSS-stöld).
- **Sannolikhet:** Medel.
- **Påverkan:** Medel–Hög (beroende på plattformens övriga staff-only-funktioner).
- **Rekommenderad åtgärd:** Åtgärdas i auth-biblioteket (slumpmässig nyckel/IV, autentiserad kryptering, `HttpOnly=true`, `Secure=true`, `SameSite`). online-payment bör inte förlita sig på cookieroll för känsliga operationer och bör skydda `/list` med server-sidig auktorisering.
- **Åtgärdsinsats:** Medel (huvudsakligen i det delade biblioteket).
- **Verifiering/test:** Verifiera cookie-flaggor och att förfalskad cookie avvisas efter åtgärd.

## Misstänkta risker som kräver verifiering

- **Beroendeversioner (kräver verifiering):** `System.Data.SqlClient` 4.8.6 och `Microsoft.AspNetCore.Mvc.Localization` 2.2.0 (körs på net8.0) är gamla. `System.Data.SqlClient` används i `SqlDataAccess` och `nlog.config`-databastarget. `Microsoft.AspNetCore.Mvc.Core` 2.2.5 refereras i Logic. Eventuella sårbarheter i dessa/transitiva paket bör kontrolleras mot aktuell rådgivningsdata. Inga CVE:er påstås här.
- **`Sh.Library.Authentication` – VERIFIERAD (ej längre osäker):** Källkoden finns i `AuthService/AuthenticationLibrary/` (källversion 1.2.13). `[NoLibraryAuth]` är en tom markörattribut som gör endpointen helt publik (`AuthAttributes.cs`, `AuthenticationMiddlewareBase.ShouldBeAuthorized`), vilket bekräftar att `/callback`, `/init`, `/pay` och `/session` är genuint oautentiserade. Roll/staff-kontroll (`LibraryAuthStaffOnly`/`IsStaff`/`GetRole`) läser rollen från den klientkontrollerade cookien `BiblAppsSession`, krypterad med hårdkodad nyckel + noll-IV (`Encryption.cs`) → förfalskningsbar. Sessionscookien sätts med `HttpOnly=false`/`Secure=false`. Middleware-registreringen är **bekräftad**: `StartupExtended.CustomConfiguration` anropar `UseLibraryApiAuthentication()` och `UseLibraryAuthentication()`, så default-deny gäller för icke-`[NoLibraryAuth]`-endpoints (t.ex. `/list`) och för `/api`-vägar. Enda kvarstående osäkerhet är versionsdriften mellan refererad NuGet 1.2.8 (`Web.csproj`) och källans 1.2.13. Slutsatserna används i fynd 4, 5, 8, 12 och 13.
- **Modellbindning `[FromBody] dynamic` i callbacken (kräver verifiering):** Dubbel serialisering/`ToString()` av en okänt stor nyttolast kan ge onödig resursförbrukning. Standardgränser för request-body (Kestrel/MVC) antas gälla men request-size-limit för endpointen är inte explicit satt.
- **`CustomHtmlHelpers.GetExternalHtmlAsync` (Startup.cs):** Hämtar godtycklig URL och injicerar svaret som rå `HtmlString`. Ingen anropare hittades i granskad kod; om den kopplas till användarstyrd URL uppstår SSRF/XSS. Kräver verifiering av användning i vyer.

## Generella härdningsrekommendationer

- Inför **rate limiting** på `/pay`, `/init`, `/js`, `/session` och `/callback`.
- Lägg till **säkerhetsheaders** och **HSTS** (`UseHsts` saknas; endast `UseHttpsRedirection` finns): `X-Content-Type-Options`, `Referrer-Policy`, restriktiv CSP där tillämpligt.
- Returnera avgränsade DTO:er i stället för interna entiteter (särskilt `/session` och `/list`).
- Höj loggnivån i produktion (bort från `Trace`), säkerställ dataminimering och retention.
- Centralisera hemligheter i nyckelvalv; inför hemlighets-/certifikatscanning i CI så att `.p12`/lösenord inte kan committas igen.
- Ta bort/aktivera medvetet den utkommenterade `AddDataProtection().PersistKeysToFileSystem(...)` om delade nycklar behövs över instanser.
- Validera indata strikt (belopp, valuta, referenser, `externalId`-format) och mot serverns förväntade värden i stället för att lita på klientdata.
- `SwishHttpClient` skapar en ny `System.Net.Http.HttpClient(handler)` per request – överväg återanvändning/`SocketsHttpHandler` för att undvika socket-utarmning (hygien, ej säkerhet).

## Applikationsspecifika observationer

Verifiering av de tidigare `swish-for-koha`-fynden och deras "kräver verifiering"-punkter mot den nu tillgängliga `OnlinePayment.Logic`-koden:

- **Tidigare fynd 1 (cert i källträdet) – BEKRÄFTAT.** `swish_certificate.p12` och `Swish_Merchant_TestCertificate_1234679304.p12` finns i `organizational-specific/` och kopieras in i webbprojektet vid bygge (fynd 3).
- **Tidigare fynd 2 (callback utan autenticitet) – BEKRÄFTAT och FÖRVÄRRAT.** Nedströmslogiken (`PaymentCallbackServiceExtended.Insert` + `KohaService.UpdateSum`) verifierades: ingen autenticitet, **ingen beloppsintegritet** (krediterar callbackens `Amount`), **ingen statusgrindning** mot Swish (`UpdateStatusIfChanged` hämtar status men används inte som villkor), **ingen idempotens/replayskydd**. Dessutom hittades en **oautentiserad SQL-injektion** i samma flöde (fynd 2/1 ovan). Se fynd 1 och 2.
- **Tidigare fynd 3 (PII via /init) – BEKRÄFTAT** (fynd 4).
- **Tidigare fynd 4 (betalningsinitiering via /pay) – BEKRÄFTAT** (fynd 5).
- **Tidigare fynd 5 (hemligheter i config) – BEKRÄFTAT** (fynd 6).
- **Tidigare fynd 6 (loggning av PII) – BEKRÄFTAT och FÖRVÄRRAT** – loggas till både fil och databas, minlevel Info, apploggning satt till Trace (fynd 9).
- **Tidigare fynd 7 (vidöppen CORS) – AVFÄRDAT (moot).** Policyn `AllowAllOrigins` (`AllowAnyOrigin().WithMethods("GET").AllowAnyHeader()`) är definierad i `StartupExtended.cs` men **aktiveras aldrig** – inget `UseCors`/`[EnableCors]` finns i hela lösningen. Den utgör därför ingen aktiv risk i nuvarande skick (men bör tas bort eller konfigureras restriktivt om den någonsin aktiveras).
- **Tidigare fynd 8 (/session utlämnar entitet) – BEKRÄFTAT** och uppgraderat i betydelse eftersom `ExternalId` som returneras möjliggör callback-förfalskningen (fynd 8).
- **Tidigare fynd 9 (TrustServerCertificate=True) – BEKRÄFTAT** (fynd 11).
- **Tidigare "kräver verifiering": faktisk callback-behandling – VERIFIERAD** (se ovan, allvarlig).
- **Tidigare "kräver verifiering": `Sh.Library.Authentication`/`[NoLibraryAuth]`-semantik – VERIFIERAD** mot källan i `AuthService/AuthenticationLibrary/`. `[NoLibraryAuth]` är en tom markörattribut → helt publik endpoint, vilket bekräftar att `/callback`, `/init`, `/pay`, `/session` och `/js` är genuint oautentiserade. Auth-middleware **är** registrerad (`UseLibraryApiAuthentication`/`UseLibraryAuthentication` i `StartupExtended.cs`), så `/api/v1`-CRUD skyddas av bearer-token (fynd 12) och `/list` av den förfalskningsbara cookien (fynd 13).
- **Tidigare "kräver verifiering": utgående TLS mot Swish – AVFÄRDAD som risk.** `SwishHttpClient.GetHttpClientHandlerWithCertificate` använder `SslProtocols.Tls12 | Tls13`, laddar klientcertifikatet via thumbprint från `LocalMachine\My` och **inaktiverar inte** servercertifikatvalidering (ingen `ServerCertificateCustomValidationCallback`/`DangerousAcceptAny`). Utgående TLS är korrekt.
- **Tidigare "kräver verifiering": HTTPS/HSTS/CSRF –** `UseHttpsRedirection` finns; **`UseHsts` saknas** (härdning). Antiforgery saknas men de känsliga endpointsen är ändå anonyma (`[NoLibraryAuth]`), så CSRF är underordnat de större bristerna.
- **Koha-integration:** `KohaHttpClient` använder Basic-auth med användarnamn/lösenord från config (inte bearer) och validerar servercertifikat (ingen inaktivering). `UpdateSum` postar en kredit mot `/patrons/{id}/account/credits` – det är denna som missbrukas i fynd 1.
- **`/js`-endpointen** genererar JavaScript till Koha OPAC. `borrowerNumber` är `int` och `lang` används endast i likhetsjämförelser, så ingen reflekterad indata skrivs ut i JS – låg XSS-risk. URL:er byggs från betrodd konfiguration.

## Delar som inte kunde granskas

- `Sh.Library.Authentication` – källkoden finns i `AuthService/AuthenticationLibrary/` (1.2.13) och har granskats; `[NoLibraryAuth]` och middleware-beteendet är verifierade. `Sh.Library.MailSender` (1.0.1) är fortfarande ett binärt NuGet-paket och kunde inte granskas. Versionsdriften online-payment→NuGet 1.2.8 vs källa 1.2.13 kvarstår att verifiera.
- Faktiska CVE-status för tredjepartsberoenden (kräver aktuell rådgivningsdata).
- Binärt innehåll i `.p12`-filerna (endast förekomst och referens bekräftades; innehåll återges ej).
- Runtime-/driftskonfiguration (reverse proxy, brandvägg, ev. IP-allowlist på infrastrukturnivå) utanför källkoden.

## Prioriterad åtgärdslista

1. **Kritiska (omedelbart):**
   - Säkra `/callback`: bekräftande statusuppslag mot Swish via mTLS, kreditera med lagrat begärt belopp, verifiera belopp/valuta/referens, inför idempotens/replayskydd och IP-allowlist (fynd 1).
   - Åtgärda SQL-injektionen i `GetByExternalId` – parametrisera och validera `externalId` (fynd 2).
   - Ta bort `.p12`-filerna och lösenfrasen ur källträdet, **rotera/återkalla** Swish-certifikatet, distribuera via säker pipeline (fynd 3).
2. **Höga:**
   - Kräv autentisering och ägarskapskontroll på `/init`, `/js` och `/pay`; stoppa PII-enumerering (fynd 4, 5).
   - Flytta alla hemligheter ur `appsettings.*.json` till secret manager och rotera dem (fynd 6).
   - Rotera den committade bearer-token och begränsa/stäng av det autogenererade `/api/v1`-CRUD-API:t i produktion (fynd 12).
3. **Medel:**
   - Skydda/flytta `/clean-up` och `/version` bakom autentisering (fynd 7).
   - Returnera minimal DTO från `/session`, exponera inte `ExternalId`/PII (fynd 8).
   - Sluta logga fullständig callback-nyttolast/PII, höj loggnivå i produktion (fynd 9).
   - Åtgärda förfalskningsbar sessionscookie/roll och cookie-flaggor i auth-biblioteket; förlita dig inte på cookieroll för `/list` (fynd 13).
4. **Låga + härdning:**
   - Parametrisera insert/update och använd invariant kultur (fynd 10).
   - `TrustServerCertificate=False` + `Encrypt=True` (fynd 11).
   - Lägg till HSTS/säkerhetsheaders och rate limiting; ta bort oanvänd CORS-policy och `GetExternalHtmlAsync`; verifiera beroendeversioner.

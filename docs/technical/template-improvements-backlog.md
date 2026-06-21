# Backlog ulepszeń szablonu ChangeMe

> **Zakres:** pomysły na rozwój repozytorium-szablonu (maintainerzy pakietu NuGet / `dotnet new changeme`).
>
> **Cel docelowy:** szablon jest mocny do rozwoju lokalnego i nauki wzorców; poniższe punkty zbliżają go do startera gotowego do wdrożenia produkcyjnego.

## Co już działa dobrze

- Jednolite skrypty w root `package.json` (start, build, test, EF, Docker, E2E, requirements).
- Architektura backendu (Web → UseCases → Domain → Infrastructure) z realnymi wzorcami: JWT, RBAC, Hangfire, SignalR, załączniki.
- CI w czterech równoległych jobach: requirements, frontend, backend, E2E z Playwright.
- Dokumentacja na trzech poziomach (`guides/`, `technical/`, `requirements/`) plus przewodnik dla agentów AI (`AGENTS.md`).
- Pakietowanie `dotnet new changeme` z tokenem `ChangeMe` (`.template.config/`, `template-pack/`).

---

## Wysoki priorytet

### 2. CI bez lint i format

**Problem:** `.github/workflows/ci.yml` uruchamia test + build, ale nie `lint:frontend`, `format:check:all` ani ESLint. Regresje stylu/konwencji mogą przejść bez sygnału.

**Propozycje:**

- Dodać job lub kroki: `npm run lint:frontend`, `npm run format:check:all`.
- Zaktualizować `docs/technical/ci.md` po wdrożeniu.

Skrypty już istnieją w root `package.json`.

### 4. Bezpieczeństwo „out of the box”

**Problem:** Szablon ma ostrzeżenia w docs, ale brakuje jednej checklisty startowej dla konsumentów wdrażających poza Docker Compose.

**Propozycje (checklist — częściowo w `deployment.md`):**

- Rotacja JWT signing key (placeholder w `appsettings.json`).
- Zabezpieczenie dashboardu Hangfire (obecnie bez auth w dev).
- Jawna konfiguracja CORS na produkcję (`AllowedOrigins` puste w bazowym `appsettings.json`).

**Planowane uzupełnienia (osobne punkty poniżej):**

- Rate limiting na endpointach auth i wrażliwych trasach — [§ Rate limiting](#planowane-rate-limiting).
- Content Security Policy (CSP) dla frontendu — [§ CSP](#planowane-csp-content-security-policy).

---

## Analiza kodu i bezpieczeństwa — profil Compose

> **Status:** wdrożone (2026-06-21) — Trivy + Gitleaks + Semgrep + ZAP + SonarQube CE w `docker-compose.analyze.yml`; raporty w `artifacts/`. Dokumentacja: [security-analysis.md](security-analysis.md).

### Co chcemy pokryć

| Warstwa              | Pytanie                                      | Przykładowe narzędzia                                         |
| -------------------- | -------------------------------------------- | ------------------------------------------------------------- |
| **Jakość / SAST**    | Duplikaty, complexity, code smells, coverage | SonarQube, Semgrep                                            |
| **Zależności (SCA)** | CVE w npm / NuGet / obrazach Docker          | Trivy, Grype, `npm audit`, `dotnet list package --vulnerable` |
| **Sekrety**          | Klucze/tokeny w repo lub historii            | Gitleaks, TruffleHog                                          |
| **DAST**             | Skan działającej aplikacji (HTTP)            | OWASP ZAP (baseline / full)                                   |
| **Obrazy**           | CVE w warstwach frontend/backend             | Trivy (image scan)                                            |

### Propozycja architektury Compose

```text
docker-compose.yml           # app stack (frontend, backend, postgres, mailhog)
docker-compose.analyze.yml   # profile security / analyze — narzędzia analityczne
npm run compose:analyze      # merge obu plików dla skanów
```

npm run analyze:deps → Trivy/Grype (filesystem + lockfiles)
npm run analyze:secrets → Gitleaks (repo mount)
npm run analyze:sast → Semgrep LUB skaner Sonar (patrz decyzja)
npm run analyze:dast → ZAP baseline vs http://host.docker.internal:4200
npm run analyze:all → sekwencja lekkich skanów (bez SonarQube server)

````

**SonarQube** jako osobny serwis w Compose (`sonarqube` + `sonarqube-db`) — cięższy (~2 GB RAM), sensowny gdy chcesz UI, historię metryk i jeden dashboard dla FE+BE. **Semgrep / Trivy / Gitleaks / ZAP** — lekkie joby `run --rm`, bez stałego serwera.

### Narzędzia do wyboru — zalety i wady

#### SonarQube (Community + skanery SonarScanner)

|                  |                                                                                                                                                                        |
| ---------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Plusy**        | Jedno UI dla C#, TS, coverage; reguły jakości; trend w czasie; znane w enterprise                                                                                      |
| **Minusy**       | Wymaga stałego kontenera + PostgreSQL; ~4–8 GB RAM komfortowo; więcej konfiguracji (`sonar-project.properties` × 2); Community Edition ma limity (np. branch analysis) |
| **W Compose**    | `sonarqube`, `sonarqube-db`, profile `security`; skanery: `sonar-scanner-cli` (FE), `dotnet-sonarscanner` (BE)                                                         |
| **Dla ChangeMe** | Dobry wybór, jeśli priorytetem jest **jakość kodu + jeden dashboard**, a maintainer akceptuje koszt RAM i setup                                                        |

#### Semgrep (OSS / CE)

|                  |                                                                                |
| ---------------- | ------------------------------------------------------------------------------ |
| **Plusy**        | Lekki (`run --rm`), szybki, reguły dla C#/TS/Angular; brak serwera; łatwy w CI |
| **Minusy**       | Inne metryki niż Sonar (mniej „technical debt dashboard”); coverage osobno     |
| **W Compose**    | Jednorazowy kontener, mount repo                                               |
| **Dla ChangeMe** | Najlepszy stosunek effort/wartość na **SAST bez infrastruktury**               |

#### Trivy

|                  |                                                                                    |
| ---------------- | ---------------------------------------------------------------------------------- |
| **Plusy**        | Jeden tool: filesystem (lockfiles), obrazy Docker, IaC; JSON/SARIF; aktywny rozwój |
| **Minusy**       | Fałszywe alarmy na devDependencies; nie zastępuje SAST                             |
| **W Compose**    | `run --rm` na `/repo` i na `changeme-frontend`, `changeme-backend`                 |
| **Dla ChangeMe** | **Rekomendowany** jako podstawa SCA + scan obrazów                                 |

#### Grype (Anchore)

|                  |                                                                            |
| ---------------- | -------------------------------------------------------------------------- |
| **Plusy**        | Podobny do Trivy, dobry SBOM                                               |
| **Minusy**       | Drugi tool obok Trivy — zwykle wystarczy jeden                             |
| **Dla ChangeMe** | Rozważyć tylko jeśli zespół już używa Anchore; inaczej **Trivy wystarczy** |

#### Gitleaks

|                  |                                                                |
| ---------------- | -------------------------------------------------------------- |
| **Plusy**        | Bardzo lekki, skan sekretów w git tree; szybki pre-commit / CI |
| **Minusy**       | Nie wykrywa logiki biznesowej ani CVE                          |
| **W Compose**    | `run --rm -v repo:/repo`                                       |
| **Dla ChangeMe** | **Rekomendowany** — niski koszt, wysoka wartość                |

#### OWASP ZAP (baseline / full scan)

|                  |                                                                                             |
| ---------------- | ------------------------------------------------------------------------------------------- |
| **Plusy**        | Standard DAST; baseline szybki; pełny skan głębszy                                          |
| **Minusy**       | Wymaga **działającego stacku** (frontend+backend); pełny skan wolny; fałszywe alarmy na SPA |
| **W Compose**    | `zap-baseline.py` / `zap-full-scan.py` → `http://host.docker.internal:4200`                 |
| **Dla ChangeMe** | **Rekomendowany** jako okresowy DAST; baseline w profilu `security`, full opcjonalnie       |

#### npm audit / dotnet list package --vulnerable

|                  |                                                  |
| ---------------- | ------------------------------------------------ |
| **Plusy**        | Zero nowych obrazów; już dostępne                |
| **Minusy**       | Tylko znane CVE w rejestrach; bez obrazów Docker |
| **Dla ChangeMe** | **Must-have w CI** (tanie); Compose opcjonalnie  |

#### CodeQL / GitHub Advanced Security

|                  |                                                                    |
| ---------------- | ------------------------------------------------------------------ |
| **Plusy**        | Głęboka analiza w GitHub Actions                                   |
| **Minusy**       | Nie lokalne Compose; wymaga GH (private repo — płatne funkcje)     |
| **Dla ChangeMe** | Osobna decyzja **CI vs Compose** — nie zastępuje lokalnego profilu |

### Macierz rekomendacji (skrót)

| Priorytet      | Narzędzie                     | Compose                             | CI (później) |
| -------------- | ----------------------------- | ----------------------------------- | ------------ |
| **1 — must**   | Trivy (deps + images)         | tak                                 | tak          |
| **1 — must**   | Gitleaks                      | tak                                 | tak          |
| **2 — should** | Semgrep **lub** SonarQube     | tak (wybór jednego)                 | tak          |
| **2 — should** | OWASP ZAP baseline            | tak (stack musi działać)            | opcjonalnie  |
| **3 — nice**   | npm audit + dotnet vulnerable | skrypt shell                        | tak          |
| **3 — nice**   | SonarQube server              | tylko jeśli wybrany zamiast Semgrep | —            |

**Pragmatyczny pakiet startowy (mniej RAM):** Trivy + Gitleaks + Semgrep + ZAP baseline — bez stałego SonarQube.

**Pakiet „jeden dashboard”:** SonarQube + Trivy + Gitleaks + ZAP baseline.

### Plan decyzji (dla maintainera)

```mermaid
flowchart TD
    A["1. Czy potrzebujesz stałego UI metryk jakości?"] -->|Tak| B["SonarQube + DB w profile security"]
    A -->|Nie / mało RAM| C["Semgrep zamiast Sonar"]
    B --> D["2. SCA: Trivy na repo + obrazy Compose"]
    C --> D
    D --> E["3. Gitleaks w profile security"]
    E --> F["4. DAST: ZAP baseline vs działający stack"]
    F --> G["5. Skrypty npm run analyze:* + docs/technical/security-analysis.md"]
    G --> H["6. Które skany w CI vs tylko lokalnie?"]
    H --> I["7. Proof of concept: jeden PR z profile security"]
````

**Krok po kroku:**

1. **RAM i cadence** — czy SonarQube ma sens lokalnie (≥8 GB wolnego), czy tylko CI / okazjonalnie?
2. **SAST: Semgrep vs SonarQube** — wybierz **jedno** na start (tabela powyżej).
3. **SCA** — zaakceptuj **Trivy** jako default; zdefiniuj `--severity HIGH,CRITICAL` na początek.
4. **Sekrety** — **Gitleaks**; `config/gitleaks.toml` z allowlistą (np. `secrets.json.example`, demo hasła w appsettings.Development).
5. **DAST** — **ZAP baseline** po `docker compose up`; target: `http://host.docker.internal:4200`; raport HTML w `./artifacts/zap/`.
6. **Interfejs** — dodać do root `package.json` np. `analyze:all`, `analyze:deps`, `analyze:secrets`, `analyze:sast`, `analyze:dast`; dokument w `docs/technical/security-analysis.md`.
7. **CI** — które skany blokują merge (propozycja: Gitleaks + Trivy HIGH; Semgrep/ZAP na start jako `continue-on-error` lub scheduled).
8. **PoC** — jeden branch `feat/security-compose-profile` z `docker-compose.analyze.yml` overlay (merge z `docker-compose.yml`).

**Decyzje (zapisane przy wdrożeniu):**

| #   | Pytanie                        | Decyzja                                                                |
| --- | ------------------------------ | ---------------------------------------------------------------------- |
| 1   | SonarQube vs Semgrep           | ☑ oba (Semgrep w `analyze:all`, SonarQube `analyze:sonar`)             |
| 2   | ZAP baseline w CI?             | ☑ tylko lokalnie (CI — później)                                        |
| 3   | Trivy fail on                  | ☑ HIGH+CRITICAL                                                        |
| 4   | Gitleaks w pre-commit?         | ☑ tylko Compose / CI (później)                                         |
| 5   | Osobny plik compose vs profile | ☑ `docker-compose.analyze.yml` + merge przez `npm run compose:analyze` |

---

## Planowane: Rate limiting

> **Priorytet:** wysoki / średni (razem z hardening auth). **Status:** nie wdrożone.

**Cel:** ograniczenie brute-force i nadużyć na endpointach auth (`/auth/login`, `/auth/refresh`) i opcjonalnie globalny limit per IP.

**Propozycja implementacji (.NET 10):**

- `Microsoft.AspNetCore.RateLimiting` — fixed/sliding window per IP (lub per user po zalogowaniu).
- Polityki w `Program.cs` / `*Config.cs`: np. `auth` — 10 req/min na IP; `api` — wyższy limit.
- Odpowiedź `429` + `Retry-After`; integracja z FastEndpoints.
- Konfiguracja w `appsettings`: `RateLimitingOptions` (włącz/wyłącz, limity) — domyślnie **włączone w Production**, łagodne w Development.
- Test integracyjny: N+1 login → 429.

**Powiązane:** `deployment.md`, backlog §4, ewentualnie wyjątek w ZAP (oczekiwane 429).

---

## Planowane: CSP (Content Security Policy)

> **Priorytet:** średni (po ustabilizowaniu deploymentu). **Status:** nie wdrożone.

**Cel:** ograniczenie XSS — jawne źródła skryptów, stylów, połączeń (`connect-src` dla API i SignalR).

**Warstwy:**

| Warstwa                | Działanie                                                                                                                 |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| **nginx** (production) | Nagłówek `Content-Security-Policy` na odpowiedziach SPA; `connect-src 'self'` wystarczy przy same-origin `/api` i `/hubs` |
| **Angular**            | Unikać inline scripts poza `runtime-config.js`; PrimeNG / Google Fonts — `style-src` / `font-src`                         |
| **Dev (`ng serve`)**   | CSP opcjonalnie łagodniejsze lub wyłączone — Vite HMR wymaga `unsafe-eval` / websocket                                    |

**Trudności w tym stacku:**

- Google Fonts w `index.html` → `style-src` / `font-src https://fonts.googleapis.com https://fonts.gstatic.com`
- SignalR WebSocket → `connect-src 'self' wss:` (przy HTTPS)
- **`runtime-config.js`** — bez nonce na start jako osobny plik statyczny (akceptowalne); docelowo nonce wymaga generowania w nginx per request (trudniejsze)

**Plan wdrożenia:**

1. Polityka **report-only** (`Content-Security-Policy-Report-Only`) w nginx — zebrać naruszenia bez psucia UI.
2. Zaostrzenie do enforcing po korektach.
3. Dokumentacja w `deployment.md` + przykład dla split-host (`connect-src` z pełnym URL API).
4. Test: E2E smoke + ręczny check konsoli przeglądarki.

**Powiązane:** backlog §4, `nginx.conf`, `index.html`.

---

## Średni priorytet

### 5. Profil Compose — analiza i bezpieczeństwo — **zrobione (2026-06-21)**

**Wdrożenie:** `docker-compose.analyze.yml` (profile `security` / `analyze`), `npm run compose:analyze` + `analyze:*`, raporty w `artifacts/`, SonarQube CE (`analyze:sonar:up`), `config/gitleaks.toml`, [security-analysis.md](security-analysis.md).

**Następny krok:** wybrane skany w CI (Gitleaks + Trivy jako bramki merge).

### 6. Rate limiting i CSP

**Status:** zaplanowane — [§ Rate limiting](#planowane-rate-limiting), [§ CSP](#planowane-csp-content-security-policy).

### 7. Governance OSS

**Brakuje:**

- `SECURITY.md` (zgłaszanie luk),
- szablonów PR/issue w `.github/`,
- `CHANGELOG.md` (szczególnie przy publikacji NuGet),
- Dependabot/Renovate,
- opcjonalnie `npm audit` / skan pakietów .NET w CI.

### 8. Drobne niespójności w dokumentacji — **zrobione (2026-06-21)**

- `e2e-guidelines.md` dodany do `docs/README.md`.
- Root `README.md` i `template-content/generated-readme/README.md` — spójna instalacja frontendu (`npm run install:frontend`).
- `docs/guides/backend-guidelines.md` — plik w repo (do commitu w git).

### 9. Pokrycie testami frontendu

**Stan:** Backend ma solidne unit + integration; frontend ma kilka speców i głównie E2E smoke (auth, users, roles, issues).

**Propozycje:**

- Świadoma decyzja: opisać w `testing-guidelines.md`, że FE weryfikujemy głównie E2E.
- Albo dodać reprezentatywne unit testy (serwisy auth, guardy, formularze).

---

## Niski priorytet (nice-to-have)

| Obszar             | Propozycja                                                          |
| ------------------ | ------------------------------------------------------------------- |
| Pre-commit         | Husky + lint-staged na `format:check` / ESLint                      |
| Coverage           | `test:frontend:coverage` w CI (nawet bez progu merge)               |
| Node version       | `"engines"` w root `package.json` (CI używa Node 24)                |
| i18n               | NFR opisuje i18n, aplikacja EN-only — sekcja „future work” w docs   |
| Dockerfile backend | Komentarz po polsku vs reszta repo po angielsku — ujednolicić język |
| CODE_OF_CONDUCT    | Opcjonalnie dla OSS                                                 |

---

## Znane niespójności (technical debt)

1. **Filozofia testów** — `testing-guidelines.md` odradza zbędne testy (dobrze), ale asymetria FE (E2E) vs BE (unit+integration) może mylić nowych contributorów bez jawnego opisu.

---

## Rekomendowana kolejność prac

```mermaid
flowchart TD
    A["1. CI: lint + format"] --> D["2. Security Compose profile — decyzja + PoC"]
    D --> R["3. Rate limiting + CSP"]
    R --> E["4. GitHub templates + Dependabot"]
```

---

## Podsumowanie

Szablon ChangeMe jest **ponadprzeciętny** jako starter full-stack z dokumentacją i testami. Największy zwrot da:

1. Bramki jakości w CI (lint, format).
2. **Profil `security` w Compose** — Trivy + Gitleaks + (Semgrep **lub** SonarQube) + ZAP baseline w `docker-compose.analyze.yml`; decyzja maintainera w tabeli w backlogu.
3. **Rate limiting** i **CSP** — hardening po PoC analiz.
4. Governance OSS (Dependabot, SECURITY.md).
5. Checklist bezpieczeństwa — częściowo w `deployment.md`; rozszerzyć po rate limit / CSP.

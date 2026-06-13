# Testing Guidelines

> Scope: which test layer owns which behavior, and when **not** to add automated tests. For commands, CI, and test project paths, see `AGENTS.md` and `docs/technical/ci.md`.

## Core rule

Each test must catch a failure that **no lower layer already covers**. Ground scenarios in touched `FR-*` bullets and inherited `FR-UI-001` / `_shared/` docs — not ad-hoc acceptance tables.

Prefer the **lowest** layer that can prove the requirement. Extend existing tests before adding a higher layer.

## Layer ownership

| Layer                     | Owns                                                                                   | Does not own                                         |
| ------------------------- | -------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| Backend unit              | Domain invariants, aggregate behavior, small helpers without app startup               | HTTP, auth middleware, persistence                   |
| Backend integration       | Routes, status codes, auth, server validation, persistence side effects, API contracts | Angular routing, templates, browser session          |
| Frontend unit / component | Client logic, forms, guards, UI state, service orchestration (mocked `ApiService`)     | Server rules already proven through HTTP             |
| E2E                       | Multi-screen user journeys, session/cookies, compliance gates, WebAuthn, SignalR       | Per-field API validation, exhaustive CRUD per screen |

Colocate frontend specs next to the source file (`*.spec.ts`). Integration tests live under `tests/ChangeMe.Backend.IntegrationTests/Endpoints/<Feature>/`.

## Backend tests

### Unit

- Domain invariants and aggregate behavior.
- Infrastructure helpers that do not need full app startup.

Skip when behavior exists only at the HTTP boundary or the type has no rules.

### Integration

- Endpoint happy path, validation failures, auth (anonymous / authenticated / forbidden), and persistence when data changes.

Skip when there is no API or persistence change.

When adding or changing an endpoint, add or update integration tests in the same PR (see `docs/guides/feature-recipes.md`).

## Frontend tests

Vitest + TestBed. Mock HTTP through `ApiService`; stub heavy child components and the layout shell.

### Add or update when

- TypeScript logic changes (not markup-only).
- Non-trivial form validation, guards, or compliance redirects.
- Permission- or security-sensitive conditional UI.
- A client-side regression (stale state, double submit, wrong post-error UI).

### Skip when

- Markup, layout, or styling only — lint and manual check.
- Duplicating server validation already covered by integration tests.
- Smoke tests with no asserted behavior (`should create the component`, PrimeNG widget presence).
- Real backend calls or full-template snapshots.

Test observable behavior (copy, disabled controls, navigation), not private fields.

## E2E tests

Use only when unit and integration tests cannot prove the journey (browser session, multi-step auth/compliance, realtime, WebAuthn).

### Add or update when

- A new or changed **user journey** in `FR-*` spans client + server + session.
- Compliance gates or strict setup modes (`docs/requirements/_shared/reference/compliance-gates.md`).
- Behavior depends on cookies, browser APIs, SignalR, or passkeys.

### Skip when

- The change is API-only, a single form, or one list screen — integration + frontend unit suffice.
- The scenario only re-checks status codes, field validation, or “save shows toast” with no unique routing logic.

Keep the suite small and stable. Do not mirror integration test matrices in the browser. Specs live in `src/ChangeMe.Frontend/e2e/`; run via `AGENTS.md` (`npm run test:e2e`). CI runs the smoke suite on every PR — see `docs/technical/ci.md`.

## Scenario templates

Pick the lowest layers that satisfy the `FR-*` bullets.

| Change                      | Backend unit        | Integration                          | Frontend unit                                                       | E2E                                                       |
| --------------------------- | ------------------- | ------------------------------------ | ------------------------------------------------------------------- | --------------------------------------------------------- |
| List screen                 | If new domain rules | GET, filters, pagination, auth       | Filter → query mapping, chips, client empty state                   | Optional: login → row → details for new domain or routing |
| Create / edit form          | If new domain rules | POST/PUT, server validation, auth    | Client validators, submit state, success navigation (mocked router) | Only multi-step or compliance workflows                   |
| Auth / session / compliance | If new domain rules | Flags, middleware, token endpoints   | Guards, toasts, redirects                                           | Required when user-visible gate journey changes           |
| Visual / layout only        | —                   | —                                    | —                                                                   | —                                                         |
| API contract only           | If domain changed   | **First** — contract source of truth | Models/services if mapping changed                                  | Only if user flow changes                                 |

Record E2E as `required`, `optional`, or `skip` (with reason) in `docs/requirements/changes/` when the change touches a user journey.

## Guardrails for AI agents

- Before adding a test, name the failure class and confirm no lower layer already covers it.
- Do not repeat HTTP contracts in frontend or E2E tests.
- Run the smallest relevant check for the change (`AGENTS.md`); integration tests need Docker (Testcontainers).
- Do not invent scenarios outside `FR-*`, inherited UI patterns, or an explicit regression.

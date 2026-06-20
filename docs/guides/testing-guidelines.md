# Testing Guidelines

> Scope: **default is no new test** unless a lower layer cannot catch the failure. Which layer owns what, anti-patterns, scenario pickers, and what to run before a PR.
>
> Commands, CI, paths, and integration-test setup: [`AGENTS.md`](../../AGENTS.md), [`docs/technical/ci.md`](../technical/ci.md), [`docs/technical/database-and-docker.md`](../technical/database-and-docker.md), and [repo-map.md](repo-map.md#test-map).

## Core rule

**Default: do not add a test** unless you can name a failure class that **no lower layer already covers**.

When you add one: ground it in touched `FR-*` bullets and inherited `FR-UI-001` / `_shared/` docs — not ad-hoc acceptance tables. Use the **lowest** layer that can prove the requirement; extend existing tests before adding a higher layer.

Automated tests do **not** use `ChangeMe.Backend.DataGenerator` — they seed via `IssueTestHelper`, `TestAuthHelper`, and Testcontainers ([repo-map.md](repo-map.md#test-map), [`data-generator.md`](../technical/data-generator.md)).

## Layer ownership

| Layer                     | Owns                                                                                   | Does not own                                         |
| ------------------------- | -------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| Backend unit              | Domain invariants, aggregate behavior, small helpers without app startup               | HTTP, auth middleware, persistence                   |
| Backend integration       | Routes, status codes, auth, server validation, persistence side effects, API contracts | Angular routing, templates, browser session          |
| Frontend unit / component | Client logic, forms, guards, UI state, service orchestration (mocked `ApiService`)     | Server rules already proven through HTTP             |
| E2E                       | Multi-screen user journeys, session/cookies, SignalR                                   | Per-field API validation, exhaustive CRUD per screen |

Colocate frontend specs as `*.spec.ts` next to the source. Integration tests: `src/ChangeMe.Backend/tests/ChangeMe.Backend.IntegrationTests/Endpoints/<Feature>/`.

## Anti-patterns

**Do not** add or extend automated tests when:

| Layer               | Skip                                                                                                                                                                                                                   |
| ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Any                 | A lower layer already covers the failure; scenario is outside `FR-*`, `_shared/`, or an explicit regression                                                                                                            |
| Backend unit        | Behavior exists only at the HTTP boundary; the type has no domain rules                                                                                                                                                |
| Backend integration | No API or persistence change                                                                                                                                                                                           |
| Frontend unit       | Markup, layout, or styling only; duplicating server validation; smoke with no real assertion (`should create`, widget presence); real HTTP; full-template snapshots; asserting private fields instead of observable UI |
| E2E                 | API-only change, single form, or one list screen; re-checking status codes, field validation, or “save shows toast” without unique routing; mirroring integration test matrices in the browser                         |

**E2E only when** unit and integration cannot prove the journey: multi-step auth flows, cookies or browser APIs, SignalR (see FR-AUTH-001 for sign-in behavior). Record `required` / `optional` / `skip` (with reason) in `docs/requirements/changes/` when a user journey changes.

When frontend unit tests are warranted: Vitest + TestBed; mock `ApiService`; stub heavy children and layout shell; assert copy, disabled controls, and navigation.

## Scenario templates

Pick the lowest layers that satisfy the `FR-*` bullets. Dash (`—`) means **skip that layer**.

| Change                      | Backend unit        | Integration                          | Frontend unit                                                       | E2E                                                       | Minimum before PR                                                                  |
| --------------------------- | ------------------- | ------------------------------------ | ------------------------------------------------------------------- | --------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| List screen                 | If new domain rules | GET, filters, pagination, auth       | Filter → query mapping, chips, client empty state                   | Optional: login → row → details for new domain or routing | Skip E2E unless routing or domain is new                                           |
| Create / edit form          | If new domain rules | POST/PUT, server validation, auth    | Client validators, submit state, success navigation (mocked router) | Only multi-step or compliance workflows                   | Skip E2E for single-screen CRUD                                                    |
| Auth / session / compliance | If new domain rules | Flags, middleware, token endpoints   | Guards, toasts, redirects                                           | Required when user-visible gate journey changes           | Integration: anonymous, authenticated, forbidden as applicable                     |
| Visual / layout only        | —                   | —                                    | —                                                                   | —                                                         | `npm run lint:frontend` only — no new automated tests                              |
| API contract only           | If domain changed   | **First** — contract source of truth | Models/services if mapping changed                                  | Only if user flow changes                                 | Skip frontend/E2E when only server contract changed and mapping is already covered |

CI does **not** run ESLint or formatting — run `npm run lint:frontend` and `npm run format:check:all` locally when you touched matching code ([`ci.md`](../technical/ci.md)).

## What to run

Prefer the **smallest** relevant check. Command details: [`AGENTS.md`](../../AGENTS.md).

| Change                          | Run                                                                                                                                           |
| ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| Frontend logic or services      | `npm run lint:frontend` plus `npm run test:frontend:ci` (or affected specs)                                                                   |
| Backend domain or helpers only  | `npm run test:backend:unit`                                                                                                                   |
| Backend endpoint or persistence | `npm run test:backend:integration` — Docker required; migrations must exist ([`database-and-docker.md`](../technical/database-and-docker.md)) |
| User journey or wide regression | `npm run test:all`; add `npm run test:e2e` when the journey or compliance gate changed                                                        |
| No Docker available             | `npm run test:frontend:ci` and `npm run test:backend:unit` first; integration when Docker is up                                               |

## Guardrails for AI agents

- Name the failure class before adding a test; if a lower layer covers it, **stop**.
- Do not repeat HTTP contracts in frontend or E2E tests.
- Do not invent scenarios outside `FR-*`, inherited UI patterns, or an explicit regression.
- Markup-only frontend change: lint — not component smoke tests.
- Cross-stack change: follow **What to run**; integration tests need Docker (Testcontainers).

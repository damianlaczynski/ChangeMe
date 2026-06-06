# 2026-06-06 — Migrate REQ-_ to FR-_ / NFR-_ / AC-_ model

**Status:** done (2026-06-06)

## Why

Replace opaque `REQ-*` identifiers with an explicit model: functional specifications (`FR-*`), non-functional requirements (`NFR-*`), and acceptance scenarios (`AC-*`). Centralize shared UI and quality docs under `docs/requirements/_shared/`.

## Functional specifications touched

All 48 legacy `REQ-*` files migrated to `docs/requirements/functional/<domain>/fr-*.md` with:

- `## Functional requirements` (was **Features**)
- `## Acceptance scenarios` (new `AC-*` tables)
- `## Non-functional requirements` (inherits `NFR-*` and `FR-UI-001`)

## Non-functional and shared docs touched

| ID / path             | Action                                                                  |
| --------------------- | ----------------------------------------------------------------------- |
| `FR-UI-001`           | **New** — `_shared/functional/ui-patterns.md` (from legacy ui-patterns) |
| `NFR-QUAL-001`        | **New** — `_shared/non-functional/product-quality.md` (index hub)       |
| `NFR-I18N-001`        | **New** — `_shared/non-functional/internationalization.md`              |
| `NFR-A11Y-001`        | **New** — `_shared/non-functional/accessibility.md`                     |
| `NFR-RSP-001`         | **New** — `_shared/non-functional/responsiveness.md`                    |
| `NFR-PERF-001`        | **New** — `_shared/non-functional/performance-and-scale.md`             |
| `_shared/reference/*` | **Moved** — glossary, account-model, compliance-gates, permissions      |

## Behavior delta

**Before:** `docs/req/` with `REQ-*` IDs, `## Features` sections, no acceptance scenario tables, mixed NFR inline.

**After:**

- Root: `docs/requirements/` with `functional/`, `_shared/{reference,functional,non-functional}/`, `changes/`.
- IDs: `FR-*` (specifications), `NFR-*` (quality), `AC-*` (scenarios).
- Validation: `npm run requirements:validate` (alias `req:validate`).
- Legacy `docs/req/` removed.

Specification **behavior text is unchanged** except ID/path renames and acceptance scenario tables.

**Out of scope on this branch:** `projects/`, `time/`, and `billing/` functional specifications (maintained on a separate branch).

## Implementation scope

- Docs and scripts only: `requirements-validate.mjs`, `requirements-readme.mjs`, process doc, templates, `AGENTS.md`, CI validation job.

# 2026-06-06 — Close REQ → FR migration on main branch

**Status:** done (2026-06-06)

## Why

Finish the documentation migration started in `2026-06-06-req-to-fr-migration.md`: remove legacy tooling, fix validation gaps, align terminology, and wire CI so the new structure stays enforceable.

## Functional specifications touched

| FR domain           | Action                                                                                                                |
| ------------------- | --------------------------------------------------------------------------------------------------------------------- |
| `access/`, `users/` | Terminology: **Out of scope for this REQ** → **Out of scope**; **this REQ** → explicit `FR-*` references where needed |

## Non-functional and shared docs touched

| ID / path                                 | Action                                                     |
| ----------------------------------------- | ---------------------------------------------------------- |
| `_shared/reference/permissions.md`        | Fixed link to `fr-rol-001` (`FR-ROL-001`)                  |
| `_shared/functional/ui-patterns.md`       | **REQ documents** → **Functional specifications document** |
| `_shared/non-functional/accessibility.md` | **Out of scope for this REQ** → **Out of scope**           |

## Behavior delta

**Before:** `docs/req/` coexisted with partial legacy scripts; `npm run requirements:validate` failed on padded AC table headers in `users/`; CI did not check requirements; stale **REQ** wording remained in shared docs.

**After:**

- Legacy `docs/req/` fully removed.
- One-time scripts removed (`requirements-migrate.mjs`, `req-migrate.mjs`, deprecated `req-validate.mjs` / `req-readme.mjs`).
- Validator accepts flexible AC table column spacing.
- CI job **Requirements** runs `npm run requirements:validate`.
- `projects/`, `time/`, and `billing/` domains explicitly deferred to a separate branch.

No product behavior change — documentation and tooling only.

## Implementation scope

- `scripts/requirements-validate.mjs`, `.github/workflows/ci.yml`, `docs/requirements-change-process.md`, change records, terminology pass on affected FR files.

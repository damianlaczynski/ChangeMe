# 2026-06-06 — Shared UI patterns and product quality docs

**Status:** done (2026-06-06)

## Why

Reduce duplication across functional specifications by centralizing recurring list, form, and feedback patterns (Direction 6) and non-functional expectations for accessibility, responsiveness, performance, and copy (Direction 7).

## Functional specifications touched

None — no atomic functional specification behavior changed. Authors inherit the new shared docs when writing or updating REQs.

## Shared docs touched

| File                         | Action                                                              |
| ---------------------------- | ------------------------------------------------------------------- |
| `_shared/ui-patterns.md`     | **New** — lists, forms, details, back navigation, feedback channels |
| `_shared/product-quality.md` | **New** — i18n, accessibility, responsiveness, performance, errors  |

## Behavior delta

**Before:** Each REQ repeated pagination, filter, validation, toast, and permission-denial rules independently.

**After:**

- List, form, detail, and feedback defaults live in `_shared/ui-patterns.md`; REQs document only overrides.
- Accessibility, English canonical copy, viewport targets, pagination scale, and export limits live in `_shared/product-quality.md`.
- `requirements:validate` requires both files; README index lists them.

## Implementation scope

- Docs and tooling only (`requirements-validate.mjs`, `requirements-readme.mjs`, functional specification template, change process).

# Functional specification template

> Copy the block below to `docs/requirements/functional/<domain>/fr-<area>-<nnn>-<slug>.md`.
> **How to write the content:** `docs/requirements/requirements-authoring-guide.md`.
> **Workflow and change record:** `docs/requirements/requirements-change-process.md`.

```markdown
---
id: FR-XXX-NNN
title: Specification Name
domain: identity | users | invitations | access | passkeys | issues
type: functional
status: active
depends_on: [FR-YYY-MMM]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The user must be able to `<main business outcome>`.

## Functional requirements

### Access

- Screen: **Screen name**
- `<permission, navigation, and visibility rules>`

### `<Feature section>`

- `<behavior description>`

### Validation

- **`<Field>`**: `<validation rule>`.
- Validation errors are shown next to the relevant fields without closing the form.

### Form actions

- **Cancel** button — `<behavior>`.
- **Save** button — `<success / failure behavior>`.

### States and business rules

- `<business rule>`

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) unless stated above.
- Document only overrides below.

## Out of scope

- `<deliberate exclusion>`
```

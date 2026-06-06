# REQ Template

> Template for a **single atomic** requirement file. Copy to `docs/req/<domain>/req-<area>-<nnn>-<slug>.md` and add a row to the change record in `docs/req/changes/`.

```markdown
---
id: REQ-XXX-NNN
title: Requirement Name
domain: identity | users | invitations | access | passkeys | projects | issues | time | billing
status: active
depends_on: [REQ-YYY-MMM]
---

## Goal

The user must be able to `<main business outcome>`.

## Features

### Functional section

- `<behavior description>`

### Validation

- **`<Field>`**: `<validation rule>`.
- Validation errors are shown next to the relevant fields without closing the form.

### Form actions

- **Cancel** button — `<behavior>`.
- **Save** button — `<success / failure behavior>`.

### States and business rules

- `<business rule>`

### Permissions and visibility

- `<who can see the feature>` — see `docs/req/_shared/permissions.md` for permission names.

### Out of scope for this REQ

- `<deliberate exclusion>`
```

---

## Writing guidelines

### Scope

- Describe **what the user and the system must do** from a business and interaction perspective.
- One file = one coherent REQ. Cross-cutting terms live in `docs/req/_shared/` — link, do not duplicate.
- When referring to behavior in another REQ, cite its identifier (for example REQ-AUTH-001).

### Structure

- Keep identifiers in the `REQ-<AREA>-NNN` format.
- **Goal** states the business outcome.
- **Features** states user-visible behavior, flows, validations, messages, and business rules.
- Deliberate exclusions use **Out of scope for this REQ**.

### Clarity and precision (mandatory)

Requirements must be **unambiguous**. A developer or tester must not need to guess what to build or verify.

- **Do not use** vague qualifiers: _optional_, _recommended_, _may_, _might_, _where applicable_, _when possible_, _or similar_, _TBD_, _etc._
- **State exact UI copy** for errors, confirmations, empty states, button labels, and success messages when wording matters.
- **State exact defaults**, visibility rules, and navigation after success and failure.
- **Use "not required"** for empty-allowed fields; **use "must not" / "cannot"** for prohibitions.
- **One behavior, one bullet.**

### No implementation details (mandatory)

**Do not include:** source code, API endpoints, HTTP details, database tables, migrations, or infrastructure keys.

**Instead describe:** user actions, screen flows, business entities, permission names from `_shared/permissions.md`, and observable outcomes.

### Shared docs

| Concept            | Document                               |
| ------------------ | -------------------------------------- |
| Business terms     | `docs/req/_shared/glossary.md`         |
| Account attributes | `docs/req/_shared/account-model.md`    |
| Post-sign-in gates | `docs/req/_shared/compliance-gates.md` |
| Permission names   | `docs/req/_shared/permissions.md`      |

### After editing

1. Add or update a record in `docs/req/changes/`.
2. Run `npm run req:validate`.

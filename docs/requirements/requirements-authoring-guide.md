# Requirements authoring guide

> **Audience:** analysts and authors of functional specifications (`FR-*`).
> **Workflow** (change records, validation, developer handoff): `docs/requirements/requirements-change-process.md`.
> **Skeleton** to copy for a new file: `docs/requirements/_functional-specification-template.md`.

## New vs updated specification

| Situation          | What to do                                                                                                                                                                                                                                                                   |
| ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **New `FR-*`**     | Copy `_functional-specification-template.md` to `docs/requirements/functional/<domain>/fr-<area>-<nnn>-<slug>.md`. Assign the next free `FR-<AREA>-NNN` in that domain (see existing files and `docs/requirements/README.md`). Fill frontmatter and sections per this guide. |
| **Updated `FR-*`** | Edit the existing file only. Do not split one specification across multiple files. Describe what changed in the pending change record (**Behavior delta**), not only in the specification body.                                                                              |

After either path, add or update a pending record in `docs/requirements/changes/` from `_changes-template.md` and run `npm run requirements:validate`.

## Before you write

1. Read relevant `_shared/reference/` docs (glossary, account model, permissions) when the feature touches those concepts.
2. Check whether behavior belongs in `_shared/functional/ui-patterns.md` (`FR-UI-001`) or another shared doc — link once, do not duplicate across specifications.
3. Search `docs/requirements/README.md` for related `FR-*` files; set `depends_on` and cite identifiers when behavior builds on another specification.

## Scope

- Describe **what the user and the system must do** from a business and interaction perspective.
- One file = one coherent functional specification. Cross-cutting terms live in `_shared/` — link, do not duplicate.
- When referring to behavior in another specification, cite its identifier (for example `FR-AUTH-001`).

## Structure

- Keep identifiers in the `FR-<AREA>-NNN` format.
- **Goal** states the business outcome.
- **Functional requirements** states user-visible behavior, flows, validations, messages, and business rules.
- **Non-functional requirements** inherits global `NFR-*` docs; override only when this specification differs.
- Deliberate exclusions use **Out of scope**.

Do **not** add a separate acceptance-scenarios or Given/When/Then table. If a bullet would only restate inherited `FR-UI-001` behavior, omit it and keep the `inherits_fr` link.

## Clarity and precision (mandatory)

Specifications must be **unambiguous**. A developer or tester must not need to guess what to build or verify.

- **Do not use** vague qualifiers: _optional_, _recommended_, _may_, _might_, _where applicable_, _when possible_, _or similar_, _TBD_, _etc._
- **State exact UI copy** for errors, confirmations, empty states, button labels, and success messages when wording matters.
- **State exact defaults**, visibility rules, and navigation after success and failure.
- **Use "not required"** for empty-allowed fields; **use "must not" / "cannot"** for prohibitions.
- **One behavior, one bullet** in functional requirements.

## No implementation details (mandatory)

**Do not include:** source code, API endpoints, HTTP details, database tables, migrations, or infrastructure keys.

**Instead describe:** user actions, screen flows, business entities, permission names from `_shared/reference/permissions.md`, and observable outcomes.

## Shared docs

| Concept                     | Document                                                                       |
| --------------------------- | ------------------------------------------------------------------------------ |
| Business terms              | `docs/requirements/_shared/reference/glossary.md`                              |
| Account attributes          | `docs/requirements/_shared/reference/account-model.md`                         |
| Permission names            | `docs/requirements/_shared/reference/permissions.md`                           |
| UI/UX patterns              | `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`)            |
| Product quality (NFR index) | `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) |

## Checklist before opening a PR

1. Every testable outcome is in **Functional requirements** or in `_shared/` when it is a cross-screen pattern.
2. Pending change record lists every touched `FR-*`, `NFR-*`, and shared doc with a clear **Behavior delta**.
3. `npm run requirements:validate` passes.

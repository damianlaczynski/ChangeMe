# REQ Template

> Template for writing functional requirements documents in this repository. It preserves the structure used in business analysis and can be copied for new areas.

# Requirements - `<Area Name>`

This document covers `<count>` REQs for the **`<Area Name>`** area:
`<short scope summary>`.

---

# `<REQ-ID-001>`: `<Requirement Name>`

## Goal

The user must be able to `<main business outcome>`.

## Features

### `<Functional Section 1>`

- `<behavior description 1>`
- `<behavior description 2>`

### `<Functional Section 2>`

| Field / element | Behavior        |
| --------------- | --------------- |
| **`<Name>`**    | `<description>` |
| **`<Name>`**    | `<description>` |

### Validation

- **`<Field>`**: `<validation rule>`.
- Validation errors are shown next to the relevant fields without closing the form.

### Form actions

- **Cancel** button — `<behavior description>`.
- **Save / Create** button — `<success / failure behavior description>`.

### States and business rules

- `<business rule 1>`
- `<business rule 2>`

### Permissions and visibility

- `<who can see the feature>`
- `<who can perform the action>`

---

# `<REQ-ID-002>`: `<Requirement Name>`

## Goal

The user must be able to `<main business outcome>`.

## Features

### `<Functional Section>`

- `<behavior description 1>`
- `<behavior description 2>`

### Actions and navigation

- `<action 1>`
- `<action 2>`

### Out of scope

- `<what is intentionally excluded from the current iteration>`

---

## Writing guidelines

### Scope of a requirements document

- Describe **what the user and the system must do** from a business and interaction perspective.
- Each document is a set of self-contained **REQ** sections. All behavior belongs inside a REQ; do not add document-level summary sections that repeat or aggregate REQ content (for example shared rule blocks or cross-cutting acceptance lists).
- When referring to existing behavior outside the current REQ, cite another REQ identifier (for example REQ-AUTH-001) or a named screen, field, or permission defined in this documentation set.

### Structure

- A single REQ describes one coherent behavior area from the user's perspective.
- Keep identifiers in the `REQ-<AREA>-XXX` format.
- The **Goal** section states the business outcome.
- The **Features** section states user-visible behavior, flows, validations, messages, and business rules.
- If the screen contains a form, add separate sections for fields, validation, and form actions.
- If the feature has side effects, describe them explicitly (notifications, history, session revocation, email).
- Deliberate exclusions use **Out of scope for this REQ**.

### Clarity and precision (mandatory)

Requirements must be **unambiguous**. A developer or tester must not need to guess what to build or verify.

- **Do not use** vague qualifiers: _optional_, _recommended_, _may_, _might_, _where applicable_, _when possible_, _or similar_, _TBD_, _etc._
- **Do not offer undefined choices** (for example "toast or inline message") unless the REQ states exactly which applies.
- **State exact UI copy** for errors, confirmations, empty states, button labels, and success messages when wording matters for acceptance.
- **State exact defaults**: sort order, filter defaults, field defaults, timeouts visible to the user or administrator.
- **State exact visibility rules**: who sees a screen, field, or action.
- **State exact navigation** after success and failure (screen name as user-facing destination).
- **State exact back navigation** with fixed button labels and destination screens (for example **Back to issues list** → **Issues list**); do not describe browser history stacks or dynamic back targets.
- **Use "not required"** for form fields that can be left empty; do not use the word _optional_.
- **Use "must not" / "cannot"** for prohibitions.
- **One behavior, one bullet.**

### No implementation details (mandatory)

Requirements are written for analysts, developers, and testers — but **must not prescribe how the system is built**.

**Do not include:**

- Source code, class names, libraries, frameworks, or storage mechanisms.
- API endpoints, HTTP methods, status codes, headers, cookies, tokens, or payload shapes.
- Database tables, columns, migrations, or infrastructure configuration keys.
- Internal service names, interceptors, guards, or component names.

**Instead describe:**

- User actions and system responses in business language.
- Screen flows, field rules, validation messages, and confirmation dialogs.
- Business entities (for example **session**, **role**, **permission**) and their attributes relevant to the user or administrator.
- Permission or role names **as defined in requirements** (for example **Users.View**).
- Observable outcomes: signed in, signed out, access denied, list refreshed, session revoked.

If technical detail is needed later, it belongs in design or implementation documents — not in REQ files.

### Tables and fields

- Field tables specify: required vs not required, min/max length, allowed values, defaults, read-only vs editable.
- Action lists specify: label, who can perform the action, confirmation text for destructive actions.

### Permissions

- Protected screens and actions state the required permission name(s) as defined in REQ-ROL-001 or the relevant REQ.
- The system must enforce permissions even when the user attempts an action outside the normal UI flow; the REQ states the user-visible denial when relevant.

### Out of scope

- Use **Out of scope for this REQ** only for deliberate exclusions.
- Do not use out of scope as a substitute for missing in-scope detail.

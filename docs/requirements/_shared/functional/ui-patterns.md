---
id: FR-UI-001
title: Shared UI and UX Patterns
domain: shared
type: functional
status: active
depends_on: []
---

# Shared UI and UX patterns

> Canonical interaction patterns for authenticated application screens. Individual functional specifications describe **feature-specific** behavior; they **inherit** these defaults unless they state an explicit override.

When writing or reviewing a functional specification, link this document instead of repeating list, form, or feedback rules. Document only what differs from the patterns below.

Do **not** add a separate acceptance-scenarios section to feature specifications. Put exact copy, visibility rules, and edge cases in **Functional requirements** bullets, or extend this document when the pattern applies to many screens.

## Scope

These patterns apply to:

- Authenticated screens inside the application shell (sidebar, header, content area).
- Guest screens (login) follow the same **feedback** and **validation** rules where applicable; layout chrome may differ per functional specification.

Out of scope here: email templates, push notification payloads, and API error shapes — those belong in the relevant functional specification or `_shared/non-functional/`.

---

## Administrative list screens

Examples: **Issues list**, **Users list**, **Roles list**.

Unless a functional specification specifies otherwise:

### Page structure

- **Page title** matches the screen name stated in the functional specification (for example **`Users list`**).
- Primary **create** or **add** action appears above the table when the user has the required permission.
- Sidebar entry visibility follows the permission named in the functional specification.

### Search and filters

- **Global search** field above the table when the functional specification defines it; placeholder and matched fields come from the functional specification. Search is **case-insensitive**.
- **Column-header filters** on columns named in the functional specification. Default control by column type unless the functional specification overrides:
  - **Text**: contains.
  - **Status / enum**: multi-select; empty selection means no restriction.
  - **Date / time**: range per column.
  - **Number**: comparison.
- Active search and column filters combine with **AND** logic.
- **Clear** control resets search, column filters, and sort to defaults and reloads from the first page.

### Sorting

- **Multi-sort**: click column headers to add or toggle sort levels (no Shift key).
- **Sortable** columns behave as stated in the functional specification; unspecified columns are not sortable.
- **Default sort** is defined in the functional specification.

### Data table

- Tabular layout with columns defined in the functional specification.
- Primary identifier column (name, title, email) links to the **details** screen when the functional specification defines one.
- Missing optional text values display **`—`** (em dash), unless the functional specification defines a different placeholder (for example **`Unassigned`**, **`Never`**).
- **Status** and categorical values use compact badges (**`p-tag`** style) with exact labels from the functional specification.
- **Actions** column uses a row **overflow menu** for secondary actions unless the functional specification defines inline controls (for example watch button on issues).

### Row overflow menu

- Actions the current user **lacks permission** for are **not shown** (not disabled).
- Destructive actions use the confirmation pattern in **Destructive and irreversible actions** below.
- Success after a row action shows a **toast** (see **Feedback channels**) and refreshes the current list in place unless the functional specification defines navigation away.

### Pagination, loading, and empty states

- Lists are **server-paginated** unless the functional specification defines a different control (for example **Show more** in a dropdown).
- Default page size: **10** rows per page.
- Paginator below the table shows current page and total count; changing page or page size reloads the list.
- Search, filter, and sort changes reset to **page 1**.
- While loading, a loading indicator appears **in the table area**; page chrome (title, actions bar) stays visible.
- Empty state copy is **exact** and defined in the functional specification (for example **`No leave requests match the filters.`**).
- When the functional specification does not define empty-state copy, use **`No items match the filters.`** for filtered lists and **`No items yet.`** for unfiltered lists.
- **Refresh** action when provided in the functional specification.

---

## Create and edit form screens

Examples: **Create issue**, **Edit user**, **Create role**.

Unless a functional specification specifies otherwise:

### Form structure

- Screen title: **Create {entity}** or **Edit {entity}** as named in the functional specification.
- Fields, defaults, and validation limits are defined in the functional specification.
- **Required** fields are marked in the functional specification; optional fields use **not required** wording.
- Read-only system metadata on edit (author, created at, identifiers) appears in a separate summary block when the functional specification lists them.

### Validation

- Validation runs on submit and on field blur when the functional specification does not state otherwise.
- Errors are **inline** next to the relevant field, or **form-level** for group rules (for example permission checkboxes).
- The form **stays open** on validation failure; entered values are preserved.
- Server-side validation errors map to the same inline positions when possible.

### Form actions

| Control                       | Behavior                                                                                                                                                                                                                    |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Back**                      | Top-of-screen control with a **fixed label and route** (see **Back navigation**). Leaves without saving.                                                                                                                    |
| **Cancel**                    | Same destination as **Back** unless the functional specification defines a different cancel target. Leaves without saving.                                                                                                  |
| **Create** / **Save changes** | Submits the form. On success: toast with exact message from the functional specification, then navigate to the destination defined in the functional specification (typically **details**). On failure: keep the form open. |

- Primary submit button label: **`Create {entity}`** on create, **`Save changes`** on edit, unless the functional specification names a different label (for example **`Submit`** for workflows).

### Loading

- Before the first load on create/edit, a loading state covers the **form area** until initial data arrives.

---

## Detail and section screens

Examples: **Issue details**, **User details**, **Role details**.

Unless a functional specification specifies otherwise:

### Layout

- **Page title** uses the entity name or identifier defined in the functional specification.
- Content is grouped into named sections (for example **Employment**, **Sessions**, **Comments**).
- Section-level **Add** actions appear only when the user has the required permission.

### Embedded lists

- Lists inside detail screens (comments, history, attachments, sessions, assigned users, notifications) are **server-paginated** with default page size **10** unless the functional specification defines **Show more** append loading.
- **Show more** loads the next page of **older** items and **appends** them without leaving the screen; the control is hidden when all items are loaded.
- Paginated embedded tables (for example **Active sessions** on **User details**) use a paginator below the table instead of **Show more**.
- Section empty states use exact copy from the functional specification.

### Header actions

- Primary actions (edit, delete, approve) appear in the page header or an actions area defined in the functional specification.
- **Back** navigates to the parent list or hub named in the functional specification.

---

## Back navigation

- **Back** uses a **fixed label** and **fixed route** — not browser history.
- Label format: **`Back to {destination screen name}`** (for example **`Back to issues list`**, **`Back to issue details`**, **`Back to my account`**).
- Guest flows may use **`Back to sign in`** → **Login** where the functional specification defines it.
- Create and edit screens use the back-destination table in the functional specification; when omitted, use:

| Context            | Back label                   | Destination                        |
| ------------------ | ---------------------------- | ---------------------------------- |
| Create from a list | **`Back to {list name}`**    | Parent list                        |
| Edit from details  | **`Back to {details name}`** | Parent details for the same entity |

---

## Feedback channels

Use the correct channel so behavior stays consistent across modules.

| Channel                                 | When to use                                                                                  | Persistence                              |
| --------------------------------------- | -------------------------------------------------------------------------------------------- | ---------------------------------------- |
| **Inline field validation**             | Field-level rule failures on forms                                                           | Until the field becomes valid            |
| **Inline screen message** (`p-message`) | Load failures, form-level errors not tied to one field                                       | Until the user retries or navigates away |
| **Toast**                               | Successful mutations; action failures not tied to a single field; background policy warnings | Auto-dismiss; see below                  |
| **Confirmation dialog**                 | Destructive or irreversible actions before execution                                         | Until confirm or cancel                  |
| **Modal dialog**                        | Short secondary flows that need input (for example reject reason)                            | Until submit, cancel, or close           |

### Toast conventions

- Success after save/create/delete: exact message from the functional specification (for example **`Role saved.`**, **`User deactivated.`**).
- Default toast lifetime: **3000** ms for success; **5000** ms for errors, unless the functional specification defines **sticky** behavior.
- Sticky toasts include an **action** button when the functional specification names one (for example **`Change now`**, **`Set up now`**).
- Do not use a toast for field validation errors.

### Permission denial

- Protected actions the user attempts without permission are rejected with message **`You do not have permission to perform this action.`** (see `docs/requirements/_shared/reference/permissions.md`).
- UI controls for unauthorized actions are **hidden**, not merely disabled, unless the functional specification explicitly requires a disabled state with explanation.

---

## Destructive and irreversible actions

Unless the functional specification defines custom copy:

- Confirmation dialog title: action name (for example **`Delete issue`**).
- Confirmation body format: **`{Action} "{entity label}"? {Consequence.}`**
  - Example: **`Delete "{issue title}"? This action cannot be undone.`**
  - Example: **`Deactivate "{full name}"? The user will be signed out and cannot sign in until reactivated.`**
- **Cancel** in the dialog closes without changes.
- **Confirm** executes the action, shows the success toast from the functional specification, and follows the functional specification navigation rule (stay in place vs navigate away).

---

## Date, time, and number display

Unless a functional specification specifies otherwise:

- Dates and times use the user's browser locale for formatting.
- Functional specifications document exact **labels** and **placeholder tokens** in backticks; they are English canonical strings (see `docs/requirements/_shared/non-functional/product-quality.md`).
- Durations and counts use plain numbers in UI copy as defined in the functional specification (for example **`{n} permissions`**, **`0.5`** for half-days).

---

## Reporting and export screens

Unless a functional specification specifies otherwise:

- Filters follow the same **Search and filters** rules as **Administrative list screens**.
- Result tables are **server-paginated** with default **10** rows per page when showing row-level detail.
- **Export** actions show progress or loading on the control; success toast: **`Export started.`** or exact copy from the functional specification.
- Large exports run asynchronously when the functional specification defines background export; the functional specification states the completion feedback channel.

---

## How to reference in a functional specification

Use a short inheritance line in **Functional requirements** or **Out of scope**:

```markdown
### List behavior

- Inherits `FR-UI-001` (**Administrative list screens**) except:
  - Default sort: **Submitted at** descending.
  - Empty state: **`No leave requests match the filters.`**
  - Global search matches **reference number** and **employee name**.
```

Do **not** copy pagination, loading, or filter control types when inheritance suffices.

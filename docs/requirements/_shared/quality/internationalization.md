---
id: NFR-I18N-001
title: Internationalization and Copy
type: quality
status: active
---

## Internationalization and copy

### Canonical language

- All user-visible strings documented in functional specifications are **English** and **exact**.
- Text in backticks in a functional specification (for example **`Save changes`**, **`No items yet.`**) is the canonical UI copy.
- The application ships **English-only** UI in the current product version. There is no runtime language switcher.

### Writing rules for analysts

- Document exact button labels, error messages, empty states, and dialog bodies in the functional specification when wording matters.
- Do not use locale placeholders (for example "translated label for save") in REQ text.
- Proper nouns (product name, role names **Administrator** / **User**, permission labels from `docs/requirements/_shared/domain/permissions.md`) keep the spelling used in the catalog.

### Future localization

- When localization is introduced, canonical English strings in REQs become the **source keys** for translation files.
- New REQs must still specify exact English copy until a localization REQ supersedes this section.

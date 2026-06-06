# 2026-06-06 — Deduplicate functional specs via FR-UI-001 inheritance

**Status:** done (2026-06-06)

## Why

After introducing `FR-UI-001`, many functional specifications still repeated pagination, loading, filter, and form-validation bullets. Shorten specs to feature-specific rules plus explicit inherit/override lines.

## Functional specifications touched

All list, form, and detail specs that duplicated `FR-UI-001` patterns — notably:

| FR                                                                     | Change                                                                           |
| ---------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| `FR-ISS-001`, `FR-USR-002`                                             | Removed filter/pagination/loading boilerplate; inherit in **Search and filters** |
| `FR-ROL-002`                                                           | Removed pagination/loading; **List behavior** inherit                            |
| `FR-ISS-002`, `FR-ROL-003`, `FR-USR-003`, `FR-AUTH-001`, `FR-AUTH-005` | Merged validation + form inherit; removed generic Back/Cancel bullets            |
| `FR-ISS-003`, `FR-ISS-006`, `FR-ISS-005`                               | Embedded lists: inherit + **Show more** overrides                                |
| `FR-ROL-004`, `FR-USR-004`, `FR-AUTH-004`                              | Embedded session/user lists inherit pagination/loading                           |
| `FR-INV-001`, `FR-USR-001`                                             | Form inherit for Back/Cancel and validation presentation                         |

## Non-functional and shared docs touched

| ID / path   | Action                                                                  |
| ----------- | ----------------------------------------------------------------------- |
| `FR-UI-001` | **Updated** — terminology (`functional specification` instead of `REQ`) |

## Behavior delta

**Before:** Each spec repeated server pagination (10/page), filter Apply/Clear, inline validation, and loading bullets.

**After:** Specs state `Inherits FR-UI-001 (…)` and document only overrides (custom filters, **Show more**, dialog-heavy flows, success messages).

No product behavior change — documentation structure only.

## Implementation scope

- One-time batch deduplication + manual structural cleanup on 18 files (no permanent script retained).

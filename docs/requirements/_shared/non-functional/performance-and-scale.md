---
id: NFR-PERF-001
title: Performance and Scale
type: non-functional
status: active
---
## Performance and scale

### List and search data

- All primary entity lists are **server-paginated** with default **10** items per page (`docs/requirements/_shared/functional/ui-patterns.md`).
- Client-side loading of unbounded full tables is **not allowed** for administrative lists.
- Search and filter queries run on the server; the functional specification defines match rules, not client-side filter algorithms.

### Detail screen sections

- Embedded lists (comments, history, sessions, attachments, notifications) are **server-paginated** unless the functional specification defines incremental **Show more** loading.
- Default page size for embedded lists: **10** unless the functional specification states otherwise.

### Mutations and feedback

- Save and delete actions show **immediate** control feedback (disabled state or loading on the button) while the request is in flight.
- Optimistic UI (showing success before server confirmation) is **not used** unless the functional specification explicitly requires it.

### Exports and reports

- CSV and report exports over large datasets run as **server-side** generation.
- The REQ defines whether export is synchronous (direct download) or asynchronous (notification or polling).
- Default export row cap per request: **50 000** rows unless the functional specification states a different limit.
- When an export exceeds the cap, the system rejects the request with message **`Export limit exceeded. Narrow the filters and try again.`** unless the functional specification defines different copy.

### Realtime

- Push updates (notifications, bell badge) refresh affected UI regions without full page reload when the functional specification defines realtime behavior.
- Realtime is **best-effort**; a manual **Refresh** control must remain available where the functional specification defines a list or panel (for example notification dropdown).


---

## Reliability and error handling

Unless a REQ specifies otherwise:

| Situation                       | User-visible behavior                                                                                   |
| ------------------------------- | ------------------------------------------------------------------------------------------------------- |
| Network or server error on load | Inline screen message: **`Could not load data. Try again.`** with **Retry** when the screen supports it |
| Network or server error on save | Toast: **`Could not save changes. Try again.`**; form stays open with values preserved                  |
| Session expired during action   | Redirect to **Login**; unsaved form data is lost                                                        |
| Concurrent edit conflict        | Message from the functional specification, or default **`This record was updated by someone else. Refresh and try again.`**  |

---
id: NFR-QUAL-001
title: Product Quality (Index)
type: non-functional
status: active
---

# Product quality and non-functional requirements

> Cross-cutting quality expectations. Functional specifications inherit these documents via `inherits_nfr` unless they document an explicit override.

| ID           | Document                                             | Scope                                 |
| ------------ | ---------------------------------------------------- | ------------------------------------- |
| NFR-I18N-001 | [internationalization.md](internationalization.md)   | English canonical copy                |
| NFR-A11Y-001 | [accessibility.md](accessibility.md)                 | Keyboard, focus, labels, themes       |
| NFR-RSP-001  | [responsiveness.md](responsiveness.md)               | Viewports, mobile out of scope        |
| NFR-PERF-001 | [performance-and-scale.md](performance-and-scale.md) | Pagination, exports, errors, realtime |

## Security presentation

- Error messages do not reveal whether an account exists when the functional specification requires ambiguity (for example login — FR-AUTH-001).
- Sensitive actions use confirmation dialogs when the functional specification requires them (for example session revoke — FR-AUTH-004).
- Permission denial copy is uniform: **`You do not have permission to perform this action.`**

## How to reference in a functional specification

```markdown
## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`).
- Override only when this specification differs (export cap, viewport behavior, etc.).
```

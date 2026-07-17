# 2026-07-17 — FR content audit

**Status:** done (2026-07-17)

## Why

After the five-layer model migration, run one targeted content pass on capability specs: dedupe Data/Validation sections, restore business-critical confirmation and rejection messages, fix `depends_on` prerequisites, and align messages with the current codebase.

## Functional specifications touched

| FR            | Changes                                                 |
| ------------- | ------------------------------------------------------- |
| `FR-ISS-001`  | `depends_on`; delete confirmation; `STD-OP-001`         |
| `FR-ISS-002`  | `depends_on`; dedupe Validation/Data                    |
| `FR-ISS-003`  | `depends_on`; delete confirmation; `STD-OP-001`         |
| `FR-ISS-004`  | `depends_on`; drop `STD-VAL-001`                        |
| `FR-ISS-005`  | `depends_on`; drop `STD-VAL-001`                        |
| `FR-ISS-006`  | delete confirmation; `STD-OP-001`; validation cleanup   |
| `FR-AUTH-001` | dedupe Data/Validation; glossary path                   |
| `FR-AUTH-003` | sign-out-everywhere confirmation; `STD-OP-001`          |
| `FR-AUTH-004` | revoke-session confirmation; `STD-OP-001`               |
| `FR-USR-001`  | remove circular `depends_on`; explicit name constraints |
| `FR-USR-002`  | drop `STD-FRM-001`                                      |
| `FR-USR-004`  | revoke-session confirmations; `STD-OP-001`              |
| `FR-USR-005`  | deactivate/activate confirmation messages               |
| `FR-ROL-001`  | drop `STD-FRM-001`                                      |
| `FR-ROL-002`  | `STD-OP-001`                                            |
| `FR-ROL-003`  | delete-role confirmation; `STD-OP-001`                  |
| `FR-ROL-004`  | remove-from-role confirmation; `STD-OP-001`             |
| `FR-ROL-006`  | fix broken frontmatter                                  |

## Behavior delta

No new product capabilities. Specs now state exact confirmation/rejection messages where they encode business or security rules, defer generic UX to `STD-*`, and use `depends_on` only for true prerequisites.

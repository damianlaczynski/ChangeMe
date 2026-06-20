# Implementation guides

> **Scope:** how to write and verify code in this repository — not product behaviour or deployment configuration.
>
> For product rules (`FR-*`), see [`docs/requirements/`](../requirements/). For run and configure, see [`docs/technical/`](../technical/).

## Documents

| Document                                         | Read when you need to…                                  |
| ------------------------------------------------ | ------------------------------------------------------- |
| [repo-map.md](repo-map.md)                       | Find where code lives and which layer owns what         |
| [frontend-guidelines.md](frontend-guidelines.md) | Angular components, forms, routing, PrimeNG, Tailwind   |
| [backend-guidelines.md](backend-guidelines.md)   | Layers, endpoints, use cases, validation, EF            |
| [testing-guidelines.md](testing-guidelines.md)   | Test layer ownership, anti-patterns, when to skip tests |
| [feature-recipes.md](feature-recipes.md)         | Step-by-step recipes for common feature work            |

## Start here by task

| Task                              | Documents                                                                                                       |
| --------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| First orientation in the codebase | [repo-map.md](repo-map.md)                                                                                      |
| Frontend change                   | [repo-map.md](repo-map.md) + [frontend-guidelines.md](frontend-guidelines.md)                                   |
| Backend change                    | [repo-map.md](repo-map.md) + [backend-guidelines.md](backend-guidelines.md)                                     |
| New endpoint or screen end-to-end | [feature-recipes.md](feature-recipes.md) + relevant guidelines + [testing-guidelines.md](testing-guidelines.md) |
| Verify before PR                  | [testing-guidelines.md](testing-guidelines.md) + [`AGENTS.md`](../../AGENTS.md) (commands)                      |

## Relationship to other docs

| Folder                       | Use for                              |
| ---------------------------- | ------------------------------------ |
| `docs/guides/` (this folder) | How to _implement_                   |
| `docs/requirements/`         | What the product _must do_           |
| `docs/technical/`            | How to _run and configure_ the stack |

Commands shared across areas: [`AGENTS.md`](../../AGENTS.md).

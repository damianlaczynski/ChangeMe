# 2026-07-17 — Five-layer requirements model

**Status:** done (2026-07-17)

## Why

Introduce a clear five-layer documentation model (Domain · Conventions · Quality · Capabilities · Implementation) so agents and contributors know where each type of rule lives, without overloading context with duplicate or screen-oriented specs.

## Functional specifications touched

All 22 capability specs — frontmatter migrated from `inherits_fr` / `inherits_nfr` to `inherits_conventions` / `inherits_quality`; quality section renamed; former ui-patterns references replaced with `STD-*`.

## Shared documents touched

| Path                                           | Action                                                                                   |
| ---------------------------------------------- | ---------------------------------------------------------------------------------------- |
| `_shared/reference/` → `_shared/domain/`       | **Renamed** — L1 Domain layer                                                            |
| `_shared/non-functional/` → `_shared/quality/` | **Renamed** — L3 Quality layer                                                           |
| `_shared/conventions/product-standards.md`     | **New** — L2 Conventions (`CONV-001`, `STD-*` sections); replaces former ui-patterns doc |
| `_shared/README.md`                            | **New** — layer index and agent load order                                               |
| `_shared/functional/ui-patterns.md`            | **Removed** — content migrated to conventions                                            |
| `requirements-authoring-guide.md`              | **Updated** — five-layer model                                                           |
| `requirements-change-process.md`               | **Updated** — five-layer model                                                           |
| `_functional-specification-template.md`        | **Updated** — `inherits_conventions`, `inherits_quality`                                 |
| `AGENTS.md`                                    | **Updated** — layer table and load order                                                 |
| `scripts/requirements-*.mjs`                   | **Updated** — validate new paths and `STD-*` ids                                         |

## Behavior delta

**Before:** Cross-cutting UI behavior lived in ui-patterns under `_shared/functional/`; domain docs in `reference/`; quality in `non-functional/`; capability specs used `inherits_fr` and `inherits_nfr`.

**After:** L1 `domain/`, L2 `conventions/product-standards.md` with `STD-*` anchors, L3 `quality/`, L4 `functional/FR-*` with `inherits_conventions` and `inherits_quality`. No intentional product behavior change.

## Implementation scope

- Documentation and validation scripts only.

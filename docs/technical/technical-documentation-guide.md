# Technical documentation guide

> **Audience:** contributors adding or changing docs in `docs/technical/`.
> **What `technical/` is for** (vs `guides/` and `requirements/`): [README.md — Relationship to other docs](README.md#relationship-to-other-docs).

## Extend vs new file

| Extend an existing `docs/technical/*.md` when…                                   | Add a new file when…                                                          |
| -------------------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| Same operational concern (more config keys, troubleshooting rows, Compose notes) | Different concern and config surface (e.g. CI vs database vs auth deployment) |
| Added material stays easy to find in the current doc                             | The target doc would grow hard to navigate (~150+ lines of new content)       |
|                                                                                  | The knowledge outlives one PR and helps the next clone or deployment          |

**Do not** add a technical doc for one-off PR notes, secrets, product behaviour (`FR-*`), or implementation how-to — see [README.md](README.md#relationship-to-other-docs).

**Do not** add catalogs of HTTP endpoints, frontend routes, or screens with behavioural detail. Surface area belongs in Swagger, source code, and `docs/guides/` / `docs/requirements/`. Mention a route **inline** only when an operator must call it to verify deployment (one URL in a troubleshooting row or checklist step — not a table of routes).

**Do not duplicate** content that already has a canonical home. Link instead:

| Principle                                          | Put it in `technical/`                        | Canonical source (link, do not copy)                      |
| -------------------------------------------------- | --------------------------------------------- | --------------------------------------------------------- |
| Product behaviour, user flows, acceptance criteria | Brief ops impact only                         | `docs/requirements/` (`FR-*`, `_shared/`)                 |
| How to implement or test in code                   | Pointers only                                 | `docs/guides/`                                            |
| Full API or UI surface                             | Inline verification URLs at most              | Swagger, route files, requirements                        |
| Config defaults for every key                      | What a setting **does** and deployment impact | `appsettings*.json`, `*Options.cs`, environment variables |
| Business limits and validation rules               | That they exist and where operators look      | Domain constants, validators, FR specs                    |
| Third-party admin-console walkthroughs             | Redirect URIs, required keys, example JSON    | Vendor documentation (UI changes often)                   |
| Shared infra (jobs, cron, compose services)        | Feature-specific flags and troubleshooting    | One operational doc per infra concern; cross-link         |
| Machine-readable config (workflows, compose)       | Human summary and local reproduction          | The committed file itself (workflow, YAML, compose)       |

**Do not** treat a summary doc as the only source of truth when a committed config file exists — keep the file authoritative and the doc explanatory.

## Conventions

- **Name:** kebab-case (`database-and-docker.md`, `ci.md`); suffix `-guide` for large operator manuals.
- **Open** with a one-line `> Scope:` blockquote.
- **Link** to `requirements/` and `guides/` instead of copying behaviour or code patterns.

## Checklist

1. Extend vs new file (table above).
2. Edit or add under `docs/technical/`.
3. Update [README.md](README.md) (documents table; **Start here by task** if needed).
4. Cross-link from related docs; one line in `AGENTS.md` only when a new major entry point appears.

## Examples

| Change                                                                      | Action                                                                                      |
| --------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| New deployment flag or troubleshooting for an **existing** operational area | Extend the doc that already covers that config surface                                      |
| New shared infra concern (background job, compose service, migration note)  | Extend the doc that owns that infra; link from feature-specific docs                        |
| New CI job or workflow step                                                 | Update the workflow file **and** the CI summary doc                                         |
| New optional dev tool with its own config                                   | New file if it would add ~150+ lines or a distinct concern; cross-link from related docs    |
| Route / endpoint / screen catalog                                           | **Do not** — link to Swagger, requirements, or guides; inline URL only for ops verification |

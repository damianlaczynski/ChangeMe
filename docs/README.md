# Documentation index

> Start with [`AGENTS.md`](../AGENTS.md) for commands, repo layout, and task-based reading order.

## Guides (`guides/`)

Implementation conventions and recipes for day-to-day development.

| Document                                                | Description                                  |
| ------------------------------------------------------- | -------------------------------------------- |
| [repo-map.md](guides/repo-map.md)                       | Where code lives and which layer owns what   |
| [frontend-guidelines.md](guides/frontend-guidelines.md) | Angular frontend conventions                 |
| [backend-guidelines.md](guides/backend-guidelines.md)   | .NET backend conventions                     |
| [testing-guidelines.md](guides/testing-guidelines.md)   | How to verify changes and where tests belong |
| [feature-recipes.md](guides/feature-recipes.md)         | Short recipes for common feature work        |

## Technical (`technical/`)

Local environment, persistence, demo data, and deployment-oriented auth configuration.

| Document                                                       | Description                                      |
| -------------------------------------------------------------- | ------------------------------------------------ |
| [database-and-docker.md](technical/database-and-docker.md)     | EF Core migrations, Docker Compose, file storage |
| [data-generator.md](technical/data-generator.md)               | Optional demo data for local development         |
| [auth-operations-guide.md](technical/auth-operations-guide.md) | `AuthOptions`, OIDC, 2FA, and operator settings  |

## Requirements (`requirements/`)

Product behaviour specifications and the change workflow.

| Document                                                                      | Description                                      |
| ----------------------------------------------------------------------------- | ------------------------------------------------ |
| [README.md](requirements/README.md)                                           | Index of `FR-*`, `NFR-*`, and reference docs     |
| [requirements-change-process.md](requirements/requirements-change-process.md) | How to propose and implement requirement changes |
| [changes/](requirements/changes/)                                             | Pending requirement deltas                       |
| [functional/](requirements/functional/)                                       | Functional specifications by domain              |
| [\_shared/](requirements/_shared/)                                            | Shared reference, UI patterns, and NFR docs      |

## Templates (`templates/`)

| Document                                                                               | Description                   |
| -------------------------------------------------------------------------------------- | ----------------------------- |
| [functional-specification-template.md](templates/functional-specification-template.md) | Template for new `FR-*` files |

Validate requirements structure: `npm run requirements:validate` from the repository root.

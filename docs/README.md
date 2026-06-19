# Documentation index

> Start with [`AGENTS.md`](../AGENTS.md) for commands, repo layout, and task-based reading order.

## Guides (`guides/`)

Implementation conventions and recipes. Start with [guides/README.md](guides/README.md).

| Document                                                | Description                                           |
| ------------------------------------------------------- | ----------------------------------------------------- |
| [README.md](guides/README.md)                           | Entry point — when to read each guide                 |
| [repo-map.md](guides/repo-map.md)                       | Where code lives and which layer owns what            |
| [frontend-guidelines.md](guides/frontend-guidelines.md) | Angular frontend conventions                          |
| [backend-guidelines.md](guides/backend-guidelines.md)   | .NET backend conventions                              |
| [testing-guidelines.md](guides/testing-guidelines.md)   | Test layer ownership and when to skip automated tests |
| [feature-recipes.md](guides/feature-recipes.md)         | Short recipes for common feature work                 |

## Technical (`technical/`)

Run, configure, and troubleshoot the stack. Start with [technical/README.md](technical/README.md).

| Document                                                                       | Description                                                |
| ------------------------------------------------------------------------------ | ---------------------------------------------------------- |
| [README.md](technical/README.md)                                               | Entry point — when to read each technical doc              |
| [technical-documentation-guide.md](technical/technical-documentation-guide.md) | When to add or extend technical docs                       |
| [database-and-docker.md](technical/database-and-docker.md)                     | EF Core migrations, Docker Compose, Hangfire, file storage |
| [data-generator.md](technical/data-generator.md)                               | Optional demo data for local development                   |
| [ci.md](technical/ci.md)                                                       | GitHub Actions workflow and local reproduction             |

## Requirements (`requirements/`)

Product behaviour and the change workflow. Start with [requirements-change-process.md](requirements/requirements-change-process.md).

| Document                                                                                     | Description                                    |
| -------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| [requirements-change-process.md](requirements/requirements-change-process.md)                | Workflow for analysts and developers           |
| [requirements-authoring-guide.md](requirements/requirements-authoring-guide.md)              | How to write and update `FR-*` specifications  |
| [README.md](requirements/README.md)                                                          | Auto-generated index of `FR-*`, NFR, reference |
| [\_functional-specification-template.md](requirements/_functional-specification-template.md) | Skeleton template for new `FR-*` files         |
| [\_changes-template.md](requirements/_changes-template.md)                                   | Template for pending requirement deltas        |
| [changes/](requirements/changes/)                                                            | Pending requirement deltas                     |
| [functional/](requirements/functional/)                                                      | Functional specifications by domain            |
| [\_shared/](requirements/_shared/)                                                           | Shared reference, UI patterns, and NFR docs    |

Validate requirements structure: `npm run requirements:validate` from the repository root.

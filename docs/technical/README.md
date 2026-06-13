# Technical documentation

> **Scope:** run, configure, and troubleshoot the application and its toolchain — not how to write feature code.
>
> For implementation conventions, see [`docs/guides/`](../guides/). For product behaviour (`FR-*`), see [`docs/requirements/`](../requirements/).
>
> **Adding or changing technical docs:** [technical-documentation-guide.md](technical-documentation-guide.md).

## Documents

| Document                                                             | Read when you need to…                                                                                                               |
| -------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| [technical-documentation-guide.md](technical-documentation-guide.md) | Rules for when to add or extend docs in this folder                                                                                  |
| [database-and-docker.md](database-and-docker.md)                     | EF Core migrations, Docker Compose, Hangfire jobs, file storage                                                                      |
| [data-generator.md](data-generator.md)                               | Fill the database with optional demo users and issues after migrations                                                               |
| [ci.md](ci.md)                                                       | Understand GitHub Actions, reproduce CI locally, or debug a failing pipeline job                                                     |
| [auth-operations-guide.md](auth-operations-guide.md)                 | Configure `AuthOptions`, SMTP, OIDC providers, 2FA, invitations, passkeys policy flags, or diagnose sign-in in deployed environments |

## Start here by task

| Task                                                       | Document                                                                                                                     |
| ---------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| First clone — run full stack locally                       | [database-and-docker.md](database-and-docker.md) → `npm run docker:up`                                                       |
| First clone — run API/tests without Compose                | [database-and-docker.md](database-and-docker.md) (migrations) → `docs/guides/testing-guidelines.md`                          |
| Populate UI with sample data                               | [data-generator.md](data-generator.md)                                                                                       |
| PR failed on GitHub                                        | [ci.md](ci.md)                                                                                                               |
| Email links / verification not working locally             | [database-and-docker.md](database-and-docker.md) (MailHog) + [auth-operations-guide.md](auth-operations-guide.md) (SMTP)     |
| Google / Microsoft / OIDC sign-in in staging or production | [auth-operations-guide.md](auth-operations-guide.md)                                                                         |
| Passkeys enabled in config but ceremonies fail             | [auth-operations-guide.md](auth-operations-guide.md) §4.12 (origin, RP ID, HTTPS) + `docs/requirements/functional/passkeys/` |
| Background jobs failing or not running                     | [database-and-docker.md](database-and-docker.md) (Hangfire dashboard, cron)                                                  |

## Relationship to other docs

| Folder               | Use for                                            |
| -------------------- | -------------------------------------------------- |
| `docs/guides/`       | How to _implement_ (code, tests, recipes)          |
| `docs/requirements/` | What the product _must do_ (`FR-*`, `_shared/`)    |
| `docs/technical/`    | How to _run and configure_ the stack (this folder) |

Auth: behaviour in `requirements/`; `AuthOptions`, OIDC, SMTP in `auth-operations-guide.md`.

Commands shared across areas: [`AGENTS.md`](../../AGENTS.md).

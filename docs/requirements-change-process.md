# Requirements change process

> Pending requirement changes for developers; trimmed after delivery.

## Two documents

| Document                                    | Role                                                                             |
| ------------------------------------------- | -------------------------------------------------------------------------------- |
| `docs/req/<area>-requirements.md`           | **Specification** — full, testable behavior.                                     |
| `docs/req/<area>-requirements-changelog.md` | **Pending delta** — what changed until it is implemented; then entry is removed. |

Long-term history lives in git (REQ edits and past changelog entries in commit history), not in an ever-growing changelog file.

## Roles

### Analyst (or author of the REQ change)

1. Update `*-requirements.md` for the affected `REQ-*` sections.
2. Add one entry at the top of `*-requirements-changelog.md` using `docs/templates/requirements-changelog-entry-template.md`.
3. Fill **Why** (if useful), **Requirements touched**, **Behavior delta**.
4. If the same release touches another area, add a matching entry there and link both ways under **Relates**.

### Developer

1. Read open entries in the relevant changelog and the linked REQ sections.
2. Implement and test against the REQ (use `docs/feature-recipes.md` for technical steps).
3. When the work is merged (or otherwise done), **delete that changelog entry** from the file.

An empty changelog (only the file title and pointer to this process) is normal.

## Sections in an entry

| Section                  | Purpose                                                                            |
| ------------------------ | ---------------------------------------------------------------------------------- |
| **Why**                  | Brief intent. Optional.                                                            |
| **Requirements touched** | Which `REQ-*` are new or updated; one line each. Details are only in the REQ file. |
| **Behavior delta**       | What is different from the old product (Before/After). Not a duplicate of the REQ. |
| **Relates**              | Optional link to another area’s open changelog entry (same release).               |

Do not add: Read next, deploy notes, implementation checklists, file paths, or endpoints.

## When to add an entry

One entry per coherent batch of REQ changes (feature slice). Skip typo-only edits that do not change behavior.

## Cross-area changes

Each area keeps its own changelog entry. Use **Relates** to point at the other file (same date and title so the link is obvious).

## For AI agents

If `*-requirements-changelog.md` has open entries, read them plus the referenced REQ sections. If the changelog is empty, use only `*-requirements.md`.

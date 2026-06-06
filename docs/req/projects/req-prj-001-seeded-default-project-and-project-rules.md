---
id: REQ-PRJ-001
title: Seeded Default Project and Project Rules
domain: projects
status: active
depends_on: [REQ-PRJ-005]
---

## Goal

When the application is first deployed, the system must provide a seeded default project so issues can always belong to a project, and enforce that every issue is assigned to exactly one project.

## Features

### Seeded default project

On first startup, the system ensures this project exists:

| Project     | **System** badge | Editable | Deletable |
| ----------- | ---------------- | -------- | --------- |
| **Default** | Yes              | No       | No        |

- If the **Default** project already exists, the system does **not** recreate it or change its name.
- The **Default** project has **Description** empty.

### Project membership rule

- Every issue belongs to **exactly one** project at all times.
- An issue **cannot** be saved without a selected project.
- An issue **cannot** exist without a project reference.

### Existing issues without a project

- On first startup after this requirement is deployed, any existing issue that has no project is assigned to the **Default** project automatically.

### Default project membership

- Every active user (**Deactivated** false) is a **Member** of the **Default** project (REQ-PRJ-005).
- On first seed, the system adds all existing active users as **Members** of **Default**.
- When a new account becomes active (registration, invitation acceptance, or reactivation), the system adds that user as a **Member** of **Default** automatically.

### System project rules

- The **Default** project follows edit and delete restrictions from REQ-PRJ-003 and REQ-PRJ-004.
- The **Default** project **cannot** be renamed or deleted.

### Permissions and visibility

- Access to **Default** and all other projects is governed by resource-scoped project RBAC (REQ-PRJ-005), not global application permissions.

### Out of scope for this REQ

- Archiving or deactivating projects.

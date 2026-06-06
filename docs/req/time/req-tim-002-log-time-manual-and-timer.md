---
id: REQ-TIM-002
title: Log Time — Manual Entry and Timer
domain: time
status: active
depends_on: [REQ-TIM-001, REQ-TIM-007, REQ-PRJ-002]
---

## Goal

An authorized user must be able to record work time manually or with a running timer, with or without a linked issue.

## Features

### Access

- **Log time** dialog opens from entry points listed in REQ-TIM-007.
- Opening **Log time** requires **Time.LogOwn**.
- Saving an entry requires **Project.Time.Log** on the selected **Project**; the acting user must be a project member on that project.

### Log time dialog

| Field           | Behavior                                                                                                                                                                                                                                              |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Project**     | Required dropdown. Lists only projects where the user is a member and has **Project.Time.Log**, sorted alphabetically by name.                                                                                                                        |
| **Issue**       | Not required. Searchable dropdown of open issues in the selected **Project** (**New**, **In Progress**, **Resolved**); includes explicit option **`No issue`**. Disabled until **Project** is selected. Cleared when **Project** changes.             |
| **Work date**   | Required date picker; default **today**; must satisfy backdating rules (REQ-TIM-001).                                                                                                                                                                 |
| **Duration**    | Required when not pre-filled from a stopped timer; **Hours** (**0–24**) and **Minutes** (**0–59**) numeric inputs side by side; combined duration **1–1440** minutes. Live summary below: **`Total: {formatted duration}`** using REQ-TIM-007 format. |
| **Description** | Not required; multiline; max **500** characters with counter (REQ-TIM-007).                                                                                                                                                                           |

- When opened with **Project** and **Issue** pre-filled (issue context), those fields are read-only.
- Quick duration presets per REQ-TIM-007.

### Validation

- **Project**: required; inline error when empty: **`Project is required.`**
- **Work date**: required; must not be in the future; must satisfy backdating limit unless the user has **Time.LogPastLimit**; inline error: **`Work date is outside the allowed range.`**
- **Duration**: required; minimum **1** minute; maximum **1440** minutes; inline error: **`Duration must be between 1 minute and 24 hours.`**
- **Description**: max **500** characters when not empty; inline error: **`Description cannot exceed 500 characters.`**
- Validation errors are inline on the relevant fields; the dialog stays open on failure.

### Form actions

- **Cancel** closes the dialog without saving.
- **Save** creates the time entry, writes the operation audit record (REQ-TIM-006), closes the dialog, and shows toast **`Time logged.`**
- After save from **Issue details** — **Time** tab, the tab data refreshes.
- After save from **My time**, the list refreshes.
- After save from the top bar or **Issues list**, no navigation occurs; if **My time** is the current screen, its list refreshes.

### Running timer

- A signed-in user with **Time.LogOwn** may start **one** running timer at a time.
- **Start timer** is available on **My time**, **Issue details** — **Time** tab, and inside the **Log time** dialog as link-style action **`Start timer instead`** below the duration fields (hidden while a timer is already running).
- When **Start timer** is invoked from **Issue details** — **Time** tab, the timer is associated with that issue's **Project** and **Issue**.
- When invoked from **My time**, the timer has no pre-selected **Project** or **Issue**.
- When invoked from **Log time** dialog **`Start timer instead`**, the timer inherits **Project** and **Issue** currently selected in the dialog (if any), then closes the dialog.
- Shell presentation and popover behavior: REQ-TIM-007.
- Elapsed time is rounded **down** to whole minutes.
- If the user starts a new timer while one is already running, show confirmation: **`You already have a running timer. Stop it and start a new one?`** On confirm, discard the previous timer without saving and start the new one.
- **Stop timer** / **Stop and log** opens **Log time** with **Duration** pre-filled from elapsed whole minutes and **Project** / **Issue** pre-filled when the timer was linked. If elapsed time is **0** minutes, show toast **`Timer must run at least 1 minute before logging.`** and keep the timer running.
- **Discard timer** shows confirmation: **`Discard running timer? Elapsed time will not be saved.`** On confirm, clear the timer with no entry created.
- Signing out discards a running timer without saving.

### Empty project list

- When the user has **Time.LogOwn** but **Project.Time.Log** on no projects, **Log time** dialog opens with empty **Project** dropdown and message: **`You are not a member of any project where you can log time.`** **Save** is disabled.

### Permissions and visibility

- **Log time**, **Start timer**, and timer controls are hidden when the user lacks **Time.LogOwn**.
- **Project** dropdown excludes projects where the user lacks **Project.Time.Log**.

### Out of scope for this REQ

- Editing or deleting existing entries (REQ-TIM-004).
- Viewing aggregated reports (REQ-TIM-005).

---

---
id: REQ-TIM-007
title: Navigation, Shell, and Presentation
domain: time
status: active
depends_on: [REQ-TIM-001, REQ-TIM-002, REQ-ISS-005]
---

## Goal

Time tracking features must be reachable from consistent navigation, use dialogs for quick data entry, and present durations and summaries with the same visual language across the application.

## Features

### Sidebar navigation

- **My time** is a sidebar entry with icon **`pi pi-clock`**; visible when the user has **Time.ViewOwn**.
- **My time** appears after **Issues** (and **Projects** when present) and before administrative entries (**Users**, **Roles**).
- **Time reports** is a sidebar entry with icon **`pi pi-chart-bar`**; visible when the user has **Time.ViewReports**.
- **Time reports** appears after **Roles** and before **My account**.
- There is **no** sidebar entry for **Log time**; logging uses the header action and in-context buttons (REQ-TIM-002).

### Header actions

- **Log time** is a primary button in the authenticated top bar, placed immediately before **Notifications**.
- **Log time** is visible when the user has **Time.LogOwn**; clicking opens the **Log time** dialog (REQ-TIM-002).
- On viewports below **`sm`**, **Log time** shows icon **`pi pi-clock`** only (no label); from **`sm`** upward the label **`Log time`** is shown.

### Running timer control

- When a timer is running (REQ-TIM-002), a **Running timer** control appears in the top bar between **Log time** and **Notifications**.
- Collapsed state shows a green status dot, elapsed time in **`{hours}h {minutes}m`** format, and updates every **60** seconds.
- Tooltip when the timer is linked to an issue: **`Timer for {issue title} ({project name})`**; when linked to project only: **`Timer for {project name}`**; when unlinked: **`Running timer`**.
- Clicking the control opens a popover anchored to it with:
  - elapsed time (live, minute granularity),
  - linked **Project** and **Issue** when set, or **`No project selected`** when none,
  - **Stop and log** (primary) — same behavior as **Stop timer** in REQ-TIM-002,
  - **Discard timer** (secondary, destructive styling) — same confirmation as REQ-TIM-002.
- Clicking outside the popover or pressing **Escape** closes the popover without stopping the timer.
- The timer control is hidden when no timer is running.

### Duration display format

All user-visible durations use whole minutes internally and render as:

| Total minutes           | Display                                   |
| ----------------------- | ----------------------------------------- |
| **0**                   | **`0m`**                                  |
| **1–59**                | **`{m}m`**                                |
| **60+**, exact hours    | **`{h}h`**                                |
| **60+**, with remainder | **`{h}h {m}m`** (omit **`0m`** remainder) |

- Examples: **`45m`**, **`2h`**, **`2h 30m`**.

### Log time and edit dialogs

- **Log time** and **Edit time entry** open as modal dialogs centered on the viewport; they do **not** navigate to a separate route.
- Dialog width: **`32rem`** maximum on desktop; full-width with margin on mobile.
- **Log time** dialog title: **`Log time`**.
- **Edit time entry** dialog title: **`Edit time entry`**.
- Closing via **Cancel**, the dialog **×** control, **Escape**, or backdrop click discards unsaved dialog changes and does not save.
- Success and error feedback uses the global toast pattern (same as other features).

### Quick duration presets

- **Log time** and **Edit time entry** dialogs show preset chips below **Duration**: **`15m`**, **`30m`**, **`1h`**, **`2h`**, **`4h`**, **`8h`**.
- Selecting a preset sets **Hours** and **Minutes** accordingly; the user may adjust values afterward.
- Presets are not shown when **Duration** was pre-filled from a stopped timer.

### Description field

- **Description** uses a multiline text area with a live character counter in format **`{n}/500`** below the field.

### Cross-screen entry points

| Location                         | Action                        | Behavior                                                                                                                                                     |
| -------------------------------- | ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Top bar                          | **Log time**                  | Opens **Log time** dialog.                                                                                                                                   |
| **My time** header               | **Log time**                  | Opens **Log time** dialog.                                                                                                                                   |
| **My time** header               | **Start timer**               | Starts timer with no project/issue (REQ-TIM-002).                                                                                                            |
| **My time** empty state          | **Log time**                  | Primary button in empty state.                                                                                                                               |
| **Issue details** — **Time** tab | **Log time**, **Start timer** | REQ-TIM-003.                                                                                                                                                 |
| **Issues list** row overflow     | **Log time**                  | Opens **Log time** dialog with **Project** and **Issue** pre-filled and read-only; requires **Time.LogOwn** and **Project.Time.Log** on the issue's project. |
| **Project details** summary      | **View time report** link     | Opens **Time reports** with **Projects** filter set to this project and default date range; requires **Time.ViewReports**.                                   |

### Issue and project summaries

- **Issue details** metadata includes **Logged time** when the user has **Project.Time.View** on the issue's project; value uses duration display format; **`0m`** when none logged. See REQ-ISS-003.
- **Issue details** **Time** tab label includes the total in parentheses when total is greater than zero, for example **`Time (2h 30m)`**; when zero, the label is **`Time`** only.
- **Project details** summary includes **Logged time** when the user has **Project.Time.View**; value is total for the project (all time, not filtered by date). See REQ-PRJ-004.

### Permissions and visibility

- Navigation entries and header controls are hidden when the user lacks the required permission; they are not shown disabled.
- **View time report** on **Project details** requires **Time.ViewReports**; the link is hidden when absent.

### Out of scope for this REQ

- Custom user preferences for duration format (always use the rules above).
- Desktop system-tray or browser-extension timer.

---

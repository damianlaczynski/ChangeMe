# Frontend Guidelines

> **L5 — Implementation.** Scope: current conventions for writing Angular code in this frontend.
>
> **Product behaviour** (lists, forms, validation UX, toasts): [`product-standards.md`](../requirements/_shared/conventions/product-standards.md) (L2). **Feature rules**: target `FR-*` (L4). This file covers _how_ to implement in Angular with **@laczynski/ui**.

## Stack summary

Angular 21 standalone application with strict TypeScript settings, ESLint, and Prettier. UI components come from [**@laczynski/ui**](https://ui.laczynski.dev/) — a Fluent-inspired Angular component library. Global layout and page chrome use shared SCSS in `src/styles.scss` backed by Laczynski design tokens (`--color-*`). State uses a mix of Angular signals and RxJS Observables. HTTP calls go through a shared `ApiService`. Feature code is grouped under `src/app/features`.

For dev server, lint, format, and test commands from `src/ChangeMe.Frontend` or from the repository root (`npm run start:frontend`, `npm run lint:frontend`, and related scripts), see `AGENTS.md`.

## Project structure

- Routes are declared centrally in `src/app/app.routes.ts`.
- Use path aliases from `tsconfig.json` instead of long relative imports when an alias exists.
- Put feature-specific models in `features/<feature>/models`.
- Put validation limits, select options, labels, and other UI-oriented constants in a single `features/<feature>/utils/<feature>.utils.ts` file (for example `issue.utils.ts`, `auth.utils.ts`). Keep DTOs, enums, and request/response shapes in `models/`.
- Put shared transport or utility contracts in `shared/`.
- Put cross-cutting app services in `core/` or `features/auth/` depending on ownership.
- Use `app-back-button` with a fixed **label** and **route** for in-app back navigation (for example **`Back to issues list`** → **`/issues`**, **`Back to issue details`** → **`/issues/:id`**). Do not use browser history stacks or `sessionStorage` navigation stacks.

## Component rules

### Standalone components

- New components should remain standalone.
- Declare Angular and Laczynski UI dependencies in the component `imports` array.
- Keep route components under `features/<feature>/components/<component-name>/`.

### State and data loading

- Services return `Observable<T>`.
- Components may use signals for local UI state and derived values.
- Avoid scattering raw `HttpClient` usage through components; route all HTTP through feature services and the shared `ApiService`.
- When loading data in response to changing state, prefer an explicit RxJS pipeline over ad hoc nested subscriptions.

### Dependency injection

- Prefer `inject()` field initializers, matching the current codebase pattern.

### Naming

- Use descriptive names for signals, request objects, and callback parameters.
- Keep identifiers in English.
- Keep route paths and selectors consistent with existing feature naming.

## Service rules

- Feature services live in `features/<feature>/services`.
- Feature services own HTTP calls and orchestration only. Do not put validation limits or static select-option lists in services; import them from `features/<feature>/utils`.
- Shared request/response handling belongs in `shared/api/services/api.service.ts`.
- Keep endpoint strings centralized per service through a `baseEndpoint` field when the service owns one API area.

## API base URL

- **Development (`ng serve`):** explicit in `environment.development.ts` (`http://localhost:5000/api/v1`).
- **Production / Docker:** explicit in `public/runtime-config.js` (`apiUrl: '/api/v1'`) or `CHANGE_ME_API_URL` — no value in `environment.ts`.
- HTTP and SignalR services use `getApiUrl()` / `getNotificationsHubUrl()` from `src/environments/runtime-config.ts`.
- Deployment patterns (nginx `/api` proxy, CORS, split hosts): [deployment.md](../technical/deployment.md).

## Forms and templates

- Follow the existing Angular standalone template style already used in the repo.
- Use typed reactive forms with explicit control binding in templates: `[formControl]="form.controls.field"` (or `filtersForm.controls.field`, `criterion.controls.content`, and so on). Do not use `formControlName`, `formGroupName`, or `formArrayName`.
- Keep `[formGroup]="form"` on the `<form>` element for submit handling and group-level validators.
- For `FormArray` rows, iterate `form.controls.arrayName.controls` and bind nested fields with `[formControl]="row.controls.field"` from the loop variable.
- Keep user-facing text consistent within a feature. If a feature is already English-only in UI text, do not partially localize one screen.
- Prefer moving formatting or mapping logic out of templates when it starts to obscure the markup.

## @laczynski/ui

### Global setup

- Library styles are registered once in `angular.json` via `src/styles.scss`, which `@use`s `@laczynski/ui/src/lib/scss/main`.
- Icon sprite assets are copied from `node_modules/@laczynski/ui/assets/icons` to `/assets/icons` in `angular.json`.
- Root shell renders `<ui-toast-container />` and `<app-confirm-dialog />` in `app.component.ts`.
- Do not add a second UI kit beside `@laczynski/ui` for application screens.

### Component usage

- Import Laczynski components from `@laczynski/ui` in the standalone `imports` array of the component that uses them.
- Prefer Laczynski field components (`ui-text`, `ui-textarea`, `ui-email`, `ui-password`, `ui-dropdown`, `ui-checkbox`, `ui-file`) with reactive forms and `[formControl]` binding.
- Bind validation messages with `[errorText]` and `fieldError()` from `@shared/forms/field-error.ts` (presentational fields; no `autoValidation`).
- Use semantic page layout classes (`app-page`, `app-page__header`, `app-page__title`, `app-page__body`) for full screens. Reserve `ui-card` for smaller, self-contained content blocks (for example product tiles or compact panels), not whole pages.
- Use `ui-nav` for sidebar navigation (see `app-sidebar-nav` and the showcase layout examples). Do not build primary navigation from `ui-button` modifiers.
- Use `ui-accordion` for grouped form sections. Accordions should be open by default — add the `appDefaultExpanded` attribute and import `DefaultExpandedAccordionDirective` from `@shared/directives/default-expanded-accordion.directive`.
- Use `ui-message-bar` for screen-level request errors.
- Use the app `ToastService` facade in features for mutation feedback; do not inject Laczynski `ToastService` directly in feature code.
- Use `ui-tag` for compact status labels such as issue status or priority (`variant`: `primary` | `secondary` | `success` | `warning` | `danger` | `info`).
- Use `ui-button` with `text`, `icon` (Fluent icon name), `variant`, `appearance`, and `(click)`.
- Use `ui-spinner` or grid loading state for in-flight data.
- Use `uiTooltip` for icon-only or compact action hints.
- Use `ConfirmService` (`@core/confirm/services/confirm.service.ts`) for destructive confirmations; do not open ad hoc dialogs for the same pattern.
- **List screens** (Issues, Users, Roles) use `<qg-ui-data-grid>` from `@query-grid/ui` with `createAppGridResource` from `@shared/data/utils/grid.utils.ts`. Wrap the page in `app-page app-page--grid` and put the grid in `app-page__body app-page__body--grid` so page chrome stays fixed and only table rows scroll inside the grid. Column templates use the `qgColumn` directive; feature services pass `GridQuery` to the API as a `grid` query parameter and return `GridResult<T>`. See `features/issues/components/issues-list/` as the reference implementation.
- **Embedded lists** (issue tabs, sessions, notifications, role assigned users) call the same API shape with `GridQuery`/`GridResult`; use `shared/data/utils/grid.utils.ts` for `createGridQuery`, `hasMoreGridItems`, and `createIssueTabGridQuery`.
- Keep business logic in feature services and component TypeScript. Laczynski UI should handle presentation only.

### Theming and layout

- Global layout classes live in `src/styles.scss` (`app-shell`, `app-page`, `app-form-workspace`, `app-detail-grid`, `app-field-group`, `app-card-grid`, and related helpers). Prefer these shared classes over one-off inline styles. Do not use inline `style` attributes in templates.
- Application theme overrides live in `src/changeme-theme.scss`, imported after Laczynski in `src/styles.scss`. Palette follows Material Design 3 baseline (primary `#6750A4`, surface containers, `#1C1B1F` dark base).
- **Content width:** authenticated pages use the full workspace width. `app-page` caps readable content at `96rem` (~1536px); add `app-page--wide` on list, detail, and form screens that should use the full cap. Do not reintroduce a ~700px content column.
- **Page chrome:** use `app-page__header` (title + subtitle), `app-page__toolbar` (back link and primary row actions), and `app-page__body` (main content). Keep back navigation in the toolbar, not mixed into the body stack.
- **Create / edit forms:** use the sticky-footer workspace pattern so **Save** / **Create** stays visible while fields scroll:
  - Page: `app-page app-page--form` (+ `app-page--wide` when the form has many columns or checklist grids).
  - Body: `app-page__body app-page__body--form`.
  - Form: `app-form-workspace` with `app-form-workspace__scroll` (scrollable fields) and `app-form-workspace__footer` (actions). Put `ui-message-bar` submit errors inside the scroll region; put Cancel / Save in the footer.
  - Field layout: `app-form__grid` (2 columns from `sm`, 3 from `lg`); use `app-form__grid--two` when three columns are too wide (for example name pairs).
  - Group fields with `ui-accordion` + `appDefaultExpanded`. Read-only metadata on edit screens uses `ui-card appearance="filled"` with `app-detail-grid` inside.
- **Permission checklists:** use `ui-card` with `[checkbox]="true"`, `[selected]`, and `(selectedChange)` in an `app-card-grid` grouped by `app-field-group`. Read-only effective permissions use the same card grid without checkbox mode. See `permission-checklist` and `effective-permissions` components.
- **Sections and dividers:** named blocks on detail screens use `ui-accordion`, `ui-card`, or `app-section`. Use `ui-divider` between unrelated stacks when an accordion is too heavy (for example before a paginated embedded table).
- **Detail screens:** stack separate `ui-card` blocks inside `app-detail-layout` — overview metadata (`app-workspace-stat`), content sections in `app-detail-card-grid`, and a tabs card with `ui-tabs` `appearance="subtle"`. Tab content (for example comments) can use nested `ui-card` items. See `issue-details` and [Tabs workspace panel](https://ui.laczynski.dev/docs/components/tabs#workspace-panel-composition).
- **Badge overlay:** compact counts on icon-only controls use the `badge` input on `ui-button` (see `app-notifications-bell`). Status labels in grids and detail panels use `ui-tag` with `appearance="tint"`.
- Use Laczynski semantic CSS variables (`var(--color-neutral-background-1-rest)`, `var(--color-brand-primary)`, and so on) when a shared class does not exist yet.
- Do **not** add feature-level `*.component.css` / `*.component.scss` files unless a screen has a rule that cannot be expressed with shared classes or Laczynski inputs.
- Application font is **Inter** via `@fontsource-variable/inter` in `src/styles.scss`.
- Dark mode: `LayoutService` sets `data-theme="dark"` and `.dark` on `<html>`. `public/theme-init.js` restores the saved theme before Angular boots to avoid a light flash.
- Toggle light/dark through `LayoutService`; the shell header theme button calls `layoutService.toggleTheme()`.
- **Reduced motion** (`NFR-A11Y-001`): `LayoutService` listens for `prefers-reduced-motion: reduce` and toggles `app-reduced-motion` on `<html>`. Global styles shorten non-essential transitions when that class is present. Do not suppress compliance toasts or required policy dialogs.

### When adding a new screen

- Look at `features/auth` for form patterns and `features/issues` for grids, filters, and detail layouts.
- Issues routes are behind `authGuard`. Do not gate issues UI with `isAuthenticated`; keep auth checks in guards, `app.component` navigation, and `NotificationsRealtimeConnectionService` (push notifications only).
- Match existing layout classes (`app-page`, `app-page--grid`, `app-form-workspace`, `app-form__grid`, `app-page__toolbar`) before introducing new patterns.

## Existing repo patterns worth preserving

- Auth session state lives in `features/auth/services/auth.service.ts`.
- API response unwrapping and error conversion live in `shared/api/services/api.service.ts`.
- Route guarding stays in `features/auth/guards`.

## When changing frontend contracts

- If a backend DTO changes, update the matching frontend model first.
- Then update the feature service.
- Then update affected components and routes.
- Re-check auth-sensitive flows if the endpoint requires a token.

## Guardrails for AI agents

- Do not introduce a second HTTP abstraction beside `ApiService`.
- Do not create a new top-level frontend folder unless the existing `core` / `features` / `shared` split cannot fit the change.
- Do not hardcode backend URLs outside `environment.*` and the shared API layer.
- Before adding a new pattern, look for the nearest example in `features/issues` or `features/auth`.
- Follow Laczynski UI defaults: `variant="secondary"`, `appearance="filled"`, `size="medium"` unless the screen needs emphasis (`variant="primary"` for the main action only).

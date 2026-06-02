# Feature Recipes

> Scope: short implementation recipes for common tasks in this repository.

## Add a backend endpoint

1. Create or update the endpoint in `src/ChangeMe.Backend.Web/<Feature>/`.
2. Pick the endpoint base type (see **Endpoint conventions** in [backend-coding-guidelines.md](backend-coding-guidelines.md)):
   - `BaseEndpoint<TRequest, TResponse>` when FastEndpoints should bind body, query, or route into `TRequest`
   - `BaseEndpointWithoutRequest<TRequest, TResponse>` when there is no HTTP payload (use a parameterless `record` for `TRequest`)
   - a custom `Endpoint` / `EndpointWithoutRequest` only for multipart upload, binary download, or other non-standard responses
3. Add or update the validator in the same file when the request DTO needs validation.
4. Add the request contract and handler in `src/ChangeMe.Backend.UseCases/<Feature>/`.
5. Reuse domain behavior or add it in `Domain` if new invariants are introduced.
6. Add integration tests under `tests/ChangeMe.Backend.IntegrationTests/Endpoints/<Feature>/`.

## Add a persisted field

1. Update the domain model if the field is part of business state.
2. Update the EF configuration in `Infrastructure/Persistence/Config`.
3. Add a migration in `Infrastructure/Persistence/Migrations`.
4. Update use case DTOs and frontend models if the field crosses the API.
5. Add unit and integration coverage for the new behavior.

## Add a frontend screen backed by an existing endpoint

1. Add or extend the model in `features/<feature>/models`.
2. Add or extend the service in `features/<feature>/services`.
3. Create a standalone component under `features/<feature>/components/<name>/`.
4. Register the route in `src/app/app.routes.ts` if it is navigable directly.
5. Run lint and relevant tests.

## Change auth-sensitive behavior

1. Check backend endpoint auth defaults in `BaseEndpoint` / `BaseEndpointWithoutRequest`.
2. Check route guards under `features/auth/guards`.
3. Check token/session handling in `features/auth/services/auth.service.ts`.
4. Add or update integration coverage for authenticated and anonymous flows.

## Add file upload and download (reference: Issues attachments)

Use the **Issues attachments** slice as the template for new file features. Shared infrastructure lives under `Domain/Common/Attachments/`, `Infrastructure/FileStorage/`, and the **`attachments`** table (TPH).

### Backend

1. Add a derived attachment entity (for example `IssueAttachment : Attachment`) and aggregate methods for owner-specific rules.
2. Reuse `Infrastructure/FileStorage/`:
   - `IFileContentValidator` + Mime-Detective content inspection
   - `IFileStorageService` + `LocalFileStorageService` (`container` + `ownerId` paths)
3. Use **file-first upload with a single DB commit** (see Issues `UploadIssueAttachmentCommand`):
   - validate content, then create attachment metadata and side effects in memory
   - write file to storage using the generated opaque key
   - `SaveChanges` once (metadata + history/notifications)
   - on failure before `SaveChanges`, delete the stored file only; on `SaveChanges` failure, rely on EF rollback and delete the file best-effort
4. Reuse `AttachmentStorageCleanupJob` (Hangfire): orphaned stored files with no matching metadata row for all attachment types.
5. Add list/upload/download/delete use cases in `UseCases/<Feature>/`.
6. Add endpoints in `Web/<Feature>/`:
   - JSON list/delete via `BaseEndpoint` (route/query/body binding as needed)
   - parameterless JSON actions via `BaseEndpointWithoutRequest` (e.g. logout, mark-all-read)
   - multipart upload via `Endpoint` + `AllowFileUploads()`; send the handler `Result<T>` with `HttpContext.SendResultAsync`
   - binary download via `EndpointWithoutRequest`; set `Content-Disposition: attachment` and `X-Content-Type-Options: nosniff`, stream bytes to `Response.Body`; use `HttpContext.SendResultAsync` only for error `Result<T>` responses
7. Cascade-delete stored files when the owning aggregate is removed.
8. Add integration tests for happy path, validation failure, auth, and delete authorization.
9. Document deployment storage (volume, backup, retention) in [database-and-docker.md](database-and-docker.md).

### Frontend

1. Extend feature models and `features/<feature>/utils` with size/extension limits.
2. Add `ApiService.postFormData()` and `getBlob()` helpers; keep endpoint strings in the feature service.
3. Add a tab or panel component with upload control, paginated list, download, and uploader-only delete.
4. Reuse PrimeNG `p-fileupload` (basic mode) or equivalent; show inline validation errors and toasts for mutations.

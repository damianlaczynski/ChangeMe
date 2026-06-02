# Database and Docker Compose

> Scope: how persistence and `docker-compose.yml` fit **this solution**.

## EF Core migrations

Migration **`.cs` files are not shipped with this starter.** Add them when you are ready (name is yours; `InitialCreate` is a common first migration):

1. From the **solution root** (folder containing `src/` and `.config/`):

   ```powershell
   dotnet tool restore
   ```

2. Still from the solution root:

   ```powershell
   dotnet ef migrations add InitialCreate --project src/ChangeMe.Backend/src/ChangeMe.Backend.Infrastructure/ChangeMe.Backend.Infrastructure.csproj --startup-project src/ChangeMe.Backend/src/ChangeMe.Backend.Web/ChangeMe.Backend.Web.csproj --output-dir Persistence/Migrations
   ```

   If you do not use the local tool manifest, install the global tool once: `dotnet tool install --global dotnet-ef` (version aligned with your SDK), then run the same `dotnet ef` command.

3. **`Database:ApplyMigrationsOnStartup`** defaults to `false` in `appsettings.json` and `appsettings.Development.json`. After migration files exist, set it to `true` in Development (or run `dotnet ef database update`) so pending migrations apply when the API starts. If enabled with zero migrations, startup fails with a clear error instead of creating an empty database.

**Production:** Prefer migrations applied from CI/CD (`dotnet ef database update`, reviewed SQL, or dedicated migration jobs) rather than many concurrent app instances all racing `Migrate()` at startup.

### PostgreSQL vs SQL Server migrations

Each EF Core provider emits **different DDL** and stores provider-specific metadata in snapshots and history. You maintain **one** migration history per provider configuration this solution was generated with; mixing snapshots across providers breaks deployments.

<!--#if (PostgreSQL) -->

## PostgreSQL

- **Docker Compose** runs `postgres` (image `postgres:16`) and wires the API to that host.
- Default connection string for local dev: `src/ChangeMe.Backend/src/ChangeMe.Backend.Web/appsettings.Development.json`.

<!--#endif-->

<!--#if (SqlServer) -->

## SQL Server

- **Docker Compose** runs `sqlserver` (`mcr.microsoft.com/mssql/server:2022-latest`) and **`sqlserver-init`** so the database named in your connection string exists before the API connects.
- Default connection string: `appsettings.Development.json` next to `Program.cs`.
- **`sa` password** in Compose is for local development only.

<!--#endif-->

- **Integration tests** use disposable databases via Testcontainers (`BackendWebApplicationFactory`).

## Demo data (optional)

**System seed** (`ApplicationDataSeeder`) runs when migrations are applied at startup or via the DataGenerator tool: system roles and optional `InitialAdministrator` from configuration.

**Demo dataset** (sample users, issues, comments, notifications) is separate. Use the DataGenerator console tool after migrations:

```powershell
npm run data:generate
npm run data:generate -- --reset
```

See [data-generator.md](data-generator.md) for architecture, configuration, and troubleshooting.

## File storage (issue attachments)

Issue attachment **file bytes** are stored on disk under **`FileStorage:RootPath`**, not in the database. **Metadata** (file name, size, content type, opaque storage key, status) lives in the shared **`attachments`** table using **TPH** inheritance: the **`AttachmentType`** discriminator distinguishes types; **`IssueAttachment`** (`AttachmentType` = `Issue`) is the current derived type. New attachment owners add another subclass and storage container name.

### Layout

```
{FileStorage:RootPath}/
  {container}/
    {ownerId}/
      {storageKey}
```

Example for issues:

```
{FileStorage:RootPath}/issues/{issueId}/{storageKey}
```

- **`storageKey`** is server-generated (GUID); user file names are never used as paths.
- Default local dev path: **`../../storage`** (relative to the Web project working directory).
- **Docker Compose** mounts a named volume at **`/app/storage`** and sets **`FileStorage__RootPath=/app/storage`** on the `backend` service so attachments survive container restarts.

### Configuration

Settings live under **`FileStorage`** in `appsettings.json` / environment variables:

| Setting                                    | Default           | Purpose                                                   |
| ------------------------------------------ | ----------------- | --------------------------------------------------------- |
| `RootPath`                                 | `../../storage`   | Root directory for all stored files                       |
| `MaxFileSizeBytes`                         | `5242880` (5 MB)  | Per-file upload limit                                     |
| `AllowedExtensions`                        | `.pdf`, `.png`, … | Whitelist enforced with Mime-Detective content inspection |
| `CleanupCronExpression`                    | `0 * * * *`       | Schedule for orphaned-file reconciliation                 |
| `CleanupConcurrentExecutionTimeoutSeconds` | `3600`            | Hangfire lock timeout for cleanup job (avoid overlap)     |

### Retention and cleanup

- Attachment metadata and stored files are kept until the attachment is deleted or the owning issue is deleted.
- **`AttachmentStorageCleanupJob`** (Hangfire) deletes **orphaned files** on disk that have no matching row in **`attachments`** (for example after a failed upload or process crash between storage write and DB commit).
- Deleting an issue cascades attachment metadata and deletes all stored files for that issue.

### Backup (production)

Include **`{FileStorage:RootPath}/issues/`** in your backup plan **together with the application database**. Restoring only the database without files leaves broken download links; restoring files without matching metadata leaves orphaned blobs (cleaned up eventually by the cleanup job, but downloads will fail until then).

For cloud deployments, consider moving **`IFileStorageService`** to object storage (S3, Azure Blob) with server-side encryption and lifecycle policies; the Issues slice is the reference pattern for metadata + opaque keys.

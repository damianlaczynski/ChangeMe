import { defineConfig, devices } from '@playwright/test';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const e2eDir = path.dirname(fileURLToPath(import.meta.url));
const frontendRoot = path.resolve(e2eDir, '..');
const repoRoot = path.resolve(frontendRoot, '../..');

const baseURL = process.env['E2E_BASE_URL'] ?? 'http://localhost:4200';

function backendWebServerEnv(): Record<string, string> {
  const env: Record<string, string> = {
    ASPNETCORE_ENVIRONMENT: 'Development',
    Auth__EmailVerification__Enabled: 'false'
  };

  const connectionString = process.env['ConnectionStrings__DefaultConnection'];
  if (connectionString) {
    env['ConnectionStrings__DefaultConnection'] = connectionString;
  }

  return env;
}

export default defineConfig({
  testDir: path.join(e2eDir, 'tests'),
  fullyParallel: false,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 1 : 0,
  workers: 1,
  timeout: 60_000,
  reporter: process.env['CI'] ? [['github'], ['list']] : [['list']],
  use: {
    baseURL,
    trace: 'on-first-retry'
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] }
    }
  ],
  webServer: [
    {
      command:
        'dotnet run --no-launch-profile --project src/ChangeMe.Backend/src/ChangeMe.Backend.Web/ChangeMe.Backend.Web.csproj',
      cwd: repoRoot,
      env: backendWebServerEnv(),
      url: 'http://localhost:5000/swagger/index.html',
      timeout: 180_000,
      reuseExistingServer: !process.env['CI']
    },
    {
      command: 'npm start',
      cwd: frontendRoot,
      url: baseURL,
      timeout: 180_000,
      reuseExistingServer: !process.env['CI']
    }
  ]
});

import { expect, type Page } from '@playwright/test';

export const e2eCredentials = {
  email: process.env['E2E_USER_EMAIL'] ?? 'admin@example.local',
  password: process.env['E2E_USER_PASSWORD'] ?? 'admin123'
};

export async function dismissOptionalPasskeyPrompt(page: Page): Promise<void> {
  const notNow = page.getByRole('button', { name: 'Not now' });
  if (await notNow.isVisible().catch(() => false)) {
    await notNow.click();
  }
}

export async function login(page: Page): Promise<void> {
  await page.goto('/login');
  await page.getByLabel('Email').fill(e2eCredentials.email);
  await page
    .locator('input[autocomplete="current-password"]')
    .fill(e2eCredentials.password);
  await page.locator('form button[type="submit"]').click();
  await dismissOptionalPasskeyPrompt(page);
  await expect(
    page.getByText('Workspaces that group issues and future project features.')
  ).toBeVisible({
    timeout: 20_000
  });
}

export async function logout(page: Page): Promise<void> {
  await page.getByRole('button', { name: 'Logout' }).first().click();
  await expect(page.getByText('Welcome back')).toBeVisible({ timeout: 15_000 });
}

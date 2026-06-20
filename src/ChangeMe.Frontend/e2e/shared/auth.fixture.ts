import { expect, type Page } from '@playwright/test';
import { e2eCredentials } from './env';

export async function dismissOptionalPasskeyPrompt(page: Page): Promise<void> {
  const notNow = page.getByRole('button', { name: 'Not now' });
  if (await notNow.isVisible().catch(() => false)) {
    await notNow.click();
  }
}

export async function loginViaUi(page: Page): Promise<void> {
  await page.goto('/login');
  await page.getByLabel('Email').fill(e2eCredentials.email);
  await page
    .locator('input[autocomplete="current-password"]')
    .fill(e2eCredentials.password);
  await page.locator('form button[type="submit"]').click();
  await dismissOptionalPasskeyPrompt(page);
  await expect(page).toHaveURL(/\/issues/, { timeout: 20_000 });
}

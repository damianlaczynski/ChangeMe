import { expect, type Page } from '@playwright/test';
import { e2eCredentials } from './env';

/** Used by globalSetup and auth specs (features/auth re-exports as `login`). */
export async function loginViaUi(page: Page): Promise<void> {
  await page.goto('/login');
  await page.getByLabel('Email').fill(e2eCredentials.email);
  await page.getByLabel('Password').fill(e2eCredentials.password);
  await page.getByRole('button', { name: 'Sign in' }).click();
  await expect(page).toHaveURL(/\/issues/, { timeout: 20_000 });
}

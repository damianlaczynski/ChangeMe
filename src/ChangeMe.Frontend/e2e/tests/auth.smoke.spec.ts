import { expect, test } from '@playwright/test';
import { login, logout } from '../fixtures/auth';

test.describe('auth smoke', () => {
  test('redirects unauthenticated users to login', async ({ page }) => {
    await page.goto('/projects');

    await expect(page).toHaveURL(/\/login\?returnUrl=%2Fprojects/);
    await expect(page.getByText('Welcome back')).toBeVisible();
  });

  test('signs in and signs out with a browser session', async ({ page }) => {
    await login(page);
    await page.reload();
    await expect(
      page.getByText('Workspaces that group issues and future project features.')
    ).toBeVisible();

    await logout(page);
    await page.goto('/projects');
    await expect(page).toHaveURL(/\/login/);
  });
});

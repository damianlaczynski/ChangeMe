import { expect, test } from '@playwright/test';
import { login } from '../auth/auth.fixture';

test.describe('invitations smoke', () => {
  test('admin can open invitations list', async ({ page }) => {
    await login(page);
    await page.getByRole('link', { name: 'Invitations' }).click();
    await expect(page).toHaveURL(/\/invitations/);
    await expect(page.getByRole('button', { name: 'Invite user' })).toBeVisible();
  });

  test('invalid accept link shows rejection message', async ({ page }) => {
    await page.goto('/invitations/accept/not-a-real-token');
    await expect(page.getByText('This invitation link is not valid.')).toBeVisible({
      timeout: 15_000
    });
  });
});

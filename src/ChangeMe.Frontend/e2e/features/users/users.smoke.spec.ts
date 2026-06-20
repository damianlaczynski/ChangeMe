import { E2eCleanupRegistry } from '../../shared/api/cleanup';
import { expect, test } from '../../shared/test';
import { gotoUsersList, selectMultiselectOption, userIdFromUrl } from './users.fixture';

const cleanup = new E2eCleanupRegistry();

test.afterAll(async ({ request }) => {
  const { E2eApiClient } = await import('../../shared/api/client');
  await cleanup.run(new E2eApiClient(request));
});

test.describe('users smoke', () => {
  test('lists users, invites a user, and edits their profile', async ({ page }) => {
    const email = `e2e-users-${crypto.randomUUID()}@example.com`;
    const firstName = 'E2E';
    const lastName = 'Invite';
    const updatedFirstName = 'E2EUpdated';

    await gotoUsersList(page);
    await page.getByRole('button', { name: 'Invite user' }).click();
    await expect(page).toHaveURL(/\/users\/invite/);

    await page.getByLabel('First name (optional)').fill(firstName);
    await page.getByLabel('Last name (optional)').fill(lastName);
    await page.getByLabel('Email').fill(email);
    await selectMultiselectOption(page, 'Roles', 'User');
    await page.getByRole('button', { name: 'Send invitation' }).click();

    await expect(page.getByRole('alert')).not.toBeVisible();
    await expect(page).toHaveURL(/\/users\/[0-9a-f-]+/i, { timeout: 20_000 });
    cleanup.registerUser(userIdFromUrl(page));
    await expect(
      page.locator('.p-card-title').getByText(`${firstName} ${lastName}`)
    ).toBeVisible();

    await page.getByRole('button', { name: 'Edit' }).click();
    await expect(page).toHaveURL(/\/users\/[0-9a-f-]+\/edit/);
    await page.getByLabel(/^First name/).fill(updatedFirstName);
    await page.getByRole('button', { name: 'Save changes' }).click();

    await expect(page).toHaveURL(/\/users\/[0-9a-f-]+(?!\/edit)/, { timeout: 20_000 });
    await expect(
      page.locator('.p-card-title').getByText(`${updatedFirstName} ${lastName}`)
    ).toBeVisible();
  });
});

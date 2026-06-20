import { E2eCleanupRegistry } from '../../shared/api/cleanup';
import { e2eTitle } from '../../shared/env';
import { expect, test } from '../../shared/test';
import {
  expectRolesList,
  gotoRolesList,
  roleIdFromUrl,
  selectViewUsersPermission
} from './roles.fixture';

const cleanup = new E2eCleanupRegistry();

test.afterAll(async ({ request }) => {
  const { E2eApiClient } = await import('../../shared/api/client');
  await cleanup.run(new E2eApiClient(request));
});

test.describe('roles smoke', () => {
  test('lists roles, creates a custom role, and edits it', async ({ page }) => {
    const roleName = e2eTitle('roles');
    const updatedRoleName = `${roleName}-edited`;

    await gotoRolesList(page);
    await page.getByRole('button', { name: 'Add role' }).click();
    await expect(page).toHaveURL(/\/roles\/create/);

    await page.locator('#name').fill(roleName);
    await selectViewUsersPermission(page);
    await page.getByRole('button', { name: 'Create role' }).click();

    await expect(page).toHaveURL(/\/roles\/[0-9a-f-]+/i, { timeout: 20_000 });
    cleanup.registerRole(roleIdFromUrl(page));
    await expect(
      page.locator('.p-card-title').getByText(roleName, { exact: true })
    ).toBeVisible();

    await page.getByRole('button', { name: 'Edit' }).click();
    await expect(page).toHaveURL(/\/roles\/[0-9a-f-]+\/edit/);
    await page.locator('#name').fill(updatedRoleName);
    await page.getByRole('button', { name: 'Save changes' }).click();

    await expect(page).toHaveURL(/\/roles\/[0-9a-f-]+(?!\/edit)/, { timeout: 20_000 });
    await expect(
      page.locator('.p-card-title').getByText(updatedRoleName, { exact: true })
    ).toBeVisible();
  });

  test('opens role details from the list', async ({ page }) => {
    await gotoRolesList(page);
    await expectRolesList(page);

    const firstRoleLink = page.locator('table tbody tr').first().getByRole('link');
    const roleName = (await firstRoleLink.textContent())?.trim();
    expect(roleName).toBeTruthy();

    await firstRoleLink.click();
    await expect(page).toHaveURL(/\/roles\/[0-9a-f-]+/i);
    await expect(
      page.getByRole('button', { name: 'Back to roles list' })
    ).toBeVisible();
    await expect(
      page.locator('.p-card-title').getByText(roleName!, { exact: true })
    ).toBeVisible();
  });
});

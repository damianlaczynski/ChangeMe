import { expect, type Page } from '@playwright/test';
import { PermissionCodes } from '@shared/authorization/permission-codes';

export async function gotoRolesList(page: Page): Promise<void> {
  await page.goto('/roles');
  await expectRolesList(page);
}

export async function expectRolesList(page: Page): Promise<void> {
  await expect(page).toHaveURL(/\/roles/);
  await expect(
    page.getByText('Browse, search and manage roles and permissions.')
  ).toBeVisible({
    timeout: 20_000
  });
}

export async function selectPermission(
  page: Page,
  permissionCode: string
): Promise<void> {
  await page.locator(`[id="${permissionCode}"]`).click();
}

export async function selectViewUsersPermission(page: Page): Promise<void> {
  await selectPermission(page, PermissionCodes.usersView);
}

export function roleIdFromUrl(page: Page): string {
  const match = page.url().match(/\/roles\/([0-9a-f-]+)/i);
  if (!match?.[1]) {
    throw new Error(`Could not parse role id from URL: ${page.url()}`);
  }

  return match[1];
}

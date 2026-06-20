import { expect, type Page } from '@playwright/test';

export async function gotoUsersList(page: Page): Promise<void> {
  await page.goto('/users');
  await expectUsersList(page);
}

export async function expectUsersList(page: Page): Promise<void> {
  await expect(page).toHaveURL(/\/users/);
  await expect(page.getByText('Browse, search and manage user accounts.')).toBeVisible({
    timeout: 20_000
  });
}

export async function selectMultiselectOption(
  page: Page,
  label: string,
  optionLabel: string
): Promise<void> {
  const field = page.locator('.flex.flex-col').filter({
    has: page.locator('label', { hasText: label })
  });
  await field.locator('.p-multiselect').click();
  await page.getByRole('option', { name: optionLabel, exact: true }).click();
  await page.keyboard.press('Escape');
}

export function userIdFromUrl(page: Page): string {
  const match = page.url().match(/\/users\/([0-9a-f-]+)/i);
  if (!match?.[1]) {
    throw new Error(`Could not parse user id from URL: ${page.url()}`);
  }

  return match[1];
}

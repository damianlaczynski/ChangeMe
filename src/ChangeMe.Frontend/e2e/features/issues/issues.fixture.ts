import { expect, type Page } from '@playwright/test';

export async function gotoIssuesList(page: Page): Promise<void> {
  await page.goto('/issues');
  await expectIssuesList(page);
}

export async function expectIssuesList(page: Page): Promise<void> {
  await expect(page).toHaveURL(/\/issues/);
  await expect(page.getByText('Browse, filter and monitor issues.')).toBeVisible({
    timeout: 20_000
  });
}

export function issueIdFromUrl(page: Page): string {
  const match = page.url().match(/\/issues\/([0-9a-f-]+)/i);
  if (!match?.[1]) {
    throw new Error(`Could not parse issue id from URL: ${page.url()}`);
  }

  return match[1];
}

import { expect, test } from '@playwright/test';
import { createIssueForE2e, getFirstProjectIdForE2e } from '../fixtures/api';
import { login } from '../fixtures/auth';

test.describe('issues smoke', () => {
  test('opens issue details from the list', async ({ page, request }) => {
    const issueTitle = `E2E list ${Date.now()}`;
    const projectId = await getFirstProjectIdForE2e(request);
    await createIssueForE2e(request, issueTitle);
    await login(page);

    await page.goto(`/projects/${projectId}/issues`);
    await page.getByRole('link', { name: issueTitle }).click();
    await expect(
      page.getByRole('button', { name: 'Back to issues list' })
    ).toBeVisible();
    await expect(page.getByRole('main').getByText(issueTitle)).toBeVisible();
  });

  test('filters the issues list by search text', async ({ page, request }) => {
    const issueTitle = `E2E filter ${Date.now()}`;
    const projectId = await getFirstProjectIdForE2e(request);
    await createIssueForE2e(request, issueTitle);
    await login(page);

    await page.goto(`/projects/${projectId}/issues`);
    await page.getByPlaceholder('Search issues...').fill(issueTitle);
    await page.getByRole('button', { name: 'Search' }).click();

    await expect(page.getByRole('link', { name: issueTitle })).toBeVisible();
  });

  test('creates an issue through the form', async ({ page, request }) => {
    const issueTitle = `E2E create ${Date.now()}`;
    const projectId = await getFirstProjectIdForE2e(request);
    await login(page);

    await page.goto(`/projects/${projectId}/issues/create`);
    await expect(
      page.getByText('Create a new issue and move to its details page after saving')
    ).toBeVisible();
    await page.getByLabel('Title').fill(issueTitle);
    await page.getByLabel('Description').fill('Created by Playwright E2E.');
    await page.locator('form button[type="submit"]').click();

    await expect(page.getByRole('button', { name: 'Back to issues list' })).toBeVisible(
      { timeout: 20_000 }
    );
    await expect(page.getByRole('main').getByText(issueTitle)).toBeVisible();
  });
});

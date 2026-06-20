import { E2eCleanupRegistry } from '../../shared/api/cleanup';
import { e2eTitle } from '../../shared/env';
import { expect, test } from '../../shared/test';
import { createIssue } from './issues.api';
import { gotoIssuesList, issueIdFromUrl } from './issues.fixture';

const cleanup = new E2eCleanupRegistry();

test.afterAll(async ({ request }) => {
  const { E2eApiClient } = await import('../../shared/api/client');
  await cleanup.run(new E2eApiClient(request));
});

test.describe('issues smoke', () => {
  test('opens issue details from the list', async ({ page, apiClient }) => {
    const issueTitle = e2eTitle('issues-list');
    const issue = await createIssue(apiClient, issueTitle);
    cleanup.registerIssue(issue.id);

    await gotoIssuesList(page);
    await page.getByRole('link', { name: issueTitle }).click();

    await expect(
      page.getByRole('button', { name: 'Back to issues list' })
    ).toBeVisible();
    await expect(page.getByRole('main').getByText(issueTitle)).toBeVisible();
  });

  test('filters the issues list by search text', async ({ page, apiClient }) => {
    const issueTitle = e2eTitle('issues-filter');
    const issue = await createIssue(apiClient, issueTitle);
    cleanup.registerIssue(issue.id);

    await gotoIssuesList(page);
    await page.getByPlaceholder('Search issues...').fill(issueTitle);
    await page.getByRole('button', { name: 'Search' }).click();

    await expect(page.getByRole('link', { name: issueTitle })).toBeVisible();
  });

  test('creates an issue through the form', async ({ page }) => {
    const issueTitle = e2eTitle('issues-create');

    await gotoIssuesList(page);
    await page.getByRole('button', { name: 'Add issue' }).click();
    await expect(
      page.getByText('Create a new issue and move to its details page after saving')
    ).toBeVisible();
    await page.getByLabel('Title').fill(issueTitle);
    await page.getByLabel('Description').fill('Created by Playwright E2E.');
    await page.locator('form button[type="submit"]').click();

    await expect(page.getByRole('button', { name: 'Back to issues list' })).toBeVisible(
      {
        timeout: 20_000
      }
    );
    await expect(page.getByRole('main').getByText(issueTitle)).toBeVisible();

    cleanup.registerIssue(issueIdFromUrl(page));
  });
});

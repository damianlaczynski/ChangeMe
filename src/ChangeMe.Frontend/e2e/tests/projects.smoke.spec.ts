import { expect, test } from '@playwright/test';
import { createProjectForE2e, registerUserForE2e } from '../fixtures/api';
import { login } from '../fixtures/auth';

test.describe('projects smoke', () => {
  test('opens project workspace from the projects list', async ({ page, request }) => {
    const projectName = `E2E workspace ${Date.now()}`;
    const projectKey = `W${Date.now().toString().slice(-5)}`;
    await createProjectForE2e(request, projectName, projectKey);
    await login(page);

    await page.goto('/projects');
    await page.getByPlaceholder('Search projects...').fill(projectName);
    await page.getByRole('button', { name: 'Search' }).click();
    await page.getByRole('link', { name: projectName }).click();

    await expect(page.getByRole('button', { name: 'All projects' })).toBeVisible({
      timeout: 15_000
    });
    await expect(page.getByText('Overview', { exact: true })).toBeVisible();
  });

  test('project owner adds a member from settings', async ({ page, request }) => {
    const projectName = `E2E members ${Date.now()}`;
    const projectKey = `M${Date.now().toString().slice(-5)}`;
    const member = await registerUserForE2e(request, 'member');
    const projectId = await createProjectForE2e(request, projectName, projectKey);
    await login(page);

    await page.goto(`/projects/${projectId}/settings`);
    await expect(page.getByText('Project workspace access and roles.')).toBeVisible();

    await page.locator('#memberUser').click();
    await page.getByText(member.displayLabel, { exact: true }).click();
    await page.getByRole('button', { name: 'Add member' }).click();

    await expect(page.getByText('Member added')).toBeVisible({ timeout: 15_000 });
    await expect(page.getByRole('main').getByText(member.displayLabel)).toBeVisible();
  });
});

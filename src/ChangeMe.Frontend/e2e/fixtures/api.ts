import { expect, type APIRequestContext } from '@playwright/test';
import { e2eCredentials } from './auth';

const apiUrl = process.env['E2E_API_URL'] ?? 'http://localhost:5000/api';

async function getAuthToken(request: APIRequestContext): Promise<string> {
  const response = await request.post(`${apiUrl}/auth/login`, {
    data: {
      email: e2eCredentials.email,
      password: e2eCredentials.password
    }
  });

  expect(response.ok()).toBeTruthy();

  const body = (await response.json()) as {
    value?: { authSession?: { token?: string } };
  };

  const token = body.value?.authSession?.token;
  expect(token).toBeTruthy();

  return token!;
}

export async function createIssueForE2e(
  request: APIRequestContext,
  title: string
): Promise<void> {
  const token = await getAuthToken(request);

  const response = await request.post(`${apiUrl}/issues`, {
    headers: {
      Authorization: `Bearer ${token}`
    },
    data: {
      title,
      description: 'Created by Playwright E2E setup.',
      status: 0,
      priority: 1,
      assignedToUserId: null,
      watchAfterCreate: false,
      acceptanceCriteria: []
    }
  });

  expect(response.ok()).toBeTruthy();
}

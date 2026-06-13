import { expect, type APIRequestContext } from '@playwright/test';
import { e2eCredentials } from './auth';

const apiUrl = process.env['E2E_API_URL'] ?? 'http://localhost:5000/api';

export async function getAuthToken(request: APIRequestContext): Promise<string> {
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

async function getDefaultProjectId(
  request: APIRequestContext,
  token: string
): Promise<string> {
  const response = await request.get(`${apiUrl}/projects/for-selection`, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  expect(response.ok()).toBeTruthy();

  const body = (await response.json()) as {
    value?: Array<{ id: string }>;
  };

  const projectId = body.value?.[0]?.id;
  expect(projectId).toBeTruthy();

  return projectId!;
}

export async function createProjectForE2e(
  request: APIRequestContext,
  name: string,
  key: string
): Promise<string> {
  const token = await getAuthToken(request);

  const response = await request.post(`${apiUrl}/projects`, {
    headers: {
      Authorization: `Bearer ${token}`
    },
    data: {
      name,
      key,
      description: 'Created by Playwright E2E setup.',
      visibility: 'INTERNAL',
      color: '#3B82F6'
    }
  });

  expect(response.ok()).toBeTruthy();

  const body = (await response.json()) as {
    value?: { id: string };
  };

  const projectId = body.value?.id;
  expect(projectId).toBeTruthy();

  return projectId!;
}

export async function registerUserForE2e(
  request: APIRequestContext,
  label: string
): Promise<{ email: string; displayLabel: string }> {
  const email = `e2e-${label}-${Date.now()}@example.com`;
  const password = 'StrongPass123!';

  const registerResponse = await request.post(`${apiUrl}/auth/register`, {
    data: {
      FirstName: 'E2E',
      LastName: 'Member',
      Email: email,
      Password: password
    }
  });

  expect(registerResponse.ok()).toBeTruthy();

  const adminToken = await getAuthToken(request);
  const usersResponse = await request.get(
    `${apiUrl}/users?searchText=${encodeURIComponent(email)}&pageNumber=1&pageSize=10`,
    {
      headers: {
        Authorization: `Bearer ${adminToken}`
      }
    }
  );

  expect(usersResponse.ok()).toBeTruthy();

  const usersBody = (await usersResponse.json()) as {
    value?: { items?: Array<{ id: string; email: string }> };
  };

  const userId = usersBody.value?.items?.find((user) => user.email === email)?.id;
  expect(userId).toBeTruthy();

  const confirmResponse = await request.post(
    `${apiUrl}/users/${userId}/confirm-email`,
    {
      headers: {
        Authorization: `Bearer ${adminToken}`
      },
      data: {}
    }
  );

  expect(confirmResponse.ok()).toBeTruthy();

  const assignableResponse = await request.get(`${apiUrl}/issues/assignable-users`, {
    headers: {
      Authorization: `Bearer ${adminToken}`
    }
  });

  expect(assignableResponse.ok()).toBeTruthy();

  const displayLabel = `E2E Member (${email})`;
  const assignableBody = (await assignableResponse.json()) as {
    value?: Array<{ displayLabel: string }>;
  };

  expect(
    assignableBody.value?.some((user) => user.displayLabel === displayLabel)
  ).toBeTruthy();

  return {
    email,
    displayLabel
  };
}

export async function createIssueForE2e(
  request: APIRequestContext,
  title: string
): Promise<void> {
  const token = await getAuthToken(request);
  const projectId = await getDefaultProjectId(request, token);

  const response = await request.post(`${apiUrl}/issues`, {
    headers: {
      Authorization: `Bearer ${token}`
    },
    data: {
      projectId,
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

export async function getFirstProjectIdForE2e(
  request: APIRequestContext
): Promise<string> {
  const token = await getAuthToken(request);
  return getDefaultProjectId(request, token);
}

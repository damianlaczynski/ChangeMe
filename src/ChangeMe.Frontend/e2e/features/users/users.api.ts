import type { E2eApiClient } from '../../shared/api/client';

export async function cancelInvitation(
  client: E2eApiClient,
  userId: string
): Promise<void> {
  await client.postNoBody(`/users/${userId}/cancel-invitation`);
}

export async function deactivateUser(
  client: E2eApiClient,
  userId: string
): Promise<void> {
  await client.postNoBody(`/users/${userId}/deactivate`);
}

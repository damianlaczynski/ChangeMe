import type { E2eApiClient } from '../../shared/api/client';

export async function deleteRole(client: E2eApiClient, roleId: string): Promise<void> {
  await client.delete(`/roles/${roleId}`);
}

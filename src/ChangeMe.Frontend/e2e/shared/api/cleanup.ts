import { E2eApiClient } from './client';

export class E2eCleanupRegistry {
  private readonly issueIds = new Set<string>();
  private readonly roleIds = new Set<string>();
  private readonly userIds = new Set<string>();

  registerIssue(id: string): void {
    this.issueIds.add(id);
  }

  registerRole(id: string): void {
    this.roleIds.add(id);
  }

  registerUser(id: string): void {
    this.userIds.add(id);
  }

  async run(client: E2eApiClient): Promise<void> {
    for (const id of this.issueIds) {
      await client.delete(`/issues/${id}`).catch(() => undefined);
    }

    for (const id of this.roleIds) {
      await client.delete(`/roles/${id}`).catch(() => undefined);
    }

    for (const id of this.userIds) {
      await client.postNoBody(`/users/${id}/cancel-invitation`).catch(async () => {
        await client.postNoBody(`/users/${id}/deactivate`).catch(() => undefined);
      });
    }
  }
}

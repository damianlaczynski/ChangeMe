import { Component, computed, input } from '@angular/core';
import { EffectivePermissionDto } from '@features/users/models/user.model';
import {
  UserMessages,
  formatFromRoles,
  groupEffectivePermissions
} from '@features/users/utils/users.utils';

@Component({
  selector: 'app-effective-permissions',
  imports: [],
  template: `
    <section class="flex flex-col gap-3">
      <h3 class="text-color m-0 text-base font-semibold">Effective permissions</h3>

      @if (emptySelectionMessage()) {
        <p class="text-muted-color m-0 text-sm">{{ emptySelectionMessage() }}</p>
      } @else if (permissions().length === 0) {
        <p class="text-muted-color m-0 text-sm">{{ UserMessages.noPermissions }}</p>
      } @else {
        @for (group of groupedPermissions(); track group.group) {
          <div class="flex flex-col gap-2">
            <h4
              class="text-muted-color m-0 text-sm font-semibold tracking-wide uppercase"
            >
              {{ group.group }}
            </h4>
            <ul class="m-0 flex list-none flex-col gap-3 p-0">
              @for (permission of group.items; track permission.code) {
                <li
                  class="border-surface-200 rounded-lg border p-3 dark:border-surface-700"
                >
                  <div class="text-color font-medium">{{ permission.label }}</div>
                  <p class="text-muted-color m-0 mt-1 text-sm">
                    {{ permission.description }}
                  </p>
                  <p class="text-muted-color m-0 mt-2 text-sm">
                    {{ formatFromRoles(permission.fromRoleNames) }}
                  </p>
                </li>
              }
            </ul>
          </div>
        }
      }
    </section>
  `
})
export class EffectivePermissionsComponent {
  readonly permissions = input<EffectivePermissionDto[]>([]);
  readonly roleIdsSelected = input(true);

  readonly UserMessages = UserMessages;
  readonly formatFromRoles = formatFromRoles;

  readonly groupedPermissions = computed(() =>
    groupEffectivePermissions(this.permissions())
  );

  readonly emptySelectionMessage = computed(() =>
    this.roleIdsSelected() ? null : UserMessages.selectRoleToPreview
  );
}

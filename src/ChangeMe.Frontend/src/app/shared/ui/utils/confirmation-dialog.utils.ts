import { Confirmation } from 'primeng/api';

export const cancelConfirmationButtonProps = {
  label: 'Cancel',
  severity: 'secondary',
  outlined: true
} as const;

export const destructiveConfirmationIcon = 'pi pi-exclamation-triangle';

export const destructiveMenuItemDangerClasses = {
  labelClass: 'text-red-600 dark:text-red-400',
  iconClass: 'text-red-600 dark:text-red-400'
} as const;

export function createDestructiveConfirmationOptions(
  acceptLabel: string
): Pick<Confirmation, 'icon' | 'acceptButtonProps' | 'rejectButtonProps'> {
  return {
    icon: destructiveConfirmationIcon,
    acceptButtonProps: { label: acceptLabel, severity: 'danger' },
    rejectButtonProps: cancelConfirmationButtonProps
  };
}

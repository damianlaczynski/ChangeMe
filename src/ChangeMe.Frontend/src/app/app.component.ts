import { Component } from '@angular/core';
import { AppShellComponent } from '@core/layout/components/app-shell/app-shell.component';
import { ToastConfig } from '@core/toast/utils/toast.utils';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Toast } from 'primeng/toast';

@Component({
  selector: 'app-root',
  imports: [AppShellComponent, Toast, ConfirmDialog],
  template: `
    <app-shell />
    <p-toast [key]="toastKey" position="top-right" />
    <p-confirmDialog />
  `
})
export class AppComponent {
  readonly toastKey = ToastConfig.KEY;
}

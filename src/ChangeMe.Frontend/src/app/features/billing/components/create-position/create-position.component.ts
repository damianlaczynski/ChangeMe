import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { PositionsService } from '@features/billing/services/positions.service';
import {
  BillingMessages,
  PositionConstraints
} from '@features/billing/utils/billing.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Textarea } from 'primeng/textarea';
import { ToggleSwitch } from 'primeng/toggleswitch';

@Component({
  selector: 'app-create-position',
  imports: [
    ReactiveFormsModule,
    BackButtonComponent,
    Card,
    Button,
    InputText,
    Textarea,
    Message,
    Panel,
    ToggleSwitch
  ],
  templateUrl: './create-position.component.html'
})
export class CreatePositionComponent {
  private readonly positionsService = inject(PositionsService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly positionConstraints = PositionConstraints;
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(PositionConstraints.NAME_MIN_LENGTH),
        Validators.maxLength(PositionConstraints.NAME_MAX_LENGTH)
      ]
    }),
    department: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(PositionConstraints.DEPARTMENT_MAX_LENGTH)]
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(PositionConstraints.DESCRIPTION_MAX_LENGTH)]
    }),
    isActive: new FormControl(true, { nonNullable: true })
  });

  shouldShowError(control: {
    invalid: boolean;
    dirty: boolean;
    touched: boolean;
  }): boolean {
    return control.invalid && (control.dirty || control.touched);
  }

  cancel(): void {
    void this.router.navigate(['/billing/positions']);
  }

  onSubmit(): void {
    this.submitError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const raw = this.form.getRawValue();

    this.positionsService
      .createPosition({
        name: raw.name.trim(),
        department: raw.department.trim() || null,
        description: raw.description.trim() || null,
        isActive: raw.isActive
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (position) => {
          this.toastService.success(BillingMessages.positionCreated);
          void this.router.navigate(['/billing/positions', position.id]);
        },
        error: (error: Error) => {
          this.submitError.set(
            error.message === BillingMessages.duplicateName
              ? BillingMessages.duplicateName
              : error.message
          );
          this.isSubmitting.set(false);
        },
        complete: () => this.isSubmitting.set(false)
      });
  }
}

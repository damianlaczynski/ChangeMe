import { Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
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
import { ProgressSpinner } from 'primeng/progressspinner';
import { Textarea } from 'primeng/textarea';
import { ToggleSwitch } from 'primeng/toggleswitch';

@Component({
  selector: 'app-edit-position',
  imports: [
    ReactiveFormsModule,
    BackButtonComponent,
    Card,
    Button,
    InputText,
    Textarea,
    Message,
    Panel,
    ToggleSwitch,
    ProgressSpinner
  ],
  templateUrl: './edit-position.component.html'
})
export class EditPositionComponent {
  readonly id = input.required<string>();

  private readonly positionsService = inject(PositionsService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly positionConstraints = PositionConstraints;
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly isLoading = signal(true);
  readonly loadError = signal<string | null>(null);

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

  constructor() {
    effect(() => {
      this.loadPosition(this.id());
    });
  }

  cancel(): void {
    void this.router.navigate(['/billing/positions', this.id()]);
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
      .updatePosition(this.id(), {
        name: raw.name.trim(),
        department: raw.department.trim() || null,
        description: raw.description.trim() || null,
        isActive: raw.isActive
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (position) => {
          this.toastService.success(BillingMessages.positionSaved);
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

  private loadPosition(id: string): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.positionsService
      .getPositionById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (position) => {
          this.form.reset({
            name: position.name,
            department: position.department ?? '',
            description: position.description ?? '',
            isActive: position.isActive
          });
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}

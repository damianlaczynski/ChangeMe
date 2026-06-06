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
import { ProjectsService } from '@features/projects/services/projects.service';
import {
  ProjectConstraints,
  ProjectMessages
} from '@features/projects/utils/projects.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Textarea } from 'primeng/textarea';

@Component({
  selector: 'app-create-project',
  imports: [
    ReactiveFormsModule,
    BackButtonComponent,
    Card,
    Button,
    InputText,
    Textarea,
    Message,
    Panel
  ],
  templateUrl: './create-project.component.html'
})
export class CreateProjectComponent {
  private readonly projectsService = inject(ProjectsService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly projectConstraints = ProjectConstraints;
  readonly ProjectMessages = ProjectMessages;
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(ProjectConstraints.NAME_MIN_LENGTH),
        Validators.maxLength(ProjectConstraints.NAME_MAX_LENGTH)
      ]
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(ProjectConstraints.DESCRIPTION_MAX_LENGTH)]
    })
  });

  shouldShowError(control: {
    invalid: boolean;
    dirty: boolean;
    touched: boolean;
  }): boolean {
    return control.invalid && (control.dirty || control.touched);
  }

  cancel(): void {
    void this.router.navigate(['/projects']);
  }

  onSubmit(): void {
    this.submitError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const raw = this.form.getRawValue();

    this.projectsService
      .createProject({
        name: raw.name.trim(),
        description: raw.description.trim() ? raw.description : null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (project) => {
          this.toastService.success(ProjectMessages.projectCreated);
          void this.router.navigate(['/projects', project.id]);
        },
        error: (error: Error) => {
          this.submitError.set(
            error.message === ProjectMessages.duplicateName
              ? ProjectMessages.duplicateName
              : error.message
          );
          this.isSubmitting.set(false);
        },
        complete: () => this.isSubmitting.set(false)
      });
  }
}

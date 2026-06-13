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
import {
  CreateProjectRequest,
  ProjectVisibility
} from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import {
  normalizeProjectKey,
  ProjectConstraints,
  ProjectMessages,
  projectVisibilities
} from '@features/projects/utils/projects.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Select } from 'primeng/select';
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
    Select,
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
  readonly projectVisibilities = projectVisibilities;
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
    key: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(ProjectConstraints.KEY_MIN_LENGTH),
        Validators.maxLength(ProjectConstraints.KEY_MAX_LENGTH)
      ]
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(ProjectConstraints.DESCRIPTION_MAX_LENGTH)]
    }),
    visibility: new FormControl(ProjectVisibility.INTERNAL, {
      nonNullable: true,
      validators: [Validators.required]
    }),
    color: new FormControl(ProjectConstraints.DEFAULT_COLOR, { nonNullable: true })
  });

  onKeyInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.form.controls.key.setValue(normalizeProjectKey(input.value), {
      emitEvent: false
    });
  }

  onSubmit(): void {
    this.submitError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request: CreateProjectRequest = {
      name: this.form.controls.name.value.trim(),
      key: normalizeProjectKey(this.form.controls.key.value),
      description: this.form.controls.description.value.trim() || null,
      visibility: this.form.controls.visibility.value,
      color: this.form.controls.color.value.trim() || null
    };

    this.isSubmitting.set(true);

    this.projectsService
      .createProject(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (project) => {
          this.isSubmitting.set(false);
          this.toastService.success(ProjectMessages.projectCreated, project.name);
          void this.router.navigate(['/projects', project.id]);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }
}

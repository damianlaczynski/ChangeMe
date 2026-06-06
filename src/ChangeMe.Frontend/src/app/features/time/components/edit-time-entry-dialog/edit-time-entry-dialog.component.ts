import {
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  output,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ToastService } from '@core/toast/services/toast.service';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  LogTimeIssueOptionDto,
  LoggableProjectOptionDto,
  TimeEntryDto,
  TimeEntryListItemDto
} from '@features/time/models/time.model';
import { TimeService } from '@features/time/services/time.service';
import {
  NO_ISSUE_OPTION_ID,
  TimeConstraints,
  TimeMessages,
  combineDurationMinutes,
  durationPresets,
  formatDescriptionCounter,
  formatDuration,
  openIssueStatuses,
  parseIsoDateString,
  splitDurationMinutes,
  toIsoDateString
} from '@features/time/utils/time.utils';
import { Button } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { Dialog } from 'primeng/dialog';
import { InputNumber } from 'primeng/inputnumber';
import { Message } from 'primeng/message';
import { Select } from 'primeng/select';
import { Textarea } from 'primeng/textarea';
import { finalize, merge } from 'rxjs';

export type EditableTimeEntry = TimeEntryDto | TimeEntryListItemDto;

type EditTimeForm = {
  projectId: FormControl<string | null>;
  issueId: FormControl<string | null>;
  workDate: FormControl<Date | null>;
  hours: FormControl<number>;
  minutes: FormControl<number>;
  description: FormControl<string>;
};

@Component({
  selector: 'app-edit-time-entry-dialog',
  imports: [
    ReactiveFormsModule,
    Dialog,
    Button,
    Select,
    DatePicker,
    InputNumber,
    Textarea,
    Message
  ],
  templateUrl: './edit-time-entry-dialog.component.html'
})
export class EditTimeEntryDialogComponent {
  readonly entry = input<EditableTimeEntry | null>(null);
  readonly visible = input(false);
  readonly visibleChange = output<boolean>();
  readonly saved = output<void>();

  private readonly timeService = inject(TimeService);
  private readonly issuesService = inject(IssuesService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly durationPresets = durationPresets;
  readonly TimeMessages = TimeMessages;
  readonly formatDescriptionCounter = formatDescriptionCounter;
  readonly formatDuration = formatDuration;

  readonly projects = signal<LoggableProjectOptionDto[]>([]);
  readonly issues = signal<LogTimeIssueOptionDto[]>([]);
  readonly isLoadingProjects = signal(false);
  readonly isLoadingIssues = signal(false);
  readonly isSaving = signal(false);
  readonly requestError = signal<string | null>(null);

  readonly issueOptions = computed(() => [
    { id: NO_ISSUE_OPTION_ID, title: 'No issue' },
    ...this.issues()
  ]);

  readonly descriptionLength = computed(
    () => this.form.controls.description.value.length
  );
  private readonly durationRevision = signal(0);
  readonly totalDurationMinutes = computed(() => {
    this.durationRevision();
    return combineDurationMinutes(
      this.form.controls.hours.value,
      this.form.controls.minutes.value
    );
  });
  readonly totalDurationLabel = computed(() =>
    formatDuration(this.totalDurationMinutes())
  );

  readonly form = new FormGroup<EditTimeForm>({
    projectId: new FormControl<string | null>(null, Validators.required),
    issueId: new FormControl<string | null>(NO_ISSUE_OPTION_ID),
    workDate: new FormControl<Date | null>(null, Validators.required),
    hours: new FormControl(0, { nonNullable: true }),
    minutes: new FormControl(0, { nonNullable: true }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(TimeConstraints.DESCRIPTION_MAX_LENGTH)]
    })
  });

  constructor() {
    effect(() => {
      if (!this.visible() || !this.entry()) {
        return;
      }

      this.populateForm(this.entry()!);
      this.loadProjects();
    });

    this.form.controls.projectId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((projectId) => {
        const currentEntry = this.entry();
        if (!currentEntry || projectId === currentEntry.projectId) {
          if (projectId) {
            this.loadIssues(projectId, this.form.controls.issueId.value);
          }
          return;
        }

        this.form.controls.issueId.setValue(NO_ISSUE_OPTION_ID);
        this.loadIssues(projectId);
      });

    merge(
      this.form.controls.hours.valueChanges,
      this.form.controls.minutes.valueChanges
    )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.durationRevision.update((value) => value + 1));
  }

  onHide(): void {
    this.visibleChange.emit(false);
  }

  applyPreset(minutes: number): void {
    const split = splitDurationMinutes(minutes);
    this.form.patchValue({
      hours: split.hours,
      minutes: split.minutes
    });
    this.form.controls.hours.markAsTouched();
    this.form.controls.minutes.markAsTouched();
    this.durationRevision.update((value) => value + 1);
  }

  save(): void {
    const currentEntry = this.entry();
    if (!currentEntry) {
      return;
    }

    this.form.markAllAsTouched();
    this.requestError.set(null);

    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const durationMinutes = combineDurationMinutes(value.hours, value.minutes);

    if (
      durationMinutes < TimeConstraints.MIN_DURATION_MINUTES ||
      durationMinutes > TimeConstraints.MAX_DURATION_MINUTES
    ) {
      return;
    }

    if (!value.projectId || !value.workDate) {
      return;
    }

    this.isSaving.set(true);

    this.timeService
      .updateTimeEntry(currentEntry.id, {
        projectId: value.projectId,
        issueId:
          value.issueId && value.issueId !== NO_ISSUE_OPTION_ID ? value.issueId : null,
        workDate: toIsoDateString(value.workDate),
        durationMinutes,
        description: value.description.trim() || null
      })
      .pipe(
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => {
          this.toastService.success(TimeMessages.timeEntrySaved);
          this.saved.emit();
          this.onHide();
        },
        error: (error: Error) => {
          this.requestError.set(error.message);
          this.toastService.showApiError(error, 'Could not save time entry');
        }
      });
  }

  private populateForm(entry: EditableTimeEntry): void {
    const split = splitDurationMinutes(entry.durationMinutes);
    const issueId =
      'issueId' in entry && entry.issueId ? entry.issueId : NO_ISSUE_OPTION_ID;

    this.form.reset({
      projectId: entry.projectId,
      issueId,
      workDate: parseIsoDateString(entry.workDate),
      hours: split.hours,
      minutes: split.minutes,
      description: entry.description ?? ''
    });
    this.durationRevision.update((value) => value + 1);

    this.requestError.set(null);
    this.loadIssues(entry.projectId, issueId);
  }

  private loadProjects(): void {
    this.isLoadingProjects.set(true);

    this.timeService
      .getLoggableProjects()
      .pipe(
        finalize(() => this.isLoadingProjects.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (projects) => {
          const entry = this.entry();
          if (!entry) {
            this.projects.set(projects);
            return;
          }

          const hasProject = projects.some((project) => project.id === entry.projectId);
          if (hasProject) {
            this.projects.set(projects);
            return;
          }

          this.projects.set([
            ...projects,
            {
              id: entry.projectId,
              name: entry.projectName
            }
          ]);
        },
        error: () => this.projects.set([])
      });
  }

  private loadIssues(projectId: string | null, selectedIssueId?: string | null): void {
    if (!projectId) {
      this.issues.set([]);
      return;
    }

    this.isLoadingIssues.set(true);

    this.issuesService
      .getAllIssues({
        pageNumber: 1,
        pageSize: 100,
        projectId,
        statuses: openIssueStatuses
      })
      .pipe(
        finalize(() => this.isLoadingIssues.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (result) => {
          const mapped = result.items.map((issue) => ({
            id: issue.id,
            title: issue.title
          }));
          const entry = this.entry();
          if (
            entry &&
            'issueId' in entry &&
            entry.issueId &&
            entry.issueTitle &&
            !mapped.some((issue) => issue.id === entry.issueId)
          ) {
            mapped.unshift({ id: entry.issueId, title: entry.issueTitle });
          }

          this.issues.set(mapped);

          if (selectedIssueId && selectedIssueId !== NO_ISSUE_OPTION_ID) {
            this.form.controls.issueId.setValue(selectedIssueId);
          }
        },
        error: () => this.issues.set([])
      });
  }
}

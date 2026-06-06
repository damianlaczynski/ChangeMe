import { Component, DestroyRef, computed, effect, inject, signal } from '@angular/core';
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
  LoggableProjectOptionDto
} from '@features/time/models/time.model';
import {
  LogTimeDialogContext,
  LogTimeDialogService
} from '@features/time/services/log-time-dialog.service';
import { RunningTimerService } from '@features/time/services/running-timer.service';
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

type LogTimeForm = {
  projectId: FormControl<string | null>;
  issueId: FormControl<string | null>;
  workDate: FormControl<Date | null>;
  hours: FormControl<number>;
  minutes: FormControl<number>;
  description: FormControl<string>;
};

@Component({
  selector: 'app-log-time-dialog',
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
  templateUrl: './log-time-dialog.component.html'
})
export class LogTimeDialogComponent {
  private readonly dialogService = inject(LogTimeDialogService);
  private readonly timeService = inject(TimeService);
  private readonly issuesService = inject(IssuesService);
  private readonly runningTimerService = inject(RunningTimerService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly dialogServiceRef = this.dialogService;
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

  readonly readonlyProject = computed(
    () => this.dialogService.context()?.readonlyProject ?? false
  );
  readonly readonlyIssue = computed(
    () => this.dialogService.context()?.readonlyIssue ?? false
  );
  readonly hidePresets = computed(
    () => this.dialogService.context()?.hidePresets ?? false
  );
  readonly showStartTimerLink = computed(
    () => !this.runningTimerService.hasRunningTimer()
  );
  readonly canSave = computed(() => this.projects().length > 0 && !this.isSaving());
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

  readonly form = new FormGroup<LogTimeForm>({
    projectId: new FormControl<string | null>(null, Validators.required),
    issueId: new FormControl<string | null>(NO_ISSUE_OPTION_ID),
    workDate: new FormControl<Date | null>(new Date(), Validators.required),
    hours: new FormControl(0, { nonNullable: true }),
    minutes: new FormControl(0, { nonNullable: true }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(TimeConstraints.DESCRIPTION_MAX_LENGTH)]
    })
  });

  private lastInitializedOpenGeneration = -1;
  private lastLoadedIssuesProjectId: string | null = null;
  private projectsRequestId = 0;
  private issuesRequestId = 0;

  constructor() {
    effect(() => {
      const visible = this.dialogService.visible();
      const openGeneration = this.dialogService.openGeneration();

      if (!visible) {
        this.lastInitializedOpenGeneration = -1;
        this.lastLoadedIssuesProjectId = null;
        return;
      }

      if (openGeneration === this.lastInitializedOpenGeneration) {
        return;
      }

      this.lastInitializedOpenGeneration = openGeneration;
      this.initializeDialog();
    });

    this.form.controls.projectId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((projectId) => {
        if (this.readonlyProject()) {
          return;
        }

        if (!projectId || projectId === this.lastLoadedIssuesProjectId) {
          return;
        }

        this.lastLoadedIssuesProjectId = projectId;
        this.form.controls.issueId.setValue(NO_ISSUE_OPTION_ID, { emitEvent: false });
        this.loadIssues(projectId);
      });

    merge(
      this.form.controls.hours.valueChanges,
      this.form.controls.minutes.valueChanges
    )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.durationRevision.update((value) => value + 1));
  }

  onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.onHide();
    }
  }

  onHide(): void {
    this.dialogService.close();
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

  startTimerInstead(): void {
    const projectId = this.form.controls.projectId.value;
    const issueId = this.form.controls.issueId.value;
    const contextIssueId = issueId && issueId !== NO_ISSUE_OPTION_ID ? issueId : null;

    this.dialogService.close();
    this.runningTimerService.startTimer({
      projectId,
      issueId: contextIssueId
    });
  }

  save(): void {
    this.form.markAllAsTouched();
    this.requestError.set(null);

    if (this.form.invalid || !this.canSave()) {
      return;
    }

    const value = this.form.getRawValue();
    const durationMinutes = combineDurationMinutes(value.hours, value.minutes);

    if (
      durationMinutes < TimeConstraints.MIN_DURATION_MINUTES ||
      durationMinutes > TimeConstraints.MAX_DURATION_MINUTES
    ) {
      this.form.controls.hours.markAsTouched();
      this.form.controls.minutes.markAsTouched();
      return;
    }

    if (!value.projectId || !value.workDate) {
      return;
    }

    this.isSaving.set(true);

    this.timeService
      .createTimeEntry({
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
          this.toastService.success(TimeMessages.timeLogged);
          this.dialogService.notifySaved();
          this.dialogService.close();
        },
        error: (error: Error) => {
          this.requestError.set(error.message);
          this.toastService.showApiError(error, 'Could not log time');
        }
      });
  }

  private initializeDialog(): void {
    const context = this.dialogService.context();
    const prefilledMinutes = context?.prefilledDurationMinutes;
    const split = splitDurationMinutes(prefilledMinutes ?? 0);
    const projectId = context?.projectId ?? null;
    const issueId = context?.issueId ?? NO_ISSUE_OPTION_ID;

    this.form.reset({
      projectId,
      issueId,
      workDate: new Date(),
      hours: split.hours,
      minutes: split.minutes,
      description: ''
    });
    this.durationRevision.update((value) => value + 1);

    this.requestError.set(null);
    this.issues.set([]);
    this.lastLoadedIssuesProjectId = projectId;

    this.loadProjects(context);

    if (projectId) {
      this.loadIssues(projectId, issueId);
    }
  }

  private loadProjects(context: LogTimeDialogContext | null): void {
    const requestId = ++this.projectsRequestId;
    this.isLoadingProjects.set(true);

    this.timeService
      .getLoggableProjects()
      .pipe(
        finalize(() => {
          if (requestId === this.projectsRequestId) {
            this.isLoadingProjects.set(false);
          }
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (projects) => {
          if (requestId !== this.projectsRequestId) {
            return;
          }

          if (
            context?.projectId &&
            context.projectName &&
            !projects.some((project) => project.id === context.projectId)
          ) {
            this.projects.set([
              ...projects,
              { id: context.projectId, name: context.projectName }
            ]);
            return;
          }

          this.projects.set(projects);
        },
        error: () => {
          if (requestId !== this.projectsRequestId) {
            return;
          }

          if (context?.projectId && context.projectName) {
            this.projects.set([{ id: context.projectId, name: context.projectName }]);
            return;
          }

          this.projects.set([]);
        }
      });
  }

  private loadIssues(projectId: string | null, selectedIssueId?: string | null): void {
    if (!projectId) {
      this.issues.set([]);
      return;
    }

    const requestId = ++this.issuesRequestId;
    this.isLoadingIssues.set(true);

    this.issuesService
      .getAllIssues({
        pageNumber: 1,
        pageSize: 100,
        projectId,
        statuses: openIssueStatuses
      })
      .pipe(
        finalize(() => {
          if (requestId === this.issuesRequestId) {
            this.isLoadingIssues.set(false);
          }
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (result) => {
          if (requestId !== this.issuesRequestId) {
            return;
          }

          const context = this.dialogService.context();
          const mapped = result.items.map((issue) => ({
            id: issue.id,
            title: issue.title
          }));

          if (
            context?.issueId &&
            context.issueTitle &&
            !mapped.some((issue) => issue.id === context.issueId)
          ) {
            mapped.unshift({ id: context.issueId, title: context.issueTitle });
          }

          this.issues.set(mapped);

          if (selectedIssueId && selectedIssueId !== NO_ISSUE_OPTION_ID) {
            this.form.controls.issueId.setValue(selectedIssueId, { emitEvent: false });
          }
        },
        error: () => {
          if (requestId !== this.issuesRequestId) {
            return;
          }

          this.issues.set([]);
        }
      });
  }
}

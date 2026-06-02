import { DatePipe } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToastService } from '@core/toast/services/toast.service';
import { IssueAttachmentDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  getDeleteAttachmentConfirmMessage,
  issueAttachmentAccept,
  IssueAttachmentConstraints
} from '@features/issues/utils/issue.utils';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { formatFileSize } from '@shared/data/utils/file-size.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { FileUpload } from 'primeng/fileupload';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';

@Component({
  selector: 'app-issue-attachments-tab',
  imports: [DatePipe, Button, FileUpload, Message, ProgressSpinner],
  templateUrl: './issue-attachments-tab.component.html',
  host: { class: 'block' }
})
export class IssueAttachmentsTabComponent {
  readonly issueId = input.required<string>();

  private readonly issuesService = inject(IssuesService);
  private readonly toastService = inject(ToastService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly issueAttachmentConstraints = IssueAttachmentConstraints;
  readonly issueAttachmentAccept = issueAttachmentAccept;
  readonly formatFileSize = formatFileSize;

  readonly attachments = signal<IssueAttachmentDto[]>([]);
  readonly attachmentsPagination = signal<PaginationResult<IssueAttachmentDto> | null>(
    null
  );
  readonly attachmentsQuery = signal({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'CreatedAt',
    ascending: false
  });
  readonly loadError = signal<string | null>(null);
  readonly uploadError = signal<string | null>(null);
  readonly selectedFile = signal<File | null>(null);
  readonly isUploading = signal(false);
  readonly isLoadingAttachments = signal(false);
  readonly isLoadingMoreAttachments = signal(false);
  readonly hasLoadedAttachments = signal(false);
  readonly downloadingAttachmentId = signal<string | null>(null);
  readonly deletingAttachmentId = signal<string | null>(null);

  readonly canShowMoreAttachments = computed(
    () => this.attachmentsPagination()?.hasNext ?? false
  );

  private lastLoadedIssueId: string | null = null;
  private attachmentsRequestId = 0;

  constructor() {
    effect(() => {
      const issueId = this.issueId();
      if (issueId === this.lastLoadedIssueId) {
        return;
      }

      this.lastLoadedIssueId = issueId;
      this.uploadError.set(null);
      this.selectedFile.set(null);
      this.reloadAttachmentsFromStart(issueId);
    });
  }

  onFileSelected(event: { files?: File[]; currentFiles?: File[] }): void {
    this.uploadError.set(null);
    const files = event.currentFiles ?? event.files ?? [];
    this.selectedFile.set(files[0] ?? null);
  }

  uploadAttachment(): void {
    const file = this.selectedFile();
    if (!file || this.isUploading()) {
      return;
    }

    if (file.size > IssueAttachmentConstraints.MAX_FILE_SIZE_BYTES) {
      this.uploadError.set(
        `File cannot exceed ${formatFileSize(IssueAttachmentConstraints.MAX_FILE_SIZE_BYTES)}.`
      );
      return;
    }

    this.isUploading.set(true);
    this.uploadError.set(null);

    this.issuesService
      .uploadIssueAttachment(this.issueId(), file)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.reloadAttachmentsFromStart(this.issueId());
          this.selectedFile.set(null);
          this.isUploading.set(false);
          this.toastService.success('Attachment uploaded');
        },
        error: (error: Error) => {
          this.uploadError.set(error.message);
          this.isUploading.set(false);
        }
      });
  }

  downloadAttachment(attachment: IssueAttachmentDto): void {
    if (this.downloadingAttachmentId()) {
      return;
    }

    this.downloadingAttachmentId.set(attachment.id);

    this.issuesService
      .downloadIssueAttachment(this.issueId(), attachment.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = attachment.originalFileName;
          document.body.appendChild(anchor);
          anchor.click();
          anchor.remove();
          window.setTimeout(() => URL.revokeObjectURL(url), 0);
          this.downloadingAttachmentId.set(null);
        },
        error: (error: Error) => {
          this.toastService.showApiError(error, 'Could not download attachment');
          this.downloadingAttachmentId.set(null);
        }
      });
  }

  confirmDeleteAttachment(attachment: IssueAttachmentDto): void {
    if (!attachment.canDelete || this.deletingAttachmentId()) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Delete attachment',
      message: getDeleteAttachmentConfirmMessage(attachment.originalFileName),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.deleteAttachment(attachment.id)
    });
  }

  showMoreAttachments(): void {
    const pagination = this.attachmentsPagination();
    if (!pagination?.hasNext || this.isLoadingMoreAttachments()) {
      return;
    }

    this.loadAttachments(this.issueId(), {
      append: true,
      pageNumber: pagination.currentPage + 1
    });
  }

  private deleteAttachment(attachmentId: string): void {
    this.deletingAttachmentId.set(attachmentId);

    this.issuesService
      .deleteIssueAttachment(this.issueId(), attachmentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.reloadAttachmentsFromStart(this.issueId());
          this.deletingAttachmentId.set(null);
          this.toastService.success('Attachment deleted');
        },
        error: (error: Error) => {
          this.toastService.showApiError(error, 'Could not delete attachment');
          this.deletingAttachmentId.set(null);
        }
      });
  }

  private reloadAttachmentsFromStart(issueId: string): void {
    this.attachmentsQuery.set({
      pageNumber: 1,
      pageSize: 10,
      sortField: 'CreatedAt',
      ascending: false
    });
    this.loadAttachments(issueId);
  }

  private loadAttachments(
    issueId: string,
    options: { append?: boolean; pageNumber?: number } = {}
  ): void {
    const append = options.append ?? false;
    const pageNumber = options.pageNumber ?? this.attachmentsQuery().pageNumber;
    const query = { ...this.attachmentsQuery(), pageNumber };
    const requestId = ++this.attachmentsRequestId;

    if (append) {
      this.isLoadingMoreAttachments.set(true);
    } else {
      this.isLoadingAttachments.set(true);
    }

    this.loadError.set(null);

    this.issuesService
      .getIssueAttachments(issueId, query)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          if (requestId !== this.attachmentsRequestId) {
            return;
          }

          if (append) {
            this.attachments.update((items) => [...items, ...result.items]);
          } else {
            this.attachments.set(result.items);
          }

          this.attachmentsQuery.set({
            pageNumber: result.currentPage,
            pageSize: result.pageSize,
            sortField: 'CreatedAt',
            ascending: false
          });
          this.attachmentsPagination.set(result);
          this.isLoadingAttachments.set(false);
          this.isLoadingMoreAttachments.set(false);
          this.hasLoadedAttachments.set(true);
        },
        error: (error: Error) => {
          if (requestId !== this.attachmentsRequestId) {
            return;
          }

          this.loadError.set(error.message);
          this.isLoadingAttachments.set(false);
          this.isLoadingMoreAttachments.set(false);
        }
      });
  }
}

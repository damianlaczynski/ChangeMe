import { Component, DestroyRef, inject, OnDestroy } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterOutlet } from '@angular/router';
import { ProjectWorkspaceService } from '@features/projects/services/project-workspace.service';
import { ProjectsService } from '@features/projects/services/projects.service';
import { Message } from 'primeng/message';
import { catchError, distinctUntilChanged, map, of, switchMap } from 'rxjs';

@Component({
  selector: 'app-project-shell',
  imports: [RouterOutlet, Message],
  templateUrl: './project-shell.component.html'
})
export class ProjectShellComponent implements OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly projectsService = inject(ProjectsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly workspace = inject(ProjectWorkspaceService);

  constructor() {
    this.route.paramMap
      .pipe(
        map((params) => params.get('projectId') ?? ''),
        distinctUntilChanged(),
        switchMap((projectId) => {
          if (!projectId) {
            return of(null);
          }

          this.workspace.beginLoad(projectId);
          return this.projectsService.getProjectOverview(projectId).pipe(
            catchError((error: Error) => {
              this.workspace.setLoadError(error.message);
              return of(null);
            })
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((overview) => {
        if (overview) {
          this.workspace.setOverview(overview);
        }
      });
  }

  ngOnDestroy(): void {
    this.workspace.clear();
  }
}

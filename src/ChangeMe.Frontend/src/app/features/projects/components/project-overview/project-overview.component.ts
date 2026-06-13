import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProjectOverviewDto } from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import {
  formatProjectDescription,
  getProjectStatusLabel,
  getProjectStatusSeverity,
  getProjectVisibilityLabel
} from '@features/projects/utils/projects.utils';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Tag } from 'primeng/tag';
import { map, switchMap } from 'rxjs';

@Component({
  selector: 'app-project-overview',
  imports: [RouterLink, Card, Button, Message, Tag],
  templateUrl: './project-overview.component.html'
})
export class ProjectOverviewComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly projectsService = inject(ProjectsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly formatProjectDescription = formatProjectDescription;
  readonly getProjectStatusLabel = getProjectStatusLabel;
  readonly getProjectStatusSeverity = getProjectStatusSeverity;
  readonly getProjectVisibilityLabel = getProjectVisibilityLabel;

  readonly overview = signal<ProjectOverviewDto | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(true);

  constructor() {
    const parentRoute = this.route.parent;
    const projectRoute = parentRoute ?? this.route;

    projectRoute.paramMap
      .pipe(
        map((params) => params.get('projectId') ?? ''),
        switchMap((projectId) => this.projectsService.getProjectOverview(projectId)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (overview) => {
          this.overview.set(overview);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}

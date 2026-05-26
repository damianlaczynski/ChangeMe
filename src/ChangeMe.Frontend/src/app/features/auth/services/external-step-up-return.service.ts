import { DestroyRef, effect, inject, Injectable, Signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { needsExternalReauth } from '@features/auth/utils/external-step-up.utils';
import { Observable } from 'rxjs';

export interface ResumeWhenExternalReauthFreshOptions {
  isResumePending: () => boolean;
  clearResumePending: () => void;
  account: Signal<MyAccountDto>;
  onReady: () => void;
}

@Injectable({
  providedIn: 'root'
})
export class ExternalStepUpReturnService {
  private readonly router = inject(Router);

  watchQueryParamReturn(
    destroyRef: DestroyRef,
    route: ActivatedRoute,
    handler: () => void
  ): void {
    let handled = false;

    route.queryParamMap.pipe(takeUntilDestroyed(destroyRef)).subscribe((params) => {
      if (params.get('externalStepUp') !== '1' || handled) {
        return;
      }

      handled = true;
      void this.clearExternalStepUpQueryParam(route);
      handler();
    });
  }

  resumeWhenExternalReauthFresh(
    destroyRef: DestroyRef,
    options: ResumeWhenExternalReauthFreshOptions
  ): void {
    const ref = effect(() => {
      if (!options.isResumePending()) {
        return;
      }

      if (needsExternalReauth(options.account())) {
        return;
      }

      options.clearResumePending();
      options.onReady();
    });

    destroyRef.onDestroy(() => ref.destroy());
  }

  watchReturnAndRefreshAccount(
    destroyRef: DestroyRef,
    route: ActivatedRoute,
    refreshAccount: () => Observable<MyAccountDto>,
    onRefreshed: (account: MyAccountDto) => void
  ): void {
    this.watchQueryParamReturn(destroyRef, route, () => {
      refreshAccount()
        .pipe(takeUntilDestroyed(destroyRef))
        .subscribe({ next: onRefreshed });
    });
  }

  private clearExternalStepUpQueryParam(route: ActivatedRoute): Promise<boolean> {
    return this.router.navigate([], {
      relativeTo: route,
      queryParams: { externalStepUp: null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }
}

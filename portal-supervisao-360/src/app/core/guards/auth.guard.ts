import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route: ActivatedRouteSnapshot, _state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    return router.createUrlTree(['/login']);
  }

  const slug = route.data?.['slug'] as string | undefined;
  if (!slug) {
    return true;
  }

  if (!authService.hasRoutePermission(slug)) {
    return router.createUrlTree(['/unauthorized']);
  }

  return true;
};

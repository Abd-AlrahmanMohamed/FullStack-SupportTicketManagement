import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { UserRole } from '../models/user.model';

/**
 * UI-level convenience only - the backend enforces authorization independently of
 * this guard (see [Authorize(Roles=...)] on the API controllers), so a user can
 * never bypass real access control just by getting past this route check.
 */
export const roleGuard = (allowedRoles: UserRole[]): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.hasRole(...allowedRoles)) {
      return true;
    }

    return router.createUrlTree(['/']);
  };
};

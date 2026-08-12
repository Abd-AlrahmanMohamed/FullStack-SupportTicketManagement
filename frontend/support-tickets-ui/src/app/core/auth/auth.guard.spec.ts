import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('authGuard', () => {
  let authServiceStub: Partial<AuthService>;
  let isAuthenticated = false;

  beforeEach(() => {
    isAuthenticated = false;
    authServiceStub = {
      isAuthenticated: () => isAuthenticated
    };

    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [{ provide: AuthService, useValue: authServiceStub }]
    });
  });

  function runGuard() {
    return TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));
  }

  it('allows navigation when the user is authenticated', () => {
    isAuthenticated = true;

    const result = runGuard();

    expect(result).toBeTrue();
  });

  it('redirects to /login when the user is not authenticated', () => {
    isAuthenticated = false;

    const result = runGuard() as UrlTree;

    const router = TestBed.inject(Router);
    expect(router.serializeUrl(result)).toBe('/login');
  });
});

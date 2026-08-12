import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';
import { LoginResponse } from '../models/user.model';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const fakeResponse: LoginResponse = {
    token: 'fake-jwt-token',
    user: {
      id: 'user-1',
      fullName: 'Alice Admin',
      email: 'admin@support.local',
      role: 'Admin',
      isActive: true,
      createdAt: new Date().toISOString()
    }
  };

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should start unauthenticated with no stored user', () => {
    expect(service.isAuthenticated()).toBeFalse();
    expect(service.currentUser()).toBeNull();
  });

  it('should store the token and user, and update currentUser, on successful login', () => {
    service.login({ email: 'admin@support.local', password: 'Admin@123' }).subscribe((response) => {
      expect(response.token).toBe('fake-jwt-token');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(fakeResponse);

    expect(service.isAuthenticated()).toBeTrue();
    expect(service.getToken()).toBe('fake-jwt-token');
    expect(service.currentUser()?.fullName).toBe('Alice Admin');
  });

  it('should clear the token and user on logout', () => {
    service.login({ email: 'admin@support.local', password: 'Admin@123' }).subscribe();
    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush(fakeResponse);

    service.logout();

    expect(service.isAuthenticated()).toBeFalse();
    expect(service.currentUser()).toBeNull();
    expect(service.getToken()).toBeNull();
  });

  it('hasRole should reflect the current user role', () => {
    service.login({ email: 'admin@support.local', password: 'Admin@123' }).subscribe();
    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush(fakeResponse);

    expect(service.hasRole('Admin')).toBeTrue();
    expect(service.hasRole('Customer', 'SupportAgent')).toBeFalse();
  });
});

export type UserRole = 'Admin' | 'SupportAgent' | 'Customer';

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
  role: UserRole;
}

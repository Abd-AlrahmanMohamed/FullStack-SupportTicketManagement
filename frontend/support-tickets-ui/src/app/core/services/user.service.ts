import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateUserRequest, User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.baseUrl);
  }

  getSupportAgents(): Observable<User[]> {
    return this.http.get<User[]>(`${this.baseUrl}/support-agents`);
  }

  createUser(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>(this.baseUrl, request);
  }
}

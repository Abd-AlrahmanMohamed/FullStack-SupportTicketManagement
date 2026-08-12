import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Activity,
  AddTimeEntryRequest,
  Comment,
  CreateTicketRequest,
  PaginatedResult,
  Ticket,
  TicketDetails,
  TicketQueryParams,
  TimeEntry
} from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly baseUrl = `${environment.apiUrl}/tickets`;

  constructor(private http: HttpClient) {}

  getTickets(query: TicketQueryParams): Observable<PaginatedResult<Ticket>> {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 10);

    if (query.status) params = params.set('status', query.status);
    if (query.priority) params = params.set('priority', query.priority);
    if (query.assignedAgentId) params = params.set('assignedAgentId', query.assignedAgentId);
    if (query.search) params = params.set('search', query.search);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    return this.http.get<PaginatedResult<Ticket>>(this.baseUrl, { params });
  }

  getTicketById(id: string): Observable<TicketDetails> {
    return this.http.get<TicketDetails>(`${this.baseUrl}/${id}`);
  }

  createTicket(request: CreateTicketRequest): Observable<Ticket> {
    return this.http.post<Ticket>(this.baseUrl, request);
  }

  assignTicket(id: string, agentId: string): Observable<Ticket> {
    return this.http.patch<Ticket>(`${this.baseUrl}/${id}/assign`, { agentId });
  }

  updateStatus(id: string, status: string): Observable<Ticket> {
    return this.http.patch<Ticket>(`${this.baseUrl}/${id}/status`, { status });
  }

  updatePriority(id: string, priority: string): Observable<Ticket> {
    return this.http.patch<Ticket>(`${this.baseUrl}/${id}/priority`, { priority });
  }

  addComment(id: string, message: string): Observable<Comment> {
    return this.http.post<Comment>(`${this.baseUrl}/${id}/comments`, { message });
  }

  addTimeEntry(id: string, request: AddTimeEntryRequest): Observable<TimeEntry> {
    return this.http.post<TimeEntry>(`${this.baseUrl}/${id}/time-entries`, request);
  }

  getTimeEntries(id: string): Observable<TimeEntry[]> {
    return this.http.get<TimeEntry[]>(`${this.baseUrl}/${id}/time-entries`);
  }

  getTimeline(id: string): Observable<Activity[]> {
    return this.http.get<Activity[]>(`${this.baseUrl}/${id}/timeline`);
  }

  closeTicket(id: string): Observable<Ticket> {
    return this.http.post<Ticket>(`${this.baseUrl}/${id}/close`, {});
  }
}

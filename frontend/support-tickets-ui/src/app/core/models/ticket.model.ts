export type TicketStatus = 'Open' | 'InProgress' | 'Resolved' | 'Closed';
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export interface Ticket {
  id: string;
  ticketNumber: string;
  title: string;
  status: TicketStatus;
  priority: TicketPriority;
  customerId: string;
  customerName: string;
  assignedAgentId: string | null;
  assignedAgentName: string | null;
  createdAt: string;
}

export interface Comment {
  id: string;
  ticketId: string;
  userId: string;
  userName: string;
  userRole: string;
  message: string;
  createdAt: string;
}

export interface Activity {
  id: string;
  ticketId: string;
  userId: string;
  userName: string;
  action: string;
  oldValue: string | null;
  newValue: string | null;
  createdAt: string;
}

export interface TimeEntry {
  id: string;
  ticketId: string;
  agentId: string;
  agentName: string;
  workDate: string;
  durationMinutes: number;
  description: string | null;
  createdAt: string;
}

export interface TicketDetails {
  id: string;
  ticketNumber: string;
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  customerId: string;
  customerName: string;
  customerEmail: string;
  assignedAgentId: string | null;
  assignedAgentName: string | null;
  createdAt: string;
  updatedAt: string | null;
  resolvedAt: string | null;
  closedAt: string | null;
  totalTimeMinutes: number;
  comments: Comment[];
  timeline: Activity[];
  timeEntries: TimeEntry[];
}

export interface PaginatedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface TicketQueryParams {
  page?: number;
  pageSize?: number;
  status?: TicketStatus | '';
  priority?: TicketPriority | '';
  assignedAgentId?: string;
  search?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: TicketPriority;
}

export interface AddTimeEntryRequest {
  workDate: string;
  durationMinutes: number;
  description?: string;
}

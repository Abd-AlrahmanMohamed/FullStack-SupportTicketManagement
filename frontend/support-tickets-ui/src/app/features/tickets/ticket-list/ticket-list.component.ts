import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TicketService } from '../../../core/services/ticket.service';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Ticket, TicketQueryParams } from '../../../core/models/ticket.model';
import { User } from '../../../core/models/user.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingStateComponent } from '../../../shared/components/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PriorityBadgeComponent } from '../../../shared/components/priority-badge/priority-badge.component';

const SORTABLE_COLUMNS: Record<string, string> = {
  ticketNumber: 'ticketNumber',
  status: 'status',
  priority: 'priority',
  createdAt: 'createdAt'
};

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
    PriorityBadgeComponent
  ],
  templateUrl: './ticket-list.component.html',
  styleUrl: './ticket-list.component.scss'
})
export class TicketListComponent implements OnInit {
  displayedColumns = [
    'ticketNumber',
    'title',
    'status',
    'priority',
    'customerName',
    'assignedAgentName',
    'createdAt'
  ];

  tickets = signal<Ticket[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  agents = signal<User[]>([]);

  pageSize = 10;
  pageIndex = 0;
  sortBy = 'createdAt';
  sortDirection: 'asc' | 'desc' = 'desc';

  filterForm = this.fb.group({
    search: [''],
    status: [''],
    priority: [''],
    assignedAgentId: ['']
  });

  constructor(
    private ticketService: TicketService,
    private userService: UserService,
    public authService: AuthService,
    private router: Router,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.load();

    if (this.authService.hasRole('Admin')) {
      this.userService.getSupportAgents().subscribe((agents) => this.agents.set(agents));
    }

    this.filterForm.valueChanges.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.pageIndex = 0;
      this.load();
    });
  }

  get hasActiveFilters(): boolean {
    const { search, status, priority, assignedAgentId } = this.filterForm.getRawValue();
    return !!(search || status || priority || assignedAgentId);
  }

  clearFilters(): void {
    this.filterForm.reset({ search: '', status: '', priority: '', assignedAgentId: '' });
  }

  load(): void {
    this.loading.set(true);

    const { search, status, priority, assignedAgentId } = this.filterForm.getRawValue();

    const query: TicketQueryParams = {
      page: this.pageIndex + 1,
      pageSize: this.pageSize,
      search: search || undefined,
      status: (status as any) || '',
      priority: (priority as any) || '',
      assignedAgentId: assignedAgentId || undefined,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
    };

    this.ticketService.getTickets(query).subscribe({
      next: (result) => {
        this.tickets.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  onSortChange(sort: Sort): void {
    if (!sort.direction) {
      this.sortBy = 'createdAt';
      this.sortDirection = 'desc';
    } else {
      this.sortBy = SORTABLE_COLUMNS[sort.active] ?? 'createdAt';
      this.sortDirection = sort.direction;
    }
    this.pageIndex = 0;
    this.load();
  }

  openTicket(ticket: Ticket): void {
    this.router.navigate(['/tickets', ticket.id]);
  }
}

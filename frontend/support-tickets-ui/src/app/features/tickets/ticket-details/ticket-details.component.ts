import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TicketService } from '../../../core/services/ticket.service';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/auth/auth.service';
import { TicketDetails, TicketStatus } from '../../../core/models/ticket.model';
import { User } from '../../../core/models/user.model';
import { LoadingStateComponent } from '../../../shared/components/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PriorityBadgeComponent } from '../../../shared/components/priority-badge/priority-badge.component';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { TimeAgoPipe } from '../../../shared/pipes/time-ago.pipe';
import { DurationMinutesPipe } from '../../../shared/pipes/duration-minutes.pipe';

const ACTIVITY_META: Record<string, { icon: string; label: string }> = {
  TicketCreated: { icon: 'add_circle_outline', label: 'Ticket created' },
  TicketAssigned: { icon: 'person_add_alt', label: 'Agent assigned' },
  StatusChanged: { icon: 'autorenew', label: 'Status changed' },
  PriorityChanged: { icon: 'flag', label: 'Priority changed' },
  CommentAdded: { icon: 'chat_bubble_outline', label: 'Comment added' },
  TimeLogged: { icon: 'schedule', label: 'Time logged' },
  TicketClosed: { icon: 'check_circle_outline', label: 'Ticket closed' }
};

@Component({
  selector: 'app-ticket-details',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDividerModule,
    MatTableModule,
    MatSnackBarModule,
    LoadingStateComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
    PriorityBadgeComponent,
    AvatarComponent,
    TimeAgoPipe,
    DurationMinutesPipe
  ],
  templateUrl: './ticket-details.component.html',
  styleUrl: './ticket-details.component.scss'
})
export class TicketDetailsComponent implements OnInit {
  ticket = signal<TicketDetails | null>(null);
  loading = signal(true);
  notFound = signal(false);
  agents = signal<User[]>([]);
  timeEntryColumns = ['workDate', 'durationMinutes', 'agentName', 'description'];

  commentForm = this.fb.group({
    message: ['', [Validators.required, Validators.maxLength(2000)]]
  });

  timeEntryForm = this.fb.group({
    workDate: ['', [Validators.required]],
    durationMinutes: [30, [Validators.required, Validators.min(1)]],
    description: ['']
  });

  selectedAgentId = signal<string | null>(null);
  selectedPriority = signal<string | null>(null);

  private ticketId!: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private ticketService: TicketService,
    private userService: UserService,
    public authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.ticketId = this.route.snapshot.paramMap.get('id')!;
    this.load();

    if (this.authService.hasRole('Admin')) {
      this.userService.getSupportAgents().subscribe((agents) => this.agents.set(agents));
    }
  }

  load(): void {
    this.loading.set(true);
    this.ticketService.getTicketById(this.ticketId).subscribe({
      next: (ticket) => {
        this.ticket.set(ticket);
        this.selectedAgentId.set(ticket.assignedAgentId);
        this.selectedPriority.set(ticket.priority);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      }
    });
  }

  get isCustomer() {
    return this.authService.hasRole('Customer');
  }

  get isAgent() {
    return this.authService.hasRole('SupportAgent');
  }

  get isAdmin() {
    return this.authService.hasRole('Admin');
  }

  get hasActions(): boolean {
    const ticket = this.ticket();
    if (!ticket) return false;

    return (
      this.isAdmin ||
      this.isAgent ||
      (this.isCustomer && ticket.status === 'Resolved')
    );
  }

  activityIcon(action: string): string {
    return ACTIVITY_META[action]?.icon ?? 'circle';
  }

  activityLabel(action: string): string {
    return ACTIVITY_META[action]?.label ?? action;
  }

  canTransitionTo(status: TicketStatus): boolean {
    const current = this.ticket()?.status;
    if (!current) return false;

    const transitions: Record<TicketStatus, TicketStatus[]> = {
      Open: ['InProgress'],
      InProgress: ['Resolved', 'Open'],
      Resolved: ['InProgress'],
      Closed: []
    };

    return transitions[current].includes(status);
  }

  changeStatus(status: TicketStatus): void {
    this.ticketService.updateStatus(this.ticketId, status).subscribe({
      next: () => {
        this.snackBar.open(`Status updated to ${status}.`, 'Dismiss', { duration: 3000 });
        this.load();
      },
      error: (err) => this.showError(err)
    });
  }

  savePriority(): void {
    const priority = this.selectedPriority();
    if (!priority) return;

    this.ticketService.updatePriority(this.ticketId, priority).subscribe({
      next: () => {
        this.snackBar.open('Priority updated.', 'Dismiss', { duration: 3000 });
        this.load();
      },
      error: (err) => this.showError(err)
    });
  }

  assignAgent(): void {
    const agentId = this.selectedAgentId();
    if (!agentId) return;

    this.ticketService.assignTicket(this.ticketId, agentId).subscribe({
      next: () => {
        this.snackBar.open('Ticket assigned.', 'Dismiss', { duration: 3000 });
        this.load();
      },
      error: (err) => this.showError(err)
    });
  }

  addComment(): void {
    if (this.commentForm.invalid) {
      this.commentForm.markAllAsTouched();
      return;
    }

    const message = this.commentForm.getRawValue().message!;
    this.ticketService.addComment(this.ticketId, message).subscribe({
      next: () => {
        this.commentForm.reset();
        this.load();
      },
      error: (err) => this.showError(err)
    });
  }

  logTime(): void {
    if (this.timeEntryForm.invalid) {
      this.timeEntryForm.markAllAsTouched();
      return;
    }

    const { workDate, durationMinutes, description } = this.timeEntryForm.getRawValue();

    this.ticketService
      .addTimeEntry(this.ticketId, {
        workDate: new Date(workDate!).toISOString(),
        durationMinutes: durationMinutes!,
        description: description || undefined
      })
      .subscribe({
        next: () => {
          this.timeEntryForm.reset({ durationMinutes: 30 });
          this.snackBar.open('Time logged.', 'Dismiss', { duration: 3000 });
          this.load();
        },
        error: (err) => this.showError(err)
      });
  }

  closeTicket(): void {
    this.ticketService.closeTicket(this.ticketId).subscribe({
      next: () => {
        this.snackBar.open('Ticket closed.', 'Dismiss', { duration: 3000 });
        this.load();
      },
      error: (err) => this.showError(err)
    });
  }

  goBack(): void {
    this.router.navigate(['/tickets']);
  }

  private showError(err: any): void {
    const message = err?.error?.message ?? 'Something went wrong. Please try again.';
    this.snackBar.open(message, 'Dismiss', { duration: 4000 });
  }
}

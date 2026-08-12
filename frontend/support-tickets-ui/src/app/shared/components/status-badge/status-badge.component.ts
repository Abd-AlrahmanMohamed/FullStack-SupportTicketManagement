import { Component, Input } from '@angular/core';
import { TicketStatus } from '../../../core/models/ticket.model';

const STATUS_LABEL: Record<TicketStatus, string> = {
  Open: 'Open',
  InProgress: 'In Progress',
  Resolved: 'Resolved',
  Closed: 'Closed'
};

const STATUS_TONE: Record<TicketStatus, string> = {
  Open: 'tone-info',
  InProgress: 'tone-warning',
  Resolved: 'tone-success',
  Closed: 'tone-neutral'
};

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `<span class="badge {{ tone }}">{{ label }}</span>`,
  styleUrl: './status-badge.component.scss'
})
export class StatusBadgeComponent {
  @Input({ required: true }) status!: TicketStatus;

  get label(): string {
    return STATUS_LABEL[this.status];
  }

  get tone(): string {
    return STATUS_TONE[this.status];
  }
}

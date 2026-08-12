import { Component, Input } from '@angular/core';
import { TicketPriority } from '../../../core/models/ticket.model';

const PRIORITY_TONE: Record<TicketPriority, string> = {
  Low: 'tone-neutral',
  Medium: 'tone-info',
  High: 'tone-warning',
  Critical: 'tone-danger'
};

@Component({
  selector: 'app-priority-badge',
  standalone: true,
  template: `
    <span class="badge {{ tone }}">
      @if (priority === 'Critical') {
        <span class="badge-dot"></span>
      }
      {{ priority }}
    </span>
  `,
  styleUrl: './priority-badge.component.scss'
})
export class PriorityBadgeComponent {
  @Input({ required: true }) priority!: TicketPriority;

  get tone(): string {
    return PRIORITY_TONE[this.priority];
  }
}

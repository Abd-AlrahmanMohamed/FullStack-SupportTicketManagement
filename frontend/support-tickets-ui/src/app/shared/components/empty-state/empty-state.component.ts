import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <div class="empty-state">
      <mat-icon class="empty-icon">{{ icon }}</mat-icon>
      <p class="empty-title">{{ title }}</p>
      @if (message) {
        <p class="empty-message">{{ message }}</p>
      }
      <ng-content></ng-content>
    </div>
  `,
  styleUrl: './empty-state.component.scss'
})
export class EmptyStateComponent {
  @Input() icon = 'inbox';
  @Input({ required: true }) title!: string;
  @Input() message?: string;
}

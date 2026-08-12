import { Component, Input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading-state',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="loading-state" [class.loading-overlay]="overlay">
      <mat-spinner [diameter]="overlay ? 32 : 40"></mat-spinner>
      @if (message) {
        <p class="loading-message">{{ message }}</p>
      }
    </div>
  `,
  styleUrl: './loading-state.component.scss'
})
export class LoadingStateComponent {
  @Input() overlay = false;
  @Input() message?: string;
}

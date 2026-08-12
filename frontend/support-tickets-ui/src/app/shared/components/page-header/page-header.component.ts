import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  standalone: true,
  template: `
    <div class="page-header">
      <div class="page-header-text">
        <h1 class="page-title">{{ title }}</h1>
        @if (description) {
          <p class="page-description">{{ description }}</p>
        }
      </div>
      <div class="page-header-actions">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styleUrl: './page-header.component.scss'
})
export class PageHeaderComponent {
  @Input({ required: true }) title!: string;
  @Input() description?: string;
}

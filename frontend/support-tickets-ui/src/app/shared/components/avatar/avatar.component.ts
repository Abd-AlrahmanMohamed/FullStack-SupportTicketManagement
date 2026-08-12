import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-avatar',
  standalone: true,
  template: `<span class="avatar" [class.avatar-sm]="size === 'sm'">{{ initials }}</span>`,
  styleUrl: './avatar.component.scss'
})
export class AvatarComponent {
  @Input({ required: true }) name = '';
  @Input() size: 'sm' | 'md' = 'md';

  get initials(): string {
    const parts = this.name.trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return '?';
    const first = parts[0][0] ?? '';
    const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
    return (first + last).toUpperCase();
  }
}

import { Component } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from './core/auth/auth.service';
import { AvatarComponent } from './shared/components/avatar/avatar.component';
import { UserRole } from './core/models/user.model';

const ROLE_LABEL: Record<UserRole, string> = {
  Admin: 'Administrator',
  SupportAgent: 'Support Agent',
  Customer: 'Customer'
};

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatTooltipModule,
    AvatarComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  isHandset = toSignal(
    this.breakpointObserver.observe('(max-width: 768px)').pipe(map((result) => result.matches)),
    { initialValue: false }
  );

  constructor(
    public authService: AuthService,
    private router: Router,
    private breakpointObserver: BreakpointObserver
  ) {}

  roleLabel(role: UserRole): string {
    return ROLE_LABEL[role];
  }

  get navItems(): { path: string; icon: string; label: string }[] {
    const role = this.authService.currentUser()?.role;
    const items: { path: string; icon: string; label: string }[] = [];

    if (role === 'Admin') {
      items.push({ path: '/dashboard', icon: 'dashboard', label: 'Dashboard' });
    }

    items.push({
      path: '/tickets',
      icon: 'confirmation_number',
      label: role === 'Customer' ? 'My Tickets' : 'Tickets'
    });

    if (role === 'Customer') {
      items.push({ path: '/tickets/create', icon: 'add_circle_outline', label: 'Create Ticket' });
    }

    if (role === 'Admin') {
      items.push({ path: '/users', icon: 'group', label: 'Users' });
    }

    return items;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

import { Component, OnInit, computed, signal } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { UserService } from '../../core/services/user.service';
import { User, UserRole } from '../../core/models/user.model';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { CreateUserDialogComponent } from './create-user-dialog/create-user-dialog.component';

const ROLE_TONE: Record<UserRole, string> = {
  Admin: 'tone-info',
  SupportAgent: 'tone-warning',
  Customer: 'tone-neutral'
};

const ROLE_LABEL: Record<UserRole, string> = {
  Admin: 'Admin',
  SupportAgent: 'Support Agent',
  Customer: 'Customer'
};

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    DatePipe,
    NgClass,
    ReactiveFormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  users = signal<User[]>([]);
  loading = signal(true);
  search = new FormControl('', { nonNullable: true });
  displayedColumns = ['fullName', 'email', 'role', 'isActive', 'createdAt'];

  filteredUsers = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) return this.users();
    return this.users().filter(
      (u) => u.fullName.toLowerCase().includes(term) || u.email.toLowerCase().includes(term)
    );
  });

  private searchTerm = signal('');

  constructor(
    private userService: UserService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.search.valueChanges.subscribe((value) => this.searchTerm.set(value));
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.userService.getUsers().subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  roleTone(role: UserRole): string {
    return ROLE_TONE[role];
  }

  roleLabel(role: UserRole): string {
    return ROLE_LABEL[role];
  }

  openCreateUserDialog(): void {
    this.dialog
      .open(CreateUserDialogComponent, { width: '420px' })
      .afterClosed()
      .subscribe((createdUser) => {
        if (createdUser) {
          this.snackBar.open('User created.', 'Dismiss', { duration: 3000 });
          this.load();
        }
      });
  }
}

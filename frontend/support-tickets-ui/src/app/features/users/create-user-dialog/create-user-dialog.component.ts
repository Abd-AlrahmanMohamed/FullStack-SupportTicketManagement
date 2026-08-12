import { Component, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../../core/services/user.service';
import { User } from '../../../core/models/user.model';

@Component({
  selector: 'app-create-user-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './create-user-dialog.component.html',
  styleUrl: './create-user-dialog.component.scss'
})
export class CreateUserDialogComponent {
  submitting = signal(false);
  errorMessage = signal<string | null>(null);

  form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: ['Customer', [Validators.required]]
  });

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private dialogRef: MatDialogRef<CreateUserDialogComponent, User>
  ) {}

  cancel(): void {
    this.dialogRef.close();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);
    const value = this.form.getRawValue();

    this.userService
      .createUser({
        fullName: value.fullName!,
        email: value.email!,
        password: value.password!,
        role: value.role as any
      })
      .subscribe({
        next: (user) => {
          this.submitting.set(false);
          this.dialogRef.close(user);
        },
        error: (err) => {
          this.submitting.set(false);
          this.errorMessage.set(err?.error?.message ?? 'Could not create the user.');
        }
      });
  }
}

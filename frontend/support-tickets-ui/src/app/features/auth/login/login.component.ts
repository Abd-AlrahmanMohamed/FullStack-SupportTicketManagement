import { Component, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);
  showDemoCredentials = !environment.production;

  demoAccounts = [
    { role: 'Admin', email: 'admin@support.local', password: 'Admin@123' },
    { role: 'Support Agent', email: 'agent@support.local', password: 'Agent@123' },
    { role: 'Customer', email: 'customer@support.local', password: 'Customer@123' }
  ];

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  fillDemoAccount(email: string, password: string): void {
    this.form.setValue({ email, password });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const { email, password } = this.form.getRawValue();

    this.authService.login({ email: email!, password: password! }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/tickets']);
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Invalid email or password.');
      }
    });
  }
}

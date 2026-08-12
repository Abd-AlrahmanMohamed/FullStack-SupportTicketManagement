import { Component, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TicketService } from '../../../core/services/ticket.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-ticket-create',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PageHeaderComponent
  ],
  templateUrl: './ticket-create.component.html',
  styleUrl: './ticket-create.component.scss'
})
export class TicketCreateComponent {
  submitting = signal(false);
  errorMessage = signal<string | null>(null);
  descriptionMaxLength = 4000;

  form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(this.descriptionMaxLength)]],
    priority: ['Medium', [Validators.required]]
  });

  constructor(
    private fb: FormBuilder,
    private ticketService: TicketService,
    private router: Router
  ) {}

  cancel(): void {
    this.router.navigate(['/tickets']);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { title, description, priority } = this.form.getRawValue();

    this.ticketService
      .createTicket({ title: title!, description: description!, priority: priority as any })
      .subscribe({
        next: (ticket) => {
          this.submitting.set(false);
          this.router.navigate(['/tickets', ticket.id]);
        },
        error: () => {
          this.submitting.set(false);
          this.errorMessage.set('Could not create the ticket. Please check the form and try again.');
        }
      });
  }
}

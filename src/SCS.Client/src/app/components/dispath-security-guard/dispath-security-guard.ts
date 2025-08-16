import { Component, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SecurityGuardService } from '../../core/services/security-guard.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-dispath-security-guard',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './dispath-security-guard.html',
  styleUrl: './dispath-security-guard.scss',
  standalone: true
})
export class DispathSecurityGuard {
  public premiseId = input.required<number>();

  private fb = inject(FormBuilder);
  private securityGuardService = inject(SecurityGuardService);
  private messageService = inject(MessageService);

  dispatchForm: FormGroup = this.fb.group({
    email: ['nqhuy031295@gmail.com', [Validators.required, Validators.email]]
  });


  onSubmit(): void {
    if (this.dispatchForm.invalid) {
      return;
    }

    const email = this.dispatchForm.get('email')?.value;
    this.securityGuardService.dispatchSecurityGuard(this.premiseId(), email).subscribe(
      () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Security guard dispatched successfully!' });
        this.dispatchForm.reset();
      },
      (_) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to dispatch security guard.' });
      }
    );
  }

  get email() {
    return this.dispatchForm.get('email');
  }
}

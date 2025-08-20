import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { PasswordModule } from 'primeng/password';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { firstValueFrom } from 'rxjs';
import { MessageService } from 'primeng/api';
import { animate, style, transition, trigger } from '@angular/animations';

@Component({
	selector: 'app-login',
	templateUrl: './login.component.html',
	styleUrls: ['./login.component.scss'],
	standalone: true,
	imports: [
		CommonModule,
		ReactiveFormsModule,
		CardModule,
		PasswordModule,
		CheckboxModule,
		ToastModule
	],
	animations: [
		trigger('fadeInUp', [
			transition(':enter', [
				style({
					opacity: 0,
					transform: 'translateY(20px)'
				}),
				animate('0.5s ease-out', style({
					opacity: 1,
					transform: 'translateY(0)'
				}))
			])
		])
	]
})
export class LoginComponent {
	loginForm: FormGroup;

	constructor(
		private formBuilder: FormBuilder,
		private authService: AuthService,
		private messageService: MessageService,
		private router: Router
	) {
		this.loginForm = this.formBuilder.group({
			empNo: [
				'',
				[
					Validators.required,
					Validators.pattern(/^[1-9][0-9]{7}$/)
				]
			]
		});
	}

	async onSubmit(): Promise<void> {
		if (this.loginForm.invalid) {
			return;
		}

		try {
			const empNo = this.loginForm.get('empNo')?.value;
			await firstValueFrom(this.authService.login(empNo));
			// Login successful
			this.router.navigate(['/']);
			this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Login successfully!' });
		} catch (error: any) {
			// Handle error here
			this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Wrong user emp-no' });
		}
	}
}

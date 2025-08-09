import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { PasswordModule } from 'primeng/password';
import { CheckboxModule } from 'primeng/checkbox';

@Component({
	selector: 'app-login',
	templateUrl: './login.component.html',
	styleUrls: ['./login.component.scss'],
	standalone: true,
	imports: [CommonModule, ReactiveFormsModule, CardModule, PasswordModule, CheckboxModule]
})
export class LoginComponent {
	loginForm: FormGroup;

	constructor(
		private formBuilder: FormBuilder,
		private authService: AuthService,
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

	onSubmit(): void {
		if (this.loginForm.invalid) {
			return;
		}
	}
}

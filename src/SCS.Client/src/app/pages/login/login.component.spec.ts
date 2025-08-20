import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { AuthService } from '../../core/services/auth.service';
import { MessageService } from 'primeng/api';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { provideZonelessChangeDetection } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';

describe('LoginComponent', () => {
    let component: LoginComponent;
    let fixture: ComponentFixture<LoginComponent>;
    let mockAuthService: jasmine.SpyObj<AuthService>;
    let mockMessageService: jasmine.SpyObj<MessageService>;
    let mockRouter: jasmine.SpyObj<Router>;

    beforeEach(async () => {
        const authServiceSpy = jasmine.createSpyObj('AuthService', ['login']);
        const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);
        const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        await TestBed.configureTestingModule({
            imports: [
                LoginComponent,
                ReactiveFormsModule,
                NoopAnimationsModule
            ],
            providers: [
                provideZonelessChangeDetection(),
                provideHttpClient(),
                FormBuilder,
                { provide: AuthService, useValue: authServiceSpy },
                { provide: Router, useValue: routerSpy }
            ]
        }).compileComponents();

        mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
        mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;

        fixture = TestBed.createComponent(LoginComponent);
        component = fixture.componentInstance;
        mockMessageService = component['messageService'] as jasmine.SpyObj<MessageService>;
        spyOn(mockMessageService, 'add');
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    describe('Form Initialization', () => {
        it('should initialize login form with empNo field', () => {
            expect(component.loginForm).toBeDefined();
            expect(component.loginForm.get('empNo')).toBeDefined();
        });

        it('should have required validator on empNo field', () => {
            const empNoControl = component.loginForm.get('empNo');
            empNoControl?.setValue('');
            expect(empNoControl?.hasError('required')).toBeTruthy();
        });

        it('should have pattern validator on empNo field', () => {
            const empNoControl = component.loginForm.get('empNo');
            empNoControl?.setValue('123');
            expect(empNoControl?.hasError('pattern')).toBeTruthy();

            empNoControl?.setValue('12345678');
            expect(empNoControl?.hasError('pattern')).toBeFalsy();
        });

        it('should reject empNo starting with 0', () => {
            const empNoControl = component.loginForm.get('empNo');
            empNoControl?.setValue('01234567');
            expect(empNoControl?.hasError('pattern')).toBeTruthy();
        });

        it('should accept valid 8-digit empNo not starting with 0', () => {
            const empNoControl = component.loginForm.get('empNo');
            empNoControl?.setValue('12345678');
            expect(empNoControl?.valid).toBeTruthy();
        });
    });

    describe('Form Submission', () => {
        it('should not submit when form is invalid', async () => {
            component.loginForm.get('empNo')?.setValue('');

            await component.onSubmit();

            expect(mockAuthService.login).not.toHaveBeenCalled();
            expect(mockRouter.navigate).not.toHaveBeenCalled();
        });

        it('should submit and navigate on successful login', async () => {
            const empNo = '12345678';
            component.loginForm.get('empNo')?.setValue(empNo);
            mockAuthService.login.and.returnValue(of({ token: 'mock-token' }));

            await component.onSubmit();

            expect(mockAuthService.login).toHaveBeenCalledWith(empNo);
            expect(mockMessageService.add).toHaveBeenCalledWith({
                severity: 'success',
                summary: 'Success',
                detail: 'Login successfully!'
            });
            expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
        });

        it('should show error message on login failure', async () => {
            const empNo = '12345678';
            component.loginForm.get('empNo')?.setValue(empNo);
            mockAuthService.login.and.returnValue(throwError(() => new Error('Login failed')));

            await component.onSubmit();

            expect(mockAuthService.login).toHaveBeenCalledWith(empNo);
            expect(mockMessageService.add).toHaveBeenCalledWith({
                severity: 'error',
                summary: 'Error',
                detail: 'Wrong user emp-no'
            });
            expect(mockRouter.navigate).not.toHaveBeenCalled();
        });

        it('should handle invalid form submission gracefully', async () => {
            component.loginForm.get('empNo')?.setValue('invalid');

            await component.onSubmit();

            expect(mockAuthService.login).not.toHaveBeenCalled();
            expect(mockMessageService.add).not.toHaveBeenCalled();
        });
    });

    describe('Form Validation', () => {
        it('should validate empNo with correct pattern', () => {
            const empNoControl = component.loginForm.get('empNo');

            // Valid cases
            empNoControl?.setValue('12345678');
            expect(empNoControl?.valid).toBeTruthy();

            empNoControl?.setValue('99999999');
            expect(empNoControl?.valid).toBeTruthy();

            // Invalid cases
            empNoControl?.setValue('1234567'); // 7 digits
            expect(empNoControl?.valid).toBeFalsy();

            empNoControl?.setValue('123456789'); // 9 digits
            expect(empNoControl?.valid).toBeFalsy();

            empNoControl?.setValue('01234567'); // starts with 0
            expect(empNoControl?.valid).toBeFalsy();

            empNoControl?.setValue('abcd1234'); // contains letters
            expect(empNoControl?.valid).toBeFalsy();
        });

        it('should show required error when empNo is empty', () => {
            const empNoControl = component.loginForm.get('empNo');
            empNoControl?.setValue('');
            empNoControl?.markAsTouched();

            expect(empNoControl?.hasError('required')).toBeTruthy();
        });
    });
});

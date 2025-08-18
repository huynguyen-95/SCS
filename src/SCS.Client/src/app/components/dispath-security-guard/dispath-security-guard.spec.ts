import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { of, throwError } from 'rxjs';

import { DispathSecurityGuard } from './dispath-security-guard';
import { SecurityGuardService } from '../../core/services/security-guard.service';
import { MessageService } from 'primeng/api';

describe('DispathSecurityGuard', () => {
  let component: DispathSecurityGuard;
  let fixture: ComponentFixture<DispathSecurityGuard>;
  let mockSecurityGuardService: jasmine.SpyObj<SecurityGuardService>;
  let mockMessageService: jasmine.SpyObj<MessageService>;

  beforeEach(async () => {
    // Create spy objects for the services
    mockSecurityGuardService = jasmine.createSpyObj('SecurityGuardService', ['dispatchSecurityGuard']);
    mockMessageService = jasmine.createSpyObj('MessageService', ['add']);

    // Setup default return values
    mockSecurityGuardService.dispatchSecurityGuard.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [DispathSecurityGuard],
      providers: [
        provideZonelessChangeDetection(),
        { provide: SecurityGuardService, useValue: mockSecurityGuardService },
        { provide: MessageService, useValue: mockMessageService }
      ]
    })
      .compileComponents();

    fixture = TestBed.createComponent(DispathSecurityGuard);
    component = fixture.componentInstance;

    // Set required input
    fixture.componentRef.setInput('premiseId', 1);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('onSubmit', () => {
    it('should not submit when form is invalid', () => {
      // Make form invalid by setting invalid email
      component.dispatchForm.get('email')?.setValue('invalid-email');

      component.onSubmit();

      expect(mockSecurityGuardService.dispatchSecurityGuard).not.toHaveBeenCalled();
      expect(mockMessageService.add).not.toHaveBeenCalled();
    });

    it('should dispatch security guard when form is valid', () => {
      // Ensure form is valid
      component.dispatchForm.get('email')?.setValue('test@example.com');

      component.onSubmit();

      expect(mockSecurityGuardService.dispatchSecurityGuard).toHaveBeenCalledWith(1, 'test@example.com');
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'success',
        summary: 'Success',
        detail: 'Security guard dispatched successfully!'
      });
      expect(component.dispatchForm.pristine).toBeTruthy(); // Form should be reset
    });

    it('should show error message when service call fails', () => {
      // Setup service to return error
      mockSecurityGuardService.dispatchSecurityGuard.and.returnValue(throwError(() => new Error('Service error')));

      // Ensure form is valid
      component.dispatchForm.get('email')?.setValue('test@example.com');

      component.onSubmit();

      expect(mockSecurityGuardService.dispatchSecurityGuard).toHaveBeenCalledWith(1, 'test@example.com');
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to dispatch security guard.'
      });
    });
  });

  describe('email getter', () => {
    it('should return email form control', () => {
      const emailControl = component.email;
      expect(emailControl).toBe(component.dispatchForm.get('email'));
    });
  });
});

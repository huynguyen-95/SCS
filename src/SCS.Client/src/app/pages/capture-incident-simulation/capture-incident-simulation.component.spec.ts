import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CaptureIncidentSimulationComponent } from './capture-incident-simulation.component';
import { SecurityGuardService } from '../../core/services/security-guard.service';
import { PremiseService } from '../../core/services/premise.service';
import { MessageService } from 'primeng/api';
import { provideZonelessChangeDetection } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { Premise } from '../../models/premise.model';

describe('CaptureIncidentSimulationComponent', () => {
  let component: CaptureIncidentSimulationComponent;
  let fixture: ComponentFixture<CaptureIncidentSimulationComponent>;
  let mockSecurityGuardService: jasmine.SpyObj<SecurityGuardService>;
  let mockPremiseService: jasmine.SpyObj<PremiseService>;
  let mockMessageService: jasmine.SpyObj<MessageService>;

  const mockPremises: Premise[] = [
    { id: 1, name: 'Building A' },
    { id: 2, name: 'Building B' }
  ];

  beforeEach(async () => {
    const securityGuardServiceSpy = jasmine.createSpyObj('SecurityGuardService', ['uploadIncidentReport']);
    const premiseServiceSpy = jasmine.createSpyObj('PremiseService', ['getPremiseListAsync']);
    const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);

    await TestBed.configureTestingModule({
      imports: [
        CaptureIncidentSimulationComponent,
        NoopAnimationsModule
      ],
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        { provide: SecurityGuardService, useValue: securityGuardServiceSpy },
        { provide: PremiseService, useValue: premiseServiceSpy },
        { provide: MessageService, useValue: messageServiceSpy }
      ]
    }).compileComponents();

    mockSecurityGuardService = TestBed.inject(SecurityGuardService) as jasmine.SpyObj<SecurityGuardService>;
    mockPremiseService = TestBed.inject(PremiseService) as jasmine.SpyObj<PremiseService>;
    mockMessageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;

    mockPremiseService.getPremiseListAsync.and.returnValue(Promise.resolve(mockPremises));

    fixture = TestBed.createComponent(CaptureIncidentSimulationComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default values', () => {
      expect(component.selectedFile).toBeNull();
      expect(component.description()).toBe('');
      expect(component.imagePreview).toBeNull();
      expect(component.premises()).toEqual([]);
      expect(component.selectedPremise).toBeNull();
    });

    it('should load premises on init', async () => {
      await component.ngOnInit();

      expect(mockPremiseService.getPremiseListAsync).toHaveBeenCalled();
      expect(component.premises()).toEqual(mockPremises);
    });

    it('should handle error when loading premises', async () => {
      mockPremiseService.getPremiseListAsync.and.returnValue(Promise.reject(new Error('Load failed')));
      spyOn(console, 'error');

      await component.ngOnInit();

      expect(console.error).toHaveBeenCalledWith('Failed to load premises:', jasmine.any(Error));
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load premises.'
      });
    });
  });

  describe('File Selection', () => {
    it('should handle file selection', () => {
      const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      const mockEvent = {
        target: {
          files: [mockFile]
        }
      };

      // Mock FileReader
      const mockFileReader = {
        readAsDataURL: jasmine.createSpy('readAsDataURL'),
        onload: null as any,
        result: 'data:image/jpeg;base64,testdata'
      };
      spyOn(window, 'FileReader').and.returnValue(mockFileReader as any);

      component.onFileSelected(mockEvent);

      expect(component.selectedFile).toBe(mockFile);
      expect(mockFileReader.readAsDataURL).toHaveBeenCalledWith(mockFile);
    });

    it('should create image preview when file is selected', () => {
      const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      const mockEvent = {
        target: {
          files: [mockFile]
        }
      };

      const mockFileReader = {
        readAsDataURL: jasmine.createSpy('readAsDataURL'),
        onload: null as any,
        result: 'data:image/jpeg;base64,testdata'
      };
      spyOn(window, 'FileReader').and.returnValue(mockFileReader as any);

      component.onFileSelected(mockEvent);

      // Simulate FileReader onload
      mockFileReader.onload({ target: { result: 'data:image/jpeg;base64,testdata' } });

      expect(component.imagePreview).toBe('data:image/jpeg;base64,testdata');
    });

    it('should handle empty file selection', () => {
      const mockEvent = {
        target: {
          files: []
        }
      };

      component.onFileSelected(mockEvent);

      expect(component.selectedFile).toBeNull();
    });
  });

  describe('Form Submission', () => {
    beforeEach(() => {
      component.selectedFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      component.description.set('Test incident description');
      component.selectedPremise = mockPremises[0];
    });

    it('should submit incident report successfully', () => {
      mockSecurityGuardService.uploadIncidentReport.and.returnValue(of('Upload successful'));

      component.onSubmit();

      expect(mockSecurityGuardService.uploadIncidentReport).toHaveBeenCalledWith(
        1,
        jasmine.any(File),
        'Test incident description'
      );
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'success',
        summary: 'Success',
        detail: 'Incident captured successfully!'
      });
    });

    it('should handle upload error', () => {
      mockSecurityGuardService.uploadIncidentReport.and.returnValue(throwError(() => new Error('Upload failed')));

      component.onSubmit();

      expect(mockSecurityGuardService.uploadIncidentReport).toHaveBeenCalledWith(
        1,
        jasmine.any(File),
        'Test incident description'
      );
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to upload incident report.'
      });
    });

    it('should show warning when file is not selected', () => {
      component.selectedFile = null;

      component.onSubmit();

      expect(mockSecurityGuardService.uploadIncidentReport).not.toHaveBeenCalled();
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a premise, image and provide a description.'
      });
    });

    it('should show warning when description is empty', () => {
      component.description.set('');

      component.onSubmit();

      expect(mockSecurityGuardService.uploadIncidentReport).not.toHaveBeenCalled();
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a premise, image and provide a description.'
      });
    });

    it('should show warning when premise is not selected', () => {
      component.selectedPremise = null;

      component.onSubmit();

      expect(mockSecurityGuardService.uploadIncidentReport).not.toHaveBeenCalled();
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a premise, image and provide a description.'
      });
    });

    it('should show warning when all required fields are missing', () => {
      component.selectedFile = null;
      component.description.set('');
      component.selectedPremise = null;

      component.onSubmit();

      expect(mockSecurityGuardService.uploadIncidentReport).not.toHaveBeenCalled();
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a premise, image and provide a description.'
      });
    });
  });

  describe('Form Reset', () => {
    beforeEach(() => {
      component.selectedFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      component.description.set('Test description');
      component.imagePreview = 'data:image/jpeg;base64,testdata';
      component.selectedPremise = mockPremises[0];
    });

    it('should reset all form fields', () => {
      // Mock DOM element
      const mockFileInput = {
        value: 'test.jpg'
      };
      spyOn(document, 'getElementById').and.returnValue(mockFileInput as any);

      component.onReset();

      expect(component.selectedFile).toBeNull();
      expect(component.description()).toBe('');
      expect(component.imagePreview).toBeNull();
      expect(component.selectedPremise).toBeNull();
      expect(mockFileInput.value).toBe('');
    });

    it('should handle missing file input element', () => {
      spyOn(document, 'getElementById').and.returnValue(null);

      // Should not throw error
      component.onReset();

      expect(component.selectedFile).toBeNull();
      expect(component.description()).toBe('');
      expect(component.imagePreview).toBeNull();
      expect(component.selectedPremise).toBeNull();
    });

    it('should reset form after successful submission', () => {
      mockSecurityGuardService.uploadIncidentReport.and.returnValue(of('Upload successful'));
      spyOn(component, 'onReset');

      component.onSubmit();

      expect(component.onReset).toHaveBeenCalled();
    });

    it('should not reset form after failed submission', () => {
      mockSecurityGuardService.uploadIncidentReport.and.returnValue(throwError(() => new Error('Upload failed')));
      spyOn(component, 'onReset');

      component.onSubmit();

      expect(component.onReset).not.toHaveBeenCalled();
    });
  });

  describe('Data Loading', () => {
    it('should handle empty premises list', async () => {
      mockPremiseService.getPremiseListAsync.and.returnValue(Promise.resolve([]));

      await component.ngOnInit();

      expect(component.premises()).toEqual([]);
    });

    it('should maintain state when premises loading fails', async () => {
      const initialPremises = component.premises();
      mockPremiseService.getPremiseListAsync.and.returnValue(Promise.reject(new Error('Network error')));

      await component.ngOnInit();

      expect(component.premises()).toBe(initialPremises);
    });
  });

  describe('Form Validation', () => {
    it('should validate all required fields are present', () => {
      // All fields present
      component.selectedFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      component.description.set('Valid description');
      component.selectedPremise = mockPremises[0];
      mockSecurityGuardService.uploadIncidentReport.and.returnValue(of('Success'));

      component.onSubmit();

      expect(mockSecurityGuardService.uploadIncidentReport).toHaveBeenCalled();
    });

    it('should validate individual field requirements', () => {
      // Test each field individually
      component.selectedFile = null;
      component.description.set('Valid description');
      component.selectedPremise = mockPremises[0];

      component.onSubmit();
      expect(mockSecurityGuardService.uploadIncidentReport).not.toHaveBeenCalled();

      component.selectedFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      component.description.set('');
      component.selectedPremise = mockPremises[0];

      component.onSubmit();
      expect(mockSecurityGuardService.uploadIncidentReport).not.toHaveBeenCalled();

      component.selectedFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      component.description.set('Valid description');
      component.selectedPremise = null;

      component.onSubmit();
      expect(mockSecurityGuardService.uploadIncidentReport).not.toHaveBeenCalled();
    });
  });

  describe('Signal Management', () => {
    it('should update description signal correctly', () => {
      expect(component.description()).toBe('');

      component.description.set('New description');
      expect(component.description()).toBe('New description');

      component.description.set('Updated description');
      expect(component.description()).toBe('Updated description');
    });

    it('should update premises signal correctly', async () => {
      expect(component.premises()).toEqual([]);

      await component.ngOnInit();
      expect(component.premises()).toEqual(mockPremises);
    });
  });

  describe('Error Handling', () => {
    it('should handle premise loading errors gracefully', async () => {
      mockPremiseService.getPremiseListAsync.and.returnValue(Promise.reject(new Error('Network timeout')));
      spyOn(console, 'error');

      await component.ngOnInit();

      expect(console.error).toHaveBeenCalledWith('Failed to load premises:', jasmine.any(Error));
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load premises.'
      });
    });

    it('should handle upload errors gracefully', () => {
      component.selectedFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      component.description.set('Test description');
      component.selectedPremise = mockPremises[0];
      mockSecurityGuardService.uploadIncidentReport.and.returnValue(throwError(() => new Error('Server error')));
      spyOn(console, 'error');

      component.onSubmit();

      expect(console.error).toHaveBeenCalledWith('Upload error:', jasmine.any(Error));
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to upload incident report.'
      });
    });
  });
});

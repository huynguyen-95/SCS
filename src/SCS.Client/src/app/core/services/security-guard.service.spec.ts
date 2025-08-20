import { TestBed } from '@angular/core/testing';
import { SecurityGuardService } from './security-guard.service';
import { ApiService } from './api.service';
import { of, throwError } from 'rxjs';
import { provideZonelessChangeDetection } from '@angular/core';

describe('SecurityGuardService', () => {
  let service: SecurityGuardService;
  let mockApiService: jasmine.SpyObj<ApiService>;

  beforeEach(() => {
    const apiServiceSpy = jasmine.createSpyObj('ApiService', ['post', 'postFormData']);

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        SecurityGuardService,
        { provide: ApiService, useValue: apiServiceSpy }
      ]
    });

    service = TestBed.inject(SecurityGuardService);
    mockApiService = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('dispatchSecurityGuard', () => {
    it('should dispatch security guard to premise successfully', (done) => {
      // Arrange
      const premiseId = 123;
      const guardEmail = 'guard@example.com';
      const expectedPayload = { premiseId, guardEmail };
      mockApiService.post.and.returnValue(of(undefined));

      // Act
      const result = service.dispatchSecurityGuard(premiseId, guardEmail);

      // Assert
      expect(mockApiService.post).toHaveBeenCalledWith(
        'api/security-guard/dispatch-to-premise',
        expectedPayload
      );
      result.subscribe(response => {
        expect(response).toBeUndefined();
        done();
      });
    });

    it('should handle dispatch security guard error', (done) => {
      // Arrange
      const premiseId = 123;
      const guardEmail = 'guard@example.com';
      const error = new Error('Dispatch failed');
      mockApiService.post.and.returnValue(throwError(() => error));

      // Act
      const result = service.dispatchSecurityGuard(premiseId, guardEmail);

      // Assert
      result.subscribe({
        next: () => fail('Expected error'),
        error: (err) => {
          expect(err).toBe(error);
          done();
        }
      });
    });

    it('should handle zero premise ID', () => {
      // Arrange
      const premiseId = 0;
      const guardEmail = 'guard@example.com';
      const expectedPayload = { premiseId, guardEmail };
      mockApiService.post.and.returnValue(of(undefined));

      // Act
      service.dispatchSecurityGuard(premiseId, guardEmail);

      // Assert
      expect(mockApiService.post).toHaveBeenCalledWith(
        'api/security-guard/dispatch-to-premise',
        expectedPayload
      );
    });

    it('should handle empty guard email', () => {
      // Arrange
      const premiseId = 123;
      const guardEmail = '';
      const expectedPayload = { premiseId, guardEmail };
      mockApiService.post.and.returnValue(of(undefined));

      // Act
      service.dispatchSecurityGuard(premiseId, guardEmail);

      // Assert
      expect(mockApiService.post).toHaveBeenCalledWith(
        'api/security-guard/dispatch-to-premise',
        expectedPayload
      );
    });
  });

  describe('uploadIncidentReport', () => {
    let mockFile: File;

    beforeEach(() => {
      mockFile = new File(['test content'], 'test.txt', { type: 'text/plain' });
    });

    it('should upload incident report successfully', (done) => {
      // Arrange
      const premiseId = 456;
      const description = 'Test incident description';
      const expectedResponse = { id: 1, status: 'uploaded' };
      mockApiService.postFormData.and.returnValue(of(expectedResponse));

      // Act
      const result = service.uploadIncidentReport(premiseId, mockFile, description);

      // Assert
      expect(mockApiService.postFormData).toHaveBeenCalledWith(
        'api/security-guard/incidents/456',
        jasmine.any(FormData)
      );

      // Verify FormData content
      const formDataCall = mockApiService.postFormData.calls.mostRecent();
      const formData = formDataCall.args[1] as FormData;
      expect(formData.get('file')).toBe(mockFile);
      expect(formData.get('description')).toBe(description);
      expect(formData.get('incidentDate')).toBeTruthy();

      result.subscribe(response => {
        expect(response).toEqual(expectedResponse);
        done();
      });
    });

    it('should handle upload incident report error', (done) => {
      // Arrange
      const premiseId = 456;
      const description = 'Test incident description';
      const error = new Error('Upload failed');
      mockApiService.postFormData.and.returnValue(throwError(() => error));

      // Act
      const result = service.uploadIncidentReport(premiseId, mockFile, description);

      // Assert
      result.subscribe({
        next: () => fail('Expected error'),
        error: (err) => {
          expect(err).toBe(error);
          done();
        }
      });
    });

    it('should handle empty description in upload', () => {
      // Arrange
      const premiseId = 456;
      const description = '';
      const expectedResponse = { id: 1, status: 'uploaded' };
      mockApiService.postFormData.and.returnValue(of(expectedResponse));

      // Act
      service.uploadIncidentReport(premiseId, mockFile, description);

      // Assert
      const formDataCall = mockApiService.postFormData.calls.mostRecent();
      const formData = formDataCall.args[1] as FormData;
      expect(formData.get('description')).toBe('');
    });

    it('should handle zero premise ID in upload', () => {
      // Arrange
      const premiseId = 0;
      const description = 'Test description';
      mockApiService.postFormData.and.returnValue(of({}));

      // Act
      service.uploadIncidentReport(premiseId, mockFile, description);

      // Assert
      expect(mockApiService.postFormData).toHaveBeenCalledWith(
        'api/security-guard/incidents/0',
        jasmine.any(FormData)
      );
    });

    it('should always include current timestamp in FormData', () => {
      // Arrange
      const premiseId = 456;
      const description = 'Test description';
      mockApiService.postFormData.and.returnValue(of({}));

      // Act
      service.uploadIncidentReport(premiseId, mockFile, description);

      // Assert
      const formDataCall = mockApiService.postFormData.calls.mostRecent();
      const formData = formDataCall.args[1] as FormData;
      expect(formData.get('incidentDate')).toBeTruthy();
      expect(typeof formData.get('incidentDate')).toBe('string');
    });
  });
});

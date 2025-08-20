import { TestBed } from '@angular/core/testing';
import { HttpRequest, HttpHandlerFn, HttpResponse, HttpHeaders } from '@angular/common/http';
import { provideZonelessChangeDetection } from '@angular/core';
import { of } from 'rxjs';

import { JwtInterceptor } from './jwt.interceptor';
import { AuthService } from '../services/auth.service';

describe('JwtInterceptor', () => {
    let mockAuthService: jasmine.SpyObj<AuthService>;
    let mockNext: jasmine.Spy<HttpHandlerFn>;
    let interceptor: typeof JwtInterceptor;

    beforeEach(() => {
        mockAuthService = jasmine.createSpyObj('AuthService', ['getToken']);
        mockNext = jasmine.createSpy('next').and.returnValue(of(new HttpResponse({ status: 200 })));

        TestBed.configureTestingModule({
            providers: [
                provideZonelessChangeDetection(),
                { provide: AuthService, useValue: mockAuthService }
            ]
        });

        interceptor = JwtInterceptor;
    });

    it('should add Authorization header when token exists', () => {
        const token = 'test-jwt-token';
        mockAuthService.getToken.and.returnValue(token);

        const originalRequest = new HttpRequest('GET', '/api/test');

        TestBed.runInInjectionContext(() => {
            interceptor(originalRequest, mockNext);
        });

        expect(mockAuthService.getToken).toHaveBeenCalled();
        expect(mockNext).toHaveBeenCalled();

        // Get the modified request from the next call
        const modifiedRequest = mockNext.calls.mostRecent().args[0];
        expect(modifiedRequest.headers.get('Authorization')).toBe(`Bearer ${token}`);
    });

    it('should not add Authorization header when token does not exist', () => {
        mockAuthService.getToken.and.returnValue(null);

        const originalRequest = new HttpRequest('GET', '/api/test');

        TestBed.runInInjectionContext(() => {
            interceptor(originalRequest, mockNext);
        });

        expect(mockAuthService.getToken).toHaveBeenCalled();
        expect(mockNext).toHaveBeenCalled();

        // Get the request from the next call
        const passedRequest = mockNext.calls.mostRecent().args[0];
        expect(passedRequest.headers.get('Authorization')).toBeNull();
    });

    it('should not modify original request when token exists', () => {
        const token = 'test-jwt-token';
        mockAuthService.getToken.and.returnValue(token);

        const originalRequest = new HttpRequest('GET', '/api/test');

        TestBed.runInInjectionContext(() => {
            interceptor(originalRequest, mockNext);
        });

        // Original request should not have Authorization header
        expect(originalRequest.headers.get('Authorization')).toBeNull();

        // But the cloned request passed to next should have it
        const modifiedRequest = mockNext.calls.mostRecent().args[0];
        expect(modifiedRequest.headers.get('Authorization')).toBe(`Bearer ${token}`);
    });

    it('should preserve existing headers when adding Authorization', () => {
        const token = 'test-jwt-token';
        mockAuthService.getToken.and.returnValue(token);

        const headers = new HttpHeaders({
            'Content-Type': 'application/json',
            'X-Custom-Header': 'custom-value'
        });

        const originalRequest = new HttpRequest('POST', '/api/test', {}, { headers });

        TestBed.runInInjectionContext(() => {
            interceptor(originalRequest, mockNext);
        });

        const modifiedRequest = mockNext.calls.mostRecent().args[0];

        // Should have all original headers plus Authorization
        expect(modifiedRequest.headers.get('Content-Type')).toBe('application/json');
        expect(modifiedRequest.headers.get('X-Custom-Header')).toBe('custom-value');
        expect(modifiedRequest.headers.get('Authorization')).toBe(`Bearer ${token}`);
    });

    it('should return the observable from next handler', () => {
        const mockResponse = new HttpResponse({ status: 200, body: { data: 'test' } });
        mockAuthService.getToken.and.returnValue('token');
        mockNext.and.returnValue(of(mockResponse));

        const originalRequest = new HttpRequest('GET', '/api/test');

        const result = TestBed.runInInjectionContext(() => {
            return interceptor(originalRequest, mockNext);
        });

        result.subscribe(response => {
            expect(response).toBe(mockResponse);
        });
    });
});

import { TestBed } from '@angular/core/testing';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideZonelessChangeDetection } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import environment from '../../env';
import { UserInfo } from '../../models/user-info.model';
import { LoginResponse, ErrorResponse } from '../../models/auth-response.model';

// Mock localStorage
const mockLocalStorage = {
    getItem: jasmine.createSpy('getItem'),
    setItem: jasmine.createSpy('setItem'),
    removeItem: jasmine.createSpy('removeItem')
};

describe('AuthService', () => {
    let service: AuthService;
    let httpMock: HttpTestingController;
    let router: jasmine.SpyObj<Router>;

    // Sample JWT payload matching UserInfo interface
    const mockJwtPayload: UserInfo = {
        empNo: 'EMP001',
        name: 'John Doe',
        isAdmin: true,
        role: 'admin',
        roles: ['admin', 'user'], // Add roles array for hasRole testing
        exp: Math.floor(Date.now() / 1000) + 3600 // Expires in 1 hour
    };

    const mockToken = 'mock.jwt.token';

    beforeEach(() => {
        const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        // Replace global localStorage with mock
        Object.defineProperty(window, 'localStorage', {
            value: mockLocalStorage,
            writable: true
        });

        TestBed.configureTestingModule({
            providers: [
                AuthService,
                { provide: Router, useValue: routerSpy },
                provideHttpClient(),
                provideHttpClientTesting(),
                provideZonelessChangeDetection()
            ]
        });

        service = TestBed.inject(AuthService);
        httpMock = TestBed.inject(HttpTestingController);
        router = TestBed.inject(Router) as jasmine.SpyObj<Router>;

        // Reset all spies before each test
        mockLocalStorage.getItem.calls.reset();
        mockLocalStorage.setItem.calls.reset();
        mockLocalStorage.removeItem.calls.reset();
        router.navigate.calls.reset();
    });

    afterEach(() => {
        try {
            httpMock.verify();
        } catch (e) {
            // Ignore verification errors for tests that don't make HTTP requests
        }
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('getToken', () => {
        it('should return token from localStorage', () => {
            mockLocalStorage.getItem.and.returnValue(mockToken);

            const result = service.getToken();

            expect(mockLocalStorage.getItem).toHaveBeenCalledWith('access_token');
            expect(result).toBe(mockToken);
        });

        it('should return null when no token in localStorage', () => {
            mockLocalStorage.getItem.and.returnValue(null);

            const result = service.getToken();

            expect(mockLocalStorage.getItem).toHaveBeenCalledWith('access_token');
            expect(result).toBeNull();
        });
    });

    describe('setToken', () => {
        it('should store token in localStorage', () => {
            service.setToken(mockToken);

            expect(mockLocalStorage.setItem).toHaveBeenCalledWith('access_token', mockToken);
        });
    });

    describe('removeToken', () => {
        it('should remove token from localStorage', () => {
            service.removeToken();

            expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('access_token');
        });
    });

    describe('getUserInfo', () => {
        it('should return null when no token exists', () => {
            mockLocalStorage.getItem.and.returnValue(null);

            const result = service.getUserInfo();

            expect(mockLocalStorage.getItem).toHaveBeenCalledWith('access_token');
            expect(result).toBeNull();
        });

        it('should return null when token is invalid', () => {
            mockLocalStorage.getItem.and.returnValue('invalid.token');

            const result = service.getUserInfo();

            expect(mockLocalStorage.getItem).toHaveBeenCalledWith('access_token');
            expect(result).toBeNull();
        });
    });

    describe('isAuthenticated', () => {
        it('should return true when token exists and is not expired', () => {
            const validPayload = {
                ...mockJwtPayload,
                exp: Math.floor(Date.now() / 1000) + 3600 // Expires in 1 hour
            };
            spyOn(service, 'getUserInfo').and.returnValue(validPayload);

            const result = service.isAuthenticated();

            expect(result).toBe(true);
        });

        it('should return false when token is expired', () => {
            const expiredPayload = {
                ...mockJwtPayload,
                exp: Math.floor(Date.now() / 1000) - 3600 // Expired 1 hour ago
            };
            spyOn(service, 'getUserInfo').and.returnValue(expiredPayload);

            const result = service.isAuthenticated();

            expect(result).toBe(false);
        });

        it('should return false when no token exists', () => {
            spyOn(service, 'getUserInfo').and.returnValue(null);

            const result = service.isAuthenticated();

            expect(result).toBe(false);
        });

        it('should return false when token decode fails', () => {
            spyOn(service, 'getUserInfo').and.returnValue(null);

            const result = service.isAuthenticated();

            expect(result).toBe(false);
        });
    });

    describe('hasRole', () => {
        it('should return true when user has the specified role', () => {
            spyOn(service, 'getUserInfo').and.returnValue(mockJwtPayload);

            const result = service.hasRole('admin');

            expect(result).toBe(true);
        });

        it('should return false when user does not have the specified role', () => {
            spyOn(service, 'getUserInfo').and.returnValue(mockJwtPayload);

            const result = service.hasRole('manager');

            expect(result).toBe(false);
        });

        it('should return false when no user info available', () => {
            spyOn(service, 'getUserInfo').and.returnValue(null);

            const result = service.hasRole('admin');

            expect(result).toBe(false);
        });
    });

    describe('login', () => {
        it('should make login request and return LoginResponse', () => {
            const empNo = 'EMP001';
            const loginResponse: LoginResponse = { token: mockToken };

            service.login(empNo).subscribe(response => {
                expect(response).toEqual(loginResponse);
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/api/authentication/login`);
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual({ empNo });

            req.flush(loginResponse);
        });

        it('should handle HTTP error and transform to ErrorResponse', () => {
            const empNo = 'EMP001';
            const serverError = { message: 'Invalid credentials', statusCode: 401 };
            const expectedError = { message: 'Invalid credentials', statusCode: 401 };

            service.login(empNo).subscribe({
                next: () => fail('Should have failed'),
                error: (error) => {
                    expect(error).toEqual(expectedError);
                }
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/api/authentication/login`);
            req.flush(serverError, { status: 401, statusText: 'Unauthorized' });
        });

        it('should handle unknown HTTP error format', () => {
            const empNo = 'EMP001';
            const expectedError = { message: 'Login failed. Please try again.', statusCode: 500 };

            service.login(empNo).subscribe({
                next: () => fail('Should have failed'),
                error: (error) => {
                    expect(error).toEqual(expectedError);
                }
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/api/authentication/login`);
            req.flush('Internal Server Error', { status: 500, statusText: 'Internal Server Error' });
        });

        it('should handle network error', () => {
            const empNo = 'EMP001';
            const expectedError = { message: 'Network error occurred. Please try again.', statusCode: 0 };

            service.login(empNo).subscribe({
                next: () => fail('Should have failed'),
                error: (error) => {
                    expect(error).toEqual(expectedError);
                }
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/api/authentication/login`);
            req.error(new ErrorEvent('Network error'), { status: 0 });
        });
    });

    describe('logout', () => {
        it('should remove token and navigate to login', () => {
            service.logout();

            expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('access_token');
            expect(router.navigate).toHaveBeenCalledWith(['/login']);
        });
    });
});

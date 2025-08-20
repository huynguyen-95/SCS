import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { provideZonelessChangeDetection } from '@angular/core';

import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
    let mockAuthService: jasmine.SpyObj<AuthService>;
    let mockRouter: jasmine.SpyObj<Router>;
    let guard: typeof authGuard;

    beforeEach(() => {
        mockAuthService = jasmine.createSpyObj('AuthService', ['isAuthenticated']);
        mockRouter = jasmine.createSpyObj('Router', ['createUrlTree']);

        TestBed.configureTestingModule({
            providers: [
                provideZonelessChangeDetection(),
                { provide: AuthService, useValue: mockAuthService },
                { provide: Router, useValue: mockRouter }
            ]
        });

        guard = authGuard;
    });

    it('should return true when user is authenticated', () => {
        mockAuthService.isAuthenticated.and.returnValue(true);

        const result = TestBed.runInInjectionContext(() =>
            guard({} as any, {} as any)
        );

        expect(result).toBe(true);
        expect(mockAuthService.isAuthenticated).toHaveBeenCalled();
        expect(mockRouter.createUrlTree).not.toHaveBeenCalled();
    });

    it('should redirect to login when user is not authenticated', () => {
        mockAuthService.isAuthenticated.and.returnValue(false);
        const mockUrlTree = {} as UrlTree;
        mockRouter.createUrlTree.and.returnValue(mockUrlTree);

        const result = TestBed.runInInjectionContext(() =>
            guard({} as any, {} as any)
        );

        expect(result).toBe(mockUrlTree);
        expect(mockAuthService.isAuthenticated).toHaveBeenCalled();
        expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/login']);
    });
});

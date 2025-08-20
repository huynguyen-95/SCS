import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { UserService } from './users.service';
import { ApiService } from './api.service';
import { UserInfo } from '../../models/user-info.model';
import { of, throwError } from 'rxjs';

describe('UserService', () => {
    let service: UserService;
    let apiServiceSpy: jasmine.SpyObj<ApiService>;

    const mockUsers: UserInfo[] = [
        {
            empNo: 'EMP001',
            name: 'John Doe',
            isAdmin: true,
            email: 'john@example.com',
            role: 'admin'
        },
        {
            empNo: 'EMP002',
            name: 'Jane Smith',
            isAdmin: false,
            email: 'jane@example.com',
            role: 'user'
        }
    ];

    const mockUser: UserInfo = {
        empNo: 'EMP003',
        name: 'Bob Wilson',
        isAdmin: false,
        email: 'bob@example.com',
        role: 'user'
    };

    beforeEach(() => {
        const spy = jasmine.createSpyObj('ApiService', ['get', 'post']);

        TestBed.configureTestingModule({
            providers: [
                UserService,
                { provide: ApiService, useValue: spy },
                provideZonelessChangeDetection()
            ]
        });

        service = TestBed.inject(UserService);
        apiServiceSpy = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('getUsersListAsync', () => {
        it('should get users list', async () => {
            apiServiceSpy.get.and.returnValue(of(mockUsers));

            const result = await service.getUsersListAsync();

            expect(apiServiceSpy.get).toHaveBeenCalledWith('api/users');
            expect(result).toEqual(mockUsers);
        });

        it('should handle error when getting users list', async () => {
            const errorResponse = new Error('API Error');
            apiServiceSpy.get.and.returnValue(throwError(() => errorResponse));

            try {
                await service.getUsersListAsync();
                fail('Should have thrown an error');
            } catch (error) {
                expect(error).toBe(errorResponse);
                expect(apiServiceSpy.get).toHaveBeenCalledWith('api/users');
            }
        });
    });

    describe('createUserAsync', () => {
        it('should create a new user', async () => {
            const createdUser = { ...mockUser, id: 123 };
            apiServiceSpy.post.and.returnValue(of(createdUser));

            const result = await service.createUserAsync(mockUser);

            expect(apiServiceSpy.post).toHaveBeenCalledWith('api/users', mockUser);
            expect(result).toEqual(createdUser);
        });

        it('should handle error when creating user', async () => {
            const errorResponse = new Error('Creation failed');
            apiServiceSpy.post.and.returnValue(throwError(() => errorResponse));

            try {
                await service.createUserAsync(mockUser);
                fail('Should have thrown an error');
            } catch (error) {
                expect(error).toBe(errorResponse);
                expect(apiServiceSpy.post).toHaveBeenCalledWith('api/users', mockUser);
            }
        });
    });
});

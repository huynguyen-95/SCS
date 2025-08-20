import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UsersComponent } from './users.component';
import { UserService } from '../../core/services/users.service';
import { MessageService } from 'primeng/api';
import { provideZonelessChangeDetection } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { UserInfo } from '../../models/user-info.model';

describe('UsersComponent', () => {
  let component: UsersComponent;
  let fixture: ComponentFixture<UsersComponent>;
  let mockUserService: jasmine.SpyObj<UserService>;
  let mockMessageService: jasmine.SpyObj<MessageService>;

  const mockUsers: UserInfo[] = [
    { empNo: 'EMP001', name: 'John Doe', email: 'john@example.com', isAdmin: true, role: 'Admin' },
    { empNo: 'EMP002', name: 'Jane Smith', email: 'jane@example.com', isAdmin: false, role: 'User' }
  ];

  beforeEach(async () => {
    const userServiceSpy = jasmine.createSpyObj('UserService', ['getUsersListAsync', 'createUserAsync']);
    const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);

    await TestBed.configureTestingModule({
      imports: [
        UsersComponent,
        NoopAnimationsModule
      ],
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        { provide: UserService, useValue: userServiceSpy },
        { provide: MessageService, useValue: messageServiceSpy }
      ]
    }).compileComponents();

    mockUserService = TestBed.inject(UserService) as jasmine.SpyObj<UserService>;
    mockMessageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;

    mockUserService.getUsersListAsync.and.returnValue(Promise.resolve(mockUsers));

    fixture = TestBed.createComponent(UsersComponent);
    component = fixture.componentInstance;
    
    // Trigger ngOnInit and wait for it to complete
    fixture.detectChanges();
    await fixture.whenStable();
    
    // Reset call count after ngOnInit to track subsequent calls
    mockUserService.getUsersListAsync.calls.reset();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default values', () => {
      expect(component.users()).toEqual(mockUsers); // After ngOnInit, users should be loaded
      expect(component.checked).toBe(true);
      expect(component.showCreateModal).toBe(false);
      expect(component.newUser).toEqual({
        empNo: '',
        name: '',
        isAdmin: false
      });
    });

    it('should load users on init', async () => {
      await component.ngOnInit();

      expect(mockUserService.getUsersListAsync).toHaveBeenCalled();
      expect(component.users()).toEqual(mockUsers);
    });

    it('should handle error when loading users', async () => {
      mockUserService.getUsersListAsync.and.returnValue(Promise.reject(new Error('Load failed')));
      spyOn(console, 'error');

      await component.ngOnInit();

      expect(console.error).toHaveBeenCalledWith('Failed to load users:', jasmine.any(Error));
    });
  });

  describe('User Management', () => {
    it('should open create modal', () => {
      component.onCreateUser();

      expect(component.showCreateModal).toBe(true);
    });

    it('should create user successfully', async () => {
      component.newUser = { empNo: 'EMP003', name: 'Bob Wilson', isAdmin: true };
      mockUserService.createUserAsync.and.returnValue(Promise.resolve(component.newUser));

      await component.onSaveUser();

      expect(mockUserService.createUserAsync).toHaveBeenCalledWith({
        empNo: 'EMP003',
        name: 'Bob Wilson',
        isAdmin: true
      });
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'success',
        summary: 'Success',
        detail: 'User created successfully!'
      });
      expect(mockUserService.getUsersListAsync).toHaveBeenCalledTimes(1); // Called once after creation
    });

    it('should not create user with empty fields', async () => {
      component.newUser = { empNo: '', name: '', isAdmin: false };

      await component.onSaveUser();

      expect(mockUserService.createUserAsync).not.toHaveBeenCalled();
    });

    it('should not create user with missing empNo', async () => {
      component.newUser = { empNo: '', name: 'Bob Wilson', isAdmin: false };

      await component.onSaveUser();

      expect(mockUserService.createUserAsync).not.toHaveBeenCalled();
    });

    it('should not create user with missing name', async () => {
      component.newUser = { empNo: 'EMP003', name: '', isAdmin: false };

      await component.onSaveUser();

      expect(mockUserService.createUserAsync).not.toHaveBeenCalled();
    });

    it('should handle user creation error', async () => {
      component.newUser = { empNo: 'EMP003', name: 'Bob Wilson', isAdmin: true };
      mockUserService.createUserAsync.and.returnValue(Promise.reject(new Error('Creation failed')));
      spyOn(console, 'error');

      await component.onSaveUser();

      expect(console.error).toHaveBeenCalledWith('Failed to create user:', jasmine.any(Error));
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to create user.'
      });
    });
  });

  describe('Modal Management', () => {
    it('should close modal and reset form', () => {
      component.showCreateModal = true;
      component.newUser = { empNo: 'EMP003', name: 'Bob Wilson', isAdmin: true };

      component.closeModal();

      expect(component.showCreateModal).toBe(false);
      expect(component.newUser).toEqual({
        empNo: '',
        name: '',
        isAdmin: false
      });
    });

    it('should reset form independently', () => {
      component.newUser = { empNo: 'EMP003', name: 'Bob Wilson', isAdmin: true };

      component.resetForm();

      expect(component.newUser).toEqual({
        empNo: '',
        name: '',
        isAdmin: false
      });
    });
  });

  describe('Utility Methods', () => {
    it('should convert expiry timestamp to date', () => {
      const timestamp = Math.floor(Date.now() / 1000) + 3600; // 1 hour from now
      const result = component.getExpiryDate(timestamp);

      expect(result).toBeInstanceOf(Date);
      expect(result.getTime()).toBe(timestamp * 1000);
    });

    it('should return correct expiry class for soon expiring token', () => {
      const soonExpiry = Math.floor(Date.now() / 1000) + 3600; // 1 hour from now
      const result = component.getExpiryClass(soonExpiry);

      expect(result).toBe('expiry-soon');
    });

    it('should return correct expiry class for warning expiry', () => {
      const warningExpiry = Math.floor(Date.now() / 1000) + 36 * 3600; // 36 hours from now
      const result = component.getExpiryClass(warningExpiry);

      expect(result).toBe('expiry-warning');
    });

    it('should return correct expiry class for normal expiry', () => {
      const normalExpiry = Math.floor(Date.now() / 1000) + 72 * 3600; // 72 hours from now
      const result = component.getExpiryClass(normalExpiry);

      expect(result).toBe('expiry-normal');
    });

    it('should handle edge case for exactly 24 hours', () => {
      const exactlyOneDayExpiry = Math.floor(Date.now() / 1000) + 24 * 3600;
      const result = component.getExpiryClass(exactlyOneDayExpiry);

      expect(result).toBe('expiry-warning');
    });

    it('should handle edge case for exactly 48 hours', () => {
      const exactlyTwoDaysExpiry = Math.floor(Date.now() / 1000) + 48 * 3600;
      const result = component.getExpiryClass(exactlyTwoDaysExpiry);

      expect(result).toBe('expiry-normal');
    });
  });

  describe('Form Validation', () => {
    it('should validate required fields before saving', async () => {
      // Test all empty
      component.newUser = { empNo: '', name: '', isAdmin: false };
      await component.onSaveUser();
      expect(mockUserService.createUserAsync).not.toHaveBeenCalled();

      // Test empty empNo
      component.newUser = { empNo: '', name: 'Test User', isAdmin: false };
      await component.onSaveUser();
      expect(mockUserService.createUserAsync).not.toHaveBeenCalled();

      // Test empty name
      component.newUser = { empNo: 'EMP123', name: '', isAdmin: false };
      await component.onSaveUser();
      expect(mockUserService.createUserAsync).not.toHaveBeenCalled();

      // Test valid data
      component.newUser = { empNo: 'EMP123', name: 'Test User', isAdmin: false };
      mockUserService.createUserAsync.and.returnValue(Promise.resolve(component.newUser));
      await component.onSaveUser();
      expect(mockUserService.createUserAsync).toHaveBeenCalled();
    });
  });

  describe('Data Loading', () => {
    it('should refresh user list after successful creation', async () => {
      component.newUser = { empNo: 'EMP003', name: 'Bob Wilson', isAdmin: true };
      mockUserService.createUserAsync.and.returnValue(Promise.resolve(component.newUser));

      await component.onSaveUser();

      expect(mockUserService.getUsersListAsync).toHaveBeenCalledTimes(1); // Called once after creation
      expect(component.showCreateModal).toBe(false);
    });

    it('should maintain user list on creation error', async () => {
      const initialUsers = component.users();
      component.newUser = { empNo: 'EMP003', name: 'Bob Wilson', isAdmin: true };
      component.showCreateModal = true; // Set modal to be open initially
      mockUserService.createUserAsync.and.returnValue(Promise.reject(new Error('Creation failed')));

      await component.onSaveUser();

      expect(component.users()).toBe(initialUsers);
      expect(component.showCreateModal).toBe(true); // Modal should remain open
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to create user.'
      });
    });
  });
});

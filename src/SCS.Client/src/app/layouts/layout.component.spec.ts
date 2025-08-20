import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LayoutComponent } from './layout.component';
import { AuthService } from '../core/services/auth.service';
import { PremiseService } from '../core/services/premise.service';
import { MessageService } from 'primeng/api';
import { RouterTestingModule } from '@angular/router/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { of, throwError } from 'rxjs';
import { UserInfo } from '../models/user-info.model';
import { Premise } from '../models/premise.model';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('LayoutComponent', () => {
    let component: LayoutComponent;
    let fixture: ComponentFixture<LayoutComponent>;
    let mockAuthService: jasmine.SpyObj<AuthService>;
    let mockPremiseService: jasmine.SpyObj<PremiseService>;
    let mockMessageService: jasmine.SpyObj<MessageService>;

    const mockUser: UserInfo = {
        empNo: 'EMP001',
        name: 'John Doe',
        email: 'john.doe@example.com',
        isAdmin: true,
        role: 'Admin'
    };

    const mockPremises: Premise[] = [
        { id: 1, name: 'Building A' },
        { id: 2, name: 'Building B' }
    ];

    beforeEach(async () => {
        const authServiceSpy = jasmine.createSpyObj('AuthService', ['getUserInfo', 'logout']);
        const premiseServiceSpy = jasmine.createSpyObj('PremiseService', ['getPremiseList']);
        const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);

        await TestBed.configureTestingModule({
            imports: [
                LayoutComponent,
                RouterTestingModule,
                NoopAnimationsModule
            ],
            providers: [
                provideZonelessChangeDetection(),
                { provide: AuthService, useValue: authServiceSpy },
                { provide: PremiseService, useValue: premiseServiceSpy },
                { provide: MessageService, useValue: messageServiceSpy }
            ]
        }).compileComponents();

        mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
        mockPremiseService = TestBed.inject(PremiseService) as jasmine.SpyObj<PremiseService>;
        mockMessageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;

        // Setup default mocks
        mockAuthService.getUserInfo.and.returnValue(mockUser);
        mockPremiseService.getPremiseList.and.returnValue(of(mockPremises));

        fixture = TestBed.createComponent(LayoutComponent);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    describe('Component Initialization', () => {
        it('should initialize with default values', () => {
            expect(component.sidebarVisible).toBe(true);
            expect(component.expandedMenus).toEqual({});
            expect(component.premises).toEqual([]);
        });

        it('should get user info from auth service on construction', () => {
            expect(mockAuthService.getUserInfo).toHaveBeenCalled();
            expect(component.user).toEqual(mockUser);
        });

        it('should handle null user from auth service', () => {
            mockAuthService.getUserInfo.and.returnValue(null);
            const newComponent = new LayoutComponent(mockAuthService, mockPremiseService);
            expect(newComponent.user).toBeNull();
        });

        it('should initialize menu items with logout command', () => {
            expect(component.items).toHaveSize(1);
            expect(component.items[0].label).toBe('Logout');
            expect(component.items[0].icon).toBe('pi pi-sign-out');
            expect(component.items[0].command).toBeDefined();
        });
    });

    describe('ngOnInit', () => {
        it('should fetch premises successfully', () => {
            component.ngOnInit();

            expect(mockPremiseService.getPremiseList).toHaveBeenCalled();
            expect(component.premises).toEqual(mockPremises);
        });

        it('should handle error when fetching premises', () => {
            const errorMessage = 'Failed to fetch premises';
            spyOn(console, 'error');
            mockPremiseService.getPremiseList.and.returnValue(throwError(() => new Error(errorMessage)));

            component.ngOnInit();

            expect(mockPremiseService.getPremiseList).toHaveBeenCalled();
            expect(console.error).toHaveBeenCalledWith('Error fetching premises:', jasmine.any(Error));
            expect(component.premises).toEqual([]);
        });

        it('should handle empty premises list', () => {
            mockPremiseService.getPremiseList.and.returnValue(of([]));

            component.ngOnInit();

            expect(component.premises).toEqual([]);
        });
    });

    describe('Sidebar functionality', () => {
        it('should toggle sidebar visibility', () => {
            expect(component.sidebarVisible).toBe(true);

            component.toggleSidebar();
            expect(component.sidebarVisible).toBe(false);

            component.toggleSidebar();
            expect(component.sidebarVisible).toBe(true);
        });

        it('should toggle submenu expansion', () => {
            const menuKey = 'premise';
            expect(component.isSubmenuExpanded(menuKey)).toBe(false);

            component.toggleSubmenu(menuKey);
            expect(component.isSubmenuExpanded(menuKey)).toBe(true);

            component.toggleSubmenu(menuKey);
            expect(component.isSubmenuExpanded(menuKey)).toBe(false);
        });

        it('should handle multiple submenu toggles', () => {
            const premiseKey = 'premise';
            const adminKey = 'admin';

            component.toggleSubmenu(premiseKey);
            component.toggleSubmenu(adminKey);

            expect(component.isSubmenuExpanded(premiseKey)).toBe(true);
            expect(component.isSubmenuExpanded(adminKey)).toBe(true);

            component.toggleSubmenu(premiseKey);
            expect(component.isSubmenuExpanded(premiseKey)).toBe(false);
            expect(component.isSubmenuExpanded(adminKey)).toBe(true);
        });

        it('should return false for non-existent menu key', () => {
            expect(component.isSubmenuExpanded('nonexistent')).toBe(false);
        });
    });

    describe('Logout functionality', () => {
        it('should call auth service logout when logout command is executed', () => {
            const logoutCommand = component.items[0].command;

            if (logoutCommand) {
                logoutCommand({});
            }

            expect(mockAuthService.logout).toHaveBeenCalled();
        });
    });

    describe('Component properties', () => {
        it('should have correct user information', () => {
            expect(component.user?.name).toBe('John Doe');
            expect(component.user?.isAdmin).toBe(true);
            expect(component.user?.role).toBe('Admin');
        });

        it('should initialize premises array correctly', () => {
            component.ngOnInit();
            expect(component.premises.length).toBe(2);
            expect(component.premises[0].name).toBe('Building A');
            expect(component.premises[1].name).toBe('Building B');
        });

        it('should have menu items configured', () => {
            expect(component.items.length).toBe(1);
            expect(component.items[0].label).toBe('Logout');
        });
    });

    describe('Error handling', () => {
        it('should handle premise service errors gracefully', () => {
            spyOn(console, 'error');
            mockPremiseService.getPremiseList.and.returnValue(throwError(() => new Error('Network error')));

            component.ngOnInit();

            expect(console.error).toHaveBeenCalled();
            expect(component.premises).toEqual([]);
        });

        it('should handle null premise response', () => {
            mockPremiseService.getPremiseList.and.returnValue(of(null as any));

            component.ngOnInit();

            expect(component.premises).toBeNull();
        });
    });

    describe('Menu state management', () => {
        it('should manage multiple menu states independently', () => {
            // Test premise menu
            component.toggleSubmenu('premise');
            expect(component.expandedMenus['premise']).toBe(true);

            // Test admin menu
            component.toggleSubmenu('admin');
            expect(component.expandedMenus['admin']).toBe(true);
            expect(component.expandedMenus['premise']).toBe(true);

            // Close premise menu
            component.toggleSubmenu('premise');
            expect(component.expandedMenus['premise']).toBe(false);
            expect(component.expandedMenus['admin']).toBe(true);
        });

        it('should return correct expansion state', () => {
            expect(component.isSubmenuExpanded('premise')).toBe(false);

            component.expandedMenus['premise'] = true;
            expect(component.isSubmenuExpanded('premise')).toBe(true);

            component.expandedMenus['premise'] = false;
            expect(component.isSubmenuExpanded('premise')).toBe(false);
        });
    });
});

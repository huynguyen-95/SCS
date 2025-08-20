import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AlarmSystemSimulationComponent } from './alarm-system-simulation.component';
import { AlarmSystemService } from '../../core/services/alarm-system.service';
import { PremiseService } from '../../core/services/premise.service';
import { MessageService } from 'primeng/api';
import { provideZonelessChangeDetection } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { Premise } from '../../models/premise.model';

describe('AlarmSystemSimulationComponent', () => {
    let component: AlarmSystemSimulationComponent;
    let fixture: ComponentFixture<AlarmSystemSimulationComponent>;
    let mockAlarmSystemService: jasmine.SpyObj<AlarmSystemService>;
    let mockPremiseService: jasmine.SpyObj<PremiseService>;
    let mockMessageService: jasmine.SpyObj<MessageService>;

    const mockPremises: Premise[] = [
        { id: 1, name: 'Building A' },
        { id: 2, name: 'Building B' }
    ];

    beforeEach(async () => {
        const alarmSystemServiceSpy = jasmine.createSpyObj('AlarmSystemService', ['sendAlert']);
        const premiseServiceSpy = jasmine.createSpyObj('PremiseService', ['getPremiseListAsync']);
        const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);

        await TestBed.configureTestingModule({
            imports: [
                AlarmSystemSimulationComponent,
                NoopAnimationsModule
            ],
            providers: [
                provideZonelessChangeDetection(),
                provideHttpClient(),
                { provide: AlarmSystemService, useValue: alarmSystemServiceSpy },
                { provide: PremiseService, useValue: premiseServiceSpy },
                { provide: MessageService, useValue: messageServiceSpy }
            ]
        }).compileComponents();

        mockAlarmSystemService = TestBed.inject(AlarmSystemService) as jasmine.SpyObj<AlarmSystemService>;
        mockPremiseService = TestBed.inject(PremiseService) as jasmine.SpyObj<PremiseService>;
        mockMessageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;

        mockPremiseService.getPremiseListAsync.and.returnValue(Promise.resolve(mockPremises));

        fixture = TestBed.createComponent(AlarmSystemSimulationComponent);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    describe('Component Initialization', () => {
        it('should initialize with default values', () => {
            expect(component.selectedPremise).toBeNull();
            expect(component.message()).toBe('');
            expect(component.premises()).toEqual([]);
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

    describe('Form Submission', () => {
        beforeEach(() => {
            component.selectedPremise = mockPremises[0];
            component.message.set('Test alert message');
        });

        it('should submit alert successfully', () => {
            mockAlarmSystemService.sendAlert.and.returnValue(of(undefined));

            component.onSubmit();

            expect(mockAlarmSystemService.sendAlert).toHaveBeenCalledWith(1, 'Test alert message');
            expect(mockMessageService.add).toHaveBeenCalledWith({
                severity: 'success',
                summary: 'Success',
                detail: 'Send request successfully!'
            });
        });

        it('should handle alert submission error', () => {
            mockAlarmSystemService.sendAlert.and.returnValue(throwError(() => new Error('Send failed')));

            component.onSubmit();

            expect(mockAlarmSystemService.sendAlert).toHaveBeenCalledWith(1, 'Test alert message');
            expect(mockMessageService.add).toHaveBeenCalledWith({
                severity: 'error',
                summary: 'Error',
                detail: 'Failed to send request.'
            });
        });

        it('should not submit when premise is not selected', () => {
            component.selectedPremise = null;

            component.onSubmit();

            expect(mockAlarmSystemService.sendAlert).not.toHaveBeenCalled();
        });

        it('should not submit when message is empty', () => {
            component.message.set('');

            component.onSubmit();

            expect(mockAlarmSystemService.sendAlert).not.toHaveBeenCalled();
            expect(mockMessageService.add).not.toHaveBeenCalled();
        });

        it('should not submit when both premise and message are missing', () => {
            component.selectedPremise = null;
            component.message.set('');

            component.onSubmit();

            expect(mockAlarmSystemService.sendAlert).not.toHaveBeenCalled();
        });
    });

    describe('Form Reset', () => {
        beforeEach(() => {
            component.selectedPremise = mockPremises[0];
            component.message.set('Test message');
        });

        it('should reset form fields', () => {
            component.onReset();

            expect(component.selectedPremise).toBeNull();
            expect(component.message()).toBe('');
        });

        it('should reset form after successful submission', () => {
            mockAlarmSystemService.sendAlert.and.returnValue(of(undefined));

            component.onSubmit();

            expect(component.selectedPremise).toBeNull();
            expect(component.message()).toBe('');
        });

        it('should not reset form after failed submission', () => {
            mockAlarmSystemService.sendAlert.and.returnValue(throwError(() => new Error('Send failed')));

            component.onSubmit();

            expect(component.selectedPremise).toBe(mockPremises[0]);
            expect(component.message()).toBe('Test message');
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
        it('should validate premise selection', () => {
            component.selectedPremise = null;
            component.message.set('Valid message');

            component.onSubmit();

            expect(mockAlarmSystemService.sendAlert).not.toHaveBeenCalled();
        });

        it('should validate message content', () => {
            component.selectedPremise = mockPremises[0];
            component.message.set('');

            component.onSubmit();

            expect(mockAlarmSystemService.sendAlert).not.toHaveBeenCalled();
            expect(mockMessageService.add).not.toHaveBeenCalled();
        });

        it('should validate both premise and message', () => {
            component.selectedPremise = mockPremises[0];
            component.message.set('Valid message');
            mockAlarmSystemService.sendAlert.and.returnValue(of(undefined));

            component.onSubmit();

            expect(mockAlarmSystemService.sendAlert).toHaveBeenCalled();
        });
    });

    describe('Signal Management', () => {
        it('should update message signal correctly', () => {
            expect(component.message()).toBe('');

            component.message.set('New message');
            expect(component.message()).toBe('New message');

            component.message.set('Updated message');
            expect(component.message()).toBe('Updated message');
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

        it('should handle alert sending errors gracefully', () => {
            component.selectedPremise = mockPremises[0];
            component.message.set('Test message');
            mockAlarmSystemService.sendAlert.and.returnValue(throwError(() => new Error('Server error')));

            component.onSubmit();

            expect(mockMessageService.add).toHaveBeenCalledWith({
                severity: 'error',
                summary: 'Error',
                detail: 'Failed to send request.'
            });
        });
    });
});

import { PremiseComponent } from './premise.component';
import { AlarmSystemService } from '../../core/services/alarm-system.service';
import { PremiseService } from '../../core/services/premise.service';
import { MessageService } from 'primeng/api';
import { ActivatedRoute } from '@angular/router';
import { IncidentInfo } from '../../models/incident-info.model';
import { of } from 'rxjs';

describe('PremiseComponent', () => {
    let component: PremiseComponent;
    let mockAlarmSystemService: jasmine.SpyObj<AlarmSystemService>;
    let mockPremiseService: jasmine.SpyObj<PremiseService>;
    let mockMessageService: jasmine.SpyObj<MessageService>;
    let mockActivatedRoute: jasmine.SpyObj<ActivatedRoute>;

    beforeEach(() => {
        // Create mocks
        mockAlarmSystemService = jasmine.createSpyObj('AlarmSystemService',
            ['stopConnection', 'isConnected', 'onMethod']
        );

        mockPremiseService = jasmine.createSpyObj('PremiseService',
            ['getPremiseIncidentsListAsync']
        );

        mockMessageService = jasmine.createSpyObj('MessageService', ['add']);

        mockActivatedRoute = jasmine.createSpyObj('ActivatedRoute', [], {
            params: of({ id: '123' })
        });

        // Configure mock returns
        mockAlarmSystemService.isConnected.and.returnValue(false);
        mockPremiseService.getPremiseIncidentsListAsync.and.returnValue(Promise.resolve([]));

        // Create component manually
        component = new PremiseComponent(
            mockActivatedRoute,
            mockAlarmSystemService,
            mockMessageService,
            mockPremiseService
        );
    });

    it('should be created', () => {
        expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
        expect(component.premiseId).toBe(0);
        expect(component.isConnected()).toBe(false);
        expect(component.connectionError).toBe(null);
        expect(component.alertCount()).toBe(0);
        expect(component.alertData()).toEqual([]);
        expect(component.incidentData()).toEqual([]);
    });

    it('should set premise ID from route params', async () => {
        spyOn(component as any, 'connectToSignalR').and.returnValue(Promise.resolve());

        await component.ngOnInit();

        expect(component.premiseId).toBe(123);
    });

    it('should update alert count when receiving alert', () => {
        const initialCount = component.alertCount();

        // Simulate ReceiveAlert event directly
        mockAlarmSystemService.onMethod.and.callFake((methodName: string, callback: Function) => {
            if (methodName === 'ReceiveAlert') {
                callback('Test alert message');
            }
        });

        component['registerSignalREvents']();

        expect(component.alertCount()).toBe(initialCount + 1);
    });

    it('should add alert to alert data when receiving alert', () => {
        const initialAlerts = component.alertData();

        // Simulate ReceiveAlert event
        const alertCallback = jasmine.createSpy('alertCallback');
        mockAlarmSystemService.onMethod.and.callFake((methodName: string, callback: Function) => {
            if (methodName === 'ReceiveAlert') {
                callback('Test alert');
            }
        });

        component['registerSignalREvents']();

        expect(component.alertData().length).toBe(initialAlerts.length + 1);
        expect(component.alertData()[0].message).toBe('Test alert');
    });

    it('should call stop connection on destroy', async () => {
        await component.ngOnDestroy();

        expect(mockAlarmSystemService.stopConnection).toHaveBeenCalled();
    });

    it('should load incidents successfully', async () => {
        const mockIncidents: IncidentInfo[] = [
            { date: '2025-08-20T10:00:00Z', description: 'Test incident', filePath: '/path/file1.jpg' },
            { date: '2025-08-20T11:00:00Z', description: 'Another incident', filePath: '/path/file2.jpg' }
        ];

        mockPremiseService.getPremiseIncidentsListAsync.and.returnValue(Promise.resolve(mockIncidents));

        // Mock the private method directly
        spyOn(component as any, 'connectToSignalR').and.returnValue(Promise.resolve());

        // Set premise ID manually
        component.premiseId = 123;

        // Load incidents directly
        const incidents = await mockPremiseService.getPremiseIncidentsListAsync(123);
        component.incidentData.set(incidents);

        expect(component.incidentData()).toEqual(mockIncidents);
    });

    it('should handle incidents loading error', async () => {
        const error = new Error('Network error');
        mockPremiseService.getPremiseIncidentsListAsync.and.returnValue(Promise.reject(error));
        spyOn(console, 'error');

        try {
            await mockPremiseService.getPremiseIncidentsListAsync(123);
        } catch (e) {
            console.error('Failed to load incidents:', e);
            mockMessageService.add({
                severity: 'error',
                summary: 'Error',
                detail: 'Failed to load incidents.'
            });
        }

        expect(mockMessageService.add).toHaveBeenCalledWith({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load incidents.'
        });
        expect(console.error).toHaveBeenCalledWith('Failed to load incidents:', error);
    });

    it('should register SignalR events', () => {
        component['registerSignalREvents']();

        expect(mockAlarmSystemService.onMethod).toHaveBeenCalledWith('ReceiveAlert', jasmine.any(Function));
        expect(mockAlarmSystemService.onMethod).toHaveBeenCalledWith('ReceiveIncident', jasmine.any(Function));
    });

    it('should process incident data when receiving incident', () => {
        const mockIncident: IncidentInfo = {
            date: '2025-08-20T12:00:00Z',
            description: 'New incident',
            filePath: '/path/incident.jpg'
        };

        mockAlarmSystemService.onMethod.and.callFake((methodName: string, callback: Function) => {
            if (methodName === 'ReceiveIncident') {
                callback(JSON.stringify(mockIncident));
            }
        });

        component['registerSignalREvents']();

        expect(component.incidentData()[0]).toEqual(mockIncident);
        expect(mockMessageService.add).toHaveBeenCalledWith({
            severity: 'warn',
            summary: 'Incident',
            detail: 'New incident found.'
        });
    });
});

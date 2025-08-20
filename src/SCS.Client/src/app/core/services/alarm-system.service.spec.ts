import { AlarmSystemService } from './alarm-system.service';
import { ApiService } from './api.service';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { of } from 'rxjs';

describe('AlarmSystemService', () => {
    let service: AlarmSystemService;
    let mockApiService: jasmine.SpyObj<ApiService>;
    let mockHubConnection: jasmine.SpyObj<HubConnection>;
    let mockHubConnectionBuilder: jasmine.SpyObj<HubConnectionBuilder>;

    beforeEach(() => {
        // Create mocks
        mockApiService = jasmine.createSpyObj('ApiService', ['post']);
        mockApiService.post.and.returnValue(of({}));

        mockHubConnection = jasmine.createSpyObj('HubConnection',
            ['start', 'stop', 'invoke', 'on', 'off'],
            { state: HubConnectionState.Disconnected }
        );

        // Configure off method to accept any arguments
        mockHubConnection.off.and.callFake(() => { });

        mockHubConnectionBuilder = jasmine.createSpyObj('HubConnectionBuilder',
            ['withUrl', 'withAutomaticReconnect', 'build']
        );

        // Chain methods for builder pattern
        mockHubConnectionBuilder.withUrl.and.returnValue(mockHubConnectionBuilder);
        mockHubConnectionBuilder.withAutomaticReconnect.and.returnValue(mockHubConnectionBuilder);
        mockHubConnectionBuilder.build.and.returnValue(mockHubConnection);

        // Mock global HubConnectionBuilder
        (window as any).HubConnectionBuilder = jasmine.createSpy().and.returnValue(mockHubConnectionBuilder);

        // Create service manually
        service = new AlarmSystemService(mockApiService);
    });

    afterEach(() => {
        delete (window as any).HubConnectionBuilder;
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should stop connection successfully', async () => {
        // Set up the service with a connection first
        service['hubConnection'] = mockHubConnection;
        mockHubConnection.stop.and.returnValue(Promise.resolve());

        await service.stopConnection();

        expect(mockHubConnection.stop).toHaveBeenCalled();
    });

    it('should invoke method successfully', async () => {
        // Set up the service with a connection first
        service['hubConnection'] = mockHubConnection;
        mockHubConnection.invoke.and.returnValue(Promise.resolve('result'));

        const result = await service.invokeMethod('TestMethod', 'arg1', 'arg2');

        expect(mockHubConnection.invoke).toHaveBeenCalledWith('TestMethod', 'arg1', 'arg2');
        expect(result).toBe('result');
    });

    it('should register event handler', () => {
        // Set up the service with a connection first
        service['hubConnection'] = mockHubConnection;
        const callback = jasmine.createSpy('callback');

        service.onMethod('TestEvent', callback);

        expect(mockHubConnection.on).toHaveBeenCalledWith('TestEvent', callback);
    });

    it('should unregister event handler', () => {
        // Set up the service with a connection first
        service['hubConnection'] = mockHubConnection;

        service.offMethod('TestEvent');

        expect(mockHubConnection.off).toHaveBeenCalled();
    });

    it('should return false when no connection', () => {
        service['hubConnection'] = null;

        expect(service.isConnected()).toBe(false);
    });

    it('should get default state when no connection', () => {
        service['hubConnection'] = null;

        expect(service.getConnectionState()).toBe('Disconnected');
    });

    it('should send alert through API', () => {
        const alertData = { message: 'Test alert' };

        service.sendAlert(123, alertData);

        expect(mockApiService.post).toHaveBeenCalledWith('api/alarm-system/simulate-alert', { premiseId: 123, message: alertData });
    });
});

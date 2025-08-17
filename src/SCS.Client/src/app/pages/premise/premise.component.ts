import { Component, OnInit, OnDestroy, WritableSignal, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AlarmSystemService } from '../../core/services/alarm-system.service';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { ChipModule } from 'primeng/chip';
import { OverlayBadgeModule } from 'primeng/overlaybadge';
import { StreamingPlayerModule } from '../../components/streaming-player/streaming-player-module';
import { TabsModule } from 'primeng/tabs';
import { AlertDataTable } from '../../components/alert-data-table/alert-data-table';
import { DispathSecurityGuard } from "../../components/dispath-security-guard/dispath-security-guard";
import { IncidentDataTable } from "../../components/incident-data-table/incident-data-table";
import { IncidentInfo } from '../../models/incident-info.model';
import { PremiseService } from '../../core/services/premise.service';

@Component({
    selector: 'app-premise',
    templateUrl: './premise.component.html',
    styleUrls: ['./premise.component.scss'],
    standalone: true,
    imports: [
        CommonModule,
        ToastModule,
        ChipModule,
        OverlayBadgeModule,
        TabsModule,
        StreamingPlayerModule,
        AlertDataTable,
        DispathSecurityGuard,
        IncidentDataTable
    ],
})
export class PremiseComponent implements OnInit, OnDestroy {
    premiseId: number = 0;
    isConnected: WritableSignal<boolean> = signal(false);
    connectionError: string | null = null;
    alertCount: WritableSignal<number> = signal(0);
    alertData: WritableSignal<AlertInfo[]> = signal([]);
    incidentData: WritableSignal<IncidentInfo[]> = signal([]);

    constructor(
        private route: ActivatedRoute,
        private alarmSystemService: AlarmSystemService,
        private messageService: MessageService,
        private premiseService: PremiseService
    ) { }

    async ngOnInit() {
        this.route.params.subscribe(async params => {
            this.premiseId = +params['id']; // Convert string to number using '+'

            if (this.premiseId) {
                await this.connectToSignalR();

                // Load initial data
                try {
                    const incidents = await this.premiseService.getPremiseIncidentsListAsync(this.premiseId);
                    this.incidentData.set(incidents);
                } catch (error) {
                    console.error('Failed to load incidents:', error);
                    this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load incidents.' });
                }
            }
        });
    }

    async ngOnDestroy() {
        // Clean up SignalR connection when component is destroyed
        await this.alarmSystemService.stopConnection();
    }

    private async connectToSignalR(): Promise<void> {
        try {
            this.connectionError = null;

            // Start SignalR connection with premise ID as groupId
            await this.alarmSystemService.startConnection(this.premiseId);
            this.isConnected.set(this.alarmSystemService.isConnected());

            // Register for alarm system events
            this.registerSignalREvents();

            console.log(`Connected to SignalR hub for premise ${this.premiseId}`);
        } catch (error) {
            console.error('Failed to connect to SignalR:', error);
            this.connectionError = 'Failed to connect to real-time updates';
            this.isConnected.set(false);
        }
    }

    private registerSignalREvents(): void {
        // Example event handlers - adjust based on your hub's methods
        this.alarmSystemService.onMethod('ReceiveAlert', (alarmData: string) => {
            this.messageService.add({ severity: 'error', summary: 'Alarm System', detail: alarmData });
            this.alertCount.update(count => count + 1);

            const newAlert: AlertInfo = {
                timestamp: Date.now(),
                message: alarmData,
            };
            this.alertData.update(data => [...data, newAlert]);
        });

        this.alarmSystemService.onMethod('ReceiveIncident', (incidentString: string) => {
            this.messageService.add({ severity: 'warn', summary: 'Incident', detail: 'New incident found.' });
            const incidentData: IncidentInfo = JSON.parse(incidentString);

            this.incidentData.update(data => [incidentData, ...data]);
        });
    }
}

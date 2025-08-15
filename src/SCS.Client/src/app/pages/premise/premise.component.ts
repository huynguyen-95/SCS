import { Component, OnInit, OnDestroy, WritableSignal, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AlarmSystemService } from '../../core/services/alarm-system.service';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { ChipModule } from 'primeng/chip';

@Component({
    selector: 'app-premise',
    templateUrl: './premise.component.html',
    styleUrls: ['./premise.component.scss'],
    standalone: true,
    imports: [CommonModule, ToastModule, ChipModule],
})
export class PremiseComponent implements OnInit, OnDestroy {
    premiseId: number = 0;
    isConnected: WritableSignal<boolean> = signal(false);
    connectionError: string | null = null;

    constructor(
        private route: ActivatedRoute,
        private alarmSystemService: AlarmSystemService,
        private messageService: MessageService
    ) { }

    async ngOnInit() {
        this.route.params.subscribe(async params => {
            this.premiseId = +params['id']; // Convert string to number using '+'

            if (this.premiseId) {
                await this.connectToSignalR();
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
        this.alarmSystemService.onMethod('ReceiveAlert', (alarmData: any) => {
            this.messageService.add({ severity: 'error', summary: 'Alarm System', detail: alarmData });
        });
    }
}

import { Component, signal, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AlarmSystemService } from '../../core/services/alarm-system.service';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-alarm-system-simulation',
    templateUrl: './alarm-system-simulation.component.html',
    styleUrls: ['./alarm-system-simulation.component.scss'],
    standalone: true,
    imports: [CommonModule, FormsModule],
})
export class AlarmSystemSimulationComponent {
    premiseId: WritableSignal<number> = signal(0);
    message: WritableSignal<string> = signal('');

    constructor(
        private alarmSystemService: AlarmSystemService,
        private messageService: MessageService) { }

    onSubmit(): void {
        if (!this.premiseId || !this.message) {
            return;
        }

        this.alarmSystemService.sendAlert(this.premiseId(), this.message()).subscribe(
            () => {
                this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Send request successfully!' });
                this.onReset();
            },
            (_) => {
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to send request.' });
            }
        );
    }

    onReset(): void {
        this.premiseId.set(0);
        this.message.set('');
    }
}

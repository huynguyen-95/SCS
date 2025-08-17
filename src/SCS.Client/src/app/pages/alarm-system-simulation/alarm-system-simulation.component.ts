import { Component, signal, WritableSignal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AlarmSystemService } from '../../core/services/alarm-system.service';
import { PremiseService } from '../../core/services/premise.service';
import { MessageService } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { Premise } from '../../models/premise.model';

@Component({
    selector: 'app-alarm-system-simulation',
    templateUrl: './alarm-system-simulation.component.html',
    styleUrls: ['./alarm-system-simulation.component.scss'],
    standalone: true,
    imports: [CommonModule, FormsModule, SelectModule],
})
export class AlarmSystemSimulationComponent implements OnInit {
    selectedPremise: Premise | null = null;
    message: WritableSignal<string> = signal('');
    premises: WritableSignal<Premise[]> = signal([]);

    constructor(
        private alarmSystemService: AlarmSystemService,
        private premiseService: PremiseService,
        private messageService: MessageService) { }

    async ngOnInit() {
        await this.loadPremises();
    }

    private async loadPremises(): Promise<void> {
        try {
            const premises = await this.premiseService.getPremiseListAsync();
            this.premises.set(premises);
        } catch (error) {
            console.error('Failed to load premises:', error);
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load premises.' });
        }
    }

    onSubmit(): void {
        if (!this.selectedPremise || !this.message) {
            return;
        }

        this.alarmSystemService.sendAlert(this.selectedPremise.id, this.message()).subscribe(
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
        this.selectedPremise = null;
        this.message.set('');
    }
}

import { Component, signal, WritableSignal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { SecurityGuardService } from '../../core/services/security-guard.service';
import { PremiseService } from '../../core/services/premise.service';
import { Premise } from '../../models/premise.model';

@Component({
    selector: 'app-capture-incident-simulation',
    templateUrl: './capture-incident-simulation.component.html',
    styleUrls: ['./capture-incident-simulation.component.scss'],
    standalone: true,
    imports: [CommonModule, FormsModule, SelectModule],
})
export class CaptureIncidentSimulationComponent implements OnInit {
    selectedFile: File | null = null;
    description: WritableSignal<string> = signal('');
    imagePreview: string | null = null;
    premises: WritableSignal<Premise[]> = signal([]);
    selectedPremise: Premise | null = null;

    constructor(
        private messageService: MessageService,
        private securityGuardService: SecurityGuardService,
        private premiseService: PremiseService
    ) { }

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

    onFileSelected(event: any): void {
        const file = event.target.files[0];
        if (file) {
            this.selectedFile = file;

            // Create image preview
            const reader = new FileReader();
            reader.onload = (e: any) => {
                this.imagePreview = e.target.result;
            };
            reader.readAsDataURL(file);
        }
    }

    onSubmit(): void {
        if (!this.selectedFile || !this.description() || !this.selectedPremise) {
            this.messageService.add({
                severity: 'warn',
                summary: 'Warning',
                detail: 'Please select a premise, image and provide a description.'
            });
            return;
        }

        this.securityGuardService.uploadIncidentReport(this.selectedPremise.id, this.selectedFile, this.description()).subscribe({
            next: (response) => {
                console.log('Upload response:', response);
                this.messageService.add({
                    severity: 'success',
                    summary: 'Success',
                    detail: 'Incident captured successfully!'
                });
                this.onReset();
            },
            error: (error) => {
                console.error('Upload error:', error);
                this.messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Failed to upload incident report.'
                });
            }
        });
    }

    onReset(): void {
        this.selectedFile = null;
        this.description.set('');
        this.imagePreview = null;
        this.selectedPremise = null;

        // Reset file input
        const fileInput = document.getElementById('imageUpload') as HTMLInputElement;
        if (fileInput) {
            fileInput.value = '';
        }
    }
}

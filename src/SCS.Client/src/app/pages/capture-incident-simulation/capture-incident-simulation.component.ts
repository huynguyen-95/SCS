import { Component, signal, WritableSignal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-capture-incident-simulation',
    templateUrl: './capture-incident-simulation.component.html',
    styleUrls: ['./capture-incident-simulation.component.scss'],
    standalone: true,
    imports: [CommonModule, FormsModule],
})
export class CaptureIncidentSimulationComponent implements OnInit {
    selectedFile: File | null = null;
    description: WritableSignal<string> = signal('');
    imagePreview: string | null = null;

    constructor(private messageService: MessageService) { }

    ngOnInit() {
        // Component initialization
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
        if (!this.selectedFile || !this.description()) {
            this.messageService.add({
                severity: 'warn',
                summary: 'Warning',
                detail: 'Please select an image and provide a description.'
            });
            return;
        }

        // TODO: Implement incident capture logic
        console.log('File:', this.selectedFile);
        console.log('Description:', this.description());

        this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Incident captured successfully!'
        });

        this.onReset();
    }

    onReset(): void {
        this.selectedFile = null;
        this.description.set('');
        this.imagePreview = null;

        // Reset file input
        const fileInput = document.getElementById('imageUpload') as HTMLInputElement;
        if (fileInput) {
            fileInput.value = '';
        }
    }
}

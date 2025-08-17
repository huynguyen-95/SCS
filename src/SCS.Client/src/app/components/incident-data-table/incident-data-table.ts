import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { TableModule } from 'primeng/table';
import { IncidentInfo } from '../../models/incident-info.model';

@Component({
    selector: 'app-incident-data-table',
    imports: [TableModule, DatePipe],
    templateUrl: './incident-data-table.html',
    styleUrl: './incident-data-table.scss',
    standalone: true,
})
export class IncidentDataTable {
    public data = input.required<IncidentInfo[]>();
}

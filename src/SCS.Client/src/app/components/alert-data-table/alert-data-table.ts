import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-alert-data-table',
  imports: [TableModule, DatePipe],
  templateUrl: './alert-data-table.html',
  styleUrl: './alert-data-table.scss',
  standalone: true,
})
export class AlertDataTable {
  public data = input.required<AlertInfo[]>();
}

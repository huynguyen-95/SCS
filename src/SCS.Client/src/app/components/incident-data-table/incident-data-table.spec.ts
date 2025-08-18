import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';

import { IncidentDataTable } from './incident-data-table';

describe('IncidentDataTable', () => {
    let component: IncidentDataTable;
    let fixture: ComponentFixture<IncidentDataTable>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [IncidentDataTable],
            providers: [
                provideZonelessChangeDetection()
            ]
        })
            .compileComponents();

        fixture = TestBed.createComponent(IncidentDataTable);
        component = fixture.componentInstance;

        // Set required input
        fixture.componentRef.setInput('data', []);

        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});

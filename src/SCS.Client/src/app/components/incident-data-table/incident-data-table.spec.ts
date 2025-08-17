import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IncidentDataTable } from './incident-data-table';

describe('IncidentDataTable', () => {
    let component: IncidentDataTable;
    let fixture: ComponentFixture<IncidentDataTable>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [IncidentDataTable]
        })
            .compileComponents();

        fixture = TestBed.createComponent(IncidentDataTable);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});

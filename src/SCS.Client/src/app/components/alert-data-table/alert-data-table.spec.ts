import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AlertDataTable } from './alert-data-table';

describe('AlertDataTable', () => {
  let component: AlertDataTable;
  let fixture: ComponentFixture<AlertDataTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlertDataTable]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AlertDataTable);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';

import { AlertDataTable } from './alert-data-table';

describe('AlertDataTable', () => {
  let component: AlertDataTable;
  let fixture: ComponentFixture<AlertDataTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlertDataTable],
      providers: [
        provideZonelessChangeDetection()
      ]
    })
      .compileComponents();

    fixture = TestBed.createComponent(AlertDataTable);
    component = fixture.componentInstance;

    // Set required input
    fixture.componentRef.setInput('data', []);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

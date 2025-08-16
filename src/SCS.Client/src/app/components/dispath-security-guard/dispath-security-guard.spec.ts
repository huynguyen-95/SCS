import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DispathSecurityGuard } from './dispath-security-guard';

describe('DispathSecurityGuard', () => {
  let component: DispathSecurityGuard;
  let fixture: ComponentFixture<DispathSecurityGuard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DispathSecurityGuard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DispathSecurityGuard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

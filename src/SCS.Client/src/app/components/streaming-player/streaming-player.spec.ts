import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StreamingPlayer } from './streaming-player';

describe('StreamingPlayer', () => {
  let component: StreamingPlayer;
  let fixture: ComponentFixture<StreamingPlayer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StreamingPlayer]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StreamingPlayer);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

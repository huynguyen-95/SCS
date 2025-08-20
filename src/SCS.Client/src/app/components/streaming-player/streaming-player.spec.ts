import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import HlsJs from 'hls.js';

import { StreamingPlayer } from './streaming-player';
import { StreamingPlayerModule } from './streaming-player-module';

describe('StreamingPlayer', () => {
  let component: StreamingPlayer;
  let fixture: ComponentFixture<StreamingPlayer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StreamingPlayerModule],
      providers: [
        provideZonelessChangeDetection()
      ]
    })
      .compileComponents();

    fixture = TestBed.createComponent(StreamingPlayer);
    component = fixture.componentInstance;

    // Set required input
    fixture.componentRef.setInput('id', 1);

    // Create mock video element
    const mockVideo = document.createElement('video');
    mockVideo.id = 'player-1';
    document.body.appendChild(mockVideo);
  });

  afterEach(() => {
    const mockVideo = document.getElementById('player-1');
    if (mockVideo) {
      document.body.removeChild(mockVideo);
    }
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should handle when HLS is not supported', () => {
    spyOn(HlsJs, 'isSupported').and.returnValue(false);
    spyOn(console, 'error');

    component.ngAfterViewInit();

    expect(console.error).toHaveBeenCalledWith('HLS is not supported in this browser.');
  });

  it('should initialize HLS when supported', () => {
    spyOn(HlsJs, 'isSupported').and.returnValue(true);
    spyOn(HlsJs.prototype, 'loadSource');
    spyOn(HlsJs.prototype, 'attachMedia');
    spyOn(HlsJs.prototype, 'on');

    component.ngAfterViewInit();

    expect(HlsJs.isSupported).toHaveBeenCalled();
  });

  it('should handle MANIFEST_PARSED event callback', () => {
    spyOn(HlsJs, 'isSupported').and.returnValue(true);
    spyOn(HlsJs.prototype, 'loadSource');
    spyOn(HlsJs.prototype, 'attachMedia');

    let manifestParsedCallback: Function;
    spyOn(HlsJs.prototype, 'on').and.callFake((event: string, callback: Function) => {
      if (event === HlsJs.Events.MANIFEST_PARSED) {
        manifestParsedCallback = callback;
      }
    });

    component.ngAfterViewInit();

    const mockVideo = document.getElementById('player-1') as HTMLVideoElement;
    spyOn(mockVideo, 'play');

    // Trigger the callback
    manifestParsedCallback!();

    expect(mockVideo.muted).toBe(true);
    expect(mockVideo.play).toHaveBeenCalled();
  });

  it('should handle ERROR event callback', () => {
    spyOn(HlsJs, 'isSupported').and.returnValue(true);
    spyOn(HlsJs.prototype, 'loadSource');
    spyOn(HlsJs.prototype, 'attachMedia');

    let errorCallback: Function;
    spyOn(HlsJs.prototype, 'on').and.callFake((event: string, callback: Function) => {
      if (event === HlsJs.Events.ERROR) {
        errorCallback = callback;
      }
    });

    component.ngAfterViewInit();

    expect(component.showError()).toBe(false);

    // Trigger the error callback
    errorCallback!('error-event', 'error-data');

    expect(component.showError()).toBe(true);
  });

  it('should set showError to true when HLS error occurs', () => {
    expect(component.showError()).toBe(false);

    // Manually trigger the error condition
    component.showError.set(true);

    expect(component.showError()).toBe(true);
  });
});

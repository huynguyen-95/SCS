import { AfterViewInit, Component, input, OnInit, signal, WritableSignal } from '@angular/core';
import HlsJs from 'hls.js';

@Component({
  selector: 'app-streaming-player',
  templateUrl: './streaming-player.html',
  styleUrl: './streaming-player.scss',
  standalone: false,
})
export class StreamingPlayer implements OnInit, AfterViewInit {
  public sourceUrl: string = 'https://scs-assessment-bucket.s3.ap-southeast-1.amazonaws.com/fl1-1/playlist.m3u8';
  public showError: WritableSignal<boolean> = signal(false);
  public id = input.required<number>();

  get videoId(): string {
    return `player-${this.id()}`;
  }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    var isSupportHLS = HlsJs.isSupported();
    if (!isSupportHLS) {
      console.error('HLS is not supported in this browser.');
      return;
    }

    const video: HTMLVideoElement = document.getElementById(this.videoId) as HTMLVideoElement;
    var hls = new HlsJs();
    hls.loadSource(this.sourceUrl);
    hls.attachMedia(video);

    hls.on(HlsJs.Events.MANIFEST_PARSED, function () {
      video.muted = true; // Mute the video to avoid autoplay restrictions
      video.play();
    });

    hls.on(HlsJs.Events.ERROR, (event, data) => {
      this.showError.set(true);
    });
  }
}

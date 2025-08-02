import { AfterViewInit, Component, OnInit } from '@angular/core';
import HlsJs from 'hls.js';

@Component({
  selector: 'app-streaming-player',
  templateUrl: './streaming-player.html',
  styleUrl: './streaming-player.scss',
  standalone: false,
})
export class StreamingPlayer implements OnInit, AfterViewInit {
  public sourceUrl: string = 'http://localhost:5050/hls/fl1-1/playlist.m3u8';

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    var isSupportHLS = HlsJs.isSupported();
    if (!isSupportHLS) {
      console.error('HLS is not supported in this browser.');
      return;
    }
    const video: HTMLVideoElement = document.getElementById('player') as HTMLVideoElement;
    var hls = new HlsJs();
    hls.loadSource(this.sourceUrl);
    hls.attachMedia(video);
    hls.on(HlsJs.Events.MANIFEST_PARSED, function () {
      video.play();
    });
  }
}

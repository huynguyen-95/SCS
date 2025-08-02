import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { BitrateOptions, VgApiService } from '@videogular/ngx-videogular/core';
import { VgHlsDirective } from '@videogular/ngx-videogular/streaming';

import HlsJs from 'hls.js';

@Component({
  selector: 'app-streaming-player',
  templateUrl: './streaming-player.html',
  styleUrl: './streaming-player.scss',
  standalone: false,
})
export class StreamingPlayer implements OnInit, AfterViewInit {
  @ViewChild(VgHlsDirective, { static: true }) vgHls: VgHlsDirective | undefined;

  public sourceUrl: string = 'http://localhost:5050/hls/fl1-1/playlist.m3u8';
  public bitrates: BitrateOptions[] = [];
  public api: VgApiService | undefined;

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

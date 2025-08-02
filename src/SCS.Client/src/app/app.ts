import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { StreamingPlayerModule } from './streaming-player/streaming-player-module';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, StreamingPlayerModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('SCS.Client');
}

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StreamingPlayer } from './streaming-player';

@NgModule({
  declarations: [StreamingPlayer],
  imports: [
    CommonModule,
    FormsModule,
  ],
  exports: [StreamingPlayer],
})
export class StreamingPlayerModule { }

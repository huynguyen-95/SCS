Command-line to stream video from cctv folder:

.\third-party\ffmpeg.exe -stream_loop -1 -i .\cctv\fl1-1.mp4 -c:v libx264 -f hls -hls_time 10 -hls_list_size 5 -hls_flags delete_segments .\camera-hls\fl1-1\playlist.m3u8
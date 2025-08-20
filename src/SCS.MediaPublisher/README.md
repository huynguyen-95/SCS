# Smart City Surveillance - Media Publisher

## Purpose

This project is a Smart City Surveillance Media Publisher that monitors HLS (HTTP Live Streaming) video segments and automatically uploads them to AWS S3 for cloud storage and distribution. It acts as a bridge between local CCTV video streams and cloud-based surveillance systems.

## Prerequisites

- **.NET 9.0** or later
- **AWS S3** account with valid credentials
- **FFmpeg** for video streaming (included in project)
- **Windows/Linux** environment

### Required AWS S3 Configuration
- AWS Access Key and Secret Key
- S3 Bucket with appropriate permissions
- AWS Region configuration

## Configuration

Update `appsettings.json` with your settings:

```json
{
  "CameraHLSFolder": "<YOUR_FOLDER>", // Ensure folder is created
  "AWS": {
    "Region": "ap-southeast-1",
    "AccessKey": "YOUR_AWS_ACCESS_KEY",
    "SecretKey": "YOUR_AWS_SECRET_KEY",
    "BucketName": "your-s3-bucket-name"
  }
}
```

### Configuration Parameters:
- **CameraHLSFolder**: Local folder path to monitor for HLS segments
- **AWS.Region**: Your AWS region (e.g., ap-southeast-1, us-east-1)
- **AWS.AccessKey**: Your AWS access key ID
- **AWS.SecretKey**: Your AWS secret access key
- **AWS.BucketName**: S3 bucket name for storing video segments

## How It Works

```
┌─────────────┐    ┌──────────────┐    ┌─────────────────┐    
│ CCTV Videos │───▶│    FFmpeg    │───▶│ HLS Segments    │    
│ (.mp4 files)│    │ (Streaming)  │    │ (.ts files)     │    
└─────────────┘    └──────────────┘    └─────────────────┘    
                                               │
                                               ▼
                                        ┌─────────────────┐    ┌─────────────┐
                                        │ File Watcher    │───▶│   AWS S3    │
                                        │ Service         │    │ (Storage)   │
                                        │ (This Project)  │    └─────────────┘
                                        └─────────────────┘
```

1. **FFmpeg** converts CCTV videos into HLS streaming format
2. **File Watcher Service** monitors the configured HLS folder
3. When new `.ts` segments are created, they are automatically uploaded to **AWS S3**

## Video Streaming Commands

### Stream video from CCTV folder to HLS segments:

```bash
.\ffmpeg\ffmpeg.exe -stream_loop -1 -i .\cctv\fl1-1.mp4 -c:v libx264 -f hls -hls_time 10 -hls_list_size 5 -hls_flags delete_segments .\camera-hls\fl1-1\playlist.m3u8
```

### Command Parameters:
- `-stream_loop -1`: Loop the input video indefinitely
- `-i .\cctv\fl1-1.mp4`: Input video file from CCTV folder
- `-c:v libx264`: Use H.264 codec for video encoding
- `-f hls`: Output format as HLS
- `-hls_time 10`: Each segment duration (10 seconds)
- `-hls_list_size 5`: Keep 5 segments in playlist
- `-hls_flags delete_segments`: Delete old segments automatically
- `.\camera-hls\fl1-1\playlist.m3u8`: Output HLS playlist and segments folder
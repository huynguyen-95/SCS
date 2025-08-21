## Media Publisher
Workflow:
1. CCTV will stream data (through protocol RTSP) to a transcoder (which could be FFMPEG or AWS Transcoder)
2. Transcoder will convert stream data to HLS format
3. A console application (**FileWatcher** service) would detect changes of HLS files and then upload to S3

## Web App
Front end website where **SCS Users** use.

**CloudFont**: consider as a CDN which cache streaming data, it would be usefull when multiple **SCS users** watch same stream, it would reduce the request to load objects from S3 (reduce cost) \
To watching streaming data, front-end would fetch metadata files (.m3u8) frequently and based on that it would load segment files (.ts) to show stream.

Front end would communicate to Back end in both protocol HTTPs and WebSocket \
For HTTPs, which is used to load data from DB. \
For websocket, which is used to notify SCS users.

## API Server
Would use ALB and many instances of EC2 (can use auto-scaling here if needed) for serving requests from both SCS Users and Security Guard.

## Alarm System
Assume the Alarm System would send notification to AWS Simple Queue Service

## Elastic
Use to monitoring logs from backend

## Sentry
Use to monitoring logs from web app (Angular)
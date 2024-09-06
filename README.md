# S3 Video Upload

This is a .NET console application that uploads a test video file to an Amazon S3 bucket. The video is generated using FFmpeg and uploaded using the AWS SDK for .NET. This tool is designed to work with AWS SSO credentials and allows specifying the S3 bucket and AWS profile via command-line arguments.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or higher)
- [FFmpeg](https://ffmpeg.org/download.html) (ensure it's in your system's PATH)
- AWS CLI configured with SSO credentials

## Getting Started

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/axm06051/UploadTestVideoToS3.git
   cd s3-video-upload
   ```

2. **Restore Dependencies:**

   ```bash
   dotnet restore
   ```

3. **Build the Project:**

   ```bash
   dotnet build
   ```

4. **Run the Application:**

   To run the application, use the following command format:

   ```bash
   dotnet run -- --s3 <bucket_name> --profile <profile_name>
   ```

   Replace `<bucket_name>` with your S3 bucket name and `<profile_name>` with your AWS SSO profile name.

## How It Works

1. **Command-Line Parsing:**

   The application expects two command-line arguments:
   - `--s3` for the S3 bucket name.
   - `--profile` for the AWS SSO profile name.

2. **Generate Test Video:**

   The application uses FFmpeg to generate a temporary video file with a duration of 10 seconds. The video is created with the following properties:
   - Resolution: 1280x720
   - Frame Rate: 30 fps
   - Bitrate: 2 Mbps

3. **Upload Video to S3:**

   The generated video is uploaded to the specified S3 bucket using the AWS SDK for .NET. The upload is handled by the `TransferUtility` class with a part size of 5 MB.

4. **Clean Up:**

   After the upload is complete, the temporary video file is deleted from the local file system.

## Error Handling

If there are any errors during the FFmpeg process or S3 upload, they will be displayed in the console output. Ensure that your FFmpeg installation is correctly set up and that your AWS credentials and S3 bucket are properly configured.

## Example

To generate and upload a video to an S3 bucket named `my-video-bucket` using an AWS profile named `my-sso-profile`, run:

```bash
dotnet run -- --s3 my-video-bucket --profile my-sso-profile
```

## Troubleshooting

- **FFmpeg Not Found:**
  Ensure that FFmpeg is installed and properly set in your system's PATH.

- **AWS Credentials Error:**
  Make sure your AWS CLI is configured correctly and that the profile specified has the necessary permissions.

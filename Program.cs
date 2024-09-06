using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace S3VideoUpload
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string? bucketName = null;
            string? profileName = null;

            // Parse command-line arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--s3" && i + 1 < args.Length)
                {
                    bucketName = args[i + 1];
                    i++;
                }
                else if (args[i] == "--profile" && i + 1 < args.Length)
                {
                    profileName = args[i + 1];
                    i++;
                }
            }

            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(profileName))
            {
                Console.WriteLine("\nUsage: dotnet run --s3 <bucket_name> --profile <profile_name>");
                Environment.Exit(1);
            }

            try
            {
                var ssoCreds = LoadSsoCredentials(profileName);
                var s3Client = new AmazonS3Client(ssoCreds, RegionEndpoint.EUWest1);

                string videoPath = await GenerateTestVideo();
                Console.WriteLine($"Temporary video file created at: {videoPath}");

                await UploadVideoToS3(s3Client, bucketName, videoPath);

                Console.WriteLine($"Deleting temporary file: {videoPath}");
                File.Delete(videoPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static async Task<string> GenerateTestVideo()
        {
            string outputPath = Path.GetTempFileName() + ".mp4";
            string ffmpegArgs = $"-f lavfi -i smptebars=duration=10:size=1280x720:rate=30 -vf \"interlace=scan=tff\" -c:v libx264 -crf 18 -preset ultrafast -r 30 -b:v 2M -y {outputPath}";

            Console.WriteLine("Starting FFmpeg process to generate test video...");
            Console.WriteLine($"FFmpeg arguments: {ffmpegArgs}");

            using (var process = new Process())
            {
                process.StartInfo.FileName = "ffmpeg";
                process.StartInfo.Arguments = ffmpegArgs;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"FFmpeg error: {error}");
                    throw new Exception($"FFmpeg process exited with code {process.ExitCode}");
                }

                Console.WriteLine($"FFmpeg output: {output}");
            }

            Console.WriteLine($"Test video successfully created at: {outputPath}");
            return outputPath;
        }

        private static async Task UploadVideoToS3(IAmazonS3 s3Client, string bucketName, string filePath)
        {
            Console.WriteLine($"\nUploading video to S3 bucket {bucketName}...");

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var fileTransferUtility = new TransferUtility(s3Client);
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = bucketName,
                        InputStream = fileStream,
                        Key = Path.GetFileName(filePath),
                        PartSize = 5 * 1024 * 1024 // 5 MB
                    };

                    await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
                    Console.WriteLine("Video upload completed successfully.");

                    string fileUrl = $"https://{bucketName}.s3.{s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{Path.GetFileName(filePath)}";
                    Console.WriteLine($"Uploaded video URL: {fileUrl}");
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when uploading an object");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unknown encountered on server. Message:'{e.Message}' when uploading an object");
            }
        }

        static AWSCredentials LoadSsoCredentials(string profile)
        {
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials(profile, out var credentials))
                throw new Exception($"Failed to find the {profile} profile");
            return credentials;
        }
    }
}

using Amazon.S3;
using Microsoft.Extensions.Options;
using SCS.MediaPublisher.Options;

namespace SCS.MediaPublisher.Services;

public interface IUploadFileService
{
    Task UploadFileAsync(string filePath, string key);
    Task<bool> DeleteFileAsync(string filePath);
}

public class UploadFileService : IUploadFileService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;

    public UploadFileService(IOptions<AwsS3Options> options)
    {
        var awsOptions = options.Value;
        _bucketName = awsOptions.BucketName;
        _region = awsOptions.Region;

        _s3Client = new AmazonS3Client(awsOptions.AccessKey, awsOptions.SecretKey, new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsOptions.Region)
        });
    }

    public async Task UploadFileAsync(string filePath, string key)
    {
        try
        {
            using var fileStream = File.OpenRead(filePath);
            await _s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key.Replace('\\', '/'), // Ensure the key uses forward slashes
                InputStream = fileStream
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Error uploading file to S3: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(_bucketName, filePath);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting file from S3: {ex.Message}", ex);
        }
    }
}

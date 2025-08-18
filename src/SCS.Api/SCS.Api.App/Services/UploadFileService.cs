using Amazon.S3;
using ErrorOr;
using Microsoft.Extensions.Options;
using SCS.Api.App.Settings;

namespace SCS.Api.App.Services;


public interface IUploadFileService
{
    Task<ErrorOr<string>> UploadFileAsync(IFormFile file, CancellationToken cancellationToken);
}

public class UploadFileService : IUploadFileService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;

    public UploadFileService(IOptions<AwsOptions> options, IAmazonS3 s3Client)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(s3Client, nameof(s3Client));

        var awsOptions = options.Value;

        _bucketName = awsOptions.BucketName;
        _region = awsOptions.Region;
        _s3Client = s3Client;
    }

    public async Task<ErrorOr<string>> UploadFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required.");
        }

        var key = Guid.CreateVersion7();

        var putRequest = new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = _bucketName,
            Key = $"{key}.{Path.GetExtension(file.FileName)}",
            InputStream = file.OpenReadStream(),
            ContentType = file.ContentType
        };

        var result = await _s3Client.PutObjectAsync(putRequest, cancellationToken);
        if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            return Error.Failure("UploadFileService.UploadFailed", "Failed to upload file to S3.");
        }

        return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{putRequest.Key}";
    }
}

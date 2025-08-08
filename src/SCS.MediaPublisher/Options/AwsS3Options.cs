namespace SCS.MediaPublisher.Options;

public class AwsS3Options
{
    public const string ConfigurationSection = "AWS";

    public required string Region { get; set; }
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public required string BucketName { get; set; }
}

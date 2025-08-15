using System;

namespace SCS.Api.App.Settings;

public class AwsOptions
{
    public const string ConfigurationSection = "AWS";

    public required string Region { get; set; }
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public required string QueueUrl { get; set; }
}
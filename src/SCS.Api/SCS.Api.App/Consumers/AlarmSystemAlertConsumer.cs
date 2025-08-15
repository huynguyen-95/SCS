using System.Text.Json;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SCS.Api.App.Events;
using SCS.Api.App.Messaging;
using SCS.Api.App.Settings;

namespace SCS.Api.App.Consumers;

public class AlarmSystemAlertConsumer : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;
    private readonly IHubContext<AlarmSystemHub> _hubContext;

    public AlarmSystemAlertConsumer(IOptions<AwsOptions> awsOptions, IHubContext<AlarmSystemHub> hubContext)
    {
        ArgumentNullException.ThrowIfNull(awsOptions, nameof(awsOptions));
        ArgumentNullException.ThrowIfNull(hubContext, nameof(hubContext));

        _queueUrl = awsOptions.Value.QueueUrl;
        _sqsClient = new AmazonSQSClient(
            awsOptions.Value.AccessKey,
            awsOptions.Value.SecretKey,
            RegionEndpoint.GetBySystemName(awsOptions.Value.Region));
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 5,
                WaitTimeSeconds = 5 // long polling
            };

            var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

            foreach (var message in response.Messages)
            {
                // TODO: Deserialize message body to your specific alert type
                var @event = JsonSerializer.Deserialize<AlarmSystemAlertEvent>(message.Body, Constants.DefaultJsonSerializerOptions);
                await _hubContext.Clients.Group(@event.PremiseId.ToString()).SendAsync("ReceiveAlert", @event.Message, stoppingToken);

                await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                Console.WriteLine("Message deleted.");
            }
        }
    }
}

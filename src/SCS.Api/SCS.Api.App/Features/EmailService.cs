using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace SCS.Api.App.Features;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string message);
}

public class EmailService : IEmailService
{
    private readonly IAmazonSimpleEmailService _awsSES;
    private readonly string _fromAddress;

    public EmailService(IAmazonSimpleEmailService awsSES, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(awsSES, nameof(awsSES));

        _awsSES = awsSES;
        _fromAddress = configuration["Email:From"];
    }

    public async Task SendEmailAsync(string to, string subject, string message)
    {
        var req = new SendEmailRequest
        {
            Source = _fromAddress,
            Destination = new Destination { ToAddresses = [to] },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Text = new Content { Charset = "UTF-8", Data = message }
                }
            }
        };

        await _awsSES.SendEmailAsync(req);
    }
}

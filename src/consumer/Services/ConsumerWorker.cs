using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace consumer.Services
{
    public class ConsumerWorker : BackgroundService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;
        private readonly ILogger<ConsumerWorker> _logger;

        public ConsumerWorker(IConfiguration config, IAmazonSQS sqsClient, ILogger<ConsumerWorker> logger)
        {
            _sqsClient = sqsClient;
            _queueUrl = config["AWS:UrlQueue"];
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new Amazon.SQS.Model.ReceiveMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        MaxNumberOfMessages = 10,
                        WaitTimeSeconds = 20,
                        VisibilityTimeout = 40,
                        MessageAttributeNames = new List<string> { "All" }
                    };

                    var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                    _logger.LogInformation($"Received {response.HttpStatusCode}.");
                    _logger.LogInformation($"Received {response.ContentLength}.");

                    if (response.Messages != null)
                    {
                        if (response.Messages.Count == 0)
                        {
                            _logger.LogInformation("No messages received from SQS.");
                            continue;
                        }

                        foreach (var message in response.Messages)
                        {
                            _logger.LogInformation($"Received message: {message.Body}");
                            _logger.LogInformation($"Message ID: {message.MessageId}");
                            await ProcessMessageAsync(message, stoppingToken);
                        }
                    } 
                    else
                    {
                        _logger.LogInformation("No messages received from SQS.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error receiving messages from SQS: {ex.Message}");
                }
            }
        }

        private async Task ProcessMessageAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Processing message ID: {message.MessageId}");
                Thread.Sleep(3000);
                await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, cancellationToken);
                _logger.LogInformation($"Processed and removed message: {message.Body}, ID: {message.MessageId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error processing message: {ex.Message}");
                _logger.LogError(ex, "Error processing message: {MessageId}", message.MessageId);
            }
        }
    }
}
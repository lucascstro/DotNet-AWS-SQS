using Amazon.SQS;

namespace publisher.Services
{
    public class SqsService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;

        public SqsService(IConfiguration config, IAmazonSQS sqsClient)
        {
            _sqsClient = sqsClient;
            _queueUrl = config["AWS:UrlQueue"];
        }

        public async Task SendMessageAsync(string msg)
        {
            try
            {
                var request = new Amazon.SQS.Model.SendMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MessageBody = msg,
                    MessageGroupId = "TesteGroupId",
                    MessageDeduplicationId = Guid.NewGuid().ToString()
                };
                
                await _sqsClient.SendMessageAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to SQS: {ex.Message}");
                throw;
            }
        }
    }
}
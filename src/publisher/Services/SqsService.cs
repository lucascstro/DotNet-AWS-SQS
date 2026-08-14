using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS;

namespace publisher.Services
{
    public class SqsService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;

        public SqsService(IAmazonSQS sqsClient, string queueUrl)
        {
            _sqsClient = sqsClient;
            _queueUrl = queueUrl;
        }

        public async Task SendMessageAsync(string msg)
        {
            try
            {
                var request = new Amazon.SQS.Model.SendMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MessageBody = msg
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
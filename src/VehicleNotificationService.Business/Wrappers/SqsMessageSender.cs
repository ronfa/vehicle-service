using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VehicleNotificationService.Business.Wrappers
{
    public class SqsMessageSender : ISqsMessageSender
    {
        public async Task<List<SendMessageResponse>> SendQueueMessagesAsync(List<object> messages)
        {

            // Target Queue si taken form Enviroment and not form config
            // because name defined during creation of teh stack
            var targetQueueUrl = Environment.GetEnvironmentVariable("Queue__SampleQueue__Url");
            List<SendMessageResponse> messageRersponses = new List<SendMessageResponse>();

            var config = new AmazonSQSConfig();
            foreach (var message in messages)
            {
                var messageRequest = new SendMessageRequest
                {
                    QueueUrl = targetQueueUrl,
                    MessageBody = JsonConvert.SerializeObject(message)
                };
                var messageClient = new AmazonSQSClient(config);
                var sendMessageResponse = await messageClient.SendMessageAsync(messageRequest);
                messageRersponses.Add(sendMessageResponse);
            }

            return messageRersponses;
        }
    }
}
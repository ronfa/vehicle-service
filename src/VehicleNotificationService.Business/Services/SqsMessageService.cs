using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VehicleNotificationService.Business.Model;

namespace VehicleNotificationService.Business.Services
{
    public class SqsMessageService: ISqsMessageService
    {
        private ILogger Logger { get; }

        public SqsMessageService(ILogger logger)
        {
            Logger = logger;
        }

        private string TargetQueueKey => "TARGET_QUEUE_URL";

        private string DeadletterQueueKey => "DEADLETTER_QUEUE_URL";

        public Task<SendMessageResponse> SendToMessageQueue(VehicleEvent message)
            => SendToQueue(JsonConvert.SerializeObject(message), GetQueueUrl(TargetQueueKey));

        public Task<SendMessageResponse> SendToDeadletterQueue(string message)
            => SendToQueue(message, GetQueueUrl(DeadletterQueueKey));


        private async Task<SendMessageResponse> SendToQueue(string message, string targetQueueUrl)
        {
            var response = new SendMessageResponse();

            var config = new AmazonSQSConfig();

            try
            {
                Logger.LogDebug(
                    $"{nameof(SendToQueue)} Sending message to queue {targetQueueUrl} - Message: {JsonConvert.SerializeObject(message)}");

                var messageRequest = new SendMessageRequest
                {
                    QueueUrl = targetQueueUrl,
                    MessageBody = message
                };

                var messageClient = new AmazonSQSClient(config);
                response = await messageClient.SendMessageAsync(messageRequest);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    Logger.LogWarning(
                        $"{nameof(SendToQueue)} Response code {response.HttpStatusCode} - Error processing message, no details available.");
                }
                else
                {
                    Logger.LogDebug($"{nameof(SendToQueue)} Response code: {response.HttpStatusCode}");
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(SendToQueue)} Exception occurred while processing message.");
            }

            return response;
        }

        private string GetQueueUrl(string key)
        {
            var queueUrl = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrEmpty(queueUrl))
            {
                throw new Exception($"{nameof(GetQueueUrl)}: Environment variable '{key}' is missing or empty.");
            }

            return queueUrl;
        }
    }
}

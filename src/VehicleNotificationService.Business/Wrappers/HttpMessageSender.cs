using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VehicleNotificationService.Business.Errors;

namespace VehicleNotificationService.Business.Wrappers
{
    public class HttpMessageSender : IHttpMessageSender
    {
        private readonly ILogger _logger;
        private HttpClient _httpClient;
        private HttpClient HttpClient => _httpClient ??= new HttpClient();

        public HttpMessageSender(ILogger logger, HttpClient httpClient = null)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<T> SendMessage<T>(Uri url, object message, params KeyValuePair<string, string>[] headers) where T : new()
        {
            var client = HttpClient;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Clear();

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);

                    _logger.LogInformation(
                        $"Adding request header {header.Key} : {header.Value}");
                }
            }

            var jsonString = JsonConvert.SerializeObject(message);

            var httpContent = new StringContent(jsonString);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            _logger.LogInformation(
                $"Sending request to url {url} with content: {jsonString}");

            HttpResponseMessage response = null;
            try
            {

                response = await client.PostAsync(url, httpContent);

                response.EnsureSuccessStatusCode(); // throws if not 200-299
            }
            catch (HttpRequestException ex)
            {
                if (response is {StatusCode: HttpStatusCode.NotFound})
                {
                    throw new UserNotFoundException();
                }

                _logger.LogError($"{nameof(HttpMessageSender)} Error sending message : {ex.Message}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(HttpMessageSender)} Error sending message : {ex.Message}");
                throw ex;
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                $"Response body: {responseBody}");

            if (response.Content != null && response.Content.Headers.ContentType.MediaType == "application/json")
            {
                var contentStream = await response.Content.ReadAsStreamAsync();

                var streamReader = new StreamReader(contentStream);
                var jsonReader = new JsonTextReader(streamReader);
                var serializer = new JsonSerializer();

                try
                {
                    return serializer.Deserialize<T>(jsonReader);
                }
                catch (JsonReaderException)
                {
                    _logger.LogError(
                        $"Error occurred while sending http message to url : {url}, with message {message} - Invalid JSON returned: {response}");
                    Console.WriteLine("Invalid JSON.");
                }
            }
            else
            {

                _logger.LogError(
                    $"Error occurred while sending http message to url : {url}, with message {message}, response is {response} ");

                throw new Exception();
            }

            return default;
        }
    }
}

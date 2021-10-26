using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VehicleNotificationService.Business.Mappings;
using VehicleNotificationService.Business.Model;
using VehicleNotificationService.Business.Model.Configuration;
using VehicleNotificationService.Business.Wrappers;

namespace VehicleNotificationService.Business.Services
{
    public class PhonixxMessageSender : IPhonixxMessageSender
    {

        private readonly ILogger _logger;
        private readonly PhonixxReceiveEndpointConfig _endpointConfig;
        private readonly IHttpMessageSender _httpMessageSender;

        public PhonixxMessageSender(IOptions<PhonixxReceiveEndpointConfig> config, IHttpMessageSender httpMessageSender,
            ILogger logger)
        {
            _endpointConfig = config.Value;
            _logger = logger;
            _httpMessageSender = httpMessageSender;
        }


        public async Task<VerifyParkingResponse> VerifyStop(VehicleEvent message, int supplierId, Guid tripId)
        {
            var request = message.ToVerifyRequest(supplierId, tripId);

            var supplierSetting = GetSupplierSettings(supplierId);

            if (supplierSetting == null)
            {
                _logger.LogError($"Could not find supplier settings for supplier id : {supplierId}");
                return new VerifyParkingResponse();
            }

            var targetEndpointUrl = supplierSetting.Url;

            _logger.LogInformation($"Target Phonixx URL : {targetEndpointUrl}");

            var xApiKeyHeader = new KeyValuePair<string, string>("x-api-key", _endpointConfig.XApiKey);
            var url = new Uri(new Uri(targetEndpointUrl), _endpointConfig.RelativeVerifyUrl);


            _logger.LogInformation($"Message is being sent to {url}  with content: {JsonConvert.SerializeObject(request)}");

            return await _httpMessageSender.SendMessage<VerifyParkingResponse>(url, request, xApiKeyHeader);
        }

        public async Task<StopParkingResponse> StopParking(VehicleEvent message, int supplierId, Guid tripId, ParkingAction parkingAction)
        {
            var request = message.ToStopParkingRequest(supplierId, tripId, parkingAction);

            var supplierSetting = GetSupplierSettings(supplierId);

            if (supplierSetting == null)
            {
                _logger.LogError($"Could not find supplier settings for supplier id : {supplierId}");
                return new StopParkingResponse();
            }

            var targetEndpointUrl = supplierSetting.Url;

            _logger.LogInformation($"Target Phonixx URL : {targetEndpointUrl}");

            var xApiKeyHeader = new KeyValuePair<string, string>("x-api-key", _endpointConfig.XApiKey);
            var url = new Uri(new Uri(targetEndpointUrl), _endpointConfig.RelativeStopUrl);


            _logger.LogInformation($"Message is being sent to {url}  with content: {JsonConvert.SerializeObject(request)}");

            return await _httpMessageSender.SendMessage<StopParkingResponse>(url, request, xApiKeyHeader);

        }

        private Supplier GetSupplierSettings(int supplierId)
        {
            Supplier supplierSetting = null;

            try
            {
                supplierSetting = _endpointConfig.Suppliers.Single(t => t.SupplierId == supplierId);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return supplierSetting;

        }
    }
}

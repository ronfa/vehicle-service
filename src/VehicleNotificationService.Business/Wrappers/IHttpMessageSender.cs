using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VehicleNotificationService.Business.Wrappers
{
    public interface IHttpMessageSender
    {
        Task<T> SendMessage<T>(Uri url, object message, params KeyValuePair<string, string>[] headers) where T : new();
    }
}

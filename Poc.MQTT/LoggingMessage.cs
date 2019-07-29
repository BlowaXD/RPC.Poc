using System;
using ChickenAPI.Messaging.Protocol;

namespace Poc.MQTT
{
    public class LoggingMessage<T> : IAsyncRpcRequest where T : IPlayerAction
    {
        public LoggingMessage(T action)
        {
            Action = action;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public T Action { get; set; }
    }
}
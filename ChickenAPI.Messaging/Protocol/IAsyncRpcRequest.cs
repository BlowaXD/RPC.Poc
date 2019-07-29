using System;

namespace ChickenAPI.Messaging.Protocol
{
    public interface IAsyncRpcRequest
    {
        Guid Id { get; set; }
    }
}
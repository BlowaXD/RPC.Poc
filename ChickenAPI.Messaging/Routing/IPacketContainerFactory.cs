using System;
using ChickenAPI.Messaging.Protocol;

namespace ChickenAPI.Messaging.Routing
{
    public interface IPacketContainerFactory
    {
        PacketContainer ToPacket<T>(T packet) where T : IAsyncRpcRequest;
        PacketContainer ToPacket(Type type, IAsyncRpcRequest packet);
    }
}
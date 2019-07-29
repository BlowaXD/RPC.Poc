using System;
using ChickenAPI.Messaging.Routing;
using Newtonsoft.Json;

namespace ChickenAPI.Messaging.Protocol
{
    public class PacketContainerFactory : IPacketContainerFactory
    {
        private static PacketContainer Create(Type type, string content) =>
            new PacketContainer
            {
                Type = type,
                Content = content
            };

        public PacketContainer ToPacket<T>(T packet) where T : IAsyncRpcRequest => ToPacket(typeof(T), packet);

        public PacketContainer ToPacket(Type type, IAsyncRpcRequest packet) => Create(type, JsonConvert.SerializeObject(packet));
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChickenAPI.Messaging.Handlers;
using ChickenAPI.Messaging.Protocol;

namespace ChickenAPI.Messaging.Utils
{
    public sealed class PacketHandlersContainer : IIpcPacketHandlersContainer
    {
        private readonly Dictionary<Type, IIpcPacketHandler> _packetHandlers = new Dictionary<Type, IIpcPacketHandler>();

        public event EventHandler<Type> Registered;
        public event EventHandler<Type> Unregistered;

        public void Register(IIpcPacketHandler handler, Type type)
        {
            if (_packetHandlers.ContainsKey(type))
            {
                return;
            }

            _packetHandlers.Add(type, handler);
            OnRegistered(type);
        }

        public async Task HandleAsync(IAsyncRpcRequest request, Type type)
        {
            if (!_packetHandlers.TryGetValue(type, out IIpcPacketHandler handler))
            {
                return;
            }

            await handler.Handle(request);
        }

        public void Unregister(Type type)
        {
            if (_packetHandlers.ContainsKey(type))
            {
                OnUnregistered(type);
                _packetHandlers.Remove(type);
            }
        }

        private void OnRegistered(Type e)
        {
            Registered?.Invoke(this, e);
        }

        private void OnUnregistered(Type e)
        {
            Unregistered?.Invoke(this, e);
        }
    }
}
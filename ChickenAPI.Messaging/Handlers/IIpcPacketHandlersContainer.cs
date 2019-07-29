using System;
using System.Threading.Tasks;
using ChickenAPI.Messaging.Protocol;

namespace ChickenAPI.Messaging.Handlers
{
    public interface IIpcPacketHandlersContainer
    {
        /// <summary>
        /// This event is invoked when the containers registers an IPC message handler
        /// </summary>
        event EventHandler<Type> Registered;

        /// <summary>
        /// This event is invoked when the containers unregisters an IPC message handler
        /// </summary>
        event EventHandler<Type> Unregistered;

        /// <summary>
        /// Registers an handler for the given IPC Message type
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="type"></param>
        void Register(IIpcPacketHandler handler, Type type);

        /// <summary>
        /// Unregisters handler for the given IPC Message type
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="type"></param>
        void Unregister(Type type);

        /// <summary>
        /// Handles the given RPC AsyncCall
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        Task HandleAsync(IAsyncRpcRequest request, Type type);
    }
}
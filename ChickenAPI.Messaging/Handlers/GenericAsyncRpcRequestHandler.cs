using System.Threading.Tasks;
using ChickenAPI.Messaging.Protocol;

namespace ChickenAPI.Messaging.Handlers
{
    public abstract class GenericAsyncRpcRequestHandler<T> : IIpcPacketHandler where T : IAsyncRpcRequest
    {
        public async Task Handle(IAsyncRpcRequest packet)
        {
            if (packet is T request)
            {
                await Handle(request);
            }
        }

        protected abstract Task Handle(T request);
    }
}
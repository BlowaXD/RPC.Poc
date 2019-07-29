using System.Threading.Tasks;
using ChickenAPI.Messaging.Protocol;

namespace ChickenAPI.Messaging.Handlers
{
    public interface IIpcPacketHandler
    {
        Task Handle(IAsyncRpcRequest packet);
    }
}
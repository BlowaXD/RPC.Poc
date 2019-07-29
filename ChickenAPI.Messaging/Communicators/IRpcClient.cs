using System.Threading.Tasks;
using ChickenAPI.Messaging.Protocol;

namespace ChickenAPI.Messaging.Communicators
{
    public interface IRpcClient
    {
        Task BroadcastAsync<T>(T packet) where T : IAsyncRpcRequest;

        /// <summary>
        /// Starts the client, message will be sent
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Stops the client, message will not be sent anymore
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
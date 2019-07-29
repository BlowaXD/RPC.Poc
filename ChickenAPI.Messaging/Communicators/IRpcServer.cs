using System.Threading.Tasks;

namespace ChickenAPI.Messaging.Communicators
{
    public interface IRpcServer
    {
        /// <summary>
        /// Starts the RPC server listening for incoming messages
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Stops the RPC server from receiving messages
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
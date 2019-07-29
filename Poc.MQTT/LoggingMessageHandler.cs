using System.Threading.Tasks;
using ChickenAPI.Messaging.Handlers;

namespace Poc.MQTT
{
    public class LoggingMessageHandler<T> : GenericAsyncRpcRequestHandler<LoggingMessage<T>> where T : IPlayerAction
    {
        private readonly IPlayerActionLogger _logger;

        public LoggingMessageHandler(IPlayerActionLogger logger)
        {
            _logger = logger;
        }

        protected override async Task Handle(LoggingMessage<T> request)
        {
            await _logger.SaveAsync(request.Action);
        }
    }
}
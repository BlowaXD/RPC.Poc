using System.Threading.Tasks;

namespace Poc.MQTT
{
    public interface IPlayerActionLogger
    {
        Task SaveAsync<T>(T action) where T : IPlayerAction;
    }
}
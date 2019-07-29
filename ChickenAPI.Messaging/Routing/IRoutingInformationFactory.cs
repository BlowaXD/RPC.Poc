using System.Threading.Tasks;

namespace ChickenAPI.Messaging.Routing
{
    public interface IRoutingInformationFactory
    {
        IRoutingInformation Create(string topic);
    }
}
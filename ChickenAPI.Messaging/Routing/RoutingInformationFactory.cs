using System.Threading.Tasks;

namespace ChickenAPI.Messaging.Routing
{
    public class RoutingInformationFactory : IRoutingInformationFactory
    {
        public IRoutingInformation Create(string topic)
        {
            return new RoutingInformation
            {
                Topic = topic
            };
        }
    }
}
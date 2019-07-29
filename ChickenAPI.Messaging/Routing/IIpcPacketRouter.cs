using System;
using System.Threading.Tasks;

namespace ChickenAPI.Messaging.Routing
{
    public interface IIpcPacketRouter
    {
        /// <summary>
        /// Will register and generate the default routing informations related to Router's configuration
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IRoutingInformation Register(Type type);


        /// <summary>
        /// Get the routing informations from the router
        /// re
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IRoutingInformation GetRoutingInformations(Type type);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChickenAPI.Messaging.Protocol;

namespace ChickenAPI.Messaging.Routing
{
    public static class TypeNameExtensions
    {
        public static string ToHypenCase(this string source)
        {
            return Regex.Replace(source, @"([a-z])([A-Z])", "$1-$2").ToLower();
        }

        public static string GetFriendlyName(this Type type)
        {
            string friendlyName = type.Name;
            if (!type.IsGenericType)
            {
                return friendlyName;
            }

            int iBacktick = friendlyName.IndexOf('`');
            if (iBacktick > 0)
            {
                friendlyName = friendlyName.Remove(iBacktick);
            }

            friendlyName += "/";
            Type[] typeParameters = type.GetGenericArguments();
            for (int i = 0; i < typeParameters.Length; ++i)
            {
                string typeParamName = GetFriendlyName(typeParameters[i]);
                friendlyName += (i == 0 ? typeParamName : "/" + typeParamName);
            }

            return friendlyName;
        }
    }

    public class RuntimeSmartIpcRouter : IIpcPacketRouter
    {
        private readonly IRoutingInformationFactory _routingInformationFactory;
        private readonly Dictionary<Type, IRoutingInformation> _infos = new Dictionary<Type, IRoutingInformation>();

        public RuntimeSmartIpcRouter(IRoutingInformationFactory routingInformationFactory)
        {
            _routingInformationFactory = routingInformationFactory;
        }

        private IRoutingInformation RegisterGenericRequestAsync(Type type)
        {
            // in case they want to register "runtime" generics objects
            Type evaluatedType = type.BaseType ?? type;

            var newTopic = new StringBuilder();
            newTopic.Append(type.GetFriendlyName().ToHypenCase());
            foreach (Type i in evaluatedType.GenericTypeArguments)
            {
                newTopic.AppendFormat("/{0}", i.Name.ToHypenCase());
            }

            string requestTopic = newTopic.ToString();


            IRoutingInformation routingInfos = _routingInformationFactory.Create(requestTopic);
            Register(type, routingInfos);
            return routingInfos;
        }

        public IRoutingInformation Register(Type type)
        {
            if (type.IsGenericType)
            {
                return RegisterGenericRequestAsync(type);
            }

            string requestTopic = "";
            if (type.GetInterfaces().Any(s => s == typeof(IAsyncRpcRequest)))
            {
                requestTopic = type.FullName.ToHypenCase();
            }

            IRoutingInformation routingInfos = _routingInformationFactory.Create(requestTopic);
            Register(type, routingInfos);
            return routingInfos;
        }

        public void Register(Type type, IRoutingInformation routingInformation)
        {
            if (_infos.ContainsKey(type))
            {
                return;
            }

            _infos.Add(type, routingInformation);
        }

        public IRoutingInformation GetRoutingInformations(Type type)
        {
            if (_infos.TryGetValue(type, out IRoutingInformation value))
            {
                return value;
            }

            return Register(type);
        }
    }
}
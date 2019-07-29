using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChickenAPI.Messaging.Configs;
using ChickenAPI.Messaging.Handlers;
using ChickenAPI.Messaging.Protocol;
using ChickenAPI.Messaging.Routing;
using ChickenAPI.Messaging.Serializers;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Diagnostics;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using Prometheus;

namespace ChickenAPI.Messaging.Communicators
{
    public sealed class MqttIpcServer : IRpcServer
    {
        private static readonly Counter MessageReceived = Metrics.CreateCounter("MessageReceived", "Numbers of messageReceived", new CounterConfiguration
        {
            LabelNames = new[] { "clientId", "topic" }
        });

        private readonly IIpcPacketHandlersContainer _packetHandlersContainer;
        private readonly HashSet<string> _queues;
        private readonly IManagedMqttClient _client;
        private readonly IIpcSerializer _serializer;
        private readonly IIpcPacketRouter _router;

        private bool IsInitialized => _client.IsStarted;
        private readonly ManagedMqttClientOptions _options;

        public MqttIpcServer(MqttServerConfigurationBuilder builder, IIpcSerializer serializer, IIpcPacketRouter router, IIpcPacketHandlersContainer packetHandlersContainer) : this(
            builder.Build(),
            serializer, router, packetHandlersContainer)
        {
        }

        public MqttIpcServer(MqttIpcServerConfiguration configuration, IIpcSerializer serializer, IIpcPacketRouter router, IIpcPacketHandlersContainer packetHandlersContainer)
        {
            _serializer = serializer;
            _router = router;
            _packetHandlersContainer = packetHandlersContainer;

            _client = new MqttFactory().CreateManagedMqttClient(new MqttNetLogger(configuration.ClientName));
            _queues = new HashSet<string>();
            _options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(configuration.ClientName)
                    .WithTcpServer(configuration.EndPoint)
                    .Build())
                .Build();

            // event handlers
            _client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(Client_OnMessage);
            _client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(Client_OnConnected);
            _client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(Client_OnDisconnected);

            _packetHandlersContainer.Registered += HandlersContainer_OnRegistered;
            _packetHandlersContainer.Unregistered += HandlersContainer_OnUnregistered;
        }

        private static Task Client_OnDisconnected(MqttClientDisconnectedEventArgs arg)
        {
            Console.WriteLine("[SERVER] Disconnected !");
            return Task.CompletedTask;
        }

        private static Task Client_OnConnected(MqttClientConnectedEventArgs arg)
        {
            Console.WriteLine($"[SERVER] Connected !");
            return Task.CompletedTask;
        }

        private void HandlersContainer_OnRegistered(object sender, Type type)
        {
            CheckRouting(type).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void HandlersContainer_OnUnregistered(object sender, Type type)
        {
            IRoutingInformation infos = _router.GetRoutingInformations(type);
            _client.UnsubscribeAsync(infos.Topic).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task Client_OnMessage(MqttApplicationMessageReceivedEventArgs message)
        {
            Console.WriteLine("Received message from topic : " + message.ApplicationMessage.Topic);
            MessageReceived.WithLabels(_options.ClientOptions.ClientId, message.ApplicationMessage.Topic).Inc();
            var container = _serializer.Deserialize<PacketContainer>(message.ApplicationMessage.Payload);
            object packet = JsonConvert.DeserializeObject(container.Content, container.Type);
            switch (packet)
            {
                case IAsyncRpcRequest request:
                    await _packetHandlersContainer.HandleAsync(request, container.Type);
                    break;
            }
        }


        private async Task<IRoutingInformation> CheckRouting(Type type)
        {
            IRoutingInformation routingInfos = _router.GetRoutingInformations(type);
            Console.WriteLine($"[SERVER] Adding topic : {routingInfos.Topic}");
            if (string.IsNullOrEmpty(routingInfos.Topic) || _queues.Contains(routingInfos.Topic))
            {
                return routingInfos;
            }

            await _client.SubscribeAsync(routingInfos.Topic);
            _queues.Add(routingInfos.Topic);
            return routingInfos;
        }

        public async Task StartAsync()
        {
            if (IsInitialized)
            {
                return;
            }

            Console.WriteLine($"[SERVER] Connecting...");
            await _client.StartAsync(_options);
        }

        public async Task StopAsync()
        {
            if (IsInitialized)
            {
                return;
            }

            await _client.StopAsync();
        }
    }
}
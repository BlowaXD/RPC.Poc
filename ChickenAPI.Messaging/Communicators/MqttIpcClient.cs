using System;
using System.Threading.Tasks;
using ChickenAPI.Messaging.Configs;
using ChickenAPI.Messaging.Protocol;
using ChickenAPI.Messaging.Routing;
using ChickenAPI.Messaging.Serializers;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Diagnostics;
using MQTTnet.Extensions.ManagedClient;
using Prometheus;

namespace ChickenAPI.Messaging.Communicators
{
    public sealed class MqttIpcClient : IRpcClient
    {
        private static readonly Counter MessageSent = Metrics.CreateCounter("MessageSent", "Numbers of MQTT messages sent to the broker", new CounterConfiguration
        {
            LabelNames = new[] { "clientId", "topic" }
        });
        private readonly IIpcPacketRouter _router;
        private readonly IIpcSerializer _serializer;
        private readonly IPacketContainerFactory _packetFactory;
        private readonly IManagedMqttClient _client;
        private readonly IManagedMqttClientOptions _options;
        private bool IsInitialized => _client.IsStarted;

        public MqttIpcClient(MqttClientConfigurationBuilder builder, IIpcPacketRouter router, IIpcSerializer serializer) : this(builder.Build(), router, serializer)
        {
        }

        public MqttIpcClient(MqttIpcClientConfiguration conf, IIpcPacketRouter router, IIpcSerializer serializer)
        {
            _router = router;
            _serializer = serializer;
            _packetFactory = new PacketContainerFactory();
            _client = new MqttFactory().CreateManagedMqttClient(new MqttNetLogger(conf.ClientName));
            _options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(conf.ClientName)
                    .WithTcpServer(conf.EndPoint)
                    .Build())
                .Build();

            _client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(Client_OnConnected);
            _client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(Client_OnDisconnected);
        }


        private IRoutingInformation GetRoutingInformations<T>()
        {
            return _router.GetRoutingInformations(typeof(T));
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

        private async Task SendAsync<T>(PacketContainer container)
        {
            IRoutingInformation infos = GetRoutingInformations<T>();
            MessageSent.WithLabels(_options.ClientOptions.ClientId, infos.Topic).Inc();
            await _client.PublishAsync(builder =>
                builder
                    .WithPayload(_serializer.Serialize(container))
                    .WithTopic(infos.Topic)
                    .WithExactlyOnceQoS()
            );

            Console.WriteLine("Sent message to topic : " + infos.Topic);
        }

        public Task BroadcastAsync<T>(T packet) where T : IAsyncRpcRequest
        {
            PacketContainer container = _packetFactory.ToPacket(packet);
            return SendAsync<T>(container);
        }

        public async Task StartAsync()
        {
            if (IsInitialized)
            {
                return;
            }

            Console.WriteLine($"[CLIENT] Connecting...");
            await _client.StartAsync(_options);
        }

        public async Task StopAsync()
        {
            if (!IsInitialized)
            {
                return;
            }

            await _client.StopAsync();
        }
    }
}
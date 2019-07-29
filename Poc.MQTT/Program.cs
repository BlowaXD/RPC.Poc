using System;
using System.Threading.Tasks;
using ChickenAPI.Messaging.Communicators;
using ChickenAPI.Messaging.Configs;
using ChickenAPI.Messaging.Routing;
using ChickenAPI.Messaging.Serializers;
using ChickenAPI.Messaging.Utils;
using MongoDB.Driver;
using Prometheus;

namespace Poc.MQTT
{
    internal class Program
    {
        private static async Task Main()
        {
            const int characterId = 1;
            var actionFactory = new PlayerActionFactory();
            IPlayerActionLogger logger = new MongoPlayerActionLogger(new MongoClient());

            var packetHandlerContainer = new PacketHandlersContainer();
            IRpcClient rpcClient = new MqttIpcClient(
                new MqttClientConfigurationBuilder().ConnectTo("localhost").WithName("world-server"),
                new RuntimeSmartIpcRouter(new RoutingInformationFactory()),
                new JsonSerializer());
            var rpcServer = new MqttIpcServer(
                new MqttServerConfigurationBuilder().ConnectTo("localhost").WithName("logging-service"),
                new JsonSerializer(),
                new RuntimeSmartIpcRouter(new RoutingInformationFactory()), packetHandlerContainer);
            await rpcClient.StartAsync();
            await rpcServer.StartAsync();
            packetHandlerContainer.Register(new LoggingMessageHandler<PlayerChatAction>(logger), typeof(LoggingMessage<PlayerChatAction>));
            packetHandlerContainer.Register(new LoggingMessageHandler<GroupInvitedAction>(logger), typeof(LoggingMessage<GroupInvitedAction>));
            var metricServer = new MetricServer("localhost", 3000);
            metricServer.Start();
            await Task.Delay(2500);
            for (int i = 0; i < 10; i++)
            {
                PlayerChatAction chatAction = actionFactory.CreateChatAction(characterId);
                await rpcClient.BroadcastAsync(new LoggingMessage<PlayerChatAction>(chatAction));
                await Task.Delay(1000);
                GroupInvitedAction groupInvitedAction = actionFactory.CreateGroupInvitedAction(characterId);
                await rpcClient.BroadcastAsync(new LoggingMessage<GroupInvitedAction>(groupInvitedAction));
                await Task.Delay(1000);
            }

            await rpcClient.StopAsync();
            await rpcServer.StopAsync();
            // wait for kill to keep prometheus stats
            Console.ReadLine();
            await metricServer.StopAsync();
        }
    }
}
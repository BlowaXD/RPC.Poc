using System;

namespace Poc.MQTT
{
    public class PlayerChatAction : IPlayerAction
    {
        public long CharacterId { get; set; }
        public DateTime ActionTime { get; set; }
    }
}
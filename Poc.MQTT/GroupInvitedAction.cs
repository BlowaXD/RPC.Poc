using System;

namespace Poc.MQTT
{
    public class GroupInvitedAction : IPlayerAction
    {
        public long CharacterId { get; set; }
        public DateTime ActionTime { get; set; }
    }
}
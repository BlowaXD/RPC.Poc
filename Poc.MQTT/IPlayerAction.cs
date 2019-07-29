using System;

namespace Poc.MQTT
{
    public interface IPlayerAction
    {
        long CharacterId { get; }
        DateTime ActionTime { get; }
    }
}
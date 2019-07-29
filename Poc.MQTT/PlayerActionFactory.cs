using System;

namespace Poc.MQTT
{
    public class PlayerActionFactory
    {
        public PlayerChatAction CreateChatAction(int characterId)
        {
            return new PlayerChatAction
            {
                CharacterId = characterId,
                ActionTime = DateTime.Now
            };
        }

        public GroupInvitedAction CreateGroupInvitedAction(int characterId)
        {
            return new GroupInvitedAction
            {
                CharacterId = characterId,
                ActionTime = DateTime.Now
            };
        }
    }
}
namespace ChickenAPI.Messaging.Serializers
{
    public interface IIpcSerializer
    {
        /// <summary>
        /// Serializes the given packet to a byteArray to facilitate MQTT transportation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packet"></param>
        /// <returns></returns>
        byte[] Serialize<T>(T packet);

        /// <summary>
        /// Deserializes the given byte array to a PacketContainer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        T Deserialize<T>(byte[] buffer);
    }
}
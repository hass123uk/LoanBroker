namespace MessageGateway.SendReceiveGateway
{
    public interface IMessageSender
    {
        void Send<T>(T message, string sendQueueId) where T : class;
    }
}

using System;
using EasyNetQ;

namespace MessageGateway.SendReceiveGateway.Impl
{
    public class MessageSenderGateway : IMessageSender, IDisposable
    {
        private readonly IBus _bus;

        public MessageSenderGateway()
        {
            _bus = RabbitHutch.CreateBus("host=localhost");
        }
        public void Send<T>(T message, string sendQueueId) where T : class
        {
            _bus.Send<T>(sendQueueId, message);
        }
        public void Dispose()
        {
            _bus?.Dispose();
        }
    }
}
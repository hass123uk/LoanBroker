using System;
using EasyNetQ;

namespace MessageGateway.SendReceiveGateway.Impl
{
    public class MessageReceiverGateway : IMessageReciever, IDisposable
    {
        private readonly IBus _bus;

        public MessageReceiverGateway()
        {
            _bus = RabbitHutch.CreateBus("host=localhost");
        }

        public void Recieve<T>(string replayQueueId, Action<T> action) where T : class
        {
            _bus.Receive<T>(replayQueueId, m => action.Invoke(m));
        }

        public void Dispose()
        {
            _bus?.Dispose();
        }
    }
}

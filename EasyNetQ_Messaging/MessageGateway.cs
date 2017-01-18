using System;
using System.Threading.Tasks;
using EasyNetQ;

namespace EasyNetQ_Messaging
{
    public class MessageGateway : IDisposable
    {
        private readonly IBus _bus;

        public MessageGateway(string connectionString)
        {
            _bus = RabbitHutch.CreateBus(connectionString);
        }

        public void Send<T>(T message, string sendQueueId) where T : class
        {
            _bus.Send<T>(sendQueueId, message);
        }

        public void Receive<T>(string replayQueueId, Action<T> action) where T : class
        {
            _bus.Receive<T>(replayQueueId, m => action.Invoke(m));
        }

        public TResponse Request<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class
        {
            return _bus.Request<TRequest, TResponse>(request);
        }

        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class
        {
            return _bus.RequestAsync<TRequest, TResponse>(request);
        }

        public void Respond<TRequest, TResponse>(Func<TRequest, TResponse> requestResponseFunction)
            where TRequest : class
            where TResponse : class
        {
            _bus.Respond(requestResponseFunction);
        }

        public void RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> requestResponseFunction)
            where TRequest : class
            where TResponse : class
        {
            _bus.RespondAsync(requestResponseFunction);
        }

        public void Subscribe<T>(string subscriptionId, Action<T> onMessage) where T : class
        {
            _bus.Subscribe(subscriptionId, onMessage);
        }

        public void Publish<T>(T message) where T : class
        {
            _bus.Publish(message);
        }

        public void Dispose()
        {
            _bus?.Dispose();
        }
    }
}
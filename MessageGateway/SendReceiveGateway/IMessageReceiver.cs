using System;

namespace MessageGateway.SendReceiveGateway
{
    public interface IMessageReciever
    {
        //void OnMessage { get; set; }
        void Recieve<T>(string replayQueueId, Action<T> action) where T : class;
    }
}

using System;
using MessageGateway.Messages;
using MessageGateway.SendReceiveGateway.Impl;

namespace Customer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Write a Message to send to the bank: ");
            var text = Console.ReadLine();
            var message = new TextMessage { Text = text };
            using (var gateway = new MessageSenderGateway())
            {
                gateway.Send(message, "bank");
            }
        }
    }
}

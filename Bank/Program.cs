using System;
using MessageGateway.Messages;
using MessageGateway.SendReceiveGateway.Impl;

namespace Bank
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var gateway = new MessageReceiverGateway())
            {
                gateway.Recieve<TextMessage>("bank", PrintMessage);
                Console.ReadLine();
            }
        }

        public static void PrintMessage(TextMessage textMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(textMessage.Text);
        }
    }
}

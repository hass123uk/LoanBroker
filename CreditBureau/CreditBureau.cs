using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using EasyNetQ_Messaging;
using Messages.CreditBureau;

namespace CreditBureau
{
    public class CreditBureau
    {
        public static void Main(string[] args)
        {
            const int numberOfWorkers = 10;
            var workers = new BlockingCollection<CreditBureauWorker>();
            for (var i = 0; i < numberOfWorkers; i++)
            {
                workers.Add(new CreditBureauWorker());
            }
            Console.WriteLine("Credit Bureau application started. " +
                              $"Amount of workers: {numberOfWorkers}");

            using (var gateway = new MessageGateway("host=localhost;timeout=60"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Listening for requests.");
                Console.ResetColor();

                gateway.RespondAsync<CreditBureauRequest, CreditBureauReply>(request =>
                Task.Factory.StartNew(() =>
                {
                    var worker = workers.Take();
                    Console.WriteLine("Received a request. " +
                              $"Current amount of workers: {numberOfWorkers}");
                    try
                    {
                        var reply = worker.HandleRequest(request);
                        Console.WriteLine($"Request handleded for CPR Number: {reply.CprNr}");
                        Console.WriteLine($"Length of Credit History: {reply.HistoryLength}");
                        Console.WriteLine($"Credit Score: {reply.CreditScore}");
                        return reply;
                    }
                    finally
                    {
                        workers.Add(worker);
                    }
                }));

                ConsoleKeyInfo key;
                do
                {
                    Console.WriteLine("Press ESC to exit application.");
                    key = Console.ReadKey(true);
                } while (key.Key != ConsoleKey.Escape);
            }
        }
    }

    public class CreditBureauWorker
    {
        public CreditBureauReply HandleRequest(CreditBureauRequest request)
        {
            return new CreditBureauReply
            {
                CprNr = request.CprNr,
                CreditScore = GetCreditScore(request.CprNr),
                HistoryLength = GetCreditHistoryLength(request.CprNr)
            };
        }

        private static int GetCreditHistoryLength(long cprNr)
        {
            return new Random().Next(600) + 300;
        }

        private static int GetCreditScore(long cprNr)
        {
            return new Random().Next(19) + 12;
        }
    }
}

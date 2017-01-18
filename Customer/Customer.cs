using EasyNetQ_Messaging;
using Messages.LoanBroker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Customer
{
    public class Customer
    {
        private static MessageGateway _messageGateway;
        private static readonly string CustomerAppId = $"customer.{ Guid.NewGuid()}";

        public static void Main(string[] args)
        {
            Console.WriteLine("Customer application started.");
            var cts = new CancellationTokenSource();
            try
            {
                _messageGateway = new MessageGateway("host=localhost;timeout=60");
                _messageGateway.Receive<LoanQuoteReply>(CustomerAppId, HandleQuoteReply);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Started Listening for Loan Quote Replies.");
                Console.ResetColor();
                
                ConsoleKeyInfo key;
                do
                {
                    Console.WriteLine("Press: 'S' to send two test messages, ESC to exit application.");
                    key = Console.ReadKey(true);

                    if (key.Key != ConsoleKey.S) continue;

                    Task.Factory.StartNew(() =>
                    {
                        var loanQuoteRequest = new LoanQuoteRequest()
                        {
                            CprNr = 1909991111,
                            LoanAmount = 10000,
                            LoanTerm = 46,
                            ReplyQueueId = CustomerAppId
                        };
                        cts.Token.ThrowIfCancellationRequested();
                        _messageGateway.Send(loanQuoteRequest, "loan.broker.customer.request");
                        Console.WriteLine($"Loan Request Sent for {loanQuoteRequest.CprNr}");
                    }, cts.Token);

                    Task.Factory.StartNew(() =>
                    {
                        var loanQuoteRequest = new LoanQuoteRequest()
                        {
                            CprNr = 1909992222,
                            LoanAmount = 5000,
                            LoanTerm = 12,
                            ReplyQueueId = CustomerAppId
                        };
                        cts.Token.ThrowIfCancellationRequested();
                        _messageGateway.Send(loanQuoteRequest, "loan.broker.customer.request");
                        Console.WriteLine($"Loan Request Sent for {loanQuoteRequest.CprNr}");
                    }, cts.Token);

                } while (key.Key != ConsoleKey.Escape);

                cts.Cancel();
            }
            catch (AggregateException e)
            {
                foreach (var ie in e.InnerExceptions)
                    PrintErrorToConsole($"{ie.GetType().Name}: {ie.Message}");
            }
            catch (Exception ex)
            {
                PrintErrorToConsole(ex.Message);
            }
            finally
            {
                cts.Cancel();
                _messageGateway.Dispose();
            }
        }

        private static void PrintErrorToConsole(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void HandleQuoteReply(LoanQuoteReply loanQuoteReply)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Interest Rate: " + loanQuoteReply.InterestRate);
            Console.WriteLine("Quote Id: " + loanQuoteReply.QuoteId);
            Console.ResetColor();
        }
    }
}

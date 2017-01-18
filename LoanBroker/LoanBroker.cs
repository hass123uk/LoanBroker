using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using EasyNetQ_Messaging;
using Messages.Bank;
using Messages.CreditBureau;
using Messages.LoanBroker;

namespace LoanBroker
{
    public class LoanBroker
    {
        private static ConcurrentDictionary<long, LoanQuoteRequest> _loanQuoteRequests;
        private static ConcurrentDictionary<long, List<BankQuoteReply>> _bankQuoteReplies;
        private static MessageTransformer _messageTransformer;

        private static void Main(string[] args)
        {
            _messageTransformer = new MessageTransformer();
            _loanQuoteRequests = new ConcurrentDictionary<long, LoanQuoteRequest>();
            _bankQuoteReplies = new ConcurrentDictionary<long, List<BankQuoteReply>>();
            Console.WriteLine("Loan Broker application started.");

            using (var messageGateway = new MessageGateway("host=localhost;timeout=60"))
            {
                messageGateway.Receive<LoanQuoteRequest>("loan.broker.customer.request", HandleLoanQuoteRequest);
                messageGateway.Receive<BankQuoteReply>("loan.broker.bank.reply", HandleBankQuoteReply);


                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Listening on customer request queue.");
                Console.WriteLine("Listening on bank reply queue.");
                Console.ResetColor();
                ConsoleKeyInfo key;
                do
                {
                    Console.WriteLine("Press ESC to exit application.");
                    key = Console.ReadKey(true);
                } while (key.Key != ConsoleKey.Escape);
            }
        }

        private static void HandleLoanQuoteRequest(LoanQuoteRequest loanQuoteRequest)
        {
            _loanQuoteRequests.TryAdd(loanQuoteRequest.CprNr, loanQuoteRequest);
            Console.WriteLine($"Received Loan request regarding Cpr.Nr {loanQuoteRequest.CprNr}");
            //Message filtering
            var creditBureauRequest = _messageTransformer.FilterMessage(loanQuoteRequest);

            Console.WriteLine($"Sent credit bureau request regarding Cpr.Nr {loanQuoteRequest.CprNr}");

            using (var messageGateway = new MessageGateway("host=localhost;timeout=60"))
            {
                var reply = messageGateway.Request<CreditBureauRequest, CreditBureauReply>(creditBureauRequest);
                HandleCreditBureauReply(reply);
            }
        }

        private static void HandleCreditBureauReply(CreditBureauReply creditBureauReply)
        {
            Console.WriteLine($"Received credit bureau reply regarding Cpr.Nr {creditBureauReply.CprNr}");
            LoanQuoteRequest loanRequest;
            if (_loanQuoteRequests.TryGetValue(creditBureauReply.CprNr, out loanRequest))
            {
                //Content Enriching
                var bankQuoteRequest = _messageTransformer.EnrichMessage(creditBureauReply, loanRequest);
                using (var messageGateway = new MessageGateway("host=localhost;timeout=60"))
                {
                    messageGateway.Publish(bankQuoteRequest);
                }
                Console.WriteLine($"Sent request to all banks regarding Cpr.Nr {bankQuoteRequest.CprNr}");


                _bankQuoteReplies.TryAdd(bankQuoteRequest.CprNr, new List<BankQuoteReply>());
                var timer = new Timer(Timeout_Elapsed, bankQuoteRequest, 5000, Timeout.Infinite);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Customer quote request could not be found when forwarding their details to the banks!");
                Console.ResetColor();
            }
        }

        private static void Timeout_Elapsed(object message)
        {
            var bankQuoteRequest = message as BankQuoteRequest;
            if (bankQuoteRequest == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("State object in bank request timeout is null.");
                Console.ResetColor();
            }
            else
            {
                List<BankQuoteReply> bankQuoteReplies;
                LoanQuoteRequest loanRequest;
                if (_bankQuoteReplies.TryGetValue(bankQuoteRequest.CprNr, out bankQuoteReplies)
                    && _loanQuoteRequests.TryGetValue(bankQuoteRequest.CprNr, out loanRequest))
                {

                    if (bankQuoteReplies.Count > 0)
                    {
                        var bestBankQuoteReply = bankQuoteReplies[0];

                        var min = 0.0;
                        bankQuoteReplies
                            .FindAll(reply => reply.ErrorCode != 1)
                            .ForEach(reply =>
                            {
                                if (!(min > reply.InterestRate)) return;
                                min = reply.InterestRate;
                                bestBankQuoteReply = reply;
                            });

                        //Content Enriching
                        var ĺoanQuoteReply = _messageTransformer.EnrichMessage(bestBankQuoteReply, loanRequest);
                        ReplyToLoanRequest(loanRequest, ĺoanQuoteReply);
                    }
                    else
                    {
                        var negativeReply = new LoanQuoteReply
                        {
                            CprNr = loanRequest.CprNr,
                            QuoteId = "ERROR_1_NoReplies"
                        };

                        ReplyToLoanRequest(loanRequest, negativeReply);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"No Loan requests or Bank replies were found regarding Cpr.Nr: {bankQuoteRequest.CprNr}.");
                    Console.ResetColor();
                }
            }
        }

        private static void ReplyToLoanRequest(LoanQuoteRequest loanRequest, LoanQuoteReply ĺoanQuoteReply)
        {
            using (var messageGateway = new MessageGateway("host=localhost;timeout=60"))
            {
                messageGateway.Send(ĺoanQuoteReply, loanRequest.ReplyQueueId);
            }
            LoanQuoteRequest removedCustomer;
            List<BankQuoteReply> removedBankQuoteReplies;
            _loanQuoteRequests.TryRemove(loanRequest.CprNr, out removedCustomer);
            _bankQuoteReplies.TryRemove(loanRequest.CprNr, out removedBankQuoteReplies);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Resolved Loan request regarding Cpr.Nr {loanRequest.CprNr}.");
            Console.ResetColor();
        }

        private static void HandleBankQuoteReply(BankQuoteReply bankQuoteReply)
        {
            List<BankQuoteReply> bankQuoteReplies;
            if (_bankQuoteReplies.TryGetValue(bankQuoteReply.CprNr, out bankQuoteReplies))
            {
                var readOnlyReplies = bankQuoteReplies;
                bankQuoteReplies.Add(bankQuoteReply);
                if (_bankQuoteReplies.TryUpdate(bankQuoteReply.CprNr, bankQuoteReplies, readOnlyReplies))
                    Console.WriteLine($"Received a reply from a bank regarding {bankQuoteReply.CprNr}. " +
                                      $"Current Number of replies {bankQuoteReplies.Count}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unable to find a record regarding Cpr.Nr: {bankQuoteReply.CprNr}.");
                Console.ResetColor();
            }
        }
    }
}

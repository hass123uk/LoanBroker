using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ_Messaging;
using Messages.Bank;

namespace Bank
{
    public class Bank
    {
        private readonly string _bankName;
        private readonly int _maxLoanTerm;
        private readonly double _primeRate;
        private readonly double _ratePremium;
        private readonly Random _random;
        private readonly string _connectionString;
        private readonly string _bankAppId;
        private int _quoteCounter;


        public Bank(string bankName, int maxLoanTerm, double primeRate, double ratePremium)
        {
            _bankName = bankName;
            _maxLoanTerm = maxLoanTerm;
            _primeRate = primeRate;
            _ratePremium = ratePremium;

            _random = new Random();
            _connectionString = "host=localhost;timeout=60";
            _bankAppId = $"bank.{Guid.NewGuid()}";
            _quoteCounter = 0;
        }

        public void Start()
        {
            using (var gateway = new MessageGateway(_connectionString))
            {
                gateway.Subscribe<BankQuoteRequest>(_bankAppId, ProcessMessage);

                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private void SendBankQuoteReply(BankQuoteReply bankQuoteReply)
        {
            using (var gateway = new MessageGateway(_connectionString))
            {
                gateway.Send(bankQuoteReply, "loan.broker.bank.reply");
            }
        }

        private void ProcessMessage(BankQuoteRequest bankQuoteRequest)
        {
            var bankQuoteReply = ComputeBankReply(bankQuoteRequest);

            SynchronizedWriteLine(
                "Received request for Cpr.Nr " +
                $"{bankQuoteRequest.CprNr} " +
                $"for {bankQuoteRequest.LoanAmount:c} " +
                $"/ {bankQuoteRequest.LoanTerm} months");

            Thread.Sleep(_random.Next(10) * 100);

            SynchronizedWriteLine("Quote: " +
                              $"{bankQuoteReply.ErrorCode} " +
                              $"{bankQuoteReply.InterestRate} " +
                              $"{bankQuoteReply.QuoteId}");

            if (bankQuoteReply.ErrorCode != 0) return;

            SendBankQuoteReply(bankQuoteReply);
            SynchronizedWriteLine("Replied to Loan broker " +
                                  $"regarding Cpr.Nr {bankQuoteRequest.CprNr}");
        }

        private BankQuoteReply ComputeBankReply(BankQuoteRequest requestStruct)
        {
            var replyStruct = new BankQuoteReply
            {
                CprNr = requestStruct.CprNr
            };

            if (requestStruct.LoanTerm <= _maxLoanTerm)
            {
                replyStruct.InterestRate = _primeRate + _ratePremium
                                           + (double)requestStruct.LoanTerm / 12 / 10
                                           + (double)_random.Next(10) / 10;
                replyStruct.ErrorCode = 0;
            }
            else
            {
                replyStruct.InterestRate = 0.0;
                replyStruct.ErrorCode = 1;
            }
            replyStruct.QuoteId = $"{_bankName}-{_quoteCounter:00000}";
            _quoteCounter++;
            return replyStruct;
        }

        private void SynchronizedWriteLine(string s)
        {
            lock (this)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(s);
                Console.ResetColor();
            }
        }
    }
}

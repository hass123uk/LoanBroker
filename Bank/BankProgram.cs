using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ_Messaging;
using Messages.Bank;

namespace Bank
{
    public class BankProgram
    {

        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Bank application started.");
            Console.WriteLine("Starting the following banks:\n");

            var bank1 = CreateBank("HSBC", 12, 3, 2);
            var bank2 = CreateBank("DanskBank", 82, 5, 2);
            var bank3 = CreateBank("Nordea", 36, 4, 2);

            Console.ResetColor();
            Task.Factory.StartNew(() => bank1.Start());
            Task.Factory.StartNew(() => bank2.Start());
            Task.Factory.StartNew(() => bank3.Start());
            
            ConsoleKeyInfo key;
            do
            {
                Console.WriteLine("Press ESC to exit application.");
                key = Console.ReadKey(true);
            }
            while (key.Key != ConsoleKey.Escape);
        }

        private static Bank CreateBank(string bankName, int maxLoanTerm, double primeRate, double ratePremium)
        {
            Console.WriteLine($"Bank Name: {bankName} - Max Loan Term: {maxLoanTerm} - " +
            $"Prime Rate: {primeRate} - Rate Premium: {ratePremium}");

            return new Bank(bankName, maxLoanTerm, primeRate, ratePremium);
        }
    }
}

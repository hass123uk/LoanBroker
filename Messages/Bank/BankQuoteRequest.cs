namespace Messages.Bank
{
    public class BankQuoteRequest
    {
        public long CprNr { get; set; }

        public int CreditScore { get; set; }

        public int HistoryLength { get; set; }

        public double LoanAmount { get; set; }

        public int LoanTerm { get; set; }
    }
}

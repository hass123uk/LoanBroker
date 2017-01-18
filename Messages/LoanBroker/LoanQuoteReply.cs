namespace Messages.LoanBroker
{
    public class LoanQuoteReply
    {
        public long CprNr { get; set; }

        public double LoanAmount { get; set; }

        public double InterestRate { get; set; }

        public string QuoteId { get; set; }
    }
}

namespace Messages.Bank
{
    public class BankQuoteReply
    {
        public long CprNr { get; set; }

        public double InterestRate { get; set; }

        public string QuoteId { get; set; }

        public int ErrorCode { get; set; }
    }
}

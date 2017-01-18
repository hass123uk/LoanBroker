namespace Messages.LoanBroker
{
    public class LoanQuoteRequest
    {
        public int CprNr { get; set; }
        public double LoanAmount { get; set; }
        public int LoanTerm { get; set; }
        public string ReplyQueueId { get; set; }
    }
}

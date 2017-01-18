using Messages.Bank;
using Messages.CreditBureau;
using Messages.LoanBroker;

namespace LoanBroker
{
    public class MessageTransformer
    {
        public CreditBureauRequest FilterMessage(LoanQuoteRequest loanQuoteRequest)
        {
            return new CreditBureauRequest
            {
                CprNr = loanQuoteRequest.CprNr
            };
        }

        public BankQuoteRequest EnrichMessage(CreditBureauReply creditBureauReply, LoanQuoteRequest loanRequest)
        {
            return new BankQuoteRequest
            {
                CprNr = creditBureauReply.CprNr,
                CreditScore = creditBureauReply.CreditScore,
                HistoryLength = creditBureauReply.HistoryLength,
                LoanAmount = loanRequest.LoanAmount,
                LoanTerm = loanRequest.LoanTerm
            };
        }

        public LoanQuoteReply EnrichMessage(BankQuoteReply bestBankQuoteReply, LoanQuoteRequest loanRequest)
        {
            return new LoanQuoteReply
            {
                CprNr = bestBankQuoteReply.CprNr,
                InterestRate = bestBankQuoteReply.InterestRate,
                LoanAmount = loanRequest.LoanAmount,
                QuoteId = bestBankQuoteReply.QuoteId
            };
        }
    }
}

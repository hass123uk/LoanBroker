using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoanBroker.Api.Models
{
    public class LoanQuoteReplyModel
    {
        public long CprNr { get; set; }

        public double LoanAmount { get; set; }

        public double InterestRate { get; set; }

        public string QuoteId { get; set; }
    }
}
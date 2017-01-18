using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoanBroker.Api.Models
{
    public class LoanQuoteRequestModel
    {
        public int CprNr { get; set; }
        public double LoanAmount { get; set; }
        public int LoanTerm { get; set; }
        public string ReplyQueueId { get; set; }
    }
}
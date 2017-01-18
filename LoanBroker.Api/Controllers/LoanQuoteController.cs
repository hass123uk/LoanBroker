
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LoanBroker.Api.Models;

namespace LoanBroker.Api.Controllers
{
    public class LoanQuoteController : ApiController
    {
        public HttpResponseMessage Get(LoanQuoteRequestModel model)
        {


            return Request.CreateResponse(HttpStatusCode.OK, new LoanQuoteReplyModel());
        }
    }
}

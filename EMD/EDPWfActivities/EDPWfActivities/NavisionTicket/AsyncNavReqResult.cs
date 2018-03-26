using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.WFActivity.NavisionTicket
{
    public enum EnumNavTicketPollingStatus { angelegt, beendet, storno, nochange, empty, error, timeoutAngelegt, timeoutBeendet, bearbeitung_v, wartet_k, bearbeitung_r };
   
    public class AsyncNavReqResult : BaseAsyncRequestResult
    {
        public string ReturnValue { get; set; }
        public EnumNavTicketPollingStatus NavisionTicketStatus { get; set; }
        public string DetailedMessage { get; set; }

        public AsyncNavReqResult(string returnVal, EnumNavTicketPollingStatus navTicketStatus)
        {
            this.ReturnValue = returnVal;
            this.NavisionTicketStatus = navTicketStatus;
            this.DetailedMessage = "";
        }

        
    }
}

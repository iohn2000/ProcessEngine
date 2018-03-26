using Kapsch.IS.Util.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.WFActivity
{
    public class WorkflowMailer
    {
        IEmailSender emailer = new WebServiceEmailSender();
        string subjectPrefix = string.Empty;

        public WorkflowMailer(string subjectPrefix)
        {
            if (subjectPrefix == null)    
                this.subjectPrefix = string.Empty;
            else
                this.subjectPrefix = subjectPrefix;
        }

        public Guid SendEmail(string from, List<string> to, string subject, string body, bool isBodyHtml)
        {
            return emailer.SendEmail(from, to, this.subjectPrefix + " " + subject, body, isBodyHtml);
        }

    }
}


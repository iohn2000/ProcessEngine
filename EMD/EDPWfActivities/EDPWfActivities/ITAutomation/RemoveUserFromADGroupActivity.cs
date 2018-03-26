using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.WFActivity.ITAutomation
{
    public class RemoveUserFromADGroupActivity : AddUserToADGroupActivity
    {
        public override string TaskName { get { return "RemoveUserFromGroup"; } }
    }
}

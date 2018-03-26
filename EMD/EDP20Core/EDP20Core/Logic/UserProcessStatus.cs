using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class UserProcessStatus : ProcessStatus
    {
        public const int STATUSITEM_RESERVED = 70;

        public UserProcessStatus()
        {
            base.statusItems.Add(new StatusItem(STATUSITEM_RESERVED, "UserReserved", "Username is reserved"));
        }
    }
}

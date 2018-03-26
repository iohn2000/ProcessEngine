using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework
{
    public class SecurityUserParameterFlags
    {
        public bool IsItself { get; set; }
        public bool IsLineManager { get; set; }
        public bool IsTeamLeader { get; set; }
        public bool IsCostcenterManager { get; set; }
        public bool IsAssistence { get; set; }

        public bool CheckPlainPermission { get; set; }
        public SecurityUserParameterFlags()
        {
            IsItself = false;
            IsLineManager = false;
            IsCostcenterManager = false;
            IsAssistence = false;
            CheckPlainPermission = false;
        }

        public SecurityUserParameterFlags(bool isItself = false, bool isLineManager = false, bool isTeamLeader = false, bool isCostcenterManager = false, bool isAssistence = false, bool checkPlainPermisson = false)
        {
            this.IsItself = isItself;
            this.IsLineManager = isLineManager;
            this.IsTeamLeader = isTeamLeader;
            this.IsCostcenterManager = isCostcenterManager;
            this.IsAssistence = isAssistence;
            this.CheckPlainPermission = checkPlainPermisson;
        }

        public override string ToString()
        {
            return string.Format("Flags:{0}|{1}|{2}|{3}|{4}|{5}", this.IsItself, this.IsLineManager, this.IsTeamLeader, this.IsCostcenterManager, this.IsAssistence, this.CheckPlainPermission);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Report
{
    public class OrgUnitLevels: EDPReport<OrgUnitLevels>
    {
        #region constructor
        public OrgUnitLevels()
        {
            //empty
        }
        #endregion

        #region variables
            //empty
        #endregion

        #region interface

        public override OrgUnitLevels Query()
        {
            this.SetViewName(); //view name based on classname .. :)
            return base.Query();
        }

        #endregion
    }
}

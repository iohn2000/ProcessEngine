using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.WF.Message
{
    /// <summary>
    /// Interface for workflow objects enhances attributes which describes old an new employment informations
    /// </summary>
    public interface IEmploymentInfos
    {
        string CostcenterOldGuid { get; set; }
        string CostcenterNewGuid { get; set; }
        string CostCenterResponsibleOldEmplGuid { get; set; }
        string CostCenterResponsibleNewEmplGuid { get; set; }
        string OrgunitOldGuid { get; set; }
        string OrgunitNewGuid { get; set; }
        string LineManagerOldEmplGuid { get; set; }
        string LineManagerNewEmplGuid { get; set; }
        string TeamleaderOldEmplGuid { get; set; }
        string TeamleaderNewEmplGuid { get; set; }
        string AssistanceOldEmplGuid { get; set; }
        string AssistanceNewEmplGuid { get; set; }
        string PersNrOld { get; set; }
        string PersNrNew { get; set; }
        string LocationOldGuid { get; set; }
        string LocationNewGuid { get; set; }
    }
}

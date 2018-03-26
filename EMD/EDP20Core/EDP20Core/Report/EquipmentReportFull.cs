using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Linq;

namespace Kapsch.IS.EDP.Core.Report
{
    public class EquipmentReportFull : EDPReport<EquipmentReportFull>
    {
        #region constructor
        public EquipmentReportFull()
        {
        }
        #endregion
        /// <summary>
        /// EquipmentGuids that will be filtered for report
        /// </summary>
        public string[] EquipmentDefinitionGuids { get; private set; }
        /// <summary>
        /// Set if equipment filter is set for query
        /// </summary>
        public bool FilterApplied { get; private set; }

        #region setter
        /// <summary>
        /// Setter for property EquipmentDefinitionGuids
        /// </summary>
        /// <param name="equipmentDefinitionGuid"> Guid of the EquipmentDefinition which shall be reported</param>
        /// <returns></returns>
        public EquipmentReportFull SetEquipmentGuidsFilter(string[] equipmentDefinitionGuids)
        {
            this.FilterApplied = true;
            this.EquipmentDefinitionGuids = equipmentDefinitionGuids;
           
            return this;
        }
        #endregion

        #region interface
        /// <summary>
        /// Prepares the sql query
        /// </summary>
        /// <returns></returns>
        public override EquipmentReportFull Query()
        {
            this.SetViewName("Rep_EquipmentReport"); //view name based on classname .. :)

            if (this.FilterApplied)
            {
                string whereClauseValues = string.Empty;
                foreach (string guid in this.EquipmentDefinitionGuids)
                {
                    if (whereClauseValues == string.Empty)
                        whereClauseValues += "'" + guid + "'";
                    else
                        whereClauseValues += ",'" + guid + "'";
                }
                this.SetQueryString(String.Format("EquipmentDefinitionGuid in ({0})", whereClauseValues));
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "EquipmentGuidsFilter not set!");
            }
            
            return base.Query();
        }

        #endregion
    }
}

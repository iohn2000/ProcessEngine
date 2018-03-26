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

namespace Kapsch.IS.EDP.Core.Report
{
    public class EquipmentReport : EDPReport<EquipmentReport>
    {
        #region constructor
        public EquipmentReport()
        {
            //this.Fields.Add("FamilyName");
            //this.Fields.Add("FirstName");
            //this.Fields.Add("EquipmentStatus");
            //this.Fields.Add("LastStatusChange");
            //this.Fields.Add("EquipmentGuid");
            //this.Fields.Add("EquipmentDefinitionGuid");
            //this.Fields.Add("EquipmentDefinitionName");

        }
        #endregion

        #region variables

        public string EquipmentDefinitionGuid { get; private set; }

        public EMDEquipmentDefinition EquipmentDefinition { get; private set; }

        #endregion

        #region setter
        /// <summary>
        /// Setter for property EquipmentGuid
        /// </summary>
        /// <param name="equipmentDefinitionGuid"> Guid of the EquipmentDefinition which shall be reported</param>
        /// <returns></returns>
        public EquipmentReport SetEquipmentGuid(string equipmentDefinitionGuid)
        {
            this.EquipmentDefinitionGuid = equipmentDefinitionGuid;
            try
            {
                this.EquipmentDefinition = new EquipmentManager().Get(this.EquipmentDefinitionGuid);
            }
            catch (Exception exc)
            {
                String msg = "given EquipmentdefinitionGuid is not valid: " + this.EquipmentDefinitionGuid + " Exception: " + exc.Message;
                throw new BaseException(ErrorCodeHandler.E_JOB_EXECUTION_GENERAL, msg, exc);
            }
            return this;
        }
        #endregion

        #region interface

        public override EquipmentReport Query()
        {
            this.SetViewName(); //view name based on classname .. :)
            this.SetQueryString(String.Format("EquipmentDefinitionGuid = '{0}'", this.EquipmentDefinitionGuid));
            return base.Query();
        }

        #endregion

    }
} 
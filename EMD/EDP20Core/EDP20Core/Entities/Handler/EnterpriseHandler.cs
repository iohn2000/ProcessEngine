using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EnterpriseHandler : EMDObjectHandler //, IEnterpriseHandler
    {
        private List<EMDEnterprise> tmpEnteList;
        private int levelsDown = -1;

        public EnterpriseHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public EnterpriseHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public EnterpriseHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public EnterpriseHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new Enterprise().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            Enterprise ente = (Enterprise)dbObject;
            EMDEnterprise emdObject = new EMDEnterprise(ente.Guid, ente.Created, ente.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        /// <summary>
        /// returns all children starting from parent up to 'levelsDown' amount of levels.
        /// </summary>
        /// <param name="enteParentGuid">parent enterprise</param>
        /// <param name="levelsDown">-1 for all levels or 1 for 1 level, etc...</param>
        /// <returns>list of EMDEnterprise objects</returns>
        public List<EMDEnterprise> GetAllSubEnterprisesFromParent(string enteParentGuid, int levelsDown = -1)
        {
            this.tmpEnteList = new List<EMDEnterprise>();
            this.levelsDown = levelsDown;
            try
            {
                this.getAllSubEnterprises(enteParentGuid);
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error trying read enterprise tree.", ex);
            }
            return this.tmpEnteList;
        }

        /// <summary>
        /// searches down from parent levelsDown amount of levels to check if child is part of tree
        /// </summary>
        /// <param name="enteParentGuid"></param>
        /// <param name="enteChildGuid"></param>
        /// <param name="levelsDown">-1 for all levels or 1 for 1 level, etc...</param>
        /// <returns></returns>
        public bool IsEnterpriseUnderParent(string enteParentGuid, string enteChildGuid, int levelsDown = -1)
        {
            this.tmpEnteList = new List<EMDEnterprise>();

            try
            {
                this.getAllSubEnterprises(enteParentGuid);
                return this.tmpEnteList.Exists(item => item.Guid == enteChildGuid);
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error trying to find IsEnterpriseUnderParent.", ex);
            }
        }

        [Obsolete ("Do not use @@queries")]
        /// <summary>
        /// redundant function but modified signature to fit the @@ syntax
        /// </summary>
        /// <param name="enteParentGuid"></param>
        /// <param name="enteChildGuid"></param>
        /// <returns></returns>
        public string IsEnterpriseUnderParentAt(string enteParentGuid, string emplChildGuid)
        {
            EMDEmployment empl = (EMDEmployment)new EmploymentHandler().GetObject<EMDEmployment>(emplChildGuid);
            EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)new EnterpriseLocationHandler().GetObject<EMDEnterpriseLocation>(empl.ENLO_Guid);
            bool isUnder = this.IsEnterpriseUnderParent(enteParentGuid, enlo.E_Guid, -1);
            if (isUnder)
                return "true";
            else
                return "false";
        }

        /// <summary>
        /// root ente hat null stehen im root spalte 
        /// </summary>
        /// <param name="enterprise"></param>
        [Obsolete("Use EnterpriseManager.Create() instead")]
        public void CreateNewEnterprise(EMDEnterprise enterprise)
        {
            new EnterpriseManager(this.transaction, this.Guid_ModifiedBy, this.ModifyComment).Create(enterprise);
        }
        

        private void getAllSubEnterprises(string enteParentGuid, int currentLevel = 0)
        {
            if ((this.levelsDown > -1) && (currentLevel > this.levelsDown))
                return;

            // add enterprise
            this.tmpEnteList.Add((EMDEnterprise)GetObject<EMDEnterprise>(enteParentGuid));

            // get all children
            List<IEMDObject<EMDEnterprise>> enteChildList = GetObjects<EMDEnterprise, Enterprise>("Guid_Parent = \"" + enteParentGuid + "\"");

            if (enteChildList.Count > 0)
            {
                currentLevel++;
                foreach (var child in enteChildList)
                {
                    if (((EMDEnterprise)child).Guid != ((EMDEnterprise)child).Guid_Parent) // only call func if enterprise parent is not pointing at itself
                        this.getAllSubEnterprises(child.Guid, currentLevel);
                }
            }
        }

    }
}

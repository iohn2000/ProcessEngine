using System;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using System.Collections.Generic;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;
using System.Linq;
using Kapsch.IS.EDP.Core.Logic;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class OrgUnitHandler : EMDObjectHandler
    {
        private int levelsDown = -1;

        public OrgUnitHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public OrgUnitHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public OrgUnitHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public OrgUnitHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new OrgUnit().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null)
                return null;

            OrgUnit orgu = (OrgUnit)dbObject;
            EMDOrgUnit emdObject = new EMDOrgUnit(orgu.Guid, orgu.Created, orgu.Modified);

            //ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.ActiveFrom = orgu.ActiveFrom;
            emdObject.ActiveTo = orgu.ActiveTo;
            emdObject.Created = orgu.Created;
            emdObject.E_Guid = orgu.E_Guid;
            emdObject.Guid = orgu.Guid;
            emdObject.Guid_Parent = orgu.Guid_Parent;
            emdObject.Guid_Root = orgu.Guid_Root;
            emdObject.HistoryGuid = orgu.HistoryGuid;
            emdObject.ID_Parent = orgu.ID_Parent;
            emdObject.ID_Root = orgu.ID_Root;
            emdObject.IsSecurity = orgu.IsSecurity;
            emdObject.Key1 = orgu.Key1;
            emdObject.Key2 = orgu.Key2;
            emdObject.Key3 = orgu.Key3;
            emdObject.Modified = orgu.Modified;
            emdObject.Name = orgu.Name;
            emdObject.Note = orgu.Note;
            emdObject.O_ID = orgu.O_ID;
            emdObject.ValidFrom = orgu.ValidFrom;
            emdObject.ValidTo = orgu.ValidTo;
            emdObject.Guid_ModifiedBy = orgu.Guid_ModifiedBy;
            emdObject.ModifyComment = orgu.ModifyComment;

            emdObject.SetValidityStatus();

            return (IEMDObject<T>)emdObject;
        }

        public List<EMDOrgUnit> GetOrgUnitsForCompany(string ente_guid)
        {
            EnterpriseHandler enteHandler = new EnterpriseHandler();
            EMDEnterprise ente = (EMDEnterprise)enteHandler.GetObject<EMDEnterprise>(ente_guid);

            List<IEMDObject<EMDOrgUnit>> listOrgUnits = GetObjects<EMDOrgUnit, OrgUnit>("Guid_Root=\"" + ente.O_Guid_Dis + "\"");

            List<EMDOrgUnit> tmp = new List<EMDOrgUnit>();
            listOrgUnits.ForEach(item => tmp.Add((EMDOrgUnit)item));
            return tmp;
        }

        public List<EMDOrgUnit> GetAllRootOrgunits(bool isSecurity)
        {
            try
            {
                List<EMDOrgUnit> orgunits = new List<EMDOrgUnit>();
                // workaround: it's not possible with dynamic LINQ to query Guid == Guid_Parent
                //List<EMDOrgUnit> orgunitsRootEqParent = GetObjects<EMDOrgUnit, OrgUnit>("Guid_Parent=Guid_Root").Cast<EMDOrgUnit>().ToList(); => Guid_Root wird nicht mehr gepflegt
                List<EMDOrgUnit> orgunitsRootEqParent = GetObjects<EMDOrgUnit, OrgUnit>().Cast<EMDOrgUnit>().Where(item => item.Guid_Parent == item.Guid && item.IsSecurity == isSecurity).ToList();

                foreach (EMDOrgUnit emdOrgunit in orgunitsRootEqParent)
                {
                    if (emdOrgunit.Guid == emdOrgunit.Guid_Parent && emdOrgunit.IsSecurity == isSecurity)
                    {
                        orgunits.Add(emdOrgunit);
                    }
                }

                return orgunits;
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error trying read Orgunit root tree.", ex);
            }

        }

        public List<EMDOrgUnit> GetAllSubOrgUnitsFromParent(string orgUnitParentGuid, bool isSecurity, int levelsDown = -1)
        {
            List<EMDOrgUnit> orgUnits = new List<EMDOrgUnit>();
            if (!string.IsNullOrEmpty(orgUnitParentGuid))
            {
                orgUnits.Add((EMDOrgUnit)GetObject<EMDOrgUnit>(orgUnitParentGuid));
            }
            else
            {
                orgUnits.Add(new EMDOrgUnit() { Name = "Root" });
            }

            int level = 0;
            int sortorder = 0;
            this.levelsDown = levelsDown;
            try
            {
                this.GetAllSubOrgUnits(ref orgUnits, orgUnitParentGuid, isSecurity, level, ref sortorder);
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error trying read enterprise tree.", ex);
            }
            return orgUnits;
        }

        private void GetAllSubOrgUnits(ref List<EMDOrgUnit> orgunits, string orgUnitParentGuid, bool isSecurity, int level, ref int sortorder)
        {
            if ((this.levelsDown > -1) && (level > this.levelsDown))
                return;

            List<EMDOrgUnit> orgUnitChildList;
            if (!string.IsNullOrEmpty(orgUnitParentGuid))
            {
                string expression = string.Format("Guid_Parent = \"" + orgUnitParentGuid + "\" AND IsSecurity = {0}", isSecurity);
                orgUnitChildList = GetObjects<EMDOrgUnit, OrgUnit>(expression).Cast<EMDOrgUnit>().ToList();
            }
            else
            {
                orgUnitChildList = GetAllRootOrgunits(isSecurity);
            }


            int currentLevel = level + 1;

            foreach (var child in orgUnitChildList)
            {
                child.Level = currentLevel;
                child.Sortorder = sortorder;
                sortorder++;

                // don't add itself
                if (child.Guid != orgUnitParentGuid)
                {
                    orgunits.Add(child);

                    if (string.IsNullOrEmpty(orgUnitParentGuid) || child.Guid != child.Guid_Parent) // only call func if enterprise parent is not pointing at itself
                    {
                        this.GetAllSubOrgUnits(ref orgunits, child.Guid, isSecurity, currentLevel, ref sortorder);
                    }
                }
            }


        }

        /// <summary>
        /// @@ func to get employmentID of the first employment which is found in an orgunit for a given roleId
        /// GetFirstEmploymentForRoleInOrgunit(10500)@@ORGU_65f9caed25fd4355806c7c3d76a023a3
        /// </summary>
        /// <param name="orgUnitGuid"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public string GetFirstEmploymentForRoleInOrgunit(string orgUnitGuid, string roleId)
        {
            String result = "";
            List<string> employmentIds = new List<string>();
            OrgUnitRoleSearchLINQ orgUnitSearch = new OrgUnitRoleSearchLINQ();
            int roleIdInt = Convert.ToInt32(roleId);
            employmentIds = orgUnitSearch.SearchOrgUnitRoleForOrgUnit(roleIdInt, orgUnitGuid);
            if (employmentIds.Count>0) result = employmentIds.FirstOrDefault();
            return result;
        }

    }
}

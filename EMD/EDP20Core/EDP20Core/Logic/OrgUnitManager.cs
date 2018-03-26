using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Linq;
using System.Collections.Generic;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using System.Collections;
using Kapsch.IS.EDP.Core.Framework;
using System.Linq;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class OrgUnitManager
        : BaseManager
        , IOrgUnitManager
    {
        private List<EMDOrgUnit> tmpOrgUnitList;
        private int levelsDown = -1;

        #region Constructors

        public OrgUnitManager()
            : base()
        {
        }

        public OrgUnitManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public OrgUnitManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public OrgUnitManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDOrgUnit Get(string guid)
        {
            OrgUnitHandler handler = new OrgUnitHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            return (EMDOrgUnit)handler.GetObject<EMDOrgUnit>(guid);
        }

        public List<EMDOrgUnit> GetOrganizationUnits(string searchUnitName, int maxItemsResult = -1)
        {
            DateTime now = DateTime.Now;
            EMD_Entities emdEntities = new EMD_Entities();
            List<OrgUnit> orgUnits = null;

            if (!string.IsNullOrEmpty(searchUnitName))
            {

                orgUnits = (from orgUnit in emdEntities.OrgUnit
                            where orgUnit.Name.Contains(searchUnitName) &&
                            orgUnit.ActiveFrom <= now && orgUnit.ValidFrom <= now && orgUnit.ActiveTo >= now && orgUnit.ValidTo >= now
                            select orgUnit
                            ).Distinct().ToList();
            }
            else
            {
                orgUnits = (from orgUnit in emdEntities.OrgUnit
                            where
                            orgUnit.ActiveFrom <= now && orgUnit.ValidFrom <= now && orgUnit.ActiveTo >= now && orgUnit.ValidTo >= now
                            select orgUnit
                     ).Distinct().ToList();
            }

            if (maxItemsResult > 0)
            {
                orgUnits = orgUnits.Take(maxItemsResult).ToList();
            }

            List<EMDOrgUnit> emdOrgUnits = new List<EMDOrgUnit>();

            foreach (OrgUnit item in orgUnits)
            {
                EMDOrgUnit unit = new EMDOrgUnit();
                OrgUnit newItem = item;
                ReflectionHelper.CopyProperties(ref newItem, ref unit);
                emdOrgUnits.Add(unit);
            }

            return emdOrgUnits;
        }

        public List<EMDOrgUnit> GetOrganizationUnitByName(string orgUnitName, string rootOrgUnitGuid, int maxItemsResult = -1)
        {
            DateTime now = DateTime.Now;
            EMD_Entities emdEntities = new EMD_Entities();
            List<OrgUnit> orgUnits = null;

            if (!string.IsNullOrEmpty(orgUnitName))
            {
                orgUnits = (from orgUnit in emdEntities.OrgUnit
                            where orgUnit.Name == orgUnitName.Trim() &&
                            orgUnit.ActiveFrom <= now && orgUnit.ValidFrom <= now && orgUnit.ActiveTo >= now && orgUnit.ValidTo >= now &&
                            orgUnit.Guid_Root == rootOrgUnitGuid
                            select orgUnit
                            ).Distinct().ToList();
            }


            if (maxItemsResult > 0)
            {
                orgUnits = orgUnits.Take(maxItemsResult).ToList();
            }

            List<EMDOrgUnit> emdOrgUnits = new List<EMDOrgUnit>();

            foreach (OrgUnit item in orgUnits)
            {
                EMDOrgUnit unit = new EMDOrgUnit();
                OrgUnit newItem = item;
                ReflectionHelper.CopyProperties(ref newItem, ref unit);
                emdOrgUnits.Add(unit);
            }

            return emdOrgUnits;
        }

        [Obsolete("function move to OrgUnitHandler, please dont use", true)]
        public List<EMDOrgUnit> getOrgUnitsForCompany(string ente_guid)
        {
            EnterpriseHandler enteHandler = new EnterpriseHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDEnterprise ente = (EMDEnterprise)enteHandler.GetObject<EMDEnterprise>(ente_guid);
            OrgUnitHandler orgunitHandler = new OrgUnitHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDOrgUnit>> listOrgUnits = orgunitHandler.GetObjects<EMDOrgUnit, OrgUnit>("Guid_Root=\"" + ente.O_Guid_Dis + "\"");

            List<EMDOrgUnit> tmp = new List<EMDOrgUnit>();
            listOrgUnits.ForEach(item => tmp.Add((EMDOrgUnit)item));
            return tmp;
        }

        public List<string> SearchOrgUnitRoleForEmployment(int role_id, string empl_guid)
        {
            return new OrgUnitRoleSearch(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment).SearchOrgUnitRoleForEmployment(role_id, empl_guid);
        }
        // on function that use orgunitrolesearch

        public EMDOrgUnit Delete(string guid)
        {
            OrgUnitHandler handler = new OrgUnitHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            OrgUnitRoleManager orgUnitRoleManager = new OrgUnitRoleManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            EMDOrgUnit emdItem = Get(guid);
            if (emdItem != null)
            {
                orgUnitRoleManager.CleanupOrgunitRoleRelations(guid);

                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDOrgUnit)handler.DeleteObject<EMDOrgUnit>(emdItem);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The Orgunit with guid: {0} was not found.", guid));
            }
        }

        private bool EqualOrgUnitExists(EMDOrgUnit orgu)
        {
            OrgUnitHandler orguHandler = new OrgUnitHandler(this.Guid_ModifiedBy, this.ModifyComment);

            string query = "Name = \"" + orgu.Name + "\" && Guid_Parent=\"" + orgu.Guid_Parent + "\"";

            //Check if there is already an orgunit with the same name and same parent
            List<EMDOrgUnit> orgunits = orguHandler.GetObjects<EMDOrgUnit, OrgUnit>(query).Cast<EMDOrgUnit>().ToList();
            //int equalOrgUnits = 

            if (!string.IsNullOrEmpty(orgu.Guid))
            {
                EMDOrgUnit found = orgunits.Find(a => a.Guid == orgu.Guid);

                orgunits.Remove(found);

            }

            if (orgunits.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Creare a new OrgUnit in Database
        /// </summary>
        /// <param name="orgu">An EMDOrgUnit Object filled with Data.</param>
        /// <returns></returns>
        public EMDOrgUnit Create(EMDOrgUnit orgu)
        {
            OrgUnitHandler orguHandler = new OrgUnitHandler(this.Guid_ModifiedBy, this.ModifyComment);

            if (this.EqualOrgUnitExists(orgu))
                throw new EntityNotAllowedException("OrgUnit", EnumEntityNotAllowedError.EntityAllowedOnlyOnceForSelectedParameters,
                    ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "An OrgUnit with the same name and parent already exists!");

            orgu = (EMDOrgUnit)orguHandler.CreateObject<EMDOrgUnit>(orgu);

            // if there is no parent
            if (string.IsNullOrEmpty(orgu.Guid_Parent))
            {
                orgu.Guid_Parent = orgu.Guid;
                orgu.Guid_Root = orgu.Guid;
            }

            EMDOrgUnit porgu = (EMDOrgUnit)orguHandler.GetObject<EMDOrgUnit>(orgu.Guid_Parent);

            if (string.IsNullOrEmpty(orgu.Guid_Root))
            {
                if (orgu.Guid_Parent == orgu.Guid)
                {
                    orgu.Guid_Root = orgu.Guid;
                }
                else
                {
                    orgu.Guid_Root = porgu.Guid_Root;
                }
            }
            //do not historize this call
            orgu = (EMDOrgUnit)orguHandler.UpdateObject<EMDOrgUnit>(orgu, historize: false);
            return orgu;
        }

        public EMDOrgUnit Update(string guid_user, EMDOrgUnit orgu, CoreTransaction transaction = null)
        {
            // if there is no parent set, we have defined a root element
            // >> Business Logic: we also must set the root to the same GUID
            if (string.IsNullOrEmpty(orgu.Guid_Parent))
            {
                orgu.Guid_Parent = orgu.Guid;
                orgu.Guid_Root = orgu.Guid;
            }
            else
            {
                // Set the root element, same like parent
                EMDOrgUnit orgunitParent = Get(orgu.Guid_Parent);
                orgu.Guid_Root = orgunitParent.Guid_Root;
            }

            // if the ID is the same as the parent, we have defined a root element
            // >> Business Logic: we also must set the root to the same GUID
            if (orgu.Guid == orgu.Guid_Parent)
            {
                orgu.Guid_Root = orgu.Guid;
            }

            OrgUnitHandler orguHandler = new OrgUnitHandler(transaction, guid_user);
            if (this.EqualOrgUnitExists(orgu))
                throw new EntityNotAllowedException("OrgUnit", EnumEntityNotAllowedError.EntityAllowedOnlyOnceForSelectedParameters,
                    ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "An OrgUnit with the same name inside this parent already exists!");


            orgu = (EMDOrgUnit)orguHandler.UpdateObject<EMDOrgUnit>(orgu);
            return orgu;
        }

        public List<EMDOrgUnit> GetAllSubOrgUnitsFromParent(string orguParentGuid, int levelsDown = -1)
        {
            this.tmpOrgUnitList = new List<EMDOrgUnit>();
            this.levelsDown = levelsDown;
            try
            {
                this.getAllSubOrgUnits(orguParentGuid);
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error trying read enterprise tree.", ex);
            }
            return this.tmpOrgUnitList;
        }

        public EMDOrgUnit GetOrgunitForEmployment(string empl_guid)
        {
            EMDOrgUnit emdOrgunit = null;

            EMDRole emdRole = null;
            try
            {
                emdRole = (EMDRole)new RoleHandler(this.Transaction).GetRoleById(RoleHandler.PERSON);
            }
            catch (Exception ex)
            {

            }

            if (emdRole != null)
            {
                EMDOrgUnitRole emdOrgunitRole = (EMDOrgUnitRole)new OrgUnitRoleHandler(this.Transaction).GetOrgUnitRole(empl_guid, emdRole.Guid);
                if (emdOrgunitRole != null)
                {
                    emdOrgunit = (EMDOrgUnit)new OrgUnitHandler(this.Transaction).GetObject<EMDOrgUnit>(emdOrgunitRole.O_Guid);
                }
            }

            return emdOrgunit;
        }

        private void getAllSubOrgUnits(string orguParentGuid, int currentLevel = 0)
        {
            if ((this.levelsDown > -1) && (currentLevel > this.levelsDown))
                return;

            OrgUnitHandler orgUnitHandler = new OrgUnitHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            // add enterprise
            this.tmpOrgUnitList.Add((EMDOrgUnit)orgUnitHandler.GetObject<EMDOrgUnit>(orguParentGuid));

            // get all children
            List<IEMDObject<EMDOrgUnit>> enteChildList = orgUnitHandler.GetObjects<EMDOrgUnit, OrgUnit>("Guid_Parent = \"" + orguParentGuid + "\"");

            if (enteChildList.Count > 0)
            {
                currentLevel++;
                foreach (var child in enteChildList)
                {
                    if (((EMDOrgUnit)child).Guid != ((EMDOrgUnit)child).Guid_Parent) // only call func if enterprise parent is not pointing at itself
                        this.getAllSubOrgUnits(child.Guid, currentLevel);
                }
            }
        }

  
    }
}

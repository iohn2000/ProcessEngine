using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kapsch.IS.EDP.Core.Logic
{
    //TODO: Refactoring
    public class OrgUnitRoleSearch
        : BaseManager
    {
        public DateTime ActiveTo = EMDEmployment.INFINITY;
        public DateTime ValidTo = EMDEmployment.INFINITY;
        public DateTime ActiveFrom = EMDEmployment.INFINITY;
        public DateTime ValidFrom = EMDEmployment.INFINITY;


        public bool SearchInActive
        {
            get
            {
                return DateTime.Now > ActiveTo;
            }
        }

        public bool SearchHistorical
        {
            get
            {
                return DateTime.Now > ValidTo;
            }
        }

        #region Constructors

        public OrgUnitRoleSearch()
            : base()
        {
        }

        public OrgUnitRoleSearch(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public OrgUnitRoleSearch(string guid_ModifiedBy, string modifyComment)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public OrgUnitRoleSearch(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public List<string> SearchOrgUnitRoleForOrgUnit(int role_id, string orgu_guid)
        {
            List<string> List_Employments = new List<string>();
            return getEmploymentIDsForRoleInOrgUnitV2(role_id, orgu_guid);
        }


        public List<string> SearchOrgUnitRoleForEmployment(int role_id, string empl_guid)
        {
            List<string> List_Employments = new List<string>();
            return getEmploymentIDsForRoleInOrgUnitByEP_IDV2(role_id, empl_guid);
        }

        public List<string> SearchOrgUnitRoleForEnterpriseAndPersNr(int role_id, string ente_guid, string persNr)
        {
            List<string> List_Employments = new List<string>();

            if (!String.IsNullOrWhiteSpace(persNr))
            {
                PersonHandler ph = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                List<IEMDObject<EMDPerson>> persons = ph.GetObjects<EMDPerson, Person>("Pers_Nr = \"" + persNr + "\"");

                if (persons.Count == 0)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No person found for persNr" + persNr);
                }
                else if (persons.Count > 1)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one person found for persNr" + persNr);
                }
                else
                {
                    EMDPerson pers = (EMDPerson)persons[0];
                    if (pers.UserID != null && pers.UserID != String.Empty)
                    {
                        EmploymentHandler emplHandler = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment) { DeliverInActive = SearchInActive, Historical = SearchInActive };
                        List<IEMDObject<EMDEmployment>> empls = emplHandler.GetObjects<EMDEmployment, Employment>("P_Guid = \"" + pers.Guid + "\" && E_Guid = " + ente_guid + "\"");
                        if (empls.Count == 0)
                        {
                            throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No employment found for " + pers.Guid + " and " + ente_guid);
                        }
                        else if (empls.Count > 1)
                        {
                            throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one employment found for " + pers.Guid + " and " + ente_guid);
                        }
                        else
                        {
                            return getEmploymentIDsForRoleInOrgUnitByEP_IDV2(role_id, empls[0].Guid);
                        }
                    }
                    else
                    {
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one person found for persNr" + persNr);
                    }
                }
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "User-Id has no value");
            }
        }

        public List<string> SearchOrgUnitRoleForEnterpriseAndUserID(int role_id, string ente_guid, string userId)
        {
            List<string> List_Employments = new List<string>();

            if (!String.IsNullOrWhiteSpace(userId))
            {
                PersonHandler ph = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                List<IEMDObject<EMDPerson>> persons = ph.GetObjects<EMDPerson, Person>("UserID = \"" + userId + "\"");

                if (persons.Count == 0)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No person found for User-Id" + userId);
                }
                else if (persons.Count > 1)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one person found for User-Id" + userId);
                }
                else
                {
                    EMDPerson pers = (EMDPerson)persons[0];
                    if (pers.UserID != null && pers.UserID != String.Empty)
                    {
                        EmploymentHandler emplHandler = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment) { DeliverInActive = SearchInActive, Historical = SearchInActive };
                        List<IEMDObject<EMDEmployment>> empls = emplHandler.GetObjects<EMDEmployment, Employment>("P_Guid = \"" + pers.Guid + "\" && E_Guid = " + ente_guid + "\"");
                        if (empls.Count == 0)
                        {
                            throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No employment found for " + pers.Guid + " and " + ente_guid);
                        }
                        else if (empls.Count > 1)
                        {
                            throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one employment found for " + pers.Guid + " and " + ente_guid);
                        }
                        else
                        {
                            return getEmploymentIDsForRoleInOrgUnitByEP_IDV2(role_id, empls[0].Guid);
                        }
                    }
                    else
                    {
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one person found for User-Id" + userId);
                    }

                }
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "User-Id has no value");
            }
        }

        //NOT!!!!!!!!!!!!!!!!!!
        //public void getEmploymentIDsForRoleInOrgUnit(int role_id, string orgu_guid, ref List<string> List_Employments)
        //{
        //    OrgUnitRoleHandler ourh = new OrgUnitRoleHandler();
        //    List<IEMDObject<EMDOrgUnitRole>> List_OrgUnitRoles = ourh.GetObjects<EMDOrgUnitRole, OrgUnitRole>("O_Guid = \"" + orgu_guid + "\" && R_ID=" + role_id);

        //    if (List_OrgUnitRoles.Count > 0)
        //    {
        //        foreach (EMDOrgUnitRole our in List_OrgUnitRoles)
        //        {
        //            if (!List_Employments.Contains(our.EP_Guid))
        //                List_Employments.Add(our.EP_Guid);
        //        }
        //    }
        //    else
        //    {
        //        OrgUnitHandler ouh = new OrgUnitHandler();
        //        EMDOrgUnit ou = (EMDOrgUnit)ouh.GetObject<EMDOrgUnit>(orgu_guid);

        //        if (ou != null && orgu_guid != ou.Guid_Root)
        //        {
        //            getEmploymentIDsForRoleInOrgUnit(role_id, ou.Guid_Parent, ref List_Employments);
        //        }
        //    }
        //}

        private List<string> getEmploymentIDsForRoleInOrgUnitV2(int role_id, string orgu_guid, int counter = 0)
        {
            RoleManager roleManager = new RoleManager();
            EMDRole role = roleManager.GetRoleById(role_id);
            if (role == null)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Role with R_ID: {0} not found!",role_id));
            }
            return getEmploymentIDsForRoleInOrgUnitV2(role.Guid, orgu_guid);
        }

        private List<string> getEmploymentIDsForRoleInOrgUnitV2(string roleGuid, string orgu_guid, int counter = 0)
        {
            int maxcount = 100;
            OrgUnitRoleHandler ourh = new OrgUnitRoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment) { DeliverInActive = SearchInActive, Historical = SearchInActive };
            List<IEMDObject<EMDOrgUnitRole>> List_OrgUnitRoles = ourh.GetObjects<EMDOrgUnitRole, OrgUnitRole>("O_Guid = \"" + orgu_guid + "\" && R_Guid= \"" + roleGuid + "\"");

            List<string> result = new List<string>();

            if (List_OrgUnitRoles.Count > 0)
            {
                foreach (EMDOrgUnitRole our in List_OrgUnitRoles)
                {
                    if (!result.Contains(our.EP_Guid))
                        result.Add(our.EP_Guid);
                }
            }
            else
            {
                if (counter < maxcount)
                {
                    counter++;
                    OrgUnitHandler ouh = new OrgUnitHandler() { DeliverInActive = SearchInActive, Historical = SearchInActive };
                    EMDOrgUnit ou = (EMDOrgUnit)ouh.GetObject<EMDOrgUnit>(orgu_guid);

                    if (ou != null && orgu_guid != ou.Guid_Root && orgu_guid != ou.Guid_Parent)
                    {
                        List<string> List_Employments2 = getEmploymentIDsForRoleInOrgUnitV2(roleGuid, ou.Guid_Parent, counter);
                        if (List_Employments2.Count > 0)
                            result.AddRange(List_Employments2);
                    }                    
                } else
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Depth of recursions for getEmploymentIDsForRoleInOrgUnitV2 exceeded, maxcount: "+maxcount);
                }
            }
            return result;
        }

        private List<String> getEmploymentIDsForRoleInOrgUnitByEP_IDV2(int role_id, string empl_guid)
        {
            List<string> List_Employments = new List<string>();

            List<EMDOrgUnitRole> List_O_IDs = getOrgUnitsForEmployment(empl_guid).Cast<EMDOrgUnitRole>().ToList();
            if (DateTime.Now > ActiveTo)
            {
                //    List_O_IDs = List_O_IDs.FindAll(a => a.ActiveTo <= this.ActiveTo && a.ActiveFrom >= this.ActiveFrom).Cast<EMDOrgUnitRole>().ToList();
                List_O_IDs = List_O_IDs.FindAll(a => a.ActiveFrom >= this.ActiveFrom && a.ActiveTo <= this.ActiveTo).Cast<EMDOrgUnitRole>().OrderBy(a => a.ActiveTo).ToList();
            }

            if (List_O_IDs.Count == 0)
            {
                //Error = " Verifizierung einer OrgUnit zu Employment " + EP_ID.ToString() + " für Stichtag " + mDate.ToString() + " nicht möglich.";
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "no active person role (10100) found for: " + empl_guid + "!");
            }
            if (List_O_IDs.Count > 1)
            {
                if (DateTime.Now > ActiveTo)
                {
                    EMDOrgUnitRole role = List_O_IDs.Last();
                    List_O_IDs = new List<EMDOrgUnitRole>();
                    List_O_IDs.Add(role);
                }
                else
                {

                    //Error += " Eindeutige Verifizierung einer OrgUnit zu Enterprise " + EP_ID.ToString() + " und UserID " + UserID + " für Stichtag " + mDate.ToString() + " nicht möglich.";
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "more than one active person roles (10100) found for: " + empl_guid + "!");
                }
            }

            EMDOrgUnitRole our = (EMDOrgUnitRole)List_O_IDs.FirstOrDefault();

            string orgu_guid = our.O_Guid;
            //int O_ID = List_O_IDs[0];
            List_Employments = getEmploymentIDsForRoleInOrgUnitV2(role_id, orgu_guid);

            //Is the Linemanager of this OrgUnit by himself => go one step up in the orgunit hirachy
            if (List_Employments.Count > 0 && List_Employments[0] == empl_guid)
            {
                List_Employments.RemoveAt(0);
                //EMDDataContext DB_Context_EMD = EMDDB_Helper.GetEMDDBContext();
                //OrgUnit OrgUnit = (from o in DB_Context_EMD.OrgUnits where o.O_ID == O_ID select o).FirstOrDefault();
                OrgUnitHandler ouh = new OrgUnitHandler() { DeliverInActive = SearchInActive, Historical = SearchInActive };
                EMDOrgUnit ou = (EMDOrgUnit)ouh.GetObject<EMDOrgUnit>(orgu_guid);

                if (ou != null && orgu_guid != ou.Guid_Root)
                {
                    List_Employments = getEmploymentIDsForRoleInOrgUnitV2(role_id, ou.Guid_Parent);
                }
            }

            return List_Employments;
        }

        //public void getEmploymentIDsForRoleInOrgUnitByEP_ID(int role_id, string empl_guid, ref List<string> List_Employments)
        //{
        //    List<IEMDObject<EMDOrgUnitRole>> List_O_IDs = getOrgUnitsForEmployment(empl_guid);
        //    if (List_O_IDs.Count == 0)
        //    {
        //        //Error = " Verifizierung einer OrgUnit zu Employment " + EP_ID.ToString() + " für Stichtag " + mDate.ToString() + " nicht möglich.";
        //        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "no active person role (10100) found for: " + empl_guid + "!");
        //    }
        //    if (List_O_IDs.Count > 1)
        //    {
        //        //Error += " Eindeutige Verifizierung einer OrgUnit zu Enterprise " + EP_ID.ToString() + " und UserID " + UserID + " für Stichtag " + mDate.ToString() + " nicht möglich.";
        //        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "more than one active person roles (10100) found for: " + empl_guid + "!");
        //    }
        //    else
        //    {
        //        EMDOrgUnitRole our = (EMDOrgUnitRole)List_O_IDs.FirstOrDefault();

        //        string orgu_guid = our.O_Guid;
        //        //int O_ID = List_O_IDs[0];
        //        getEmploymentIDsForRoleInOrgUnit(role_id, orgu_guid, ref List_Employments);

        //        //Is the Linemanager of this OrgUnit by himself => go one step up in the orgunit hirachy
        //        if (List_Employments.Count > 0 && List_Employments[0] == empl_guid)
        //        {
        //            List_Employments.RemoveAt(0);
        //            //EMDDataContext DB_Context_EMD = EMDDB_Helper.GetEMDDBContext();
        //            //OrgUnit OrgUnit = (from o in DB_Context_EMD.OrgUnits where o.O_ID == O_ID select o).FirstOrDefault();
        //            OrgUnitHandler ouh = new OrgUnitHandler();
        //            EMDOrgUnit ou = (EMDOrgUnit)ouh.GetObject<EMDOrgUnit>(orgu_guid);

        //            if (ou != null && orgu_guid != ou.Guid_Root)
        //            {
        //                getEmploymentIDsForRoleInOrgUnit(role_id, ou.Guid_Parent, ref List_Employments);
        //            }
        //        }
        //    }
        //}

        private List<IEMDObject<EMDOrgUnitRole>> getOrgUnitsForEmployment(string empl_guid)
        {
            OrgUnitRoleHandler ourh = new OrgUnitRoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment) { DeliverInActive = SearchInActive, Historical = SearchInActive };
            RoleManager roleManager = new RoleManager();
            EMDRole role = roleManager.GetRoleById(OrgUnitRoleHandler.ROLE_ID_PERSON);
            if (role == null)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Role with R_ID: {0} not found!", OrgUnitRoleHandler.ROLE_ID_PERSON));
            }

            List<IEMDObject<EMDOrgUnitRole>> orgUnitRoles = ourh.GetObjects<EMDOrgUnitRole, OrgUnitRole>("EP_Guid = \"" + empl_guid + "\" && R_Guid=\"" + role.Guid + "\"");
            return orgUnitRoles;
        }
    }
}

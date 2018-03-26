using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class OrgUnitRoleSearchLINQ
    {
        private EMD_Entities emdDataContext = new EMD_Entities();

        public List<string> SearchOrgUnitRoleForOrgUnit(int role_id, string orgu_guid, bool deliverInactive = false)
        {
            List<string> List_Employments = new List<string>();
            return getEmploymentIDsForRoleInOrgUnitV2(role_id, orgu_guid, deliverInactive);
        }

        public List<string> SearchOrgUnitRoleForEmployment(int role_id, string empl_guid, bool deliverInactive = false)
        {
            List<string> List_Employments = new List<string>();
            return getEmploymentIDsForRoleInOrgUnitByEP_IDV2(role_id, empl_guid, deliverInactive);
        }

        private Dictionary<string, List<String>> cachedEmploymentIDsForRoleInOrgUnit = new Dictionary<string, List<string>>();

        private List<string> getEmploymentIDsForRoleInOrgUnitV2(int role_id, string orgu_guid, bool deliverInactive = false)
        {
            List<Role> roles = (from item in emdDataContext.Role where item.R_ID == role_id && item.Guid == item.HistoryGuid && item.ActiveTo > DateTime.Now select item).ToList();
            if (roles.Count == 0)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Role with R_ID: {0} not found!",role_id));
            }
            else if (roles.Count == 1)
            {
                return getEmploymentIDsForRoleInOrgUnitV2(roles.FirstOrDefault().Guid, orgu_guid , deliverInactive);
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Role with R_ID: {0} found more than once!", role_id));
            }
            
        }

        private List<string> getEmploymentIDsForRoleInOrgUnitV2(string roleGuid, string orgu_guid, bool deliverInactive = false)
        {
            string cacheKey = string.Format("roleGuid{0}-orgu_guid{1}-deliverInactive{2}", roleGuid, orgu_guid, deliverInactive);

            if (cachedEmploymentIDsForRoleInOrgUnit.ContainsKey(cacheKey))
            {
                return cachedEmploymentIDsForRoleInOrgUnit[cacheKey];
            }

            List<OrgUnitRole> orgunitRoleList;
            if (deliverInactive)
            {
                orgunitRoleList = (from our in emdDataContext.OrgUnitRole where our.O_Guid == orgu_guid && our.R_Guid == roleGuid && our.ValidFrom < DateTime.Now && our.ValidTo > DateTime.Now select our).ToList();
            }
            else
            {
                orgunitRoleList = (from our in emdDataContext.OrgUnitRole where our.O_Guid == orgu_guid && our.R_Guid == roleGuid && our.ActiveFrom < DateTime.Now && our.ActiveTo > DateTime.Now && our.ValidFrom < DateTime.Now && our.ValidTo > DateTime.Now select our).ToList();
            }

            List<string> employmentIdList = new List<string>();

            if (orgunitRoleList.Count > 0)
            {
                foreach (OrgUnitRole our in orgunitRoleList)
                {
                    if (!employmentIdList.Contains(our.EP_Guid))
                        employmentIdList.Add(our.EP_Guid);
                }
            }
            else
            {
                //use of conditional-operator (?) to decide wether historcal or active data is quried
                OrgUnit ou = (from item
                              in emdDataContext.OrgUnit
                              where item.Guid == orgu_guid
                                    && (deliverInactive ? true : item.ActiveFrom < DateTime.Now
                                                                  && item.ActiveTo > DateTime.Now)
                                    && item.ValidFrom < DateTime.Now
                                    && item.ValidTo > DateTime.Now
                              select item).FirstOrDefault();

                if (ou != null && orgu_guid != ou.Guid_Parent)
                {
                    List<string> List_Employments2 = getEmploymentIDsForRoleInOrgUnitV2(roleGuid, ou.Guid_Parent, deliverInactive);
                    if (List_Employments2.Count > 0)
                        employmentIdList.AddRange(List_Employments2);
                }
            }

            cachedEmploymentIDsForRoleInOrgUnit.Add(cacheKey, employmentIdList);

            return employmentIdList;
        }


        private Dictionary<string, List<String>> cachedEmploymentIDsForRoleInOrgUnitByEP = new Dictionary<string, List<string>>();

        public List<String> getEmploymentIDsForRoleInOrgUnitByEP_IDV2(int role_id, string empl_guid, bool deliverInactive = false)
        {
            string cacheKey = string.Format("role_id{0}-empl_guid{1}-deliverInactive{2}", role_id, empl_guid, deliverInactive);

            if (cachedEmploymentIDsForRoleInOrgUnitByEP.ContainsKey(cacheKey))
            {
                return cachedEmploymentIDsForRoleInOrgUnitByEP[cacheKey];
            }

            List<string> employmentIdList = new List<string>();


            List<OrgUnitRole> List_O_IDs = GetOrgUnitsForEmployment(empl_guid, deliverInactive);
            if (List_O_IDs.Count == 0)
            {
                //Error = " Verifizierung einer OrgUnit zu Employment " + EP_ID.ToString() + " für Stichtag " + mDate.ToString() + " nicht möglich.";
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "no active person role (10100) found for: " + empl_guid + "!");
            }
            if (List_O_IDs.Count > 1)
            {
                //Error += " Eindeutige Verifizierung einer OrgUnit zu Enterprise " + EP_ID.ToString() + " und UserID " + UserID + " für Stichtag " + mDate.ToString() + " nicht möglich.";
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "more than one active person roles (10100) found for: " + empl_guid + "!");
            }
            else
            {
                OrgUnitRole our = List_O_IDs.FirstOrDefault();

                string orgu_guid = our.O_Guid;
                //int O_ID = List_O_IDs[0]; //bookmark
                employmentIdList = getEmploymentIDsForRoleInOrgUnitV2(role_id, orgu_guid, deliverInactive);

                //Is the Linemanager of this OrgUnit by himself => go one step up in the orgunit hirachy
                if (employmentIdList.Count > 0 && employmentIdList[0] == empl_guid)
                {
                    employmentIdList.RemoveAt(0);
                    OrgUnit ou = (from item
                                  in emdDataContext.OrgUnit
                                  where item.Guid == orgu_guid
                                        && (deliverInactive ? true : item.ActiveFrom < DateTime.Now
                                                                      && item.ActiveTo > DateTime.Now)
                                        && item.ValidFrom < DateTime.Now
                                        && item.ValidTo > DateTime.Now
                                  select item).FirstOrDefault();

                    if (ou != null && orgu_guid != ou.Guid_Parent)
                    {
                        employmentIdList = getEmploymentIDsForRoleInOrgUnitV2(role_id, ou.Guid_Parent, deliverInactive);
                    }
                }
            }

            cachedEmploymentIDsForRoleInOrgUnitByEP.Add(cacheKey, employmentIdList);

            return employmentIdList;
        }

        private Dictionary<string, List<OrgUnitRole>> cachedOrgUnitsForEmployment = new Dictionary<string, List<OrgUnitRole>>();


        private List<OrgUnitRole> GetOrgUnitsForEmployment(string empl_guid, bool deliverInactive = false)
        {
            string cacheKey = string.Format("empl_guid{0}-deliverInactive{1}", empl_guid, deliverInactive);

            if (cachedOrgUnitsForEmployment.ContainsKey(cacheKey))
            {
                return cachedOrgUnitsForEmployment[cacheKey];
            }

            string roleGuid = string.Empty;
            List<Role> roles = (from item in emdDataContext.Role where item.R_ID == OrgUnitRoleHandler.ROLE_ID_PERSON && item.Guid == item.HistoryGuid && item.ActiveTo > DateTime.Now select item).ToList();
            if (roles.Count == 0)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Role with R_ID: {0} not found!", OrgUnitRoleHandler.ROLE_ID_PERSON));
            }
            else if (roles.Count == 1)
            {
                roleGuid = roles.FirstOrDefault().Guid;
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Role with R_ID: {0} found more than once!", roleGuid));
            }

            List<OrgUnitRole> orgUnitRoles = (from our
                                              in emdDataContext.OrgUnitRole
                                              where our.EP_Guid == empl_guid
                                                    && our.R_Guid == roleGuid
                                                    && our.ValidFrom < DateTime.Now
                                                    && our.ValidTo > DateTime.Now
                                                    //If deliverInactive is false append ActiveClause
                                                    && (deliverInactive ? true : (our.ActiveFrom < DateTime.Now
                                                                                  && our.ActiveTo > DateTime.Now))
                                              select our).ToList();

            cachedOrgUnitsForEmployment.Add(cacheKey, orgUnitRoles);
            return orgUnitRoles;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System.Runtime.Caching;

namespace Kapsch.IS.EDP.Core.Framework
{
    public class SecurityUser
    {
        internal IISLogger logger = ISLogger.GetLogger("SecurityUser");

        public string UserId { get; set; }
        public bool IsAdmin { get; set; }
        public string ViewedId { get; set; }
        //public EMDEmployment Employment {get; set; }
        //public List<SecurityPermission> Permissions { get; set; }

        OrgUnitRoleSearchLINQ search = new OrgUnitRoleSearchLINQ();

        #region Caching
        internal static ObjectCache cache = MemoryCache.Default;

        //Create a custom Timeout of 10 seconds
        private static CacheItemPolicy policy = new CacheItemPolicy();

        //Create a custom Timeout for extended cache
        private static CacheItemPolicy policyExtended = new CacheItemPolicy();

        internal List<string> GetCacheKeys()
        {

            List<string> cacheKeys = new List<string>();
            if (Utils.Configuration.DOCACHESECURITYUSER)
            {
                IEnumerable<KeyValuePair<string, object>> en = cache.AsEnumerable();

                cacheKeys = (from x in en select x.Key).ToList();
            }
            return cacheKeys;
        }


        internal static void ClearCache()
        {
            if (Utils.Configuration.DOCACHESECURITYUSER)
            {
                IEnumerable<KeyValuePair<string, object>> en = cache.AsEnumerable();

                List<string> cacheKeys = (from x in en select x.Key).ToList();

                if (cacheKeys != null)
                {
                    foreach (string cacheKey in cacheKeys)
                    {
                        cache.Remove(cacheKey);
                    }
                }
            }
        }
        #endregion

        internal static string GetCacheKey(string userId, string viewedId = null)
        {
            return string.Format("SecurityUser:{0}-{1}", userId,viewedId);
        }

        public static SecurityUser NewSecurityUser(string userId, string viewedId = null)
        {
            // Invalidate modificationID to check if any Entity has changed
            string guidModificationString = cache.Get("ModificationID") as string;
            if (!string.IsNullOrEmpty(guidModificationString))
            {
                Guid lastModificationId = new Guid(guidModificationString);

                if (lastModificationId != Utils.Configuration.MODIFICATION_ID)
                {
                    ClearCache();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Utils.Configuration.CACHINGTIMEINMINUTES_EXTENDED);
                    cache.Add("ModificationID", Utils.Configuration.MODIFICATION_ID.ToString(), policy);
                }
            }
            else
            {
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Utils.Configuration.CACHINGTIMEINMINUTES_EXTENDED);
                cache.Add("ModificationID", Utils.Configuration.MODIFICATION_ID.ToString(), policy);
            }
            SecurityUser securityUser = cache.Get(GetCacheKey(userId, viewedId)) as SecurityUser;

            if (securityUser == null)
            {
                securityUser = new SecurityUser(userId);
            }

            return securityUser;
        }

        private void UpdateCache()
        {
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Utils.Configuration.CACHINGTIMEINMINUTES_EXTENDED);
            cache.Add(GetCacheKey(this.UserId,this.ViewedId), this, policy);
        }

        private SecurityUser(string userId, string viewedId = null)
        {
            UserId = userId;
            IsAdmin = IsAdminUser();
            ViewedId = viewedId;
            UpdateCache();
        }

        private bool? isItself;

        public bool IsItSelf(string guid, bool isPerson = false)
        {
            if (isItself.HasValue)
            {
                Log("isItself", string.Format("HasValue:{0}", isItself.Value));
                return isItself.Value;
            }

            if (guid != null && !String.IsNullOrWhiteSpace(guid))
            {
                Log("UserId/Guid", string.Format("{0}/{1}", UserId, guid));
                if (isPerson)
                {
                    PersonManager persManager = new PersonManager();
                    isItself = persManager.IsItSelf(UserId, guid);
                    Log("IsPerson",string.Format("isItself:{0}",isItself));
                }
                else
                {
                    EmploymentManager emplManager = new EmploymentManager();
                    isItself = emplManager.IsEmploymentOfPerson(UserId, guid);
                    Log("IsEmployment", string.Format("isItself:{0}", isItself));
                }
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Permission check 'IsItself' requires parameter guid!");
            }

            return isItself.Value;
        }

        private Dictionary<string, bool> cachedPermissions = new Dictionary<string, bool>();
        private Dictionary<string, EMDEmployment> cachedEmployments = new Dictionary<string, EMDEmployment>();

        private Dictionary<string, List<EMDEmployment>> cachedEmploymentsForPerson = new Dictionary<string, List<EMDEmployment>>();



        EmploymentHandler emplHandler = new EmploymentHandler();
        EmploymentManager emplManager = new EmploymentManager();

        private EMDEmployment GetCachedEmployment(string guid)
        {
            if (!cachedEmployments.ContainsKey(guid))
            {
                cachedEmployments.Add(guid, (EMDEmployment)emplHandler.GetObject<EMDEmployment>(guid));
            }


            return cachedEmployments[guid];
        }

        private List<EMDEmployment> GetCachedEmploymentsForPerson(string pers_guid)
        {
            if (!cachedEmploymentsForPerson.ContainsKey(pers_guid))
            {
                cachedEmploymentsForPerson.Add(pers_guid, emplManager.GetEmploymentsForPerson(pers_guid));
            }

            return cachedEmploymentsForPerson[pers_guid];
        }

        /// <summary>
        /// writes the given text value pair to the log file
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        public void Log(string text, string value)
        {
            IEDPLogger logger = EDPLogger.GetLogger("SecurityUserLogCacheKey");
            if (this.UserId.ToLower().Trim() == "richarda" || this.UserId.ToLower().Trim() == "hengsberg")
            {
                logger.Debug(string.Format("{0}-{1}: {2}", this.UserId.ToLower().Trim(), text,value));
            }
        }

        public bool hasPermission(string permission, SecurityUserParameterFlags parameterFlags, string ente_guid = null, string emplGuid = null, string persGuid = null)
        {
            string cachedPermissionKey = string.Format("Flags:{0}|{1}|{2}|{3}|{4}|{5}|{6}", permission, parameterFlags.ToString(), ente_guid, emplGuid, persGuid, this.UserId, this.IsAdmin);
            Log("cachedPermissionKey", cachedPermissionKey);

            if (cachedPermissions.ContainsKey(cachedPermissionKey))
            {
                Log("cachedPermissionKey found", cachedPermissionKey);
                return cachedPermissions[cachedPermissionKey];
            }

            if (IsAdmin)
            {
                cachedPermissions.Add(cachedPermissionKey, true);
                Log("added cachedPermissionKey IsAdmin", cachedPermissionKey);
                return true;
            }
            if (parameterFlags.IsItself)
            {
                if (emplGuid != null)
                {
                    if (IsItSelf(emplGuid))
                    {
                        cachedPermissions.Add(cachedPermissionKey, true);
                        Log("added cachedPermissionKey IsItself emplGuid", cachedPermissionKey);
                        return true;
                    }
                }
                else if (persGuid != null)
                {
                    if (IsItSelf(persGuid, true))
                    {
                        cachedPermissions.Add(cachedPermissionKey, true);
                        Log("added cachedPermissionKey IsItself persGuid", cachedPermissionKey);
                        return true;
                    }
                }
                Log("Error IsItself was not hit for", cachedPermissionKey);
            }

            if (parameterFlags.CheckPlainPermission && !string.IsNullOrWhiteSpace(permission))
            {
                bool returnValue = IsAllowedPermission(permission);
                cachedPermissions.Add(cachedPermissionKey, returnValue);
                return returnValue;
            }
            if (parameterFlags.IsLineManager && emplGuid != null)
            {
                EMDEmployment empl = GetCachedEmployment(emplGuid);
                if (IsLineManager(empl))
                {
                    cachedPermissions.Add(cachedPermissionKey, true);
                    return true;
                }
            }
            if (parameterFlags.IsTeamLeader && emplGuid != null)
            {
                EMDEmployment empl = GetCachedEmployment(emplGuid);
                if (IsTeamLeader(UserId, empl))
                {
                    cachedPermissions.Add(cachedPermissionKey, true);
                    return true;
                }
            }
            if (parameterFlags.IsCostcenterManager && emplGuid != null)
            {
                EMDEmployment empl = GetCachedEmployment(emplGuid);
                if (IsCostcenterManager(UserId, empl))
                {
                    cachedPermissions.Add(cachedPermissionKey, true);
                    return true;
                }
            }
            if (parameterFlags.IsAssistence && emplGuid != null)
            {
                EMDEmployment empl = GetCachedEmployment(emplGuid);
                if (IsAssistence(UserId, empl))
                {
                    cachedPermissions.Add(cachedPermissionKey, true);
                    return true;
                }
            }

            //Hier dann die Allowed Company usw.
            if (!string.IsNullOrWhiteSpace(permission) && ente_guid != null)
            {
                bool returnValue = IsAllowedPermissionForEnterprise(permission, ente_guid.ToString());
                cachedPermissions.Add(cachedPermissionKey, returnValue);
                return returnValue;
            }
            else if (!string.IsNullOrWhiteSpace(permission) && emplGuid != null)
            {
                bool returnValue = false;

                EmploymentHandler emplHandler = new EmploymentHandler();
                EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
                EMDEmployment empl = GetCachedEmployment(emplGuid);
                EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(empl.ENLO_Guid);

                returnValue = IsAllowedPermissionForEnterprise(permission, enlo.E_Guid);

                // overrule permission if the user has AdvancedSearchView permission
                if (!returnValue && permission == SecurityPermission.Personprofile_View_Historical)
                {
                    returnValue = hasPermission(SecurityPermission.AdvancedSearch_View, new SecurityUserParameterFlags(checkPlainPermisson: true));
                }

                cachedPermissions.Add(cachedPermissionKey, returnValue);
                return returnValue;
            }
            else if (string.IsNullOrWhiteSpace(permission)) //Für den Fall das nur isItSelf & isLinemanager erwartet wird
            {
                cachedPermissions.Add(cachedPermissionKey, false);
                return false;
            }

            cachedPermissions.Add(cachedPermissionKey, false);
            UpdateCache();
            return false;
        }


        private Dictionary<string, bool> cachedHasPermissions = new Dictionary<string, bool>();

        //Checks the permission for all enterprises the person belongs to
        public bool hasPermission(string permission, string pers_guid, bool checkIfItSelf = true)
        {
            string cacheKey = string.Format("hasPermission_{0}{1}{2}{3}{4}", permission, pers_guid, checkIfItSelf, this.UserId, this.IsAdmin);

            if (cachedHasPermissions.ContainsKey(cacheKey))
            {
                return cachedHasPermissions[cacheKey];
            }

            if (IsAdmin)
            {
                return true;
            }

            List<EMDEmployment> empls = GetCachedEmploymentsForPerson(pers_guid);
            DateTime now = DateTime.Now;
            EMD_Entities db_Context = new EMD_Entities();
            bool result = false;
            foreach (EMDEmployment empl in empls)
            {
                Enterprise enterprise = (from enlo in db_Context.EnterpriseLocation
                                         join ente in db_Context.Enterprise on enlo.E_Guid equals ente.Guid
                                         where enlo.Guid == empl.ENLO_Guid
                                         && enlo.ValidFrom < now && enlo.ValidTo > now && ente.ValidFrom < now && ente.ValidTo > now
                                         select ente).FirstOrDefault();

                SecurityUserParameterFlags flags = new SecurityUserParameterFlags();
                if (checkIfItSelf)
                    flags.IsItself = true;

                result = hasPermission(permission, flags, enterprise.Guid, empl.Guid);

                if (result)
                    return result;

            }

            cachedHasPermissions.Add(cacheKey, result);
            UpdateCache();
            return result;
        }


        public List<EMDEnterpriseLocation> AllowedEnterpriseLocations(string permission)
        {
            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
            List<EMDEnterpriseLocation> enterpriseLocations = null;

            if (IsAdmin)
            {
                enterpriseLocations = enloHandler.GetObjects<EMDEnterpriseLocation, EnterpriseLocation>().Cast<EMDEnterpriseLocation>().ToList();
            }
            else
            {
                List<string> allowedEnterpriseGuids = (from item in AllowedEnterprises(permission)
                                                       where item.ValidFrom < DateTime.Now && item.ValidTo > DateTime.Now
                                                       select item.Guid).ToList();

                EMD_Entities db_Context = new EMD_Entities();
                List<EnterpriseLocation> dbEnterpriseLocations = (from enlo in db_Context.EnterpriseLocation where allowedEnterpriseGuids.Contains(enlo.E_Guid) select enlo).ToList();
                enterpriseLocations = new List<EMDEnterpriseLocation>();

                dbEnterpriseLocations.ForEach(item =>
                {
                    EMDEnterpriseLocation emdEnlo = new EMDEnterpriseLocation();
                    if (!enterpriseLocations.Contains(emdEnlo))
                        enterpriseLocations.Add(emdEnlo);
                });
            }

            return enterpriseLocations;
        }

        public List<EMDLocation> AllowedLocations(string permission)
        {
            LocationHandler locaHandler = new LocationHandler();
            List<EMDLocation> locations = null;

            if (IsAdmin)
            {
                locations = locaHandler.GetObjects<EMDLocation, Location>().Cast<EMDLocation>().ToList();
            }
            else
            {
                List<string> allowedEnterpriseGuids = (from item in AllowedEnterprises(permission)
                                                       where item.ValidFrom < DateTime.Now && item.ValidTo > DateTime.Now
                                                       select item.Guid).ToList();

                EMD_Entities db_Context = new EMD_Entities();
                List<Location> dbLocations = (from enlo in db_Context.EnterpriseLocation join loca in db_Context.Location on enlo.L_Guid equals loca.Guid where allowedEnterpriseGuids.Contains(enlo.E_Guid) select loca).ToList();
                locations = new List<EMDLocation>();

                dbLocations.ForEach(item =>
                {
                    EMDLocation emdLoca = new EMDLocation();
                    if (!locations.Contains(emdLoca))
                        locations.Add(emdLoca);
                });
            }

            return locations;
        }

        private Dictionary<string, List<EMDEnterprise>> cachedAllowedEnterprises = new Dictionary<string, List<EMDEnterprise>>();



        public List<EMDEnterprise> AllowedEnterprises(string permission)
        {
            if (cachedAllowedEnterprises.ContainsKey(permission))
            {
                return cachedAllowedEnterprises[permission];
            }


            List<Enterprise> dbEnterprises = new List<Enterprise>();
            List<EMDEnterprise> enterprises = new List<EMDEnterprise>();
            EnterpriseManager enterpriseManager = new EnterpriseManager();

            if (IsAdmin)
            {
                enterprises = enterpriseManager.GetList();
            }
            else
            {
                List<SecurityPermission> permissions = AllowedPermissions();

                permissions = permissions.Where(item => item.Permission.Contains(permission)).ToList();
                List<string> enteGuidsSummary = new List<string>();
                if (permissions.Count > 0)
                {
                    EMD_Entities db_Context = new EMD_Entities();

                    string whereClause = String.Empty;
                    foreach (SecurityPermission perm in permissions)
                    {
                        List<string> enteGuids = perm.ENTE_Guids.Split(',').ToList();
                        foreach (string enteGuid in enteGuids)
                        {
                            if (!enteGuidsSummary.Contains(enteGuid))
                                enteGuidsSummary.Add(enteGuid);
                        }
                    }

                    enteGuidsSummary = enteGuidsSummary.Distinct().ToList();

                    dbEnterprises = (from ente in db_Context.Enterprise where enteGuidsSummary.Contains(ente.Guid) select ente).ToList();

                    dbEnterprises.ForEach(ente =>
                    {
                        EMDEnterprise emdEnte = new EMDEnterprise();
                        emdEnte.ActiveFrom = ente.ActiveFrom;
                        emdEnte.ActiveTo = ente.ActiveTo;
                        emdEnte.ARA = ente.ARA;
                        emdEnte.Created = ente.Created;
                        emdEnte.DVR = ente.DVR;
                        emdEnte.HasEmployees = ente.HasEmployees;
                        emdEnte.E_ID = ente.E_ID;
                        emdEnte.E_ID_new = ente.E_ID_new;
                        emdEnte.E_ID_Parent = ente.E_ID_Parent;
                        emdEnte.E_ID_Root = ente.E_ID_Root;
                        emdEnte.FibuGericht = ente.FibuGericht;
                        emdEnte.FibuNummer = ente.FibuNummer;
                        emdEnte.Guid = ente.Guid;
                        emdEnte.Guid_Parent = ente.Guid_Parent;
                        emdEnte.Guid_Root = ente.Guid_Root;
                        emdEnte.HistoryGuid = ente.HistoryGuid;
                        emdEnte.HomeInternet = ente.HomeInternet;
                        emdEnte.HomeIntranet = ente.HomeIntranet;
                        emdEnte.IntranetCOM = ente.IntranetCOM;
                        emdEnte.Modified = ente.Modified;
                        emdEnte.NameLong = ente.NameLong;
                        emdEnte.NameShort = ente.NameShort;
                        emdEnte.O_Guid_Dis = ente.O_Guid_Dis;
                        emdEnte.O_Guid_Prof = ente.O_Guid_Prof;
                        emdEnte.O_ID_Dis = ente.O_ID_Dis;
                        emdEnte.O_ID_Prof = ente.O_ID_Prof;
                        emdEnte.Synonyms = ente.Synonyms;
                        emdEnte.UID1 = ente.UID1;
                        emdEnte.UID2 = ente.UID2;
                        emdEnte.USDO_Guid = ente.USDO_Guid;
                        emdEnte.ValidFrom = ente.ValidFrom;
                        emdEnte.ValidTo = ente.ValidTo;
                        if (!enterprises.Contains(emdEnte))
                            enterprises.Add(emdEnte);

                        //Add SubEnterprises
                        EnterpriseTree enterpriseTree = new EnterpriseTree();
                        enterpriseTree.Fill(enterpriseManager.Get(emdEnte.Guid_Root));
                        List<EMDEnterprise> childs = enterpriseTree.GetAllChildrenOf(emdEnte.Guid);

                        //List<EMDEnterprise> childs = enteHandler.GetAllSubEnterprisesFromParent(emdEnte.Guid);
                        foreach (EMDEnterprise childEnte in childs)
                        {
                            if (!enterprises.Any(item => item.Guid == childEnte.Guid))
                            {

                                // logger.Debug("Add ente: " + childEnte.Guid);
                                enterprises.Add(childEnte);

                            }
                        }
                    });

                }
            }
            enterprises = enterprises.OrderBy(e => e.NameShort).ToList();
            //enterprises = enterprises.Distinct().ToList();
            cachedAllowedEnterprises.Add(permission, enterprises);
            UpdateCache();
            return enterprises;
        }

        public bool IsAllowedEnterprise(string permission, string enteGuid)
        {
            if (IsAdmin)
            {
                return true;
            }
            else
            {
                if (this.AllowedEnterprises(permission).Where(item => item.Guid == enteGuid).ToList().Count > 0)
                    return true;
                else
                    return false;
            }
        }

        public List<EMDOrgUnit> AllowedOrgUnits(string securityPermission)
        {
            OrgUnitHandler ouHandler = new OrgUnitHandler();

            Dictionary<string, OrgUnitTree> orgunitsTrees = new Dictionary<string, OrgUnitTree>();

            List<EMDOrgUnit> orgUnits = null;
            if (IsAdmin)
            {
                orgUnits = ouHandler.GetObjects<EMDOrgUnit, OrgUnit>().Cast<EMDOrgUnit>().ToList();
                return orgUnits;
            }
            else
            {
                List<EMDEnterprise> enterprises = this.AllowedEnterprises(securityPermission);
                List<string> orgUnitGuids = new List<string>();
                enterprises.ForEach(item =>
                {
                    if (!string.IsNullOrWhiteSpace(item.O_Guid_Dis))
                        orgUnitGuids.Add(item.O_Guid_Dis);
                }
            );

                orgUnits = (from ou in ouHandler.GetObjects<EMDOrgUnit, OrgUnit>().ToList() where orgUnitGuids.Contains(ou.Guid) select ou).Cast<EMDOrgUnit>().ToList();
            }

            List<EMDOrgUnit> orgUnitsWithChildren = new List<EMDOrgUnit>();

            if (orgUnits.Count > 0)
            {
                //OrgUnitTree orgUnitTree = new OrgUnitTree();

                //orgUnitTree.Fill(ouHandler.GetObject<EMDOrgUnit>(orgUnits[0].Guid_Root));

                foreach (EMDOrgUnit ou in orgUnits)
                {
                    OrgUnitTree orgUnitTree;
                    // get the tree from the dictionary (in case the same root is requested)
                    if (orgunitsTrees.ContainsKey(ou.Guid_Root))
                    {
                        orgUnitTree = orgunitsTrees[ou.Guid_Root];
                    }
                    else
                    {
                        orgUnitTree = new OrgUnitTree();
                        orgUnitTree.Fill(ouHandler.GetObject<EMDOrgUnit>(ou.Guid_Root));

                        orgunitsTrees.Add(ou.Guid_Root, orgUnitTree);
                    }


                    if (!orgUnitsWithChildren.Any(item => item.Guid == ou.Guid))
                        orgUnitsWithChildren.Add(ou);

                    List<EMDOrgUnit> children = orgUnitTree.GetAllChildrenOf(ou.Guid);
                    //List<EMDOrgUnit> children = ouHandler.GetAllSubOrgUnitsFromParent(ou.Guid);

                    children.ForEach(child =>
                    {
                        if (!orgUnitsWithChildren.Any(item => item.Guid == child.Guid))
                            orgUnitsWithChildren.Add(child);
                    });
                }
            }

            return orgUnitsWithChildren;
        }

        public bool IsAllowedOrgUnit(string orguGuid, bool isEdit = false)
        {
            if (IsAdmin)
            {
                return true;
            }
            else
            {
                string securityPermission = SecurityPermission.OrgUnitManager_View;
                if (isEdit)
                    securityPermission = SecurityPermission.OrgUnitManager_View_Manage;

                List<string> orgUnits = (from ou in this.AllowedOrgUnits(securityPermission) where ou.Guid == orguGuid select ou.Guid).ToList();

                if (orgUnits.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        private Dictionary<string, List<EMDAccount>> cachedAllowedCostCenters = new Dictionary<string, List<EMDAccount>>();

        public List<EMDAccount> AllowedCostCenters(string securityPermission)
        {
            if (cachedAllowedCostCenters.ContainsKey(securityPermission))
            {
                return cachedAllowedCostCenters[securityPermission];
            }

            List<EMDEnterprise> enterprises = this.AllowedEnterprises(securityPermission);
            List<string> enterpriseGuids = new List<string>();
            enterprises.ForEach(item =>
                    {
                        enterpriseGuids.Add(item.Guid);
                    }
                );

            AccountHandler accHandler = new AccountHandler();
            List<EMDAccount> accounts = (from acc in accHandler.GetObjects<EMDAccount, Account>().Cast<EMDAccount>().ToList() select acc).Where(item => enterpriseGuids.Contains(item.E_Guid)).ToList();

            cachedAllowedCostCenters.Add(securityPermission, accounts);
            UpdateCache();
            return accounts;
        }

        public bool IsAllowedCostCenter(string accoGuid, bool isEdit = false)
        {
            if (IsAdmin)
            {
                return true;
            }
            else
            {
                string securityPermission = SecurityPermission.CostCenterManager_View;
                if (isEdit)
                    securityPermission = SecurityPermission.CostCenterManager_View_Manage;

                List<string> accounts = (from acc in this.AllowedCostCenters(securityPermission) where acc.Guid == accoGuid select acc.Guid).ToList();
                if (accounts.Count > 0)
                    return true;
                else
                    return false;
            }
        }


        private Dictionary<string, List<SecurityPermission>> cachedAllowedPermissions = new Dictionary<string, List<SecurityPermission>>();

        public List<SecurityPermission> AllowedPermissions()
        {
            if (cachedAllowedPermissions.ContainsKey(this.UserId))
            {
                return cachedAllowedPermissions[this.UserId];
            }

            PersonManager persManager = new PersonManager();
            EMDPerson person = (EMDPerson)persManager.GetPersonByUserId(this.UserId);

            List<EMDEmployment> employments = GetCachedEmploymentsForPerson(person.Guid);
            List<SecurityPermission> securityPermissions = new List<SecurityPermission>();
            foreach (EMDEmployment empl in employments)
            {
                securityPermissions.AddRange(AllowedPermissionsForEmployment(empl.Guid));
            }

            cachedAllowedPermissions.Add(this.UserId, securityPermissions);
            UpdateCache();
            return securityPermissions;
        }

        private string ResolveChildEnterprises(string ente_guids)
        {
            List<string> enterpriseGuids = ente_guids.Split(',').ToList();
            EnterpriseHandler enteHandler = new EnterpriseHandler();
            foreach (string enteGuid in enterpriseGuids)
            {
                List<EMDEnterprise> childs = enteHandler.GetAllSubEnterprisesFromParent(enteGuid);
                foreach (EMDEnterprise childEnte in childs)
                {
                    if (!ente_guids.Contains(childEnte.Guid))
                        ente_guids += "," + childEnte.Guid;
                }
            }

            return ente_guids;
        }

        public class OrgunitSecurityRole
        {
            public OrgUnitRole secOrgUnitRole { get; set; }
            public OrgUnit orgu { get; set; }
            public Role ro { get; set; }
        }


        private Dictionary<string, List<OrgunitSecurityRole>> cachedListOrgUnitSecurityRoles = new Dictionary<string, List<OrgunitSecurityRole>>();

        private List<SecurityPermission> AllowedPermissionsForEmployment(string empl_guid)
        {
            List<SecurityPermission> securityPermissions = new List<SecurityPermission>();

            DateTime now = DateTime.Now;
            EMD_Entities db_Context = new EMD_Entities();

            List<OrgunitSecurityRole> listOrgUnitSecurityRoles = new List<OrgunitSecurityRole>();


            if (cachedListOrgUnitSecurityRoles.ContainsKey(empl_guid))
            {
                listOrgUnitSecurityRoles = cachedListOrgUnitSecurityRoles[empl_guid];
            }
            else
            {

                var listOrgUnitSecurityRolesVar = (from secOrgUnitRole in db_Context.OrgUnitRole
                                                   join orgu in db_Context.OrgUnit on secOrgUnitRole.O_Guid equals orgu.Guid
                                                   join ro in db_Context.Role on secOrgUnitRole.R_Guid equals ro.Guid
                                                   where secOrgUnitRole.EP_Guid == empl_guid && orgu.IsSecurity == true &&
                                                       secOrgUnitRole.ValidFrom < now && secOrgUnitRole.ValidTo > now && secOrgUnitRole.ActiveFrom < now && secOrgUnitRole.ActiveTo > now &&
                                                       orgu.ValidFrom < now && orgu.ValidTo > now && orgu.ActiveFrom < now && orgu.ActiveTo > now &&
                                                       ro.ValidFrom < now && ro.ValidTo > now && ro.ActiveFrom < now && ro.ActiveTo > now
                                                   select new { secOrgUnitRole, orgu, ro }
                                                                ).ToList();

                foreach (var item in listOrgUnitSecurityRolesVar)
                {
                    listOrgUnitSecurityRoles.Add(new OrgunitSecurityRole() { secOrgUnitRole = item.secOrgUnitRole, orgu = item.orgu, ro = item.ro });
                }

                cachedListOrgUnitSecurityRoles.Add(empl_guid, listOrgUnitSecurityRoles);

            }

            foreach (var item in listOrgUnitSecurityRoles)
            {
                List<SecurityAction> secActions = (from secact in db_Context.SecurityAction
                                                   where secact.ROLE_Guid == item.ro.Guid
                                                   && secact.ValidFrom < now && secact.ValidTo > now && secact.ActiveFrom < now && secact.ActiveTo > now
                                                   select secact
                 ).ToList();

                foreach (SecurityAction secAct in secActions)
                {
                    SecurityPermission secPerm = securityPermissions.Where(perm => perm.Permission == secAct.Action).FirstOrDefault();

                    if (secPerm == null)
                    {
                        //Create entry
                        secPerm = new SecurityPermission(secAct.Action, item.orgu.E_Guid);
                        securityPermissions.Add(secPerm);
                    }
                    else
                    {
                        //Update entry with enterprise
                        if (!secPerm.ENTE_Guids.Contains(item.orgu.E_Guid))
                            secPerm.ENTE_Guids += "," + item.orgu.E_Guid;
                    }
                }
            }

            return securityPermissions;
        }

        public bool IsAllowedPermission(string permission)
        {
            List<SecurityPermission> permissions = AllowedPermissions();

            SecurityPermission specificPermission = permissions.Where(perm => perm.Permission.Contains(permission)).FirstOrDefault();
            if (specificPermission != null)
            {
                return true;
            }
            return false;
        }

        private Dictionary<string, bool> cachedIsAllowedPermissionForEnterprise = new Dictionary<string, bool>();

        public bool IsAllowedPermissionForEnterprise(string permission, string ente_guid)
        {
            string cacheKey = string.Format("IsAllowedPermissionForEnterprise_permission:{0}ente_guid:{1}userId:{2}IsAdmin:{3}", permission, ente_guid, this.UserId, this.IsAdmin);

            if (cachedIsAllowedPermissionForEnterprise.ContainsKey(cacheKey))
            {
                return cachedIsAllowedPermissionForEnterprise[cacheKey];
            }


            List<SecurityPermission> permissions = AllowedPermissions();

            SecurityPermission specificPermission = permissions.Where(perm => perm.Permission.Contains(permission)).FirstOrDefault();
            if (specificPermission != null)
            {
                string enterpriseWithChilds = ResolveChildEnterprises(specificPermission.ENTE_Guids);
                if (enterpriseWithChilds.Contains(ente_guid))
                {
                    //Permission gefunden & gültig für diese enterprise
                    cachedIsAllowedPermissionForEnterprise.Add(cacheKey, true);
                    return true;
                }
            }

            cachedIsAllowedPermissionForEnterprise.Add(cacheKey, false);
            UpdateCache();
            return false;
        }

        private bool IsAdminUser()
        {
            return IsAdminUser(UserId.ToLower());

        }

        public bool IsAdminUser(string userId)
        {

            if (ConfigurationManager.AppSettings["EDP20Core.Admins"] != null && ConfigurationManager.AppSettings["EDP20Core.Admins"] != String.Empty)
            {
                List<string> listAdmins = ConfigurationManager.AppSettings["EDP20Core.Admins"].ToLower().Split(',').ToList();
                foreach (string entry in listAdmins)
                {
                    if (entry == userId.ToLower())
                        return true;
                }
            }
            return false;
        }



        #region "IsLineManager"

        public bool IsLineManager(EMDEmployment employmentToCheckFor)
        {
            PersonManager persMgr = new PersonManager();
            EMDPerson persLineManager = (EMDPerson)persMgr.GetPersonByUserId(UserId);

            return IsLineManager(persLineManager, employmentToCheckFor);
        }

        public bool IsLineManager(EMDPerson personFromUser, EMDEmployment employmentToCheckFor)
        {
            EmploymentHandler emplHandler = new EmploymentHandler();
            PersonManager persManager = new PersonManager();

            List<EMDEmployment> iemdEmployments = GetCachedEmploymentsForPerson(personFromUser.Guid);
            List<EMDEmployment> employmentsFromUser = new List<EMDEmployment>();
            foreach (EMDEmployment empl in iemdEmployments)
            {
                employmentsFromUser.Add(empl);
            }
            return IsLineManager(employmentsFromUser, employmentToCheckFor);
        }

        public bool IsLineManager(EMDEmployment employmentFromUser, EMDEmployment employmentToCheckFor)
        {
            List<EMDEmployment> employmentsFromUser = new List<EMDEmployment>();
            employmentsFromUser.Add(employmentFromUser);
            return IsLineManager(employmentsFromUser, employmentToCheckFor);
        }

        public bool IsLineManager(List<EMDEmployment> employmentsFromUser, EMDEmployment employmentToCheckFor)
        {
            if (employmentToCheckFor.ActiveTo > DateTime.Now)
                return HasOrgUnitRole(employmentsFromUser, employmentToCheckFor, 10500);
            else
                return false;
        }
        #endregion

        #region "#IsTeamleader"

        public bool IsTeamLeader(List<EMDEmployment> employmentsFromUser, EMDEmployment employmentToCheckFor)
        {
            if (employmentToCheckFor.ActiveTo > DateTime.Now)
                return HasOrgUnitRole(employmentsFromUser, employmentToCheckFor, 10400);
            else
                return false;
        }

        public bool IsTeamLeader(EMDEmployment employmentFromUser, EMDEmployment employmentToCheckFor)
        {
            List<EMDEmployment> employmentsFromUser = new List<EMDEmployment>();
            employmentsFromUser.Add(employmentFromUser);
            return IsTeamLeader(employmentsFromUser, employmentToCheckFor);
        }

        public bool IsTeamLeader(string userId, EMDEmployment employmentToCheckFor)
        {
            PersonManager persMgr = new PersonManager();
            EMDPerson persLineManager = (EMDPerson)persMgr.GetPersonByUserId(UserId);

            return IsTeamLeader(persLineManager, employmentToCheckFor);
        }

        public bool IsTeamLeader(EMDPerson personFromUser, EMDEmployment employmentToCheckFor)
        {
            EmploymentHandler emplHandler = new EmploymentHandler();
            PersonManager persManager = new PersonManager();

            List<EMDEmployment> iemdEmployments = GetCachedEmploymentsForPerson(personFromUser.Guid);
            List<EMDEmployment> employmentsFromUser = new List<EMDEmployment>();
            foreach (EMDEmployment empl in iemdEmployments)
            {
                employmentsFromUser.Add(empl);
            }
            return IsTeamLeader(employmentsFromUser, employmentToCheckFor);
        }

        private bool HasOrgUnitRole(List<EMDEmployment> employmentsFromUser, EMDEmployment employmentToCheckFor, int RoleId)
        {
            if (employmentsFromUser.Count > 0)
            {

                try
                {
                    List<string> supervisors = search.SearchOrgUnitRoleForEmployment(RoleId, employmentToCheckFor.Guid);
                    foreach (string sup in supervisors)
                    {
                        foreach (EMDEmployment empl in employmentsFromUser)
                        {
                            if (sup.Equals(empl.Guid))
                                return true;
                        }
                    }
                }
                catch (BaseException ex)
                {
                    IISLogger logger = ISLogger.GetLogger("SecurityUser");
                    logger.Warn("function HasOrgUnitRole - Error :" + ex.Message + " " + ex.StackTrace);
                }
            }
            return false;
        }
        #endregion

        public bool IsCostcenterManager(string userId, EMDEmployment employmentToCheckFor)
        {
            PersonManager persMgr = new PersonManager();
            EMDPerson persLineManager = (EMDPerson)persMgr.GetPersonByUserId(UserId);

            return IsCostcenterManager(persLineManager, employmentToCheckFor);
        }

        private bool IsCostcenterManager(EMDEmployment employmentFromUser, EMDEmployment employmentToCheckFor)
        {
            List<EMDEmployment> employmentsFromUser = new List<EMDEmployment>();
            employmentsFromUser.Add(employmentFromUser);
            return IsCostcenterManager(employmentsFromUser, employmentToCheckFor);
        }

        private bool IsCostcenterManager(List<EMDEmployment> employmentsFromUser, EMDEmployment employmentToCheckFor)
        {
            EmploymentManager emplManager = new EmploymentManager();
            return emplManager.IsCostcenterManager(employmentsFromUser, employmentToCheckFor);
        }

        public bool IsCostcenterManager(EMDPerson personFromUser, EMDEmployment employmentToCheckFor)
        {
            EmploymentHandler emplHandler = new EmploymentHandler();
            PersonManager persManager = new PersonManager();

            List<EMDEmployment> iemdEmployments = GetCachedEmploymentsForPerson(personFromUser.Guid);
            List<EMDEmployment> employmentsFromUser = new List<EMDEmployment>();
            foreach (EMDEmployment empl in iemdEmployments)
            {
                employmentsFromUser.Add(empl);
            }
            return IsCostcenterManager(employmentsFromUser, employmentToCheckFor);
        }

        public bool IsAssistence(string userId, EMDEmployment employmentToCheckFor)
        {
            PersonManager persMgr = new PersonManager();
            EMDPerson persLineManager = (EMDPerson)persMgr.GetPersonByUserId(UserId);

            return IsAssistence(persLineManager, employmentToCheckFor);
        }

        public bool IsAssistence(List<EMDEmployment> employmentsFromUser, EMDEmployment employmentToCheckFor)
        {
            EmploymentManager emplManager = new EmploymentManager();
            return emplManager.IsAssistence(employmentsFromUser, employmentToCheckFor);
        }

        public bool IsAssistence(EMDPerson personFromUser, EMDEmployment employmentToCheckFor)
        {
            EmploymentHandler emplHandler = new EmploymentHandler();
            PersonManager persManager = new PersonManager();

            List<EMDEmployment> iemdEmployments = GetCachedEmploymentsForPerson(personFromUser.Guid);
            List<EMDEmployment> employmentsFromUser = new List<EMDEmployment>();
            foreach (EMDEmployment empl in iemdEmployments)
            {
                employmentsFromUser.Add(empl);
            }
            return IsAssistence(employmentsFromUser, employmentToCheckFor);
        }

        private Dictionary<string, List<EMDPersonEmployment>> cachedAllowedEmploymentsForCostcenterManager = new Dictionary<string, List<EMDPersonEmployment>>();

        public List<EMDPersonEmployment> AllowedEmploymentsForCostcenterManager()
        {
            return AllowedEmploymentsForCostcenterManager(this.UserId);
        }

        public List<EMDPersonEmployment> AllowedEmploymentsForCostcenterManager(string userId)
        {
            if (cachedAllowedEmploymentsForCostcenterManager.ContainsKey(userId))
            {
                return cachedAllowedEmploymentsForCostcenterManager[userId];
            }

            PersonManager persManager = new PersonManager();
            PersonHandler persHandler = new PersonHandler();
            EMDPerson persUser = persManager.GetPersonByUserId(userId);
            //EmploymentHandler emplHandler = new EmploymentHandler();
            EmploymentManager emplManager = new EmploymentManager();

            AccountHandler accHandler = new AccountHandler();
            EmploymentAccountHandler emplAccHandler = new EmploymentAccountHandler();

            var accountGuids = (from empl in GetCachedEmploymentsForPerson(persUser.Guid)
                                join acc in accHandler.GetObjects<EMDAccount, Account>().Cast<EMDAccount>().ToList() on empl.Guid equals acc.Responsible
                                select new { acc.Guid }
                    ).ToList();

            List<string> accountGuidList = new List<string>();

            accountGuids.ForEach(item =>
            {
                accountGuidList.Add(item.Guid);
            });

            var empls = (from emplAcc in emplAccHandler.GetObjects<EMDEmploymentAccount, EmploymentAccount>().Cast<EMDEmploymentAccount>().ToList()
                         //join empl in emplHandler.GetObjects<EMDEmployment, Employment>().Cast<EMDEmployment>().ToList() on emplAcc.EP_Guid equals empl.Guid
                         join empl in emplManager.GetActiveEmployments() on emplAcc.EP_Guid equals empl.Guid
                         join pers in persHandler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>().ToList() on empl.P_Guid equals pers.Guid
                         where accountGuidList.Contains(emplAcc.AC_Guid)
                         && pers.UserID != null
                         && !String.IsNullOrWhiteSpace(pers.UserID)
                         select new { pers, empl }
                         ).ToList();

            List<EMDPersonEmployment> employments = new List<EMDPersonEmployment>();
            foreach (var item in empls)
            {
                employments.Add(new EMDPersonEmployment(item.pers, item.empl));
            }
            cachedAllowedEmploymentsForCostcenterManager.Add(userId, employments);
            UpdateCache();
            return employments;
        }

        private Dictionary<string, List<EMDPersonEmployment>> cachedAllowedEmploymentsForEnterprises = new Dictionary<string, List<EMDPersonEmployment>>();

        public List<EMDPersonEmployment> AllowedEmploymentsForEnterprises()
        {
            return AllowedEmploymentsForEnterprises(this.UserId);
        }

        public List<EMDPersonEmployment> AllowedEmploymentsForEnterprises(string userId)
        {
            if (cachedAllowedEmploymentsForEnterprises.ContainsKey(userId))
            {
                return cachedAllowedEmploymentsForEnterprises[userId];
            }

            //List<EMDEnterprise> enterprises = this.AllowedEnterprises(SecurityPermission.CostCenterManager_View_Manage);
            List<EMDEnterprise> enterprises = this.AllowedEnterprises(SecurityPermission.Enterprise_View_Manage);
            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
            //EmploymentHandler emplHandler = new EmploymentHandler();
            EmploymentManager emplManager = new EmploymentManager();
            PersonHandler persHandler = new PersonHandler();
            List<string> enteGuids = new List<string>();
            enterprises.ForEach(item =>
            {
                enteGuids.Add(item.Guid);
            });

            //from ente in enterprises
            var empls = from enlo in enloHandler.GetObjects<EMDEnterpriseLocation, EnterpriseLocation>().Cast<EMDEnterpriseLocation>()
                        //join empl in emplHandler.GetObjects<EMDEmployment, Employment>().Cast<EMDEmployment>() on enlo.Guid equals empl.ENLO_Guid
                        join empl in emplManager.GetActiveEmployments().Cast<EMDEmployment>() on enlo.Guid equals empl.ENLO_Guid
                        join pers in persHandler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>() on empl.P_Guid equals pers.Guid
                        where enteGuids.Contains(enlo.E_Guid)
                        select new { pers, empl };

            List<EMDPersonEmployment> employments = new List<EMDPersonEmployment>();
            foreach (var item in empls)
            {
                employments.Add(new EMDPersonEmployment(item.pers, item.empl));
            }
            cachedAllowedEmploymentsForEnterprises.Add(userId, employments);
            UpdateCache();
            return employments;
        }

        private Dictionary<string, List<EMDPersonEmployment>> cachedAllowedEmploymentsForAssistence = new Dictionary<string, List<EMDPersonEmployment>>();

        public List<EMDPersonEmployment> AllowedEmploymentsForAssistence()
        {
            return AllowedEmploymentsForAssistence(this.UserId);
        }

        public List<EMDPersonEmployment> AllowedEmploymentsForAssistence(string userId)
        {
            if (cachedAllowedEmploymentsForAssistence.ContainsKey(userId))
            {
                return cachedAllowedEmploymentsForAssistence[userId];
            }

            PersonManager persManager = new PersonManager();
            PersonHandler persHandler = new PersonHandler();
            EMDPerson persUser = persManager.GetPersonByUserId(userId);
            EmploymentHandler emplHandler = new EmploymentHandler();
            EmploymentManager emplManager = new EmploymentManager();
            AccountHandler accHandler = new AccountHandler();
            AccountGroupHandler accGroupHandler = new AccountGroupHandler();
            EmploymentAccountHandler emplAccHandler = new EmploymentAccountHandler();
            GroupHandler grpHandler = new GroupHandler();
            GroupMemberHandler grpMemberHandler = new GroupMemberHandler();

            EMDPerson pers = persManager.GetPersonByUserId(userId);

            List<EMDEmployment> employmentsInGroups = GetCachedEmploymentsForPerson(pers.Guid);
            List<string> emplsInGroupsGuids = new List<string>();
            employmentsInGroups.ForEach(item =>
            {
                emplsInGroupsGuids.Add(item.Guid);
            });

            var groups = (from grpmbr in grpMemberHandler.GetObjects<EMDGroupMember, GroupMember>().Cast<EMDGroupMember>().ToList()
                          join grp in grpHandler.GetObjects<EMDGroup, Group>().Cast<EMDGroup>().ToList() on grpmbr.G_Guid equals grp.Guid
                          where emplsInGroupsGuids.Contains(grpmbr.EP_Guid)
                          select new { grp.Guid }).ToList();

            List<string> groupGuids = new List<string>();
            groups.ForEach(item =>
            {
                groupGuids.Add(item.Guid);
            });

            var empls = (from accgrp in accGroupHandler.GetObjects<EMDAccountGroup, AccountGroup>().Cast<EMDAccountGroup>().ToList()
                         join acc in accHandler.GetObjects<EMDAccount, Account>().Cast<EMDAccount>().ToList() on accgrp.AC_Guid equals acc.Guid
                         join emplacc in emplAccHandler.GetObjects<EMDEmploymentAccount, EmploymentAccount>().Cast<EMDEmploymentAccount>().ToList() on acc.Guid equals emplacc.AC_Guid
                         //join empl in emplHandler.GetObjects<EMDEmployment, Employment>().Cast<EMDEmployment>().ToList() on emplacc.EP_Guid equals empl.Guid
                         join empl in emplManager.GetActiveEmployments() on emplacc.EP_Guid equals empl.Guid
                         join per in persHandler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>().ToList() on empl.P_Guid equals per.Guid
                         where groupGuids.Contains(accgrp.G_Guid)
                                 && pers.UserID != null
                                 && !String.IsNullOrWhiteSpace(pers.UserID)
                         select new { per, empl }
                            ).ToList();

            List<EMDPersonEmployment> employments = new List<EMDPersonEmployment>();
            foreach (var item in empls)
            {
                employments.Add(new EMDPersonEmployment(item.per, item.empl));
            }

            cachedAllowedEmploymentsForAssistence.Add(userId, employments);
            UpdateCache();
            return employments;
        }

        private Dictionary<string, List<EMDPersonEmployment>> cachedAllowedEmployments = new Dictionary<string, List<EMDPersonEmployment>>();

        private Dictionary<string, Dictionary<string, EMDPersonEmployment>> cachedDictAllowedEmployments = new Dictionary<string, Dictionary<string, EMDPersonEmployment>>();

        public EMDPersonEmployment GetAllowedEmployment(string emplGuid, bool includeOwnEmployments = false)
        {
            string cacheKey = string.Format("AllowedEmplDict_{0}-{1}-{2}-{3}", emplGuid, includeOwnEmployments, this.UserId, this.IsAdmin);

            Dictionary<string, EMDPersonEmployment> dict = null;

            if (cachedDictAllowedEmployments.ContainsKey(cacheKey))
            {
                dict = cachedDictAllowedEmployments[cacheKey];
            }
            else
            {
                dict = new Dictionary<string, EMDPersonEmployment>();

                List<EMDPersonEmployment> allowedEmpls = AllowedEmployments(includeOwnEmployments);

                foreach (EMDPersonEmployment personEmpl in allowedEmpls)
                {
                    if (!dict.ContainsKey(personEmpl.Empl.Guid))
                    {
                        dict.Add(personEmpl.Empl.Guid, personEmpl);
                    }
                }

                cachedDictAllowedEmployments.Add(cacheKey, dict);
            }


            EMDPersonEmployment foundEmpl = null;
            dict.TryGetValue(emplGuid, out foundEmpl);

            return foundEmpl;
        }


        public List<EMDPersonEmployment> AllowedEmployments(bool IncluedeOwnEmployments = false)
        {
            return this.AllowedEmployments(this.UserId, IncluedeOwnEmployments);
        }

        public List<EMDPersonEmployment> AllowedEmployments(string userId, bool includeOwnEmployments = false)
        {
            string cacheKey = string.Format("{0}-{1}-{2}-{3}", userId, includeOwnEmployments, this.UserId, this.IsAdmin);

            if (cachedAllowedEmployments.ContainsKey(cacheKey))
            {
                return cachedAllowedEmployments[cacheKey];
            }

            List<EMDPersonEmployment> employments = new List<EMDPersonEmployment>();

            if (IsAdmin)
            {
                //EmploymentHandler emplHandler = new EmploymentHandler();
                EmploymentManager emplManager = new EmploymentManager();
                PersonHandler persHandler = new PersonHandler();

                //var items = (from empl in emplHandler.GetObjects<EMDEmployment, Employment>().Cast<EMDEmployment>()
                var items = (from empl in emplManager.GetActiveEmployments()
                            join pers in persHandler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>() on empl.P_Guid equals pers.Guid
                             where pers.UserID != null && !String.IsNullOrWhiteSpace(pers.UserID)
                             select new { pers, empl }).ToList();

                items.ForEach(item =>
                {
                    employments.Add(new EMDPersonEmployment(item.pers, item.empl));
                });
            }
            else
            {
                employments = this.AllowedEmploymentsForCostcenterManager();
                employments.AddRange(this.AllowedEmploymentsForAssistence());
                employments.AddRange(this.AllowedEmploymentsForEnterprises());
                if (includeOwnEmployments)
                {
                    employments.AddRange(this.MyEmployments());
                }
                //TODO: Implement for Linemanager
                //employments.AddRange(this.getAllowedEmploymentsForRole(10500));
                //TODO: Implement for Prime => getAllowedEmploymntsForEnterprise
                employments = employments.Distinct().ToList();
            }

            cachedAllowedEmployments.Add(cacheKey, employments);
            UpdateCache();
            return employments;
        }

        public List<EMDPersonEmployment> MyEmployments()
        {
            List<EMDPersonEmployment> myEmployments = new List<EMDPersonEmployment>();
            EmploymentHandler emplHandler = new EmploymentHandler();
            PersonHandler persHandler = new PersonHandler();

            var items = (from empl in emplHandler.GetObjects<EMDEmployment, Employment>().Cast<EMDEmployment>()
                         join pers in persHandler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>() on empl.P_Guid equals pers.Guid
                         where pers.UserID == this.UserId
                         select new { pers, empl }).ToList();

            items.ForEach(item =>
            {
                myEmployments.Add(new EMDPersonEmployment(item.pers, item.empl));
            });

            return myEmployments;
        }


        public bool IsAllowedPerson(string persGuid, string securityPermission)
        {
            if (this.IsAdmin)
                return true;
            else
                return this.IsAllowedPerson(this.UserId, persGuid, securityPermission);
        }

        public bool IsAllowedPerson(string userId, string persGuid, string securityPermission)
        {
            bool isAllowed = false;
            if (this.IsAdmin)
                isAllowed = true;
            else
            {
                PersonManager persManager = new PersonManager();
                EMDPerson me = persManager.GetPersonByUserId(UserId);
                EMDPerson viewedPerson = persManager.Get(persGuid);

                if (viewedPerson.Guid_ModifiedBy == me.Guid) //Check if Person was created by me
                    isAllowed = true;
                else
                {
                    List<string> allowedEnterpriseGuids = this.AllowedEnterprises(securityPermission).Select(item => item.Guid).ToList();

                    EmploymentManager emplManager = new EmploymentManager();

                    EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
                    EnterpriseHandler enteHandler = new EnterpriseHandler();

                    var empls = from empl in GetCachedEmploymentsForPerson(persGuid)
                                join enlo in enloHandler.GetObjects<EMDEnterpriseLocation, EnterpriseLocation>().Cast<EMDEnterpriseLocation>().ToList() on empl.ENLO_Guid equals enlo.Guid
                                join ente in enteHandler.GetObjects<EMDEnterprise, Enterprise>().Cast<EMDEnterprise>().ToList() on enlo.E_Guid equals ente.Guid
                                where allowedEnterpriseGuids.Contains(ente.Guid)
                                select new { empl.Guid };

                    if (empls.Count() > 0)
                        isAllowed = true;
                    else
                        isAllowed = false;
                }
            }
            return isAllowed;
        }

        public bool IsAllowedEmployment(string userId, string emplGuid)
        {
            bool isAllowed = false;
            if (IsAdmin)
            {
                isAllowed = true;
            }
            else if (this.IsItSelf(emplGuid))
            {
                return true;
            }
            else
            {
                if (this.IsAllowedEmploymentForCostcenterManager(userId, emplGuid)
                    || this.IsAllowedEmploymentForAssistence(userId, emplGuid)
                    || this.IsAllowedEmploymentForEnterprise(userId, emplGuid)
                    // || this.IsAllowedEmploymentsForRole(userId, emplGuid)
                    )
                {
                    isAllowed = true;
                }
                //TODO: Implement for Linemanager
            }
            return isAllowed;
        }

        public bool IsAllowedEmploymentForCostcenterManager(string userId, string emplGuid)
        {
            if (this.IsAdmin)
                return true;

            List<EMDPersonEmployment> employments = AllowedEmploymentsForCostcenterManager();
            employments = employments.Where(item => item.Empl.Guid == emplGuid).ToList();

            if (employments.Count > 0)
                return true;
            else
                return false;
        }

        public bool IsAllowedEmploymentForAssistence(string userId, string emplGuid)
        {
            if (this.IsAdmin)
                return true;

            List<EMDPersonEmployment> employments = AllowedEmploymentsForAssistence();
            employments = employments.Where(item => item.Empl.Guid == emplGuid).ToList();

            if (employments.Count > 0)
                return true;
            else
                return false;
        }

        public bool IsAllowedEmploymentForEnterprise(string userId, string emplGuid)
        {
            if (this.IsAdmin)
                return true;

            List<EMDPersonEmployment> employments = AllowedEmploymentsForEnterprises();
            employments = employments.Where(item => item.Empl.Guid == emplGuid).ToList();

            if (employments.Count > 0)
                return true;
            else
                return false;
        }

        public bool IsMyEmployment(string userId, string emplGuid)
        {
            List<EMDPersonEmployment> employments = MyEmployments();
            employments = employments.Where(item => item.Empl.Guid == emplGuid).ToList();

            if (employments.Count > 0)
                return true;
            else
                return false;
        }

        public List<EMDPersonEmployment> AllowedEmploymentsForRole(int RoleId)
        {
            return AllowedEmploymentsForRole(this.UserId, RoleId);
        }

        public List<EMDPersonEmployment> AllowedEmploymentsForRole(string userId, int RoleId)
        {
            throw new NotImplementedException();
        }


        

    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.EDP.Core.Entities.Enums;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EmploymentHandler : EMDObjectHandler
    {
        public EmploymentHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }


        public EmploymentHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public EmploymentHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public EmploymentHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override IEMDObject<T> CreateObject<T>(IEMDObject<T> emdObject, string guid = null, bool datesAreSet = false)
        {
            return base.CreateObject(emdObject, guid, datesAreSet);
        }

        public override Type GetDBObjectType()
        {
            return new Employment().GetType();
        }

        internal override T CreateDataFromDBObject<T, S>(S dbObject, IPropertyCopier<S, T> propCopier)
        {
            Employment empl = dbObject as Employment;
            if (dbObject == null)
                return default(T);
            T emdObject = (T)Activator.CreateInstance(typeof(T), new object[] { empl.Guid, empl.Created, empl.Modified });
            EMDEmployment emdEmployment = emdObject as EMDEmployment;
            if (dbObject is Kapsch.IS.EDP.Core.DB.Employment && emdEmployment != null)
            {
                Kapsch.IS.EDP.Core.DB.Employment dbEmployment = dbObject as Kapsch.IS.EDP.Core.DB.Employment;
                emdEmployment.Guid = dbEmployment.Guid;
                emdEmployment.HistoryGuid = dbEmployment.HistoryGuid;
                emdEmployment.P_Guid = dbEmployment.P_Guid;
                emdEmployment.ET_Guid = dbEmployment.ET_Guid;
                emdEmployment.DGT_Guid = dbEmployment.DGT_Guid;
                emdEmployment.ENLO_Guid = dbEmployment.ENLO_Guid;
                emdEmployment.EP_ID = dbEmployment.EP_ID;
                emdEmployment.P_ID = dbEmployment.P_ID;
                emdEmployment.ET_ID = dbEmployment.ET_ID;
                emdEmployment.Entry = dbEmployment.Entry;
                emdEmployment.Exit = dbEmployment.Exit;
                emdEmployment.LastDay = dbEmployment.LastDay;
                emdEmployment.PersNr = dbEmployment.PersNr;
                emdEmployment.dpwKey = dbEmployment.dpwKey;
                emdEmployment.ValidFrom = dbEmployment.ValidFrom;
                emdEmployment.ValidTo = dbEmployment.ValidTo;
                emdEmployment.Created = dbEmployment.Created;
                emdEmployment.Modified = dbEmployment.Modified;
                emdEmployment.Exit_Report = dbEmployment.Exit_Report;
                emdEmployment.DGT_ID = dbEmployment.DGT_ID;
                emdEmployment.Sponsor = dbEmployment.Sponsor;
                emdEmployment.Sponsor_Guid = dbEmployment.Sponsor_Guid;
                emdEmployment.FirstWorkDay = dbEmployment.FirstWorkDay;
                emdEmployment.Status = dbEmployment.Status;
                emdEmployment.ActiveFrom = dbEmployment.ActiveFrom;
                emdEmployment.ActiveTo = dbEmployment.ActiveTo;
                emdEmployment.Guid_ModifiedBy = dbEmployment.Guid_ModifiedBy;
                emdEmployment.ModifyComment = dbEmployment.ModifyComment;
                emdEmployment.LeaveFrom = dbEmployment.LeaveFrom;
                emdEmployment.LeaveTo = dbEmployment.LeaveTo;
            }
            else
                ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return emdObject;
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            Employment empl = dbObject as Employment;
            if (dbObject == null)
                return null;
            EMDEmployment emdEmployment = new EMDEmployment(empl.Guid, empl.Created, empl.Modified);
            if (dbObject is Kapsch.IS.EDP.Core.DB.Employment && emdEmployment != null)
            {
                Kapsch.IS.EDP.Core.DB.Employment dbEmployment = dbObject as Kapsch.IS.EDP.Core.DB.Employment;
                emdEmployment.Guid = dbEmployment.Guid;
                emdEmployment.HistoryGuid = dbEmployment.HistoryGuid;
                emdEmployment.P_Guid = dbEmployment.P_Guid;
                emdEmployment.ET_Guid = dbEmployment.ET_Guid;
                emdEmployment.DGT_Guid = dbEmployment.DGT_Guid;
                emdEmployment.ENLO_Guid = dbEmployment.ENLO_Guid;
                emdEmployment.EP_ID = dbEmployment.EP_ID;
                emdEmployment.P_ID = dbEmployment.P_ID;
                emdEmployment.ET_ID = dbEmployment.ET_ID;
                emdEmployment.Entry = dbEmployment.Entry;
                emdEmployment.Exit = dbEmployment.Exit;
                emdEmployment.LastDay = dbEmployment.LastDay;
                emdEmployment.PersNr = dbEmployment.PersNr;
                emdEmployment.dpwKey = dbEmployment.dpwKey;
                emdEmployment.ValidFrom = dbEmployment.ValidFrom;
                emdEmployment.ValidTo = dbEmployment.ValidTo;
                emdEmployment.Created = dbEmployment.Created;
                emdEmployment.Modified = dbEmployment.Modified;
                emdEmployment.Exit_Report = dbEmployment.Exit_Report;
                emdEmployment.DGT_ID = dbEmployment.DGT_ID;
                emdEmployment.Sponsor = dbEmployment.Sponsor;
                emdEmployment.Sponsor_Guid = dbEmployment.Sponsor_Guid;
                emdEmployment.FirstWorkDay = dbEmployment.FirstWorkDay;
                emdEmployment.Status = dbEmployment.Status;
                emdEmployment.ActiveFrom = dbEmployment.ActiveFrom;
                emdEmployment.ActiveTo = dbEmployment.ActiveTo;
                emdEmployment.Guid_ModifiedBy = dbEmployment.Guid_ModifiedBy;
                emdEmployment.ModifyComment = dbEmployment.ModifyComment;
                emdEmployment.LeaveFrom = dbEmployment.LeaveFrom;
                emdEmployment.LeaveTo = dbEmployment.LeaveTo;
            }
            else
                ReflectionHelper.CopyProperties(ref dbObject, ref emdEmployment);
            return (IEMDObject<T>)emdEmployment;
        }

        /// <summary>
        /// An Employment is never Deleted with ActiveTo = DateTime.Now
        /// In case of Deletion only the Status is set to Removed and the lastDay and ExitDate is set to DateTime.Now
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="emdObject"></param>
        /// <param name="historize"></param>
        /// <param name="deletebase">Calls the base logic to set ActiveTo to DateTime.Now</param>
        /// <returns></returns>
        //public override IEMDObject<T> DeleteObject<T>(IEMDObject<T> emdObject, bool historize = true, bool deletebase = false)
        //{
        //    if (deletebase)
        //    {
        //        return base.DeleteObject(emdObject, historize);
        //    }

        //    return DeleteObject(emdObject, DateTime.Now, DateTime.Now, historize);
        //}

        /// <summary>
        /// An Employment is never Deleted with ActiveTo = DateTime.Now
        /// In case of Deletion only the Status is set to Removed and the lastDay and ExitDate is set to DateTime.Now
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="emdObject"></param>
        /// <param name="lastDay"></param>
        /// <param name="exitDate"></param>
        /// <param name="historize"></param>
        /// <returns></returns>
        [Obsolete("Use the method in EmploymentManager")]
        private IEMDObject<T> DeleteObject<T>(IEMDObject<T> emdObject, DateTime lastDay, DateTime exitDate, bool historize = true)
        {
            EMDEmployment employment = emdObject as EMDEmployment;
            if (employment != null)
            {
                DateTime now = DateTime.Now;
                employment.Status = ProcessStatus.STATUSITEM_REMOVED;
                employment.SetExitAndLastDay(now, null);

                this.UpdateObject(employment, historize: historize);
            }

            return employment as IEMDObject<T>;
        }

        public List<IEMDObject<EMDEmployment>> GetEmploymentsForPerson(string pers_guid)
        {
            List<IEMDObject<EMDEmployment>> employments = GetObjects<EMDEmployment, Employment>("P_Guid = \"" + pers_guid + "\"");
            return employments;
        }

        /// <summary>
        /// counts the amount of active employments for a person
        /// throws BaseException
        /// </summary>
        /// <param name="persGuid"></param>
        /// <returns>amount of employments</returns>
        public int GetEmploymentCountForPerson(string persGuid)
        {
            DateTime now = DateTime.Now;
            int amount;
            try
            {
                var dbCtx = this.transaction.dbContext;
                amount = (from e in dbCtx.Employment
                          where e.P_Guid == persGuid &&
                                e.ActiveFrom <= now && e.ValidFrom <= now && e.ActiveTo >= now && e.ValidTo >= now
                          select e).Count();
            }
            catch (Exception ex)
            {
                string msg = "cannot query amount of employments for person : " + persGuid;

                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg, ex);
            }
            return amount;
        }

        public EMDEmployment GetMainEmploymentForPerson(string pers_guid)
        {
            List<IEMDObject<EMDEmployment>> employments = GetObjects<EMDEmployment, Employment>("P_Guid = \"" + pers_guid + "\"");

            ObjectFlagHandler flagHandler = new ObjectFlagHandler();

            EMDEmployment mainEmployment = new EMDEmployment();
            int mainEmploymentCounter = 0;
            if (employments.Count == 0)
            {

            }
            foreach (EMDEmployment empl in employments)
            {
                if (flagHandler.HasFlagByGuid(empl.Guid, EnumObjectFlagType.MainEmployment))
                {
                    mainEmploymentCounter += 1;
                    mainEmployment = empl;
                }
            }
            if (mainEmploymentCounter == 0)
            {

                return null;
            }
            else if (mainEmploymentCounter == 1)
            {
                return mainEmployment;
            }
            else
            {

                return mainEmployment;
            }
        }

        public int GetNextFreeEP_ID()
        {
            bool puffer = this.Historical;

            this.Historical = true;
            IQueryable<int> query = (from item in transaction.dbContext.Employment where item.EP_ID < 9999999 orderby item.EP_ID descending select item.EP_ID);
            //Some performance improvement
            IQueryable<int> newQuery = query.Take(1);

            List<int> result = newQuery.ToList();

            //set back to the stored value
            this.Historical = puffer;
            //return the + 1 added new Int.
            return result.Single() + 1;
        }

        [Obsolete]
        /// <summary>
        /// @@ func to get comma separated list of EmploymentIDs for approvers
        /// </summary>
        /// <param name="approverCode"></param>
        /// <param name="effectedPersonEmployment"></param>
        /// <returns></returns>
        public string GetTaskApproversForEffectedEmployment(string effectedPersonEmployment, string approverCode)
        {
            List<string> apprCodes = new List<string>();
            apprCodes.Add(approverCode);
            var result = new TaskItemManager().FindTaskApproverForEffectedPerson(apprCodes, effectedPersonEmployment);
            if (result != null && result.Count > 0)
            {
                string emplIds = "";
                // take persGuid and get full name
                foreach (var r in result)
                {
                    string emplGuid = r.Item1;
                    emplIds += emplGuid + ", ";
                }
                return emplIds;
            }
            else
                return "";
        }

        [Obsolete]
        /// <summary>
        /// @@ func to get full name for approver
        /// </summary>
        /// <param name="approverCode"></param>
        /// <param name="effectedPersonEmployment"></param>
        /// <returns></returns>
        public string GetNameTaskApproverForEffectedPerson(string effectedPersonEmployment, string approverCode)
        {
            List<string> apprCodes = new List<string>();
            List<Tuple<string, string>> result = null;
            apprCodes.Add(approverCode);
            try
            {
                result = new TaskItemManager().FindTaskApproverForEffectedPerson(apprCodes, effectedPersonEmployment);
            }
            catch (BaseException bEx)
            {
            }
            if (result != null && result.Count > 0)
            {
                string fullName = "";
                // take persGuid and get full name
                foreach (var r in result)
                {
                    string persGuid = r.Item2;
                    EMDPerson pers = (EMDPerson)new PersonHandler().GetObject<EMDPerson>(persGuid);
                    fullName = fullName + pers.FirstName + " " + pers.FamilyName + ", ";
                }
                return fullName;
            }
            else
                return "";
        }

        [Obsolete]
        /// <summary>
        /// @@ function
        /// find account (cost center) guid for given employment
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <param name="approverCode"></param>
        /// <returns>account guid</returns>
        public string GetAccountGuidForEmployment(string effectedPersonEmployment, string emplGuid)
        {
            try
            {
                EmploymentAccountHandler emacH = new EmploymentAccountHandler();

                if (string.IsNullOrWhiteSpace(emplGuid))
                    emplGuid = effectedPersonEmployment;

                var result = emacH.GetObjects<EMDEmploymentAccount, EmploymentAccount>("EP_Guid = \"" + emplGuid + "\"").ToList();
                if (result != null && result.Count > 0)
                {
                    // take first one
                    EMDEmploymentAccount emac = (EMDEmploymentAccount)result[0];
                    AccountHandler accoH = new AccountHandler();
                    var acco = (EMDAccount)accoH.GetObject<EMDAccount>(emac.AC_Guid);
                    if (acco != null)
                    {
                        return acco.Guid;
                    }
                    else
                    {
                        string errMsg = "cannot find ACCO for Employment : " + emplGuid;

                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errMsg);
                    }
                }
                else
                {
                    string errMsg = "cannot find EMAC for Employment : " + emplGuid;

                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errMsg);
                }
            }
            catch(TargetInvocationException tiEx)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, tiEx.InnerException);
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, ex);
            }
        }

        [Obsolete]
        /// <summary>
        /// @@ function
        /// find orgunit guid for given employment and roleID
        /// e.g. what is wolfgang stagl's orgunit he is in role person for the employment x
        /// </summary>
        /// <param name="effectedPersonEmployment"></param>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public string GetOrgunitGuidforEmployment(string effectedPersonEmployment, string roleID)
        {
            /*select u.* from Orgunitrole as o
                join OrgUnit as u on  o.O_Guid = u.Guid
                where o.ActiveTo > getdate() 
                and o.EP_Guid = 'EMPL_03dc3347164049178880975070451bec'
                and o.R_ID = 10100
                order by o.R_Guid
                */
            OrgUnitHandler orguH = new OrgUnitHandler();
            OrgUnitRoleHandler orroH = new OrgUnitRoleHandler();
            var result = orroH.GetObjects<EMDOrgUnitRole, OrgUnitRole>("EP_Guid = \"" + effectedPersonEmployment + "\" and R_ID = " + roleID);
            if (result != null && result.Count == 1)
            {
                EMDOrgUnitRole orro = (EMDOrgUnitRole)result[0];
                EMDOrgUnit orgu = (EMDOrgUnit)orguH.GetObject<EMDOrgUnit>(orro.O_Guid);
                if (orgu != null)
                {
                    return orgu.Guid;
                }
                else
                {
                    string errMsg = "found no orgunit for employment:" + effectedPersonEmployment + " with RoleID:" + roleID;

                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errMsg);
                }
            }
            else
            {
                string errMsg = "found no or more than 1 orgunit role for employment:" + effectedPersonEmployment + " with RoleID:" + roleID;

                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errMsg);
            }
        }

        [Obsolete]
        /// <summary>
        /// @@ function 
        /// uses vorgesetzen search to find an empl guid
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public string SearchResponsibleRoleForEmployment(string emplGuid, string roleID)
        {
            OrgUnitManager omgr = new OrgUnitManager();
            List<string> result = omgr.SearchOrgUnitRoleForEmployment(Int32.Parse(roleID), emplGuid);
            if (result != null && result.Count > 0)
                return result[0].ToString();
            else
                return "";
        }

        [Obsolete]
        /// <summary>
        /// @@ function
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <param name="contactType"></param>
        /// <returns></returns>
        public string SearchContactItemForEmployment(string emplGuid, string contactType)
        {
            ContactHandler cH = new ContactHandler();
            EMDContact c = cH.GetContactByContactType(emplGuid, contactType);
            if (c != null)
                return c.Guid;
            else
                return "";

        }

        [Obsolete]
        /// <summary>
        /// @@ function
        /// get contact via old contact type id
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <param name="contactTypeID"></param>
        /// <returns></returns>
        public string SearchContactItemByCTIDForEmployment(string emplGuid, string contactTypeID)
        {
            ContactHandler cH = new ContactHandler();
            EMDContact c = cH.GetContactByContactType(emplGuid, int.Parse(contactTypeID));
            if (c != null)
                return c.Guid;
            else
                return "";

        }

        [Obsolete]
        /// <summary>
        /// @@ function
        /// checks if employment can be offboarded.
        /// </summary>
        /// <param name="emplGuid">employment to be checked</param>
        /// <param name="parameterNotNeeded">entity query needs this parameter to exist. it is not used here</param>
        /// <returns>returns the value "allowed" if it is ok to offboard or a string message describing the reason why it is not allowed.</returns>
        public string IsOffboardingAllowedForEmployment(string emplGuid, string parameterNotNeeded)
        {
            string result = "";

            EmploymentManager emplMgr = new EmploymentManager();
            HashSet<EnumOffboardingDeclined> checkResult = emplMgr.CheckIfOffboardingAllowed(emplGuid);
            if (checkResult.Count > 0)
            {
                foreach (EnumOffboardingDeclined e in checkResult)
                {
                    result += e.ToString() + " AND ";
                }
            }
            else
                result = "allowed";

            return result;
        }

        internal override void MapDataToDBObject<T>(ref object dbObject, ref IEMDObject<T> emdObject)
        {
            if (dbObject is Kapsch.IS.EDP.Core.DB.Employment && emdObject is Kapsch.IS.EDP.Core.Entities.EMDEmployment)
            {
                Kapsch.IS.EDP.Core.DB.Employment dbEmployment = dbObject as Kapsch.IS.EDP.Core.DB.Employment;
                Kapsch.IS.EDP.Core.Entities.EMDEmployment emdEmployment = emdObject as Kapsch.IS.EDP.Core.Entities.EMDEmployment;

                dbEmployment.Guid = emdEmployment.Guid;
                dbEmployment.HistoryGuid = emdEmployment.HistoryGuid;
                dbEmployment.P_Guid = emdEmployment.P_Guid;
                dbEmployment.ET_Guid = emdEmployment.ET_Guid;
                dbEmployment.DGT_Guid = emdEmployment.DGT_Guid;
                dbEmployment.ENLO_Guid = emdEmployment.ENLO_Guid;
                dbEmployment.EP_ID = emdEmployment.EP_ID;
                dbEmployment.P_ID = emdEmployment.P_ID;
                dbEmployment.ET_ID = emdEmployment.ET_ID;
                dbEmployment.Entry = emdEmployment.Entry;
                dbEmployment.Exit = emdEmployment.Exit;
                dbEmployment.LastDay = emdEmployment.LastDay;
                dbEmployment.PersNr = emdEmployment.PersNr;
                dbEmployment.dpwKey = emdEmployment.dpwKey;
                dbEmployment.ValidFrom = emdEmployment.ValidFrom;
                dbEmployment.ValidTo = emdEmployment.ValidTo;
                dbEmployment.Created = emdEmployment.Created;
                dbEmployment.Modified = emdEmployment.Modified;
                dbEmployment.Exit_Report = emdEmployment.Exit_Report;
                dbEmployment.DGT_ID = emdEmployment.DGT_ID;
                dbEmployment.Sponsor = emdEmployment.Sponsor;
                dbEmployment.Sponsor_Guid = emdEmployment.Sponsor_Guid;
                dbEmployment.FirstWorkDay = emdEmployment.FirstWorkDay;
                dbEmployment.Status = emdEmployment.Status;
                dbEmployment.ActiveFrom = emdEmployment.ActiveFrom;
                dbEmployment.ActiveTo = emdEmployment.ActiveTo;
                dbEmployment.Guid_ModifiedBy = emdEmployment.Guid_ModifiedBy;
                dbEmployment.ModifyComment = emdEmployment.ModifyComment;
                dbEmployment.LeaveFrom = emdEmployment.LeaveFrom;
                dbEmployment.LeaveTo = emdEmployment.LeaveTo;

            }
            else
                base.MapDataToDBObject<T>(ref dbObject, ref emdObject);
        }

    }
}

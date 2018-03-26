using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class OrgUnitRoleHandler : EMDObjectHandler
    {
        public const int ROLE_ID_PERSON = 10100;
        public const int ROLE_ID_DISPATCHER = 10200;
        public const int ROLE_ID_TEAMLEADER = 10400;
        public const int ROLE_ID_LINEMANAGER = 10500;
        public const int ROLE_ID_AREAMANAGER = 10600;

        public OrgUnitRoleHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public OrgUnitRoleHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public OrgUnitRoleHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public OrgUnitRoleHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new OrgUnitRole().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            OrgUnitRole ouro = (OrgUnitRole)dbObject;
            EMDOrgUnitRole emdObject = new EMDOrgUnitRole(ouro.Guid, ouro.Created, ouro.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        public IEMDObject<EMDOrgUnitRole> GetOrgUnitRole(string Employment_Guid, string Role_Guid)
        {
            List<IEMDObject<EMDOrgUnitRole>> orgUnitRoles = GetObjects<EMDOrgUnitRole, OrgUnitRole>("EP_Guid = \"" + Employment_Guid + "\" and R_Guid=\"" + Role_Guid + "\"");

            if (orgUnitRoles.Count == 0)
                return null;
            else if (orgUnitRoles.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Consistencyproblem: more than one Orgunit-Role found for EP_Guid {0} and R_ID {1}", Employment_Guid, Role_Guid));
            }
            else
            {
                return orgUnitRoles.First();
            }
        }

        public IEMDObject<EMDOrgUnitRole> GetOrgUnitRole(string Employment_Guid, int Role_Id)
        {
            List<IEMDObject<EMDOrgUnitRole>> orgUnitRoles = GetObjects<EMDOrgUnitRole, OrgUnitRole>("EP_Guid = \"" + Employment_Guid + "\" and R_ID=" + Role_Id);

            if (orgUnitRoles.Count == 0)
                return null;
            else if (orgUnitRoles.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Consistencyproblem: more than one Orgunit-Role found for EP_Guid {0} and Role_Id {1}", Employment_Guid, Role_Id));
            }
            else
            {
                return orgUnitRoles.First();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <param name="orguGuid">Orgunit Guid</param>
        /// <param name="roleID">roleID 10100 for 'Person' is default </param>
        public EMDOrgUnitRole AddOrgUnitRoleToEmployment(string emplGuid, string orguGuid, int roleID = 10100, string userGuid = null, string modifyComment = null)
        {
            // convert (old)roleID to (new)R_Guid
            EMDOrgUnitRole orro = null;

            CoreTransaction cta = this.transaction;

            bool isNewTransaction = false;

            if (cta == null)
            {
                cta = new CoreTransaction();
                isNewTransaction = true;
                cta.Begin();
            }

            try
            {
                RoleHandler roleH = new RoleHandler(cta, userGuid, modifyComment);
                EMDRole r = roleH.GetRoleById(roleID);
                if (r != null)
                {

                    orro = new EMDOrgUnitRole();
                    orro.EP_Guid = emplGuid;
                    orro.O_Guid = orguGuid;
                    orro.R_Guid = r.Guid;
                    // keep old IDs
                    orro.R_ID = r.R_ID;
                    orro.O_ID = ((EMDOrgUnit)(new OrgUnitHandler(cta).GetObject<EMDOrgUnit>(orguGuid))).O_ID;
                    orro.EP_ID = ((EMDEmployment)(new EmploymentHandler(cta).GetObject<EMDEmployment>(emplGuid))).EP_ID;
                    orro.OR_ID = this.getNextFreeOrgUnitRoleID();
                    this.CreateObject(orro);
                    if (isNewTransaction)
                    {
                        cta.Commit();
                    }
                    return orro;
                }
                else
                {
                    string msg = string.Format("No OrgUnitRole created. Cannot find a EMDRole for RoldID = {0}. Effected Emplyoment = {1}", roleID.ToString(), emplGuid);

                    throw new BaseException(
                                ErrorCodeHandler.E_EDP_BUSINESS_LOGIK,
                                msg);
                }
            }
            catch (Exception ex)
            {
                if (isNewTransaction)
                {
                    cta.Rollback();
                }
                string msg = string.Format("Error creating OrgUnitRole. RoldID was {0}. Effected Emplyoment was {1}", roleID.ToString(), emplGuid);
                
                throw new BaseException(
                            ErrorCodeHandler.E_EDP_BUSINESS_LOGIK,
                            msg);
            }
        }

        private int getNextFreeOrgUnitRoleID()
        {
            bool puffer = this.Historical;

            this.Historical = true;
            IQueryable<int> query = (from item in transaction.dbContext.OrgUnitRole orderby item.OR_ID descending select item.OR_ID);
            IQueryable<int> newQuery = query.Take(1);
            List<int> result = newQuery.ToList();
            this.Historical = puffer;
            return result.Single() + 1;
        }
    }
}
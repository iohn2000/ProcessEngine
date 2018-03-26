using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Logic.Interface;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class RoleManager
        : BaseManager
        , IRoleManager
    {
        #region Constructors

        public RoleManager()
            : base()
        {
        }

        public RoleManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public RoleManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public RoleManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDRole Get(string guid)
        {
            RoleHandler handler = new RoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            return (EMDRole)handler.GetObject<EMDRole>(guid);
        }

        public EMDRole GetRoleById(int Role_Id)
        {
            RoleHandler handler = new RoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDRole>> roles = handler.GetObjects<EMDRole, Role>("R_ID = " + Role_Id);

            if (roles.Count == 0)
                return null;
            else if (roles.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Consistencyproblem: more than one role found for R_ID {0}", Role_Id));
            }
            else
            {
                return (EMDRole)roles.First();
            }
        }

        /// <summary>
        /// Returns the next highest number for a roleId-number
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public int GetNextRoleIdNumber(int roleId)
        {
            int nextRoleId = roleId;
            if (nextRoleId == 0)
            {
                nextRoleId++;
            }

            while (nextRoleId < 10000)
            {
                nextRoleId = nextRoleId * 10;
            }

            RoleHandler roleHandler = new RoleHandler();
            List<EMDRole> roles = roleHandler.GetObjects<EMDRole, Role>(string.Format("R_ID >= {0}", roleId)).Cast<EMDRole>().OrderBy(a => a.R_ID).ToList();

            if (roles != null && roles.Count > 0)
            {
                while (true)
                {
                    EMDRole foundId = roles.FindLast(a => a.R_ID == nextRoleId);
                    if (foundId == null)
                    {
                        break;
                    }

                    nextRoleId++;

                }
            }

            return nextRoleId;

        }

        /// <summary>
        /// this function exits because creating a role from gui
        /// doesnt contain the root guid. this function fills it in
        /// throws exception
        /// </summary>
        /// <param name="role">an EMDRole object without Guid_Root</param>
        public EMDRole Create(EMDRole role)
        {
            EMDRole returnValue = null;
            try
            {


                if (!IsRoleIdAvailable(role.R_ID))
                {
                    throw new Exception(string.Format("The Role ID: {0} is not available!", role.R_ID));
                }

                RoleHandler roleHandler = new RoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

                if (role.Guid_Parent != null)
                {
                    var dbCtx = roleHandler.Transaction.dbContext;
                    var parentRole = (from r in dbCtx.Role where r.Guid == role.Guid_Parent select r).Single();

                    role.Guid_Root = parentRole.Guid_Root;

                    // old IDs
                    role.ID_Parent = parentRole.R_ID;
                    role.ID_Root = parentRole.ID_Root;
                }
                returnValue = (EMDRole)roleHandler.CreateObject(role);
                if (role.Guid_Parent == null)
                {
                    returnValue.Guid_Root = returnValue.Guid;
                    returnValue.Guid_Parent = returnValue.Guid;
                    returnValue.ID_Parent = returnValue.R_ID;
                    returnValue.ID_Root = returnValue.R_ID;

                    returnValue = (EMDRole)roleHandler.UpdateObject(returnValue, historize: false);
                }
            }
            catch (Exception ex)
            {
                string msg = "error trying to set role guid_root and old IDs";
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg, ex);
            }

            return returnValue;
        }


        /// <summary>
        /// this function exits because creating a role from gui
        /// doesnt contain the root guid. this function fills it in
        /// throws exception
        /// </summary>
        /// <param name="role">an EMDRole object without Guid_Root</param>
        public EMDRole Update(EMDRole role)
        {
            EMDRole returnValue = null;
            try
            {
                RoleHandler roleHandler = new RoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

                if (!IsRoleIdAvailable(role.R_ID, role))
                {
                    throw new Exception(string.Format("The Role ID: {0} is not available!", role.R_ID));
                }

                if (role.Guid_Parent != null)
                {
                    var dbCtx = roleHandler.Transaction.dbContext;
                    var parentRole = (from r in dbCtx.Role where r.Guid == role.Guid_Parent select r).Single();

                    role.Guid_Root = parentRole.Guid_Root;

                    // old IDs
                    role.ID_Parent = parentRole.R_ID;
                    role.ID_Root = parentRole.ID_Root;
                }
                returnValue = (EMDRole)roleHandler.UpdateObject(role);
                if (role.Guid_Parent == null)
                {
                    returnValue.Guid_Root = returnValue.Guid;
                    returnValue.Guid_Parent = returnValue.Guid;
                    returnValue.ID_Parent = returnValue.R_ID;
                    returnValue.ID_Root = returnValue.R_ID;

                    returnValue = (EMDRole)roleHandler.UpdateObject(returnValue, historize: false);
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, ex.Message, ex);
            }

            return returnValue;
        }

        /// <summary>
        /// Checks if a RoleId is available
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool IsRoleIdAvailable(int roleId, EMDRole existingEmdRole = null)
        {
            bool isAvailable = true;

            RoleHandler roleHandler = new RoleHandler() { };
            List<EMDRole> roles = roleHandler.GetObjects<EMDRole, Role>(string.Format("R_ID == {0}", roleId)).Cast<EMDRole>().OrderBy(a => a.R_ID).ToList();

            if (existingEmdRole != null)
            {
                EMDRole foundRole = roles.FindLast(a => a.Guid != existingEmdRole.Guid);
                if (foundRole != null)
                {
                    isAvailable = false;
                }
            }
            else if (roles != null && roles.Count > 0)
            {
                isAvailable = false;
            }

            return isAvailable;
        }

        public EMDRole Delete(string guid)
        {
            RoleHandler handler = new RoleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDRole emdRole = Get(guid);
            if (emdRole != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDRole)handler.DeleteObject<EMDRole>(emdRole);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The Role with guid: {0} was not found.", guid));
            }
        }
    }
}

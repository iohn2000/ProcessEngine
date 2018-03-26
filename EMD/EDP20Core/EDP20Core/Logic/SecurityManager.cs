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
    public class SecurityManager : BaseManager
    {
        #region Constructors

        public SecurityManager()
            : base()
        {
        }

        public SecurityManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public SecurityManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public SecurityManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDSecurityAction CreateSecurityAction(string permission, string role_guid, string guidModifiedBy = null, string modifyComment = null)
        {
            SecurityActionManager secActManager = new SecurityActionManager(this.Transaction, guidModifiedBy, modifyComment);
            EMDSecurityAction secAct = new EMDSecurityAction();
            secAct.Action = permission;
            secAct.ROLE_Guid = role_guid;
            secAct.Guid_ModifiedBy = guidModifiedBy;
            secAct.ModifyComment = modifyComment;
            return secActManager.Create(secAct);
        }

        public EMDRole CreateRole(string name, int R_ID, bool isSecurity, string guidParent = "", string guidRoot = "", string guidModifiedBy = null, string modifyComment = null)
        {
            RoleHandler roleHandler = new RoleHandler(this.Transaction);
            EMDRole role = new EMDRole();
            role.Name = name;
            role.R_ID = R_ID;
            role.IsSecurity = isSecurity;
            role.Guid_ModifiedBy = guidModifiedBy; 
            role.ModifyComment = modifyComment;
            if (guidParent == String.Empty && guidRoot == String.Empty)
            {
                role = (EMDRole)roleHandler.CreateObject<EMDRole>(role);
                role.Guid_Parent = role.Guid;
                role.Guid_Root = role.Guid;
                role = (EMDRole)roleHandler.UpdateObject<EMDRole>(role, historize:false);
            }
            else
            {
                role.Guid_Parent = guidParent;
                role.Guid_Root = guidRoot;
                role = (EMDRole)roleHandler.CreateObject<EMDRole>(role);
            }
            return role;
        }

        public EMDOrgUnitRole AddPersonToSecurityRole(string userId, string orgu_guid, string role_guid)
        {
            EMDOrgUnitRole newOrgUnitRole = null;
            PersonManager persManager = new PersonManager(this.Transaction);
            EMDPerson pers = (EMDPerson)persManager.GetPersonByUserId(userId);
            if (pers != null)
            {
                EmploymentHandler emplHandler = new EmploymentHandler(this.Transaction);
                EMDEmployment employment = emplHandler.GetMainEmploymentForPerson(pers.Guid);

                if (employment != null)
                {
                    OrgUnitRoleHandler orgUnitRoleHandler = new OrgUnitRoleHandler(this.Transaction);
                    EMDOrgUnitRole our = new EMDOrgUnitRole();
                    our.O_Guid = orgu_guid;
                    our.R_Guid = role_guid;
                    our.EP_Guid = employment.Guid;
                    newOrgUnitRole = (EMDOrgUnitRole)orgUnitRoleHandler.CreateObject<EMDOrgUnitRole>(our);
                }
            }
            return newOrgUnitRole;
        }

        public EMDOrgUnit CreateSecurityOrgUnit(string name, string ente_guid, string guidParent = "", string guidRoot = "")
        {
            OrgUnitHandler orgUnitHandler = new OrgUnitHandler(this.Transaction);
            EMDOrgUnit newOrgUnit = new EMDOrgUnit();
            newOrgUnit.Name = name;
            newOrgUnit.E_Guid = ente_guid;
            newOrgUnit.IsSecurity = true;
            if (guidParent == String.Empty && guidRoot == String.Empty)
            {
                newOrgUnit = (EMDOrgUnit)orgUnitHandler.CreateObject<EMDOrgUnit>(newOrgUnit);
                newOrgUnit.Guid_Root = newOrgUnit.Guid;
                newOrgUnit.Guid_Parent = newOrgUnit.Guid;
                newOrgUnit = (EMDOrgUnit)orgUnitHandler.UpdateObject<EMDOrgUnit>(newOrgUnit);
            }
            else
            {
                newOrgUnit.Guid_Root = guidRoot;
                newOrgUnit.Guid_Parent = guidParent;
                newOrgUnit = (EMDOrgUnit)orgUnitHandler.CreateObject<EMDOrgUnit>(newOrgUnit);
            }

            return newOrgUnit;
        }
    }
}

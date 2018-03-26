using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    /// <summary>
    /// Model which holds a Group Entity which holds corresponding Employments (GroupMembers) and Accounts (AccountGroup)
    /// </summary>
    public class GroupAccountManagementModel : BaseModel
    {
    


        public override String CanManagePermissionString { get { return SecurityPermission.CostCenterManager_View_Manage; } }

        public override String CanViewPermissionString { get { return SecurityPermission.CostCenterManager_View; } }

        /// <summary>
        /// Guid for Group
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Name for Group
        /// </summary>
        [Required, Display(Name = "Group Name")]
        public string Name { get; set; }

        /// <summary>
        /// Dependend Enterprise for Group
        /// </summary>
        [Required, Display(Name = "Enterprise")]
        public string E_Guid { get; set; }

        /// <summary>
        /// Dependend EnterpriseName for Group
        /// </summary>
        public string EnterpriseName { get; set; }

        #region RelationTables

        /// <summary>
        /// Relation to AccountGroup
        /// </summary>
        public string Acgr_Guid { get; set; }


        /// <summary>
        /// Relation to Account
        /// </summary>
        [Required, Display(Name = "Costcenter")]
        public string Acco_Guid { get; set; }


        public SelectionViewModel AccountSelection { get; set; }

        /// <summary>
        /// Relation to Account (Name)
        /// </summary>
        [Display(Name = "Cost center")]
        public string AccountName { get; set; }

        /// <summary>
        /// Available employments for the Enterprise
        /// </summary>
        public IList<TextValueModel> AvailableEmployments { get; set; }

        /// <summary>
        /// Relation list to Employments (group member empl_guids)
        /// </summary>
        public IList<TextValueModel> AssignedEmployments { get; set; }

        public string[] AssignedEmploymentStrings
        {
            get
            {
                if (AssignedEmployments != null)
                {
                    string[] assignedList = new string[this.AssignedEmployments.Count];

                    if (AssignedEmployments != null)
                    {
                        for (int i = 0; i < this.AssignedEmployments.Count; i++)
                        {
                            assignedList[i] = this.AssignedEmployments[i].Value;
                        }
                    }

                    return assignedList;
                }
                else
                {
                    return new string[0];
                }
            }
        }

        public List<GroupMemberModel> GroupMembers { get; set; }
        public SelectionViewModel EnterpriseSelection { get; internal set; }

        #endregion

        public GroupAccountManagementModel()
        {
            this.AssignedEmployments = new List<TextValueModel>();
            this.GroupMembers = new List<GroupMemberModel>();
            this.AccountSelection = new SelectionViewModel();
            this.EnterpriseSelection = new SelectionViewModel();
        }


        public static GroupAccountManagementModel Map(EMDGroup emdGroup)
        {
            GroupAccountManagementModel accountGroupModel = new GroupAccountManagementModel();
            ReflectionHelper.CopyProperties<EMDGroup, GroupAccountManagementModel>(ref emdGroup, ref accountGroupModel);
            return accountGroupModel;
        }

        public static EMDGroup Map(GroupAccountManagementModel groupModel)
        {
            EMDGroup emdAccountGroup = new EMDGroup();
            ReflectionHelper.CopyProperties<GroupAccountManagementModel, EMDGroup>(ref groupModel, ref emdAccountGroup);
            return emdAccountGroup;
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }
    }
}
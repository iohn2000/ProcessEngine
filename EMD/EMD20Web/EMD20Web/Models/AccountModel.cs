using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class AccountModel : BaseModel
    {
        public string Guid { get; set; }
        public int AC_ID { get; set; }
        public int E_ID { get; set; }
        [Required(), Display(Name = "Enterprise")]
        public string E_Guid { get; set; }
        [Required(), Display(Name = "Cost center ID")]
        public string KstID { get; set; }
        [Required(), Display(Name = "Name")]
        public string Name { get; set; }
        public Nullable<int> MainOrgUnit { get; set; }
        public string Responsible { get; set; }
        public Nullable<int> Responsible_EP_ID { get; set; }
        public string ResponsibleName { get; set; }

        public SelectionViewModel ResponsibleSelection { get; set; }


        public string KstIDAndName { get; set; }


        [Obsolete]
        public List<TextValueModel> EnterpriseList = new List<TextValueModel>();


        public IList<TextValueModel> AvailableGroups { get; set; }

        public IList<TextValueModel> ConfiguredGroups { get; set; }


        public override String CanManagePermissionString { get { return SecurityPermission.CostCenterManager_View_Manage; } }

        public override String CanViewPermissionString { get { return SecurityPermission.CostCenterManager_View; } }

        [Display(Name="Enterprise")]
        public string EnterpriseDisplayName { get; set; }

        [Display(Name = "Assistance")]
        public string AssistanceGroups { get; set; }
        public SelectionViewModel EnterpriseSelection { get; internal set; }

        public AccountModel()
        {
            ResponsibleSelection = new SelectionViewModel();
            AvailableGroups = new List<TextValueModel>();
            ConfiguredGroups = new List<TextValueModel>();
        }

        public static List<AccountModel> Map(List<EMDAccount> accounts)
        {
            List<AccountModel> accountModels = new List<AccountModel>();

            foreach (EMDAccount emdAccount in accounts)
            {
                accountModels.Add(Initialize(emdAccount));
            }

            return accountModels;
        }
        
        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public static AccountModel Initialize(Account acc)
        {
            AccountModel accemo = new AccountModel();
            ReflectionHelper.CopyProperties(ref acc, ref accemo);
            return accemo;
        }

        public static AccountModel Initialize(EMDAccount acc, EMDEnterprise ente)
        {
            AccountModel accemo = new AccountModel();
            ReflectionHelper.CopyProperties(ref acc, ref accemo);
            if (ente != null )
                accemo.EnterpriseDisplayName = ente.NameShort;

            return accemo;
        }

        public static AccountModel Initialize(EMDAccount acc)
        {
            AccountModel accmo = new AccountModel();
            ReflectionHelper.CopyProperties(ref acc, ref accmo);
            return accmo;
        }

        public static EMDAccount Update(EMDAccount emdAccount, AccountModel accountModel)
        {
            emdAccount.AC_ID = accountModel.AC_ID;
            emdAccount.E_Guid = accountModel.E_Guid;
            emdAccount.E_ID = accountModel.E_ID;
            emdAccount.KstID = accountModel.KstID;
            emdAccount.MainOrgUnit = accountModel.MainOrgUnit;
            emdAccount.Name = accountModel.Name;
            emdAccount.Responsible = accountModel.Responsible;
            emdAccount.Responsible_EP_ID = accountModel.Responsible_EP_ID;

            return emdAccount;
        }

        public static EMDAccount Map(AccountModel accountModel)
        {
            return new EMDAccount()
            {
                AC_ID = accountModel.AC_ID,
                E_Guid = accountModel.E_Guid,
                E_ID = accountModel.E_ID,
                KstID = accountModel.KstID,
                MainOrgUnit = accountModel.MainOrgUnit,
                Name = accountModel.Name,
                Responsible = accountModel.Responsible,
                Responsible_EP_ID = accountModel.Responsible_EP_ID


            };
        }

        public bool IsAllowedObject(string userId, string guid, bool isEdit = false)
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser(userId);
            return secUser.IsAllowedCostCenter(guid, isEdit);
        }

    }
}
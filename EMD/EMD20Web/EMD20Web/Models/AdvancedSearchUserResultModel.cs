using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class AdvancedSearchUserResultModel : UserModel
    {
        
        public string PERS_Guid { get; set; }

        
        public override String CanManagePermissionString { get {return SecurityPermission.NotDefined; } }
        public override String CanViewPermissionString { get { return SecurityPermission.AdvancedSearch_View_User_ViewDetail; } }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            this.CanManage = false;
        }

        public new static AdvancedSearchUserResultModel Map(User user)
        {
            AdvancedSearchUserResultModel advancedUserModel = new AdvancedSearchUserResultModel();

            ReflectionHelper.CopyProperties<User, AdvancedSearchUserResultModel>(ref user, ref advancedUserModel);

            EmploymentHandler handler = new EmploymentHandler();
            EMDEmployment employment = (EMDEmployment)handler.GetObject<EMDEmployment>(advancedUserModel.EMPL_Guid);

            if (employment != null)
            {
                advancedUserModel.PERS_Guid = employment.P_Guid;
            }

            //PersonManager personManager = new PersonManager();
            //EMDPerson emdPerson = personManager.GetPersonByEmployment(advancedUserModel.EMPL_Guid);
            //if (emdPerson != null)
            //{
            //    advancedUserModel.PERS_Guid = emdPerson.Guid;
            //}

            return advancedUserModel;
        }

        public new static List<AdvancedSearchUserResultModel> Map(List<User> users)
        {
            List<AdvancedSearchUserResultModel> advancedUserModels = new List<AdvancedSearchUserResultModel>();

            foreach (User user in users)
            {
                advancedUserModels.Add(Map(user));
            }

            return advancedUserModels;
        }
    }
}
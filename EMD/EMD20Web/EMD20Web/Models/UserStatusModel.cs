using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class UserStatusModel : BaseModel
    {

        public UserStatusModel()
        {

        }

        public UserStatusModel(EnumUserStatus status)
        {
            this.Status = status;
        }


        public string Name
        {
            get
            {
                return UserModel.GetStatusDisplayName(Status);
            }
        }

        public EnumUserStatus Status { get; set; }


        public static List<UserStatusModel> GetUserStatusModelList()
        {
            List<UserStatusModel> enumStatusModelList = new List<UserStatusModel>();
            List<EnumUserStatus> statusList = System.Enum.GetValues(typeof(EnumUserStatus)).Cast<EnumUserStatus>().ToList();

            foreach (EnumUserStatus userStatus in statusList)
            {
                enumStatusModelList.Add(new UserStatusModel(userStatus));
            }


            return enumStatusModelList;
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {

        }
    }
}
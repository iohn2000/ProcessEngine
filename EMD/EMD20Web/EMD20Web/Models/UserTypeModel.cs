using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class UserTypeModel : BaseModel
    {
        public UserTypeModel()
        {

        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {

        }

        public UserTypeModel(EnumUserType type)
        {
            this.Type = type;
        }


        public string Name
        {
            get
            {
                return UserModel.GetUserTypeDisplayName(this.Type);
            }
        }

        public EnumUserType Type { get; set; }


        public static List<UserTypeModel> GetUserTypeModelList(bool hideDeleted = false)
        {
            List<UserTypeModel> enumTypeModelList = new List<UserTypeModel>();
            List<EnumUserType> typeList = System.Enum.GetValues(typeof(EnumUserType)).Cast<EnumUserType>().ToList();

            foreach (EnumUserType userType in typeList)
            {
                if (!hideDeleted || (hideDeleted && !EnumUserTypeAttribute.GetIsDeleted(userType)))
                {
                    enumTypeModelList.Add(new UserTypeModel(userType));
                }
            }


            return enumTypeModelList;
        }
    }
}
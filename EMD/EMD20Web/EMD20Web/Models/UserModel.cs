using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class UserModel : BaseModel
    {
        public bool IsNew
        {
            get
            {
                return string.IsNullOrEmpty(Guid);
            }
        }

        [Required, Display(Name = "Username")]
        public new String Username { get; set; }

        public bool isEditable { get; set; }
        public string Guid { get; set; }

        public string EMPL_Guid { get; set; }

        public string USDO_Guid { get; set; }

        public bool HideDeletedUsertypes;

        [Display(Name = "Is Main")]
        public bool IsMainUser { get; set; }

        public UserModel()
        {

        }

        public UserModel(bool hideDeletedUsertypes)
        {
            this.HideDeletedUsertypes = hideDeletedUsertypes;
        }


        [Required(), Display(Name = "Domain")]
        public string USDO_UI
        {
            get
            {
                return USDO_Guid;
            }
            set
            {
                USDO_Guid = value;
            }
        }

        [Display(Name = "Domain")]
        public string UserDomainDisplayName
        {
            get
            {
                EMDUserDomain emdUserDomain = null;
                if (!string.IsNullOrEmpty(USDO_Guid))
                {
                    emdUserDomain = Manager.UserDomainManager.Get(USDO_Guid);
                }

                if (emdUserDomain != null)
                {
                    return emdUserDomain.Name;
                }

                return string.Empty;
            }
        }

        [Required(), Display(Name = "Usertyp")]
        public EnumUserType UserType { get; set; }

        [Display(Name = "Usertype")]
        public string UserTypeDisplayName
        {
            get
            {
                return GetUserTypeDisplayName(this.UserType);
            }
        }

        public static string GetUserTypeDisplayName(EnumUserType type)
        {

            string name = "No defined";


            switch (type)
            {
                case EnumUserType.ADUserLimitedAccount:
                    name = "Limited";
                    break;
                case EnumUserType.ADUserFullAccount:
                    name = "Full";
                    break;
                case EnumUserType.ADUserExternalSupplier:
                    name = "Supporter";
                    break;
                case EnumUserType.ADUserTest:
                    name = "Test User";
                    break;
                case EnumUserType.ADUserAdmin:
                    name = "Admin User";
                    break;
                case EnumUserType.ADUserSystem:
                    name = "System User";
                    break;
                default:
                    break;
            }

            return name;


        }

        /// <summary>
        /// Used only for UserInterface
        /// </summary>
        public int SelectedUserTypeIndex
        {
            get
            {
                int selectedIndex = -1;

                int i = 1;
                foreach (UserTypeModel userTypeModel in UserTypeModel.GetUserTypeModelList())
                {
                    if (userTypeModel.Type == this.UserType)
                    {
                        selectedIndex = i;
                        break;
                    }
                    i++;
                }

                return selectedIndex;
            }
        }

        [Required(), Display(Name = "Status")]
        public EnumUserStatus Status { get; set; }

        [Display(Name = "Status")]
        public string StatusDisplayName
        {
            get
            {
                return GetStatusDisplayName(Status);

            }
        }


        public static string GetStatusDisplayName(EnumUserStatus status)
        {

            string name = "No status found";


            switch (status)
            {
                case EnumUserStatus.Reserverd:
                    name = "Reserved";
                    break;
                case EnumUserStatus.InUse:
                    name = "In Use";
                    break;

            }

            return name;


        }


        /// <summary>
        /// Used only for UserInterface
        /// </summary>
        public int SelectedStatusIndex
        {
            get
            {
                int selectedIndex = -1;

                int i = 1;
                foreach (UserStatusModel userStatusModel in UserStatusModel.GetUserStatusModelList())
                {
                    if (userStatusModel.Status == this.Status)
                    {
                        selectedIndex = i;
                        break;
                    }
                    i++;
                }

                return selectedIndex;
            }
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public static UserModel Map(EMDUser emdUser, bool mapIsMainUser = false)
        {
            UserModel userModel = new UserModel();
            EMDUser tempEmdUser = emdUser;

            ReflectionHelper.CopyProperties<EMDUser, UserModel>(ref tempEmdUser, ref userModel);

            if (mapIsMainUser && userModel.UserType == EnumUserType.ADUserFullAccount)
            {
                EMDPerson person = Manager.PersonManager.GetPersonByUserId(userModel.Username);
                if (person != null && person.USER_GUID == userModel.Guid)
                {
                    userModel.IsMainUser = true;
                }
            }

            return userModel;
        }



        public static List<UserModel> Map(List<EMDUser> emdUsers, bool mapIsMainUser = false)
        {
            List<UserModel> userModels = new List<UserModel>();

            foreach (EMDUser emdUser in emdUsers)
            {
                UserModel model = Map(emdUser, mapIsMainUser);
                userModels.Add(model);
            }

            return userModels;
        }

        public static UserModel Map(User user)
        {
            UserModel userModel = new UserModel();
            User tempUser = user;

            ReflectionHelper.CopyProperties<User, UserModel>(ref tempUser, ref userModel);

            return userModel;
        }

        public static EMDUser Map(UserModel userModel)
        {
            EMDUser emdUser = new EMDUser()
            {
                EMPL_Guid = userModel.EMPL_Guid,
                USDO_Guid = userModel.USDO_Guid,
                UserType = (byte)userModel.UserType,
                Status = (byte)userModel.Status,
                Username = userModel.Username
            };

            return emdUser;
        }

        public static EMDUser Update(EMDUser emdUser, UserModel userModel)
        {
            emdUser.USDO_Guid = userModel.USDO_Guid;
            emdUser.UserType = (byte)userModel.UserType;
            emdUser.Status = (byte)userModel.Status;
            emdUser.Username = userModel.Username;

            return emdUser;
        }

        public static List<UserModel> Map(List<User> users)
        {
            List<UserModel> userModels = new List<UserModel>();

            foreach (User user in users)
            {

                userModels.Add(Map(user));
            }

            return userModels;
        }
    }
}
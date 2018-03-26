using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using System.Configuration;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class PersonProfileModel : PersonModel
    {
        IISLogger logger = ISLogger.GetLogger("PersonProfileModel");

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }
        [StringLength(255), Display(Name = "DirectPhone Dial")]
        public string DirectPhone { get; set; }


        public string Mobile { get; set; }

        public string Phone { get; set; }

        public string Room { get; set; }

        public string RoomLink
        {
            get
            {
                if (EL_ID > 0)
                    return string.Format("/Home/Index?ATR=ZimmerNr&TeleSuchString={0}&ELID={1}", Room, EL_ID);
                else
                    return string.Format("/Home/Index?ATR=ZimmerNr&TeleSuchString={0}&ELID=", Room);
            }
        }

        [StringLength(255), Display(Name = "Cost center name")]
        public string CostCenterName { get; set; }

        [StringLength(255), Display(Name = "Cost center")]
        public string CostCenterKstID { get; set; }

        [StringLength(255), Display(Name = "Cost center")]
        public string CostCenterKstIDLink
        {
            get
            {
                return string.Format("/Home/Index?ATR=AC&TeleSuchString={0}", AC_ID);
            }
        }


        public string ImageNotVisibleInPhonebookText { get; set; }

        public string ImageURL { get; set; }

        public bool CanUploadPicture { get; set; }

        public bool ShowImageNotVisibleInPhonebookText { get; set; }

        public bool ShowGuidEntities { get; set; }

        public bool ShowSettingsEmploymentGridExtended { get; set; }

        public bool CanViewPictureVisibleAD { get; set; }

        //public bool CanViewPictureVisiblePhonebook { get; set; }

        public bool ShowTabPackages { get; set; }
        public bool ShowTabSettings { get; set; }
        public int AC_ID { get; private set; }
        public int EL_ID { get; private set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            IsAdmin = securityUser.IsAdmin;
        }

        /// <summary>
        /// writes the given text value pair to the log file
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        public void Log(SecurityUser securityUser, string text, string value)
        {
            IEDPLogger logger = EDPLogger.GetLogger("PersonProfileModelCreateObject");
            if (securityUser.UserId.ToLower().Trim() == "richarda" || securityUser.UserId.ToLower().Trim() == "hengsberg")
            {
                logger.Debug(string.Format("{0}: {1}", text, value));
            }
        }

        public static PersonProfileModel createObject(String pers_guid, SecurityUser securityUser)
        {
            IEDPLogger logger = EDPLogger.GetLogger("PersonProfileModelCreateObject");
            PersonHandler persH = new PersonHandler();
            EMDPerson pers = (EMDPerson)persH.GetObject<EMDPerson>(pers_guid);
            EmploymentManager emplManager = new EmploymentManager();
            ObjectFlagManager ofm = new ObjectFlagManager();

            PersonProfileModel pp = copyFromObject(pers);

            //TODO MainEmployment einbauen
            EmploymentHandler emplH = new EmploymentHandler();
            List<IEMDObject<EMDEmployment>> empls = emplH.GetObjects<EMDEmployment, Employment>("P_Guid = \"" + pers_guid + "\"");
            //EMDEmployment empl = (EMDEmployment)emplH.GetObjects<EMDEmployment, Employment>("P_Guid = \"" + pers_guid + "\"").FirstOrDefault();

            SecurityUserParameterFlags flagIsItSelf = new SecurityUserParameterFlags(isItself: true);
            bool isItSelf = securityUser.hasPermission("", flagIsItSelf, persGuid: pers_guid);

            EMDEmployment empl = null;
            foreach (EMDEmployment myempl in empls)
            {
                if (ofm.IsMainEmployment(myempl.Guid))
                {
                    empl = myempl;
                }
            }

            if (securityUser.IsAdmin || isItSelf)
            {
                pp.ShowTabPackages = true;
                pp.ShowTabSettings = true;
                pp.CanManage = true;
            }

            if (securityUser.UserId.ToLower().Trim() == "richarda" || securityUser.UserId.ToLower().Trim() == "hengsberg")
            {
                logger.Debug(string.Format("{0}-{1}: {2}", securityUser.UserId.ToLower().Trim(), "ShowTabPackages", pp.ShowTabPackages.ToString()));
                logger.Debug(string.Format("{0}-{1}: {2}", securityUser.UserId.ToLower().Trim(), "ShowTabSettings", pp.ShowTabSettings.ToString()));
            }

            pp.IsVisibleInPhonebook = ofm.IsPersonVisibleInPhonebook(pers_guid);
            pp.IsPictureVisible = ofm.IsPictureVisible(pers_guid);
            pp.IsPictureVisibleInAD = ofm.IsPictureVisibleAD(pers_guid);
            //SecurityUserParameterFlags flagAdminOnly = new SecurityUserParameterFlags();
            //bool isAdmin = secUser.hasPermission("", flagAdminOnly);

            //pp.CanManagePersonVisible = secUser.hasPermission(SecurityPermission.PersonManagement_View_Manage_Flags_PersonVisibleInPhonebook, flagAdminOnly);
            if (securityUser.IsAdmin || isItSelf || securityUser.hasPermission(SecurityPermission.PersonManagement_Flags_Manage_PictureVisibleInPhonebook, pp.Guid))
            {
                pp.CanManagePictureVisiblePhonebook = true;
                pp.CanManage = true;

                if (pp.IsPictureVisible)
                    pp.ImageNotVisibleInPhonebookText = "The picture is visible in the intranet phonebook";
                else
                    pp.ImageNotVisibleInPhonebookText = "The picture is not visible in the intranet phonebook";

            }

            if (pp.CanManagePictureVisiblePhonebook)
                pp.ShowImageNotVisibleInPhonebookText = false;
            else
                pp.ShowImageNotVisibleInPhonebookText = true;

            pp.CanOnboard = securityUser.hasPermission(SecurityPermission.Onboarding, new SecurityUserParameterFlags(checkPlainPermisson: true));
            pp.ShowGuidEntities = securityUser.hasPermission(SecurityPermission.Personprofile_View_Guids_View, new SecurityUserParameterFlags(checkPlainPermisson: true));
            pp.ShowSettingsEmploymentGridExtended = securityUser.hasPermission(SecurityPermission.Personprofile_View_Settings_View_Employments_View_Extended, new SecurityUserParameterFlags(checkPlainPermisson: true));


            // reset the userId, because it's from old migration an not related to new UserTable
            pp.UserID = string.Empty;

            if (pers != null && !string.IsNullOrEmpty(pers.USER_GUID))
            {
                try
                {
                    EMDUser emdUser = Manager.UserManager.Get(pers.USER_GUID);
                    if (emdUser.Status != (byte)EnumUserStatus.InUse)
                    {
                        pp.MainMail = string.Empty;
                    }

                    pp.UserID = emdUser.Username;
                }
                catch (Exception ex)
                {
                    pp.MainMail = string.Empty;
                    // UserID not found
                    ISLogger.GetLogger("PersonProfileModel").Error(string.Format("No User found for UserGuid:{0}", pers.USER_GUID), ex);
                }

            }


            //Bild URL
            pp.ImageURL = ConfigurationManager.AppSettings["EMD20Web.WebPathPersonImages"] + ConfigurationManager.AppSettings["EMD20Web.PersonImageFileNameBlank"];      //Default


            bool hasUploadPicturePermission = securityUser.hasPermission(SecurityPermission.Personprofile_View_Manage_UploadImage,pp.Guid);
            if (pp.IsVisibleInPhonebook && pp.IsPictureVisible || isItSelf || hasUploadPicturePermission)
            {
                if (!string.IsNullOrEmpty(pp.UserID))
                {

                    String ImageFilePath = ConfigurationManager.AppSettings["EMD20Web.WebPathPersonImages"] + pp.UserID.ToUpper() + ".jpg";
                    String ImageUNCPath = ConfigurationManager.AppSettings["EMD20Web.FolderPathAdPersonImage"] + pp.UserID.ToUpper() + ".jpg";
                    if (System.IO.File.Exists(ImageUNCPath))
                        pp.ImageURL = ImageFilePath;
                    else
                        pp.ImageURL = ConfigurationManager.AppSettings["EMD20Web.WebPathPersonImages"] + ConfigurationManager.AppSettings["EMD20Web.PersonImageFileNameBlank"];

                    if (isItSelf)
                    {
                        pp.ShowImageNotVisibleInPhonebookText = true;
                    }
                }
            }

            pp.CanUploadPicture = securityUser.hasPermission(SecurityPermission.Personprofile_View_Manage_UploadImage, new SecurityUserParameterFlags(checkPlainPermisson: true));

            bool canViewSettingsFromRole = false;
            canViewSettingsFromRole = securityUser.IsAllowedPerson(pers_guid, SecurityPermission.Personprofile_Settings_View);

            pp.EL_ID = 0;
            if (empl != null)
            {
                EnterpriseLocationManager enloManager = new EnterpriseLocationManager();
                EMDEnterpriseLocation enlo = enloManager.Get(empl.ENLO_Guid);
                if (enlo != null)
                {
                    LocationManager locaManager = new LocationManager();
                    EMDLocation loca = locaManager.Get(enlo.L_Guid);
                    if (loca != null && loca.EL_ID != null)
                    {
                        pp.EL_ID = Convert.ToInt32(loca.EL_ID);
                    }
                }

                //pp.CanManagePictureVisibleAD = secUser.hasPermission(SecurityPermission.PersonManagement_View_Manage_Flags_PictureVisibleInAD, pers_guid);

                //pp.ShowTabPackages = securityUser.hasPermission(SecurityPermission.Personprofile_View_Package, empl.P_Guid);
                if (!isItSelf && !securityUser.IsAdmin)
                {
                    SecurityUserParameterFlags paramFlags = new SecurityUserParameterFlags(true, false, false, true, false, false);
                    pp.ShowTabPackages = securityUser.hasPermission(SecurityPermission.Personprofile_View_Package, paramFlags, null, empl.Guid);
                pp.ShowTabSettings = securityUser.hasPermission(SecurityPermission.Personprofile_Flags_Manage_Employment_Main, new SecurityUserParameterFlags(isItself: true, isLineManager: true, isAssistence: true), null, empl.Guid);

                }

                if (securityUser.UserId.ToLower().Trim() == "richarda" || securityUser.UserId.ToLower().Trim() == "hengsberg")
                {
                    logger.Debug(string.Format("{0}-{1}: {2}", securityUser.UserId.ToLower().Trim(), "ShowTabPackages", pp.ShowTabPackages.ToString()));
                    logger.Debug(string.Format("{0}-{1}: {2}", securityUser.UserId.ToLower().Trim(), "ShowTabSettings", pp.ShowTabSettings.ToString()));
                }



                EmploymentAccountHandler empAch = new EmploymentAccountHandler();
                //EMDEmploymentAccount empAcc = (EMDEmploymentAccount)empAch.GetObjects<EMDEmploymentAccount, EmploymentAccount>("EP_Guid = \"" + empl.Guid + "\" and Main=1").First();
                List<IEMDObject<EMDEmploymentAccount>> empAcc = empAch.GetObjects<EMDEmploymentAccount, EmploymentAccount>("EP_Guid = \"" + empl.Guid + "\"");
                EMDEmploymentAccount empAccMain = null;

                foreach (EMDEmploymentAccount emplAccount in empAcc)
                {
                    if (ofm.IsMainAccount(emplAccount.Guid))
                    {
                        empAccMain = emplAccount;
                    }
                }

                if (empAccMain != null)
                {
                    AccountHandler ach = new AccountHandler();
                    EMDAccount Acc = (EMDAccount)ach.GetObject<EMDAccount>(empAccMain.AC_Guid);

                    pp.CostCenterKstID = Acc.KstID;
                    pp.AC_ID = Acc.AC_ID;
                    pp.CostCenterName = Acc.Name;
                }

                ContactHandler ch = new ContactHandler();
                pp.JobTitle = ch.GetContactStringByContactType(empl.Guid, ContactTypeHandler.JOBTITLE);
                pp.DirectPhone = ch.GetContactStringByContactType(empl.Guid, ContactTypeHandler.DIRECTDIAL);
                pp.Phone = ch.GetContactStringByContactType(empl.Guid, ContactTypeHandler.PHONE);
                pp.Mobile = ch.GetContactStringByContactType(empl.Guid, ContactTypeHandler.MOBILE);
                pp.Room = ch.GetContactStringByContactType(empl.Guid, ContactTypeHandler.ROOM);
            }

            if (canViewSettingsFromRole)
                pp.ShowTabSettings = true;

            if (securityUser.UserId.ToLower().Trim() == "richarda" || securityUser.UserId.ToLower().Trim() == "hengsberg")
            {
                logger.Debug(string.Format("{0}-{1}: {2}", securityUser.UserId.ToLower().Trim(), "ShowTabPackages", pp.ShowTabPackages.ToString()));
                logger.Debug(string.Format("{0}-{1}: {2}", securityUser.UserId.ToLower().Trim(), "ShowTabSettings", pp.ShowTabSettings.ToString()));
            }

            return pp;
        }

        public static PersonProfileModel copyFromObject(EMDPerson person)
        {
            PersonProfileModel profileModel = new PersonProfileModel();
            ReflectionHelper.CopyProperties<EMDPerson, PersonProfileModel>(ref person, ref profileModel);
            return profileModel;
        }
    }
}
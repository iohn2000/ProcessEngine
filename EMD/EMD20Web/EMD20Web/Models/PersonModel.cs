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
using Kapsch.IS.EDP.Core.Logic;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class PersonModel : BaseModel
    {
        public List<System.Web.Mvc.SelectListItem> GenderList { get; set; }

        //[ScaffoldColumn(true)]
        public string Guid { get; set; }

        [Editable(false)]
        public string HistoryGuid { get; set; }


        [Editable(false)]
        public System.DateTime Created { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedDateOnly
        {
            get { return this.Created.Date; }
        }

        [Editable(false)]
        public Nullable<System.DateTime> Modified { get; set; }

        public Nullable<DateTime> ModifiedDateOnly
        {
            get { return this.Modified?.Date; }
        }

        public int P_ID { get; set; }

        [StringLength(255), Required(), Display(Name = "Surname")]
        public string FamilyName { get; set; }

        [StringLength(255), Required(), Display(Name = "First Name")]
        public string FirstName { get; set; }
        public string Synonyms { get; set; }

        [Required()]
        [Display(Name = "Gender")]
        public string Sex { get; set; }

        [Display(Name = "Gender")]
        public string SexDisplay { get; set; }

        [StringLength(25), Display(Name = "Degree")]
        public string DegreePrefix { get; set; }

        [StringLength(25), Display(Name = "Degree Suffix")]
        public string DegreeSuffix { get; set; }

        [StringLength(255), Required(), Display(Name = "Surname (Reduced 128 Bit)", Description = "Family name without special characters")]
        public string C128_FamilyName { get; set; }

        [StringLength(255), Required(), Display(Name = "First Name (Reduced 128 Bit)", Description = "First name without special characters")]
        public string C128_FirstName { get; set; }

        [StringLength(25), Display(Name = "Degree (Reduced 128 Bit)", Description = "Degree to display in IT Systems like Phonebook, Outlook, etc.")]
        public string C128_DegreePrefix { get; set; }

        [StringLength(25), Display(Name = "Degree Suffix (Reduced 128 Bit)", Description = "Degree suffix to display in IT Systems like Phonebook, Outlook, etc.")]
        public string C128_DegreeSuffix { get; set; }

        [Display(Name = "UserID")]
        public string UserID { get; set; }

        public string USER_GUID { get; set; }


        [Display(Name = "Main Mail")]
        public string MainMail { get; set; }
        [Display(Name = "UnixID")]
        public string UnixID { get; set; }


        [Display(Name = "Person visible in phonebook")]
        public bool IsVisibleInPhonebook { get; set; }

        [Display(Name = "Picture visible in phonebook")]
        public bool IsPictureVisible { get; set; }

        [Display(Name = "Picture visible in AD, Outlook, Lync & SharePoint")]
        public bool IsPictureVisibleInAD { get; set; }

        [Display(Name = "Language")]
        public string Language { get; set; }


        [StringLength(255), Required(), Display(Name = "Surname (Display)", Description = "Surname to display in IT Systems like Phonebook, Outlook, etc.")]
        public string Display_FamilyName { get; set; }

        [StringLength(255), Required(), Display(Name = "Firstname (Display)", Description = "Firstname to display in IT Systems like Phonebook, Outlook, etc.")]
        public string Display_FirstName { get; set; }

        public Nullable<System.DateTime> AD_Picture_UpdDT { get; set; }

        public bool IsInDpw { get; set; }

        public bool CanManagePersonVisible { get; set; }

        public bool CanManagePictureVisiblePhonebook { get; set; }

        public bool CanManagePictureVisibleAD { get; set; }

        public bool CanManagePersonMainData { get; set; }

        public bool CanSave { get; set; }

        public bool CanOnboard { get; set; }

        public bool CanManageGender { get; set; }

        /// <summary>
        /// true if the personGuid modified = logged in user personGuid
        /// </summary>
        [Display(Name = "Created by me")]
        public bool IsCreatedByMe { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.PersonManagement_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.PersonManagement_View; } }

        private bool isItSelf { get; set; }
        public bool IsItSelf
        {
            get { return isItSelf; }
        }

        public bool CanRemovePictureVisibleAD { get; private set; }

        public override void InitializeBaseSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);


            this.CanOnboard = securityUser.hasPermission(SecurityPermission.Onboarding, new SecurityUserParameterFlags(checkPlainPermisson: true));
            this.CanView = true; //Everyone can see person
        }
        public override void InitializeSecurity(SecurityUser securityUser)
        {
            this.InitializeBaseSecurity(securityUser);

            ObjectFlagManager ofm = new ObjectFlagManager();

            //bool isItSelf = secUser.IsItSelf(this.Guid, true);
            isItSelf = securityUser.IsItSelf(this.Guid, true);
            this.CanManageGender = false;

            //EmploymentManager employmentManager = new EmploymentManager();
            //List<EMDEmployment> employments = employmentManager.GetEmploymentsForPerson(this.Guid).Cast<EMDEmployment>().ToList();

            //foreach (EMDEmployment emp in employments)
            //{
            //    secUser.hasPermission(SecurityPermission.PersonManagement_Gender_View_Manage, new SecurityUserParameterFlags(), null, null, this.Guid);

            //}

            this.CanManageGender = securityUser.hasPermission(SecurityPermission.PersonManagement_Gender_View_Manage, this.Guid, false);

            //PictureVisibleAD
            this.CanManagePictureVisibleAD = securityUser.hasPermission(SecurityPermission.PersonManagement_Flags_Manage_PictureVisibleInAD, this.Guid);
            this.CanRemovePictureVisibleAD = securityUser.hasPermission(SecurityPermission.PersonManagement_Flags_Manage_PictureVisibleInAD, this.Guid);
            if (this.CanManagePictureVisibleAD && !this.IsPictureVisibleInAD)
            {
                if (!(isItSelf == true || securityUser.IsAdmin))
                {
                    this.CanManagePictureVisibleAD = false;
                }
            }


            this.CanManagePictureVisibleAD = this.CanManagePictureVisibleAD;

            //Find out if person is in dpw
            EmploymentHandler emplH = new EmploymentHandler();
            List<IEMDObject<EMDEmployment>> empls = emplH.GetObjects<EMDEmployment, Employment>("P_Guid = \"" + Guid + "\"");
            EMDEmployment empl = null;
            foreach (EMDEmployment myempl in empls)
            {
                if (ofm.IsMainEmployment(myempl.Guid))
                {
                    empl = myempl;
                }
                if (myempl.dpwKey != null && !String.IsNullOrWhiteSpace(myempl.dpwKey))
                {
                    this.IsInDpw = true;
                }
            }

            this.CanManagePersonVisible = securityUser.hasPermission(SecurityPermission.PersonManagement_Flags_Manage_PersonVisibleInPhonebook, this.Guid, false);
            this.CanManagePictureVisiblePhonebook = securityUser.hasPermission(SecurityPermission.PersonManagement_Flags_Manage_PictureVisibleInPhonebook, this.Guid, false);


            //MainData
            this.CanManagePersonMainData = securityUser.hasPermission(SecurityPermission.PersonManagement_View_Manage, new SecurityUserParameterFlags(checkPlainPermisson: true));


            //Only by persons that are not in dpw the Main Data can be edited
            if (this.IsInDpw)
            {
                this.CanManagePersonMainData = false;
            }
            else
            {
                if (securityUser.hasPermission(SecurityPermission.PersonManagement_View_Manage, this.Guid))
                    this.CanManagePersonMainData = true;
            }

            if (this.IsAdmin)
            {
                this.CanManagePersonMainData = true;
                this.CanManagePersonVisible = true;
                this.CanManagePictureVisibleAD = true;
                this.CanManagePictureVisiblePhonebook = true;
                this.CanManageGender = true;
            }

            if (CanManagePersonMainData)
                CanManageGender = true;

            //Save Button
            if (this.CanManagePersonMainData || this.CanManagePersonVisible || this.CanManagePictureVisibleAD || this.CanManagePictureVisiblePhonebook)
            {
                this.CanSave = true;
                this.CanManage = true;
            }
            else
            {
                this.CanSave = false;
                this.CanManage = false;
            }
        }


        public static PersonModel Initialize(Person person)
        {
            PersonModel scheisslich = new PersonModel();
            ReflectionHelper.CopyProperties<Person, PersonModel>(ref person, ref scheisslich);
            PersonModel.GetGenderList().ForEach(item =>
            {
                if (item.Key == scheisslich.Sex)
                    scheisslich.SexDisplay = item.Name;
            });

            return scheisslich;
        }

        public static PersonModel Initialize(EMDPerson person)
        {
            PersonModel scheisslich = new PersonModel();
            ReflectionHelper.CopyProperties<EMDPerson, PersonModel>(ref person, ref scheisslich);
            PersonModel.GetGenderList().ForEach(item =>
            {
                if (item.Key == scheisslich.Sex)
                    scheisslich.SexDisplay = item.Name;
            });

            return scheisslich;
        }

        public static List<GenderModel> GetGenderList()
        {
            List<GenderModel> Genders = new List<GenderModel>();
            Genders.Add(new GenderModel("Male", "M"));
            Genders.Add(new GenderModel("Female", "F"));
            Genders.Add(new GenderModel("Neutral", "N"));
            return Genders;
        }
    }

}
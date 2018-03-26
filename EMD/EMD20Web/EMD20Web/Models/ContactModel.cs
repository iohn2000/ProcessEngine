using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    /// <summary>
    /// Keep Care ContactModel is not 1:1 mapping
    /// </summary>
    public class ContactModel : BaseModel
    {
        public override String CanManagePermissionString { get { return SecurityPermission.ContactManager_View_Manage; } }

        public override String CanViewPermissionString { get { return SecurityPermission.ContactManager_View; } }

        public string ActiveFromString
        {
            get
            {
                if (ActiveFrom > DateTime.Now)
                {
                    return ActiveFrom.ToString("dd.MM.yyyy");
                }
                return string.Empty;
            }
        }

        public string Guid { get; set; }

        public string Name
        {
            get
            {
                string name = string.Empty;

                EnumContactType contactType = (EnumContactType)C_CT_ID;
                try
                {
                    name = ObjectHelper.GetEnumDescription(contactType);
                }
                catch (Exception)
                {
                    name = C_CT_ID.ToString();
                }


                return name;
            }
        }

        public static ContactModel New(string empl_guid, bool isFuture = false)
        {
            ContactModel contactModel = new ContactModel()
            {
                EP_Guid = empl_guid
            };

            if (isFuture)
            {
                contactModel.ActiveFrom = GetFutureDate();
            }

            return contactModel;
        }

        public static DateTime GetFutureDate()
        {
            DateTime now = DateTime.Now;
            DateTime today = new DateTime(now.Year, now.Month, now.Day);
            return today.AddDays(1);
        }

        public bool ShowInGrid
        {
            get
            {
                return !IsFuture || (IsFuture && !string.IsNullOrEmpty(Guid));
            }
        }


        public string CssActionButtonDeleteVisible
        {
            get
            {
                return IsFuture ? "visible" : "hidden";
            }
        }

        public string CssActionButtonAddFutureVisible { get; set; }

        public string CT_Guid { get; set; }

        public string EP_Guid { get; set; }

        public string P_Guid { get; set; }

        public string E_Guid { get; set; }

        public string L_Guid { get; set; }

        public int C_ID { get; set; }

        public int? C_EP_ID { get; set; }

        public int? C_E_ID { get; set; }

        public int? C_P_ID { get; set; }

        public int? C_L_ID { get; set; }

        public int C_CT_ID { get; set; }

        public int ELID { get; set; }

        public bool ACDDisplay { get; set; }



        [RegularExpression(@"^\+[1-9][0-9]*$", ErrorMessage = "First letter must be a '+' and first digit mustn't start with 0")]
        [StringLength(4, ErrorMessage = "The number must be 1-3 digits", MinimumLength = 2)]
        public string NationalCode { get; set; }

        /// <summary>
        /// International standard E.164 allows only 4, but we can define also non international numbers
        /// </summary>
        [RegularExpression(@"^[1-9][0-9]*$", ErrorMessage = "The prefix must be numeric and mustn't start with 0")]
        [StringLength(6, ErrorMessage = "The number must be 1-6 digits", MinimumLength = 1)]
        public string Prefix { get; set; }


        [RegularExpression("^[0-9 ]*$", ErrorMessage = "The number must be numeric (allows spaces)")]
        public string Number { get; set; }

        [Display(Name = "Number")]
        public string NumberAsText { get; set; }

        public string Text { get; set; }

        public bool IsVisibleInPhoneBook { get; set; }

        public bool IsVisibleInCallCenter { get; set; }

        public bool IsAllowedEmployment { get; set; }

        public bool IsFuture
        {
            get
            {
                return ActiveFrom > DateTime.Now;
            }
        }



        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            if (!string.IsNullOrWhiteSpace(EP_Guid))
                IsAllowedEmployment = securityUser.IsAllowedEmployment(securityUser.UserId, EP_Guid);
            else
                IsAllowedEmployment = false;
        }

        public static ContactModel GetContactModel(ContactHandler contactHandler, string guidEmployment, int contactType, bool isFuture = false)
        {
            if (contactHandler == null)
            {
                contactHandler = new ContactHandler();
            }

            ContactModel contactModel = ContactModel.New(guidEmployment, isFuture);


            EMDContact emdContact = null;
            emdContact = contactHandler.GetContactByContactType(guidEmployment, contactType, isFuture);

            EMDEmployment employment = new EmploymentManager().GetEmployment(guidEmployment);




            if (emdContact == null)
            {
                contactModel.EP_Guid = guidEmployment;
                EMDContactType emdContactType = new ContactTypeHandler().GetObjects<EMDContactType, ContactType>("CT_ID == " + contactType + "").Cast<EMDContactType>().FirstOrDefault();


                contactModel.CT_Guid = emdContactType.Guid;
                contactModel.C_CT_ID = emdContactType.CT_ID;
            }
            else
            {
                contactModel = Map(emdContact);
            }

            if (contactModel.C_CT_ID == 7)
            {
                EnterpriseLocationManager enloManager = new EnterpriseLocationManager();
                EMDEnterpriseLocation enlo = enloManager.Get(employment.ENLO_Guid);
                EMDLocation loca = new LocationManager().Get(enlo.L_Guid);

                contactModel.ELID = loca.EL_ID.HasValue ? loca.EL_ID.Value : 0;
            }


            return contactModel;
        }


        public static void UpdateModels(ref List<ContactModel> models)
        {
            foreach (ContactModel item in models)
            {
                item.CssActionButtonAddFutureVisible = "hidden";
                if (!item.IsFuture)
                {
                    ContactModel foundFutureContact = models.Find(c => c.C_CT_ID == item.C_CT_ID && c.IsFuture == true);

                    if (foundFutureContact != null && !foundFutureContact.ShowInGrid)
                    {
                        item.CssActionButtonAddFutureVisible = "visible";
                    }

                }

            }
        }

        public static ContactModel Map(EMDContact emdContact)
        {
            ContactModel contactModel = new ContactModel();
            ReflectionHelper.CopyProperties<EMDContact, ContactModel>(ref emdContact, ref contactModel);
            //     contactModel = ContactModel.Map(emdContact);

            contactModel.Text = contactModel.IsFuture ? ContactModel.GetFutureTextInfo(emdContact) : emdContact.Text?.Trim();

            switch (contactModel.C_CT_ID)
            {
                case ContactTypeHandler.JOBTITLE:
                    contactModel.Number = emdContact.Text?.Trim();
                    break;
                case ContactTypeHandler.ROOM:
                    contactModel.Number = emdContact.Text?.Trim();
                    emdContact.VisiblePhone = true;
                    emdContact.VisibleKatce = true;
                    break;

                case ContactTypeHandler.DIRECTDIAL:
                case ContactTypeHandler.DIRECTDIAL2:
                case ContactTypeHandler.DIRECTEFAX:
                    contactModel.Prefix = emdContact.Details?.Trim();
                    contactModel.Number = emdContact.Text?.Trim();
                    break;

                case ContactTypeHandler.MOBILE:
                case ContactTypeHandler.PHONE:
                case ContactTypeHandler.EFAX:
                    string[] numberSplit = emdContact.Text.Split(' ');

                    string number = string.Empty;
                    if (numberSplit.Length > 1)
                    {
                        string twoStringPrefix = string.Format("{0} {1}", numberSplit[0]?.Trim(), numberSplit[1]?.Trim());
                        number = emdContact.Text.Replace(twoStringPrefix, string.Empty);
                    }
                    else
                    {
                        contactModel.NationalCode = string.Empty;
                        contactModel.Prefix = string.Empty;
                        contactModel.Number = numberSplit[0];
                    }



                    if (numberSplit.Length > 1)
                    {
                        contactModel.NationalCode = numberSplit[0]?.Trim();
                        contactModel.Prefix = numberSplit[1]?.Trim();
                        contactModel.Number = number?.Trim();
                    }

                    break;

                default:
                    break;
            }

            contactModel.IsVisibleInPhoneBook = emdContact.VisiblePhone;
            contactModel.IsVisibleInCallCenter = emdContact.VisibleKatce;

            contactModel.NumberAsText = contactModel.Number;

            return contactModel;
        }

        public static string GetFutureTextInfo(EMDContact emdContact)
        {
            if (emdContact != null)
            {
                return string.Format("{0} ({1})", emdContact.Text, emdContact.ActiveFrom.ToString("dd.MM.yyyy"));
            }
            return string.Empty;

        }
    }
}
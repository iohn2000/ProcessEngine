using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Framework;
using System.ComponentModel.DataAnnotations;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class ContactDataFutureModel : BaseModel
    {
        public override String CanManagePermissionString { get { return SecurityPermission.ContactManager_View_Manage; } }

        public override String CanViewPermissionString { get { return SecurityPermission.ContactManager_View; } }

        private DateTime activeFromFuture;

        public bool IsAllowedEmployment { get; set; }
        /// <summary>
        /// workaround, because Telerik Javascript can't find element with FutureContact.ActiveFrom
        /// </summary>
        [Required]
        public DateTime ActiveFromFuture
        {
            get { return activeFromFuture; }
            set
            {
                this.activeFromFuture = value;
                if (this.FutureContact != null)
                {
                    this.FutureContact.ActiveFrom = value;
                }
            }
        }


        private ContactModel futureContact;


        public ContactModel CurrentContact { get; set; }

        public ContactModel FutureContact
        {
            get { return this.futureContact; }
            set
            {
                this.ActiveFromFuture = value.ActiveFrom;
                this.futureContact = value;
            }
        }

        private bool? isFutureChecked;

        public bool IsFutureChecked
        {
            get
            {
                return this.isFutureChecked.HasValue ? this.isFutureChecked.Value : false;
            }
            set
            {
                this.isFutureChecked = value;
            }
        }

        public bool HasFutureContact
        {
            get
            {
                bool hasFuture = !string.IsNullOrEmpty(FutureContact.Guid);

                if (this.isFutureChecked == null)
                {
                    IsFutureChecked = hasFuture;
                }

                return hasFuture;
            }
        }

        public ContactDataFutureModel()
        {
            CurrentContact = new ContactModel();
            FutureContact = new ContactModel();
        }

        public void Init(ContactModel current, ContactModel future)
        {
            CurrentContact = current;
            FutureContact = future;
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            CurrentContact.InitializeSecurity(securityUser);
            this.IsAllowedEmployment = CurrentContact.IsAllowedEmployment;

        }
    }
}
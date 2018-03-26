using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public abstract class BaseModel : IBaseModel
    {
        [Display(Name = "Username")]
        public String Username { get; set; }

        public string UserFullName { get; set; }

        [Editable(false)]
        [UIHint("Date"), Display(Name = "Valid From")]
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// for Grid Filtering to find a date without time
        /// </summary>
        [Display(Name = "Valid From")]
        public DateTime ValidFromDateOnly
        {
            get { return this.ValidFrom.Date; }
        }

        /// <summary>
        /// Date when this entity stops to be valid
        /// </summary>
        [UIHint("Date"), Display(Name = "Valid To")]
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// for Grid Filtering to find a date without time
        /// </summary>
        [Display(Name = "Valid To")]
        public DateTime ValidToDateOnly
        {
            get { return this.ValidTo.Date; }
        }

        /// <summary>
        /// Date when this entity starts to be acitve
        /// </summary>
        [UIHint("Date"), Display(Name = "Active From")]
        public DateTime ActiveFrom { get; set; }

        /// <summary>
        /// for Grid Filtering to find a date without time
        /// </summary>
        [Display(Name = "Active From")]
        public DateTime ActiveFromDateOnly
        {
            get { return this.ActiveFrom.Date; }
        }

        /// <summary>
        /// Date when this entity stops to be acitve
        /// </summary>
        [UIHint("Date"), Display(Name = "Active To")]
        public DateTime ActiveTo { get; set; }

        /// <summary>
        /// for Grid Filtering to find a date without time
        /// </summary>
        [Display(Name = "Active To")]
        public DateTime ActiveToDateOnly
        {
            get { return this.ActiveTo.Date; }
        }

        //public bool CanManagePermissionString { get; set; }
        //public bool CanViewPermissionString { get; set; }
        public virtual String CanViewPermissionString
        {
            get { return SecurityPermission.NotDefined; }
        }

        public virtual String CanManagePermissionString
        {
            get { return SecurityPermission.NotDefined; }
        }

        public bool CanManage { get; set; }

        public bool CanView { get; set; }

        public bool IsAdmin { get; set; }

        public abstract void InitializeSecurity(SecurityUser securityUser);

        public virtual void InitializeBaseSecurity(SecurityUser securityUser)
        {
            SecurityUserParameterFlags flags = new SecurityUserParameterFlags(checkPlainPermisson: true);
            this.CanView = securityUser.hasPermission(CanViewPermissionString, flags);
            this.CanManage = securityUser.hasPermission(CanManagePermissionString, flags);
            this.IsAdmin = securityUser.IsAdmin;
        }
    }
}
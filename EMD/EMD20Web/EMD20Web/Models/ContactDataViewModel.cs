using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    /// <summary>
    /// Only used to view contactdata items
    /// </summary>
    public class ContactDataViewModel
    {
        /// <summary>
        /// Used for updating Base info for the personprofile view
        /// </summary>
        public bool IsMainEmployment { get; set; }

        public string EnterpriseName { get; set; }

        public string JobTitle { get; internal set; }

        [Display(Name = "Room number")]
        public string Room { get; set; }


    

    }
}
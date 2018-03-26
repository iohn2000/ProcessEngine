using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models.Shared
{
    public class RuleFilterModel
    {
        public List<String> Enterprises { get; set; }

        public bool EnterpriseInvertFlag { get; set; }

        public List<String> Locations { get; set; }

        public bool LocationInvertFlag { get; set; }


        public List<String> Accounts { get; set; }

        public bool AccountInvertFlag { get; set; }

        public List<String> EmploymentTypes { get; set; }

        public bool EmploymentTypeInvertFlag { get; set; }

        public List<TextValueModel> AvailableUserTypes { get; set; }

        public List<String> UserTypes { get; set; }

        public bool UserTypeInvertFlag { get; set; }

        public bool EnteIsNotInherited { get; set; }

        public RuleFilterModel()
        {
            this.Enterprises = new List<string>();
            this.Locations = new List<string>();
            this.Accounts = new List<string>();
            this.EmploymentTypes = new List<string>();
            this.UserTypes = new List<string>();

            this.AvailableUserTypes = new List<TextValueModel>();

            foreach (string item in System.Enum.GetNames(typeof(EnumUserType)))
            {
                EnumUserType currentItem = (EnumUserType)System.Enum.Parse(typeof(EnumUserType), item, true);

                this.AvailableUserTypes.Add(new TextValueModel(UserModel.GetUserTypeDisplayName(currentItem), item));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Utils;

namespace Kapsch.IS.EDP.Core.Entities
{
    public enum EnumUserType
    {
        [Description("Limited Account")]
        ADUserLimitedAccount = 10,
        [Description("Full Account")]
        ADUserFullAccount = 20,
        [Description("No defined AD-type"), EnumUserTypeAttribute(true)]
        ADUserNotDefined = 30,
        [Description("External Supplier"), EnumUserTypeAttribute(true)]
        ADUserExternalSupplier = 40,
        [Description("Test User")]
        ADUserTest = 50,
        [Description("Admin User")]
        ADUserAdmin = 60,
        [Description("System User")]
        ADUserSystem = 70
    }


    /// <summary>
    /// Describes if an enum is Active or not
    /// </summary>
    public class EnumUserTypeAttribute : Attribute
    {
        internal EnumUserTypeAttribute(bool isDeleted)
        {
            this.IsDeleted = isDeleted;
        }


        public bool IsDeleted { get; set; }



        public static bool GetIsDeleted(EnumUserType enumUserType)
        {
            var ortyAttr = enumUserType.GetAttribute<EnumUserTypeAttribute>();
            if (ortyAttr == null)
            {
                return false;
            }

            return ortyAttr.IsDeleted;
        }

    }
}


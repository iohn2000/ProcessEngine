using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Utils;

namespace Kapsch.IS.EDP.Core.Logic
{
    /// <summary>
    /// DataBase C-Table Relation
    /// This enum is the 1:1 relation of the database-table ObjectContainer 
    /// do never change these values in the database !!!
    /// </summary>
    public enum EnumObjectRelationType
    {
        [EnumObjectRelationTypeAttribute("ORTY_0b425b2f4176455294f3a40b6775be9b", "EMPL", "EQDE")]
        EquipmentByPackage = 10,
        [EnumObjectRelationTypeAttribute("ORTY_6fd217bdc9b14a4ca02bcdcbff5e7929", "EMPL", "EQDE")]
        EquipmentByEmploymentPackage = 20,
        [EnumObjectRelationTypeAttribute("ORTY_be44c28d8edc47a192382f8a1fb38cbf", "OBCO", "EMPL")]
        PackageByEmployment = 30
    }

    public class EnumObjectRelationTypeAttribute : Attribute
    {
        internal EnumObjectRelationTypeAttribute(string guid, string object1, string object2)
        {
            this.Guid = guid;
            this.Object1 = object1;
            this.Object2 = object2;
        }


        public string Guid { get; set; }

        public string Object1 { get; set; }

        public string Object2 { get; set; }

        internal static string GetGuid(EnumObjectRelationType enumorty)
        {
            var ortyAttr = enumorty.GetAttribute<EnumObjectRelationTypeAttribute>();
            return ortyAttr.Guid;
        }
        internal static string GetObject1(EnumObjectRelationType enumorty)
        {
            var ortyAttr = enumorty.GetAttribute<EnumObjectRelationTypeAttribute>();
            return ortyAttr.Object1;
        }
        internal static string GetObject2(EnumObjectRelationType enumorty)
        {
            var ortyAttr = enumorty.GetAttribute<EnumObjectRelationTypeAttribute>();
            return ortyAttr.Object2;
        }
    }

}

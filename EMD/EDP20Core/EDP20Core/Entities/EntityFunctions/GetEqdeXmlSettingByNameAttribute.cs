using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    public class GetEqdeXmlSettingByNameAttribute : EntityFunction<GetEqdeXmlSettingByNameAttribute>
    {
        /// <summary>
        /// get setting from eqdeGuid
        /// string eqdeGuid, 
        /// string elementName
        /// </summary>
        /// <returns></returns>

        protected override GetEqdeXmlSettingByNameAttribute Worker()
        {
            EquipmentDefinitionHandler eqdeH = new EquipmentDefinitionHandler();
            base.Result = eqdeH.GetEqdeXmlSettingByNameAttribute(this.EntityGuid, this.Parameter);
            return this;
        }
    }
}

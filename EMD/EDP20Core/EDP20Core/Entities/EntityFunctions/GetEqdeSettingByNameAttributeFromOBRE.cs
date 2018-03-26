using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    public class GetEqdeSettingByNameAttributeFromOBRE : EntityFunction<GetEqdeSettingByNameAttributeFromOBRE>
    {
        /// <summary>
        /// implementation of Call for NullFunction
        /// string obreGuid, 
        /// string elementName
        /// </summary>
        /// <param name="myParameter"></param>
        /// <returns></returns>
        protected override GetEqdeSettingByNameAttributeFromOBRE Worker()
        {
            ObjectRelationHandler obreH = new ObjectRelationHandler();
            base.Result = obreH.GetEqdeSettingByNameAttributeFromOBRE(this.EntityGuid, this.Parameter);
            return this;
        }

    }
}

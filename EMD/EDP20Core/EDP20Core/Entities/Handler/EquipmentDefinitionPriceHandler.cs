using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EquipmentDefinitionPriceHandler : EMDObjectHandler
    {
        public EquipmentDefinitionPriceHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public EquipmentDefinitionPriceHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public EquipmentDefinitionPriceHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public EquipmentDefinitionPriceHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new EquipmentDefinitionPrice().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            EquipmentDefinitionPrice cont = (EquipmentDefinitionPrice)dbObject;
            EMDEquipmentDefinitionPrice emdObject = new EMDEquipmentDefinitionPrice(cont.Guid, cont.Created, cont.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

    }
}

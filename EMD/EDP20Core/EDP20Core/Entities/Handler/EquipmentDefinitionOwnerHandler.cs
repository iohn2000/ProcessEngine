using System;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Handler for EquipmentDefinitionOwners
    /// </summary>
    public class EquipmentDefinitionOwnerHandler : EMDObjectHandler
    {
        public EquipmentDefinitionOwnerHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public EquipmentDefinitionOwnerHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public EquipmentDefinitionOwnerHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public EquipmentDefinitionOwnerHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new EquipmentDefinitionOwner().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            EquipmentDefinitionOwner equipmentDefinitionOwner = (EquipmentDefinitionOwner)dbObject;
            EMDEquipmentDefinitionOwner emdObject = new EMDEquipmentDefinitionOwner(equipmentDefinitionOwner.Guid, equipmentDefinitionOwner.Created, equipmentDefinitionOwner.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

    }
}

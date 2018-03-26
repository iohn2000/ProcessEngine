using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities.EquipmentDef;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class ObjectRelationHandler : EMDObjectHandler
    {
        public ObjectRelationHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public ObjectRelationHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public ObjectRelationHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public ObjectRelationHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new ObjectRelation().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            ObjectRelation obre = (ObjectRelation) dbObject;
            EMDObjectRelation emdObject = new EMDObjectRelation(obre.Guid, obre.Created, obre.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>) emdObject;
        }

        [Obsolete]
        /// <summary>
        /// @@ function
        /// go from obre (equipment) to eqde and get a settting
        /// this can be handy b/c workflow might not have eqdeGuid but only obreGuid
        /// </summary>
        /// <param name="obreGuid"></param>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public string GetEqdeSettingByNameAttributeFromOBRE(string obreGuid, string elementName)
        {
            ObjectRelationHandler obreH = new ObjectRelationHandler();
            EMDObjectRelation obre = (EMDObjectRelation) obreH.GetObject<EMDObjectRelation>(obreGuid);

            ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler();
            string objectNumber = ortyH.GetObjectNumberWithPrefix(obre.ORTYGuid, "EQDE");

            // now get eqde guid out of obre
            string eqdeGuid = (string) ReflectionHelper.GetPropValue(obre, objectNumber);

            return new EquipmentDefinitionHandler().GetEqdeXmlSettingByNameAttribute(eqdeGuid, elementName);
        }

        public List<DynamicField> GetDynamicEquipmentFieldsForObreGuid(string obreGuid)
        {
            EquipmentDefinitionHandler eqdeH = new EquipmentDefinitionHandler();
            EMDEquipmentDefinition eqde = new EMDEquipmentDefinition();
            ObjectRelationHandler obreH = new ObjectRelationHandler();
            EMDObjectRelation obre = (EMDObjectRelation) obreH.GetObject<EMDObjectRelation>(obreGuid);

            ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler();
            string objectNumber = ortyH.GetObjectNumberWithPrefix(obre.ORTYGuid, "EQDE");

            string eqdeGuid = (string) ReflectionHelper.GetPropValue(obre, objectNumber);
            eqde = (EMDEquipmentDefinition) eqdeH.GetObject<EMDEquipmentDefinition>(eqdeGuid);
            EquipmentDefinitionConfig eqCfg = eqde.GetEquipmentDefinitionConfig();
            return eqCfg.DynamicFields;
        }
    }
}

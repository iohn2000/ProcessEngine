using System;
using System.Collections.Generic;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class ObjectRelationTypeHandler : EMDObjectHandler
    {
        public ObjectRelationTypeHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public ObjectRelationTypeHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public ObjectRelationTypeHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public ObjectRelationTypeHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new ObjectRelationType().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            ObjectRelationType orty = (ObjectRelationType) dbObject;
            EMDObjectRelationType emdObject = new EMDObjectRelationType(orty.Guid, orty.Created, orty.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>) emdObject;
        }

        /// <summary>
        /// find the object number for a given orty relation name and prefix.
        /// e.g. for relation name = 'EquipmentByPackage' (guid = ORTY_04ef33428f804faaa1497072dc2ab40f) get the Object number for "EQDE" --> Object2        
        /// </summary>
        /// <param name="relationTypeName"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public string FindObjectField(string relationTypeName, string prefix)
        {
            var xx = (List<IEMDObject<EMDObjectRelationType>>) 
                this.GetObjects<EMDObjectRelationType, ObjectRelationType>("RelationName = \"" + relationTypeName + "\"");

            if (xx.Count > 0)
            {
                EMDObjectRelationType t = (EMDObjectRelationType) xx[0];
                if (t.Object1 == prefix)
                    return "Object1";
                else if (t.Object2 == prefix)
                    return "Object2";
                else
                    return "";
            }
            else
                return "";
        }

        /// <summary>
        /// find the object number for a given orty guid and prefix.
        /// e.g. for orty = ORTY_04ef33428f804faaa1497072dc2ab40f (=EquipmentByPackage) get the Object number for "EQDE" --> Object2
        /// </summary>
        /// <param name="ortyGuid"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public string GetObjectNumberWithPrefix(string ortyGuid, string prefix)
        {
            EMDObjectRelationType orty = (EMDObjectRelationType) this.GetObject<EMDObjectRelationType>(ortyGuid);
            if (orty != null)
            {
                 if (orty.Object1 == prefix)
                    return "Object1";
                else if (orty.Object2 == prefix)
                    return "Object2";
                else
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK,"cannot find prefix " + prefix + " for orty:" + ortyGuid);
                }
                    
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK,"cannot find orty:" + ortyGuid);
            }
        }

        public string GetGuidForRelationName(string relationTypeName)
        {
            string g = "";
            var xx = (List<IEMDObject<EMDObjectRelationType>>) 
                this.GetObjects<EMDObjectRelationType, ObjectRelationType>("RelationName = \"" + relationTypeName + "\"");

            if (xx.Count > 0)
            {
                EMDObjectRelationType t = (EMDObjectRelationType) xx[0];
                g = t.Guid;
            }
            return g;
        }
    }
}

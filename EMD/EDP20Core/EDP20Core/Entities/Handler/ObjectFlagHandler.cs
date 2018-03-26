using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class ObjectFlagHandler : EMDObjectHandler
    {
        public ObjectFlagHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public ObjectFlagHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public ObjectFlagHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public ObjectFlagHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new ObjectFlag().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            ObjectFlag objFlag = (ObjectFlag) dbObject;
            EMDObjectFlag emdObject = new EMDObjectFlag(objFlag.Guid, objFlag.Created, objFlag.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>) emdObject;
        }

        public bool HasFlagByGuid(string guid, EnumObjectFlagType flagType)
        {

            List<IEMDObject<EMDObjectFlag>> flagList = GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid = \"" + guid + "\" &&  FlagType = \"" + flagType.ToString() + "\"", null);

            if (flagList.Count > 0)
                return true;
            else
                return false;
        }
    }
}

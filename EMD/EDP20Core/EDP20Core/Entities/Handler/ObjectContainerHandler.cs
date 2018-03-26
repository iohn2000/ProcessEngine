using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using System;
using System.Reflection;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class ObjectContainerHandler : EMDObjectHandler
    {
        public ObjectContainerHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public ObjectContainerHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public ObjectContainerHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public ObjectContainerHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new ObjectContainer().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            ObjectContainer obco = (ObjectContainer)dbObject;
            EMDObjectContainer emdObject = new EMDObjectContainer(obco.Guid, obco.Created, obco.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }       
    }
}

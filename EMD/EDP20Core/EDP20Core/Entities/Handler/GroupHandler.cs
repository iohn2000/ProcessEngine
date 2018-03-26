using System;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class GroupHandler : EMDObjectHandler
    {
        public GroupHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public GroupHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public GroupHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public GroupHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new Group().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            Group grou = (Group)dbObject;
            EMDGroup emdObject = new EMDGroup(grou.Guid, grou.Created, grou.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }
    }
}

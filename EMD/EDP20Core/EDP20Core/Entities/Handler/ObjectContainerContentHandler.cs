using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kapsch.IS.Util.ReflectionHelper;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class ObjectContainerContentHandler : EMDObjectHandler
    {

        public ObjectContainerContentHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public ObjectContainerContentHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public ObjectContainerContentHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public ObjectContainerContentHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new ObjectContainerContent().GetType();
        }


        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            ObjectContainerContent obco = (ObjectContainerContent) dbObject;
            EMDObjectContainerContent emdObject = new EMDObjectContainerContent(obco.Guid, obco.Created, obco.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>) emdObject;
        }

        [Obsolete("doesn not consider valid from and to !! dont use unless you know what you are doing!!", error: true)]
        public List<String> GetOjectContainerContentGUIDsByOCGuid(string oc_Guid)
        {
            List<String> lstOCCGuids = new List<String>();

            IQueryable<String> query = (from item in transaction.dbContext.ObjectContainerContent where item.OC_Guid == oc_Guid select item.ObjectGuid);
            lstOCCGuids = query.ToList();

            return lstOCCGuids;
        }

        public List<string> GetOBCCGuidsByOBCOGuid(string obcoGuid)
        {
            List<string> obccGuids = new List<string>();
            var r = this.GetObjects<EMDObjectContainerContent, ObjectContainerContent>("OC_Guid = \"" + obcoGuid + "\"");
            r.ForEach(item => obccGuids.Add(item.Guid));
            return obccGuids;
        }
    }
}

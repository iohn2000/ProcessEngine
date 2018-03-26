using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class DatabaseVersionHandler : EMDObjectHandler
    {
        public DatabaseVersionHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public DatabaseVersionHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public override Type GetDBObjectType()
        {
            return new DatabaseVersion().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            DatabaseVersion dbve = (DatabaseVersion)dbObject;
            EMDDatabaseVersion emdObject = new EMDDatabaseVersion(dbve.Guid, dbve.Created, dbve.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }
    }
}

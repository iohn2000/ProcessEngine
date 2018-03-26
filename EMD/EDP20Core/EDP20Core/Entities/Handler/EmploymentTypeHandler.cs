using System;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EmploymentTypeHandler : EMDObjectHandler
    {
        public EmploymentTypeHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public EmploymentTypeHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public EmploymentTypeHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public EmploymentTypeHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new EmploymentType().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            EmploymentType emty = (EmploymentType)dbObject;
            EMDEmploymentType emdObject = new EMDEmploymentType(emty.Guid, emty.Created, emty.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }


    }
}

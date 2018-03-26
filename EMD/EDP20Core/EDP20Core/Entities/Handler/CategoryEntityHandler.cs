using System;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class CategoryEntityHandler : EMDObjectHandler
    {
        public CategoryEntityHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public CategoryEntityHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public CategoryEntityHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public CategoryEntityHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }


        public override Type GetDBObjectType()
        {
            return new CategoryEntity().GetType();
        }
        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            CategoryEntity categoryEntity = (CategoryEntity)dbObject;
            EMDCategoryEntity emdObject = new EMDCategoryEntity(categoryEntity.Guid, categoryEntity.Created, categoryEntity.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }
    }
}

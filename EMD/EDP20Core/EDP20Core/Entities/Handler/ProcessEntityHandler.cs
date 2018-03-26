using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    internal class ProcessEntityHandler : EMDObjectHandler
    {
        /// <inheritdoc/>
        public ProcessEntityHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        /// <inheritdoc/>
        public ProcessEntityHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        /// <inheritdoc/>
        public ProcessEntityHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        /// <inheritdoc/>
        public ProcessEntityHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        /// <inheritdoc/>
        public override Type GetDBObjectType()
        {
            return new ProcessEntity().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            ProcessEntity user = (ProcessEntity)dbObject;
            EMDProcessEntity emdObject = new EMDProcessEntity(user.Guid, user.Created, user.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }
    }
}

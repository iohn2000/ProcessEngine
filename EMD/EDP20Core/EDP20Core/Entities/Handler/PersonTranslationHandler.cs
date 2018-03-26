using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using System;
using System.Reflection;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class PersonTranslationHandler : EMDObjectHandler
    {
        public PersonTranslationHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public PersonTranslationHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public PersonTranslationHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public PersonTranslationHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new PersonTranslation().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            PersonTranslation personTranslation = (PersonTranslation) dbObject;
            EMDPersonTranslation emdObject = new EMDPersonTranslation(personTranslation.Guid, personTranslation.Created, personTranslation.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>) emdObject;
        }
    }
}

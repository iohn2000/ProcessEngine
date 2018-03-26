using System;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using System.Collections.Generic;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class ContactTypeHandler : EMDObjectHandler
    {
        public ContactTypeHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public ContactTypeHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }
        public ContactTypeHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public ContactTypeHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }


        public override Type GetDBObjectType()
        {
            return new ContactType().GetType();
        }
        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            ContactType coty = (ContactType)dbObject;
            EMDContactType emdObject = new EMDContactType(coty.Guid, coty.Created, coty.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        public const int EMAIL = 1;
        public const int PHONE = 2;
        public const int FAX = 3;
        public const int MOBILE = 4;
        public const int EFAX = 5;
        public const int DIRECTDIAL = 6;
        public const int ROOM = 7;
        public const int AGENTKATCE = 8;
        public const int MONITORKATCE = 9;
        public const int ISMDKATCE = 10;
        public const int SERVERKATCE = 11;
        public const int JOBTITLE = 12;
        public const int HIERARCHY = 13;
        public const int DIRECTDIAL2 = 14;
        public const int DIRECTDIALKATCE = 15;
        public const int LOCATIONPHONE = 16;
        public const int LOCATIONFAX = 17;
        public const int LOCATIONEMAIL = 18;
        public const int DIRECTEFAX = 19;


        public string GetGuidFromId(int ct_id)
        {
            List<IEMDObject<EMDContactType>> listContacttypes = this.GetObjects<EMDContactType, ContactType>("CT_ID = " + ct_id.ToString());
            if (listContacttypes.Count == 0)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Contacttype for CT_ID=" + ct_id.ToString() + " not found!");
            }
            else if(listContacttypes.Count == 1)
            {
                return listContacttypes[0].Guid;
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than on Contact-Types found for CT_ID=" + ct_id.ToString() );
            }
        }
    }
}

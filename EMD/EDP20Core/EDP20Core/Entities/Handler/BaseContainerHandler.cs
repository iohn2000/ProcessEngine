using System;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using System.Collections.Generic;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class BaseContainerHandler : EMDObjectHandler
    {
        public BaseContainerHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public BaseContainerHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public BaseContainerHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public BaseContainerHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new BaseContainer().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            BaseContainer baco = (BaseContainer) dbObject;
            EMDBaseContainer emdObject = new EMDBaseContainer(baco.Guid, baco.Created, baco.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>) emdObject;
        }

        public List<string> GetOBCCGuidsByBACOGuid(string bacoGuid)
        {
            List<string> obccGuids = new List<string>();
            EMDBaseContainer baco = (EMDBaseContainer) this.GetObject<EMDBaseContainer>(bacoGuid);

            var r = new ObjectContainerContentHandler().GetObjects<EMDObjectContainerContent, ObjectContainerContent>("OC_Guid like \"" + baco.OBCOGuid + "\"");
            r.ForEach(item => obccGuids.Add(item.Guid));
            return obccGuids;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageGuid"></param>
        /// <param name="bacoPrefix">ENTE or LOCA</param>
        /// <returns></returns>
        public bool BaseContainerExitsForPackage(string packageGuid, string bacoPrefix)
        {
            try
            {
                string whereC = string.Format("BACOPrefix = \"{0}\" and OBCOGuid = \"{1}\"", bacoPrefix, packageGuid);
                var baco = this.GetObjects<EMDBaseContainer, BaseContainer>(whereC);
                if (baco != null && baco.Count > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("error in BaseContainerExitsForPackage. packageGuid={0} , bacoPrefix={1} ", packageGuid, bacoPrefix);

                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY, ex);
            }
        }

        /// <summary>
        /// throws baseexception, does logging
        /// </summary>
        /// <param name="guid"></param>
        public void DeleteByGuid(string guid)
        {
            try
            {
                var obj = this.GetObject<EMDBaseContainer>(guid);
                this.DeleteObject((EMDBaseContainer) obj);
            }
            catch (Exception ex)
            {
                string errmsg = string.Format("error trying to delete base container, guid={0}", guid);

                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY, errmsg, ex);
            }
        }

        /// <summary>
        /// throws base exception and logs
        /// </summary>
        /// <param name="obcoGuid"></param>
        /// <param name="bacoPrefix"></param>
        /// <returns></returns>
        public EMDBaseContainer GetBaseContainer(string obcoGuid, string bacoPrefix)
        {
            try
            {
                string whereC = string.Format("BACOPrefix = \"{0}\" and OBCOGuid = \"{1}\"", bacoPrefix, obcoGuid);
                var bacos = this.GetObjects<EMDBaseContainer, BaseContainer>(whereC);
                if (bacos != null)
                {
                    return (EMDBaseContainer) bacos[0];
                }
                else
                {
                    string errmsg = string.Format("could not find base container for obcoGuid: '{0}' and bacoPrefix: '{1}'", obcoGuid, bacoPrefix);
                    throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY, errmsg);
                }

            }
            catch (Exception ex)
            {
                string errMsg = string.Format("error in BaseContainerExitsForPackage. obcoGuid={0} , bacoPrefix={1} ", obcoGuid, bacoPrefix);

                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY, ex);
            }
        }
    }
}

using System;
using System.Reflection;
using System.Collections.Generic;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EnterpriseLocationHandler : EMDObjectHandler
    {
        public EnterpriseLocationHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public EnterpriseLocationHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }
        public EnterpriseLocationHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public EnterpriseLocationHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }


        public override Type GetDBObjectType()
        {
            return new EnterpriseLocation().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            EnterpriseLocation enlo = (EnterpriseLocation) dbObject;
            EMDEnterpriseLocation emdObject = new EMDEnterpriseLocation(enlo.Guid, enlo.Created, enlo.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>) emdObject;
        }

        /// <summary>
        /// returns enterpriselocation corresponding to the given set of enterprise and location. If it doesn't exist it is created newly.
        /// </summary>
        /// <param name="enteGuid"></param>
        /// <param name="locaGuid"></param>
        /// <returns></returns>
        public EMDEnterpriseLocation CheckForOrCreateEnLo(string enteGuid, string locaGuid, byte enloStatus = 10)
        {
            //TODO write unittest
            EMDEnterpriseLocation enlo;

            List<IEMDObject<EMDEnterpriseLocation>> enloList = this.GetObjects<EMDEnterpriseLocation, EnterpriseLocation>("E_Guid=\"" + enteGuid + "\"&L_Guid=\"" + locaGuid + "\"");
            if (enloList.Count > 1)
            {
                
                //TODO Datenbank enthält zu viele Elemente des gleichen Typs. reperaturlogik anstoßen!
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, String.Format("Found to many entries for this combination of Enterprise and Location: {0}, {1}", enteGuid, locaGuid));
            }
            else if (enloList.Count == 0)
            {
                //create a new enlo
                enlo = new EMDEnterpriseLocation();
                EMDEnterprise ente;
                EMDLocation loca;
                try
                {
                    ente = (EMDEnterprise) new EnterpriseHandler().GetObject<EMDEnterprise>(enteGuid);
                    loca = (EMDLocation) new LocationHandler().GetObject<EMDLocation>(locaGuid);

                    //TODO: Einen Eintrag erzeugen, und idealerweise Workflow starten der das Doing für die Erstellung der nötigen AD Gruppen veranlasst
                    enlo.DistList_int = "UNDEFINED";
                    enlo.DistList_ext = "UNDEFINED";

                    enlo.E_Guid = ente.Guid;
                    enlo.E_ID = ente.E_ID;
                    enlo.L_Guid = loca.Guid;
                    enlo.L_ID = loca.EL_ID;

                    enlo.Status = enloStatus;

                    enlo = (EMDEnterpriseLocation)CreateObject(enlo);
                }
                catch (Exception exc)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, String.Format("Error while creating enlo with one of the given objects: {0}, {1}", enteGuid, locaGuid), exc);
                }
            }
            else
            {
                enlo = (EMDEnterpriseLocation) enloList[0];
            }
            return enlo;
        }
        public EMDEnterpriseLocation CheckForOrCreateEnLo(string enteGuid, string locaGuid, CoreTransaction transaction, byte enloStatus = 10)
        {
            if (transaction == null)
                return CheckForOrCreateEnLo(enteGuid, locaGuid, enloStatus);

            //TODO write unittest
            EMDEnterpriseLocation enlo;

            List<IEMDObject<EMDEnterpriseLocation>> enloList = this.GetObjects<EMDEnterpriseLocation, EnterpriseLocation>("E_Guid=\"" + enteGuid + "\"&L_Guid=\"" + locaGuid + "\"");

            if (enloList.Count > 1)
            {
                
                //TODO Datenbank enthält zu viele Elemente des gleichen Typs. reperaturlogik anstoßen!
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, String.Format("Found to many entries for this combination of Enterprise and Location: {0}, {1}", enteGuid, locaGuid));
            }
            else if (enloList.Count == 0)
            {
                //create a new enlo
                enlo = new EMDEnterpriseLocation();
                EMDEnterprise ente;
                EMDLocation loca;
                try
                {
                    ente = (EMDEnterprise)new EnterpriseHandler(transaction).GetObject<EMDEnterprise>(enteGuid);
                    loca = (EMDLocation)new LocationHandler(transaction).GetObject<EMDLocation>(locaGuid);

                    //TODO: Einen Eintrag erzeugen, und idealerweise Workflow starten der das Doing für die Erstellung der nötigen AD Gruppen veranlasst
                    enlo.DistList_int = "UNDEFINED";
                    enlo.DistList_ext = "UNDEFINED";

                    enlo.E_Guid = ente.Guid;
                    enlo.E_ID = ente.E_ID;
                    enlo.L_Guid = loca.Guid;
                    enlo.L_ID = loca.EL_ID;

                    enlo.Status = enloStatus;

                    enlo = (EMDEnterpriseLocation)this.CreateObject(enlo);
                }
                catch (Exception exc)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, String.Format("Error while creating enlo with one of the given objects: {0}, {1}", enteGuid, locaGuid), exc);
                }
            }
            else
            {
                enlo = (EMDEnterpriseLocation)enloList[0];
            }
            return enlo;
        }

    }
}

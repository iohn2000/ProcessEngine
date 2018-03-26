using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Kapsch.IS.EDP.Core.DB;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class EnterpriseLocationManager
        : BaseManager
    {
        protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public EnterpriseLocationManager()
            : base()
        {
        }

        public EnterpriseLocationManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public EnterpriseLocationManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public EnterpriseLocationManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDEnterpriseLocation Get(string guid)
        {
            EnterpriseLocationHandler handler = new EnterpriseLocationHandler(this.Transaction);

            return (EMDEnterpriseLocation)handler.GetObject<EMDEnterpriseLocation>(guid);
        }

        public bool DoesEnterpriseLocationExist(string guid_ente, string guid_loca)
        {
            bool exists = false;

            EnterpriseLocationHandler handler = new EnterpriseLocationHandler();
            EMDEnterpriseLocation enterpriseLocation = Get(guid_ente, guid_loca);

            if (enterpriseLocation != null)
            {
                exists = true;
            }

            return exists;
        }

        public EMDEnterpriseLocation Get(string guid_ente, string guid_loca)
        {
            EMDEnterpriseLocation enterpriseLocation = null;

            EnterpriseLocationHandler handler = new EnterpriseLocationHandler();
            List<EMDEnterpriseLocation> enterpriseLocations = handler.GetObjects<EMDEnterpriseLocation, DB.EnterpriseLocation>(string.Format("E_Guid=\"{0}\" and L_Guid=\"{1}\"", guid_ente, guid_loca)).Cast<EMDEnterpriseLocation>().ToList();

            if (enterpriseLocations != null)
            {
                if (enterpriseLocations.Count == 1)
                {
                    enterpriseLocation = enterpriseLocations[0];
                }
                else if (enterpriseLocations.Count > 1)
                {
                    throw new System.Exception(string.Format("More than 1 enterprise locations exists for guid_ente:{0} guid_loc{1}", guid_ente, guid_loca));
                }
            }

            return enterpriseLocation;
        }

        public EMDEnterpriseLocation Delete(string guid)
        {
            EnterpriseLocationHandler handler = new EnterpriseLocationHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDEnterpriseLocation emdEnterpriseLocation = Get(guid);
            if (emdEnterpriseLocation != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDEnterpriseLocation)handler.DeleteObject<EMDEnterpriseLocation>(emdEnterpriseLocation);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The Enterprise-Location with guid: {0} was not found.", guid));
            }
        }

        /// <summary>
        /// create a new enlo with status ordere and start 'new enlow' workflow
        /// </summary>
        /// <param name="requestingPersonEmplGuid"></param>
        /// <param name="enteGuid"></param>
        /// <param name="locaGuid"></param>
        public EnloAddWorkflowMessage GetWorkflowVariablesForNewEnterpriseLocation(CoreTransaction transaction, string requestingPersonEmplGuid, string enteGuid, string locaGuid)
        {
            // create new enlo with status ordered
            EnterpriseLocationHandler enloH = new EnterpriseLocationHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDEnterpriseLocation enlo = enloH.CheckForOrCreateEnLo(enteGuid, locaGuid, EnterpriseLocationProcessStatus.STATUSITEM_ORDERED);

            // find a workflow for it
            ProcessMappingHandler prmpH = new ProcessMappingHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);

            EnloAddWorkflowMessage enloWorkflowVars = new EnloAddWorkflowMessage();
            enloWorkflowVars = prmpH.GetWorkflowMapping<EnloAddWorkflowMessage>(null, null);

            logger.Info(string.Format("Mapped process to Workflow with Definition-ID {0}", enloWorkflowVars.WFDefID));

            enloWorkflowVars.RequestingPersonEmploymentGuid = requestingPersonEmplGuid;
            enloWorkflowVars.EnteGuid = enteGuid;
            enloWorkflowVars.LocaGuid = locaGuid;
            enloWorkflowVars.EnloGuid = enlo.Guid;


            return enloWorkflowVars;
        }

        public List<EMDEnterpriseLocation> GetEnterpriseLocationsForEnterprise(string enteGuid)
        {
            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler(this.Guid_ModifiedBy, this.ModifyComment);
            List<EMDEnterpriseLocation> enlos = enloHandler.GetObjects<EMDEnterpriseLocation, EnterpriseLocation>("E_Guid = \"" + enteGuid +  "\"").Cast<EMDEnterpriseLocation>().ToList();

            return enlos;
        }

        public List<string> GetEnterpriseLocationGuidsForEnterprise(string enteGuid)
        {
            List<EMDEnterpriseLocation> enlos = this.GetEnterpriseLocationsForEnterprise(enteGuid);
            List<string> enloGuids = enlos.Select(item => item.Guid).ToList();
            return enloGuids;
        }
    }
}

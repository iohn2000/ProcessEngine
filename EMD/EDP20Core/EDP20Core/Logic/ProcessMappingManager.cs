using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class ProcessMappingManager
        : BaseManager
    {
        #region Constructors

        public ProcessMappingManager()
            : base()
        {
        }

        public ProcessMappingManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public ProcessMappingManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public ProcessMappingManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDProcessMapping Get(string guid)
        {
            ProcessMappingHandler processMappingHandler = new ProcessMappingHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            return (EMDProcessMapping) processMappingHandler.GetObject<EMDProcessMapping>(guid);
        }

        public EMDProcessMapping Delete(string guid)
        {
            ProcessMappingHandler processMappingHandler = new ProcessMappingHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            EMDProcessMapping emdProcessMapping = Get(guid);
            if (emdProcessMapping != null)
            {
                Hashtable hashTable = processMappingHandler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDProcessMapping) processMappingHandler.DeleteObject<EMDProcessMapping>(emdProcessMapping);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The user with guid: {0} was not found.", guid));
            }
        }

        /// <summary>
        /// create a new mapping or update existing mapping between EMD entitiy and workflow
        /// </summary>
        /// <param name="mappingItem"></param>
        /// <returns>new or updated mapping item or null if new item could not be created</returns>
        public EMDProcessMapping CreateMapping(EMDProcessMapping mappingItem)
        {
            EMDProcessMapping result = null;

            ProcessMappingHandler processMappingHandler = new ProcessMappingHandler();

            if (!string.IsNullOrEmpty(mappingItem.Guid))
            {
                // update existing mapping
                processMappingHandler.UpdateObject(mappingItem);
                result = mappingItem;
            }
            else
            {
                // create new one
                // check if a combination TypePrefix, EntitiyGuid, Method doesnt exists yet
                string query = "TypePrefix = \"{0}\" AND EntityGuid = \"{1}\" AND Method = \"{2}\"";
                query = string.Format(query, mappingItem.TypePrefix, mappingItem.EntityGuid, mappingItem.Method);
                var combi = processMappingHandler.GetObjects<EMDProcessMapping, ProcessMapping>(query);
                if (combi == null || combi.Count() == 0)
                {
                    result = (EMDProcessMapping) processMappingHandler.CreateObject<EMDProcessMapping>(mappingItem);
                }
                else
                {
                    //dont create a duplicate
                    result = null;
                }
            }

            return result;
        }

        /// <summary>
        /// throw an exception
        /// </summary>
        /// <param name="workflowID"></param>
        public void CheckIsWorkflowMapped(string workflowID)
        {
            ProcessMappingHandler prmaH = new ProcessMappingHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            var mappings = prmaH.GetObjects<EMDProcessMapping, ProcessMapping>("WorkflowID = \"" + workflowID + "\"");

            if (mappings != null && mappings.Count > 0)
            {
                // build a meaningful msg
                string errMsg = "";
                foreach (var pm in mappings)
                {
                    StringWriter sw = new StringWriter();
                    ObjectDumper.Write(((EMDProcessMapping) pm), 5, sw);
                    errMsg += sw.ToString() + Environment.NewLine;
                }
                throw new Exception("Cannot delete worflow with id='" + workflowID + "' because it is still mapped to :" +
                    Environment.NewLine +
                    errMsg);
            }
        }
    }
}

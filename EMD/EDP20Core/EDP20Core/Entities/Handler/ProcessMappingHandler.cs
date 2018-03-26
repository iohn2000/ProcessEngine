using System;
using System.Collections.Generic;
using System.Reflection;
using Kapsch.IS.Util.Serialiser;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.Util.ErrorHandling;

using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class ProcessMappingHandler : EMDObjectHandler
    {
        public ProcessMappingHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public ProcessMappingHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public ProcessMappingHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public ProcessMappingHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new ProcessMapping().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            ProcessMapping p = (ProcessMapping)dbObject;
            EMDProcessMapping emdObject = new EMDProcessMapping(p.Guid, p.Created.Value, p.Modified);
            Util.ReflectionHelper.ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">as class derived from BaseDataHelper</typeparam>
        /// <param name="templateGuid">guid of template entity e.g. EQDE. In case of oboarding can be null.</param>
        /// <param name="c">FilterRules that have to match for ProcessMapping Item, if filterrule is null everybody is get the same workflow</param>
        /// <returns>Returns a class with all variables necessary for the resulting workflow ID</returns>
        public T GetWorkflowMapping<T>(string templateGuid, FilterCriteria c) where T : WorkflowBaseMessage, new()
        {
            T derivedDataHelper = new T();

            int countMatches = 0;
            string where;

            if (string.IsNullOrEmpty(templateGuid))
                where = string.Format("TypePrefix = \"{0}\" && Method = \"{1}\" ", derivedDataHelper.Prefix, derivedDataHelper.Method);
            else
                where = string.Format("TypePrefix = \"{0}\" && Method = \"{1}\" && EntityGuid = \"{2}\" ", derivedDataHelper.Prefix, derivedDataHelper.Method, templateGuid);

            var tmpList = (List<IEMDObject<EMDProcessMapping>>)GetObjects<EMDProcessMapping, ProcessMapping>(where);

            if (tmpList.Count == 0)
            {
                string msg = string.Format("ProcessMapping query = '{0}' doesnt return any matches", where);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }

            // 
            // special case no filter rule
            //
            if (tmpList.Count == 1 && c == null)
            {
                EMDProcessMapping procMapItem = (EMDProcessMapping)tmpList[0];
                derivedDataHelper.WFDefID = procMapItem.WorkflowID;
                return derivedDataHelper;
            }
            else if (tmpList.Count != 1 && c == null)
            {
                string msg = string.Format("Cannot map request to Workflow. More than one result matches. ProcessMapping query = '{0}' FilterCriteria = null",
                                    where);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }

            // go through list of mappings and check if FilterCritera match 
            // there can (should) only be one
            foreach (var item in tmpList)
            {
                EMDProcessMapping procMapItem = (EMDProcessMapping)item;
                FilterManager filtMgr = new FilterManager(procMapItem.Guid);
                if (filtMgr.CheckRule(procMapItem.Guid, c)) countMatches++;
            }

            if (countMatches == 0)
            {
                string msg = string.Format("No FilterRule matches mapping request. ProcessMapping query = '{0}' FilterCriteria : {1}", where, c.ToString());
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }
            if (countMatches == 1)
            {
                //build WorkflowMsgQData
                EMDProcessMapping procMapItem = (EMDProcessMapping)tmpList[0];
                derivedDataHelper.WFDefID = procMapItem.WorkflowID;
                return derivedDataHelper;
            }
            else
            {
                string msg = string.Format("Cannot map request to Workflow. More than one result matches. ProcessMapping query = '{0}' FilterCriteria : {1}",
                    where, c.ToString());
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }

        }

    }

    public enum enumMappingReturnStatus
    {
        OK,
        ErrorNoMappingFound,
        InfoNoFilterRulesMatchNotAllowed,
        ErrorMoreThanOneMatch
    }
}
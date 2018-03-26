using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity
{
    public class WfHelper
    {
        protected internal IEDPLogger logger;
        private string WorkflowLoggingContext = "";

        public WfHelper()
        {
            this.logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public WfHelper(string workflowLoggingContext)
        {
            this.logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            this.WorkflowLoggingContext = workflowLoggingContext;
        }

        [Obsolete("not needed anymore, this call is in CORE now.", true)]
        public static WorkflowMessageData GetWorkflowMessageData(WorkflowBaseMessage baseDataHelper)
        {
            WorkflowMessageData workflowMessageDataItem = new WorkflowMessageData();
            workflowMessageDataItem.WorkflowDefinitionID = baseDataHelper.WFDefID;

            Type t = baseDataHelper.GetType();

            workflowMessageDataItem.WorkflowVariables = new List<WorkflowMessageVariable>();//new WorkflowMessageVariable[t.GetProperties().Length - 1];


            for (int i = 0; i < t.GetProperties().Length; i++)
            {
                string propValue;
                object propObject = null;
                //do only read if property is public!
                if (t.GetProperties()[i].CanRead)
                {
                    propObject = t.GetProperties()[i].GetValue(baseDataHelper);
                }


                if (t.GetProperties()[i].PropertyType == new XElement("a").GetType())
                {
                    propValue = baseDataHelper.SerializeXElement((XElement) propObject);
                }
                else
                {

                    if (propObject != null)
                    {
                        propValue = propObject.ToString();
                    }
                    else
                    {
                        propValue = null;
                    }
                }

                if (propValue != null)
                {
                    WorkflowMessageVariable wfvar = new WorkflowMessageVariable()
                    {
                        VarName = t.GetProperties()[i].Name,
                        VarValue = propValue
                    };
                    workflowMessageDataItem.WorkflowVariables.Add(wfvar);
                }
            }

            return workflowMessageDataItem;

        }

        public List<Tuple<string, string, string>> BuildRecipientListForMailing(string recipientCode, string effectedPersonEmplGuid)
        {
            EntityQuery entityQuery = new EntityQuery();
            Type propType;
            List<Tuple<string, string, string>> recipientList = new List<Tuple<string, string, string>>(); // item1 = mail, 2=firname, 3=familyname
            string mm, fi, fa;
            string[] separator = { ";" };


            List<string> allRecipientsCodes = recipientCode.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList();

            StringWriter sw = new StringWriter();
            ObjectDumper.Write(allRecipientsCodes, 2, sw);
            logger.Debug(this.WorkflowLoggingContext + "BuildRecipientListForMailing: allRecipientsCodes : " + sw.ToString());


            var listOfRecipients = new TaskItemManager().FindTaskApproverForEffectedPerson(allRecipientsCodes, effectedPersonEmplGuid);
            sw = new StringWriter();
            ObjectDumper.Write(listOfRecipients, 4, sw);

            foreach (var recp in listOfRecipients)
            {
                if (recp.Item1 != null)
                {
                    mm = entityQuery.Query("MainMail@@" + recp.Item2, out propType).ToString();
                    fi = entityQuery.Query("FirstName@@" + recp.Item2, out propType).ToString();
                    fa = entityQuery.Query("FamilyName@@" + recp.Item2, out propType).ToString();
                }
                else // special case where there is no empl or pers, just an email
                {
                    mm = recp.Item2;
                    fi = "";
                    fa = "";
                }
                Tuple<string, string, string> oneItem = new Tuple<string, string, string>(mm, fi, fa);
                recipientList.Add(oneItem);
            }
            sw = new StringWriter();
            ObjectDumper.Write(recipientList, 5, sw);
            logger.Debug(this.WorkflowLoggingContext + "BuildRecipientListForMailing: after @@ queries  empfaengerList : " + sw.ToString());

            return recipientList;
        }

        public Dictionary<string, object> AddAllNullPunksVarsToRenderDictionary(EngineContext engineContext, Dictionary<string, object> renderDictionary)
        {
            var nullPunkts = (List<Variable>) engineContext.WorkflowModel.GetPunktVariables("[starts-with(@name,'0.')]");
            foreach (var item in nullPunkts)
            {
                Variable tmp = engineContext.GetWorkflowVariable(item.Name);
                if (renderDictionary.ContainsKey("item.Value.Name") == false)
                    renderDictionary.Add(item.Name, item.VarValue);
            }

            return renderDictionary;
        }

        /// <summary>
        /// add or update the renderDictioniary for document template rendering (e.g. emails)
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public Dictionary<string, string> AddOrUpdateDictionary(Dictionary<string, string> dic, string key, string val)
        {
            if (dic.ContainsKey(key))
                dic[key] = val;
            else
                dic.Add(key, val);
            return dic;
        }
    }
}

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Entities.EquipmentDef;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.TemplateEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.WFActivity.TaskDecision
{
    public class TaskHelper
    {
        /// <summary>
        /// <![CDATA[
        ///<!-- config that defines what to do on this task. at the moment its a comma separated list with options 
        ///<?xml version="1.0" encoding="utf-8"?>
        ///<Taskfields>
        ///  <Field id="taskphonenumber" type="textbox" regex="" name="Mobile Number" description="Enter a phone number"></Field>
        ///  
        ///  <Field id="taskphonedropdown" type="dropdown" name="Phone model" description="Select a phone model">
        ///      <option value="sam">Samsung</option>
        ///      <option value="lg">LG</option>
        ///      <option value="ip">IPhone</option>
        ///  </Field>
        ///  
        ///  <Field id="taskdate" name="Task Date" type="date"></Field>
        ///  
        ///  <Field id="taskdatetime" name="Task Datetime" type="datetime"></Field>
        ///  
        ///  <Field id="taskphonemultiselect" name="Phone model multi" type="multiselect">
        ///    <option value="sam">Samsung</option>
        ///    <option value="lg">LG</option>
        ///    <option value="ip">IPhone</option>
        ///  </Field>
        ///</Taskfields>
        ///-->
        /// ]]>
        /// </summary>
        /// <param name="decisionOptions"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static string AddDynamicFieldsToDecisionOptions(string decisionOptions, List<DynamicField> dynamicFields)
        {
            string CONST_DYNPREFIX = DynamicField.CONST_DYNPREFIX;
            XDocument xdoc = XDocument.Parse(decisionOptions);
            XElement taskFields = xdoc.XPathSelectElement("/Taskfields");

            foreach (DynamicField dField in dynamicFields)
            {
                XElement newField = new XElement("Field");
                XAttribute idAttr = new XAttribute("id", CONST_DYNPREFIX + dField.Identifier.ToString());
                XAttribute nameAttr = new XAttribute("name", dField.Name.ToString());
                XAttribute descAttr = new XAttribute("description", dField.Name.ToString());
                XAttribute typeAttr = null;
                switch (dField.Type)
                {
                    case EnumDynamicFieldType.Decimal:
                    case EnumDynamicFieldType.Int:
                    case EnumDynamicFieldType.String:
                        typeAttr = new XAttribute("type", "textbox");
                        break;
                    case EnumDynamicFieldType.Boolean:
                        typeAttr = new XAttribute("type", "dropdown");
                        break;
                    default:
                        break;
                }
                newField.Add(idAttr);
                newField.Add(nameAttr);
                newField.Add(descAttr);
                newField.Add(typeAttr);
                if (dField.Type == EnumDynamicFieldType.Boolean)
                {
                    XElement boolDrop = new XElement("option");
                    boolDrop.SetValue("true"); boolDrop.Add(new XAttribute("value", "true"));
                    newField.Add(boolDrop);
                    boolDrop = new XElement("option");
                    boolDrop.SetValue("false"); boolDrop.Add(new XAttribute("value", "false"));
                    newField.Add(boolDrop);
                }
                taskFields.Add(newField);
            }
            return taskFields.ToString(SaveOptions.None);
        }

        /// <summary>
        /// add all dynamic fields as 0. variables
        ///  0.EqDyn__V_Identifier ... value
        ///  0.EqDyn__N_Identifier ... name
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="dynFields"></param>
        /// <param name="renderDic"></param>
        /// <returns></returns>
        public static void AddEqdeDynVarsToWoinAsNullPunkt(EngineContext engineContext, AsyncTaskRequestResult taskResult)
        {
            // step one foreach item create a variable mit key,value
            string currentNr = engineContext.CurrenActivity.Nr;

            XDocument x = XDocument.Parse(taskResult.ReturnValue);
            var items = x.XPathSelectElements("/TaskGuiResponse/Items/Item");
            foreach (var i in items)
            {
                string name = i.Attribute("key").Value;
                string val = i.Value;
                engineContext.WorkflowModel.UpdateWorkflowVariableInXmlModel("0." + name, val, EnumVariablesDataType.stringType, EnumVariableDirection.both);
            }
            //foreach (DynamicField df in dynFields)
            //{
            //    engineContext.WorkflowModel.UpdateWorkflowVariableInXmlModel("0.EqDyn_N_" + df.Identifier.ToString(), df.Identifier.ToString(),
            //        EnumVariablesDataType.stringType, EnumVariableDirection.both);
            //    engineContext.WorkflowModel.UpdateWorkflowVariableInXmlModel("0.EqDyn_V" + df.Identifier.ToString() + df.Identifier, "",
            //        EnumVariablesDataType.stringType, EnumVariableDirection.both);
            //}
        }

        public static void SendTaskMail(BaseEDPActivity theBase, EngineContext engineContext, Dictionary<string, string> renderDictionary, List<Tuple<string, string>> approverEMPLGuids,
            string bodyTemplate, string taskTitle, bool htmlBody)
        {
            IEDPLogger logger;
            logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            WorkflowMailer emailer = new WorkflowMailer(theBase.GetEmailSubjectPrefix());
            ITemplateEngine renderer = new NustacheRenderer();
            PersonHandler persH = new PersonHandler();

            string demoRecipient = theBase.GetSettingWithFallbackToAppConfig(engineContext, "demoRecipient", "DefaultDemoModeRecipient", "KIBSI-EDP-admin@kapsch.net");
            if (theBase.isDemoModeOn(engineContext))
                logger.Debug(theBase.getWorkflowLoggingContext(engineContext) + " : DEMO Mode is ON. Recipient is now : " + demoRecipient);

            foreach (var item in approverEMPLGuids)
            {
                EMDPerson p = (EMDPerson) persH.GetObject<EMDPerson>(item.Item2);

                if (renderDictionary.ContainsKey("RecipientMainMail"))
                    renderDictionary["RecipientMainMail"] = p.MainMail;
                else
                    renderDictionary.Add("RecipientMainMail", p.MainMail);

                if (renderDictionary.ContainsKey("RecipientFirstname"))
                    renderDictionary["RecipientFirstname"] = p.FirstName;
                else
                    renderDictionary.Add("RecipientFirstname", p.FirstName);

                if (renderDictionary.ContainsKey("RecipientSurname"))
                    renderDictionary["RecipientSurname"] = p.FamilyName;
                else
                    renderDictionary.Add("RecipientSurname", p.FamilyName);

                DatabaseAccess db = new DatabaseAccess();
                DocumentTemplate docTemplate = db.GetDocumentTemplateByName(bodyTemplate);
                string renderedContent = renderer.RenderTemplateFromString(docTemplate.TMPL_Content, renderDictionary);

                string rmail = theBase.isDemoModeOn(engineContext) ? demoRecipient : p.MainMail;

                emailer.SendEmail("KIBSI-EDP-NoReply@kapsch.net", new List<string>() { rmail }, taskTitle, renderedContent, htmlBody);

                string logLine = string.Format("{0} : Sent Task Email : To={1} Subject={2}", theBase.getWorkflowLoggingContext(engineContext), rmail, taskTitle);
                logger.Debug(logLine);
            }
        }
    }
}

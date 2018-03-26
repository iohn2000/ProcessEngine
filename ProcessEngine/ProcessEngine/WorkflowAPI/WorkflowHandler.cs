using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.Serialiser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.ProcessEngine
{
    //TODO: Warum gibt es diese Klasse? => sollte diese eine Methode nicht einfach static in ProcessEngine sein, da es keine andere erlaubte Variante gibt diese Methode eine ProcessInstance zu erzeugen?

    /// <summary>
    /// High Level Functions for Workflow Management
    /// </summary>
    public class WorkflowHandler
    {
        private IEDPLogger logger;

        /// <summary>
        /// 
        /// </summary>
        public WorkflowHandler()
        {
            this.logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        /// create a new workflow instance 
        /// </summary>
        /// <param name="msgData"></param>
        /// <returns></returns>
        public ProcessInstance CreateNewWorkflowInstance(WorkflowMessageData msgData)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            logger.Debug("SWWH0: Start WorkflowHandler.CreateNewWorkflowInstance: " + sw.ElapsedMilliseconds);

            string newInstanceID = "";

            //Create ProcessDefinition
            ProcessDefinition workflowDefinition = new ProcessDefinition();

            //Create ProcessInstance from ProcessDefinition
            newInstanceID = Engine.Prefix_WorkflowInstance + "_" + Guid.NewGuid().ToString("N");
            logger.Debug("New WF InstanceID-Name generated : " + newInstanceID);

            ProcessInstance inst = workflowDefinition.CreateWorkflowInstance(msgData.WorkflowDefinitionID, newInstanceID);
            logger.Debug("SWWH1: After CreateWorkflowInstance WorkflowHandler.CreateNewWorkflowInstance: " + sw.ElapsedMilliseconds);

            string wfVarsStr;
            try
            {
                wfVarsStr = XmlSerialiserHelper.SerialiseIntoXmlString(msgData.WorkflowVariables);
                logger.Debug("SWWH2: After SerialiseIntoXMLString WorkflowHandler.CreateNewWorkflowInstance: " + sw.ElapsedMilliseconds); 
            }
            catch (Exception ex)
            {
                wfVarsStr = "";
            }

            //Execute instance (create EngineAlert)
            inst.Execute(wfVarsStr);
            logger.Debug("SWWH3: After Execute() WorkflowHandler.CreateNewWorkflowInstance: " + sw.ElapsedMilliseconds);

            logger.Debug(string.Format("Executed Workflow {0} successfully.", newInstanceID));

            return inst;
        }
               
    }
}

using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   The class ProcessInstance represents an instance of a workflow definition.
    ///              </summary>
    ///
    /// <remarks>   Fleckj, 18.03.2015. </remarks>
    #endregion
    public class ProcessInstance
    {
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DatabaseAccess databaseLayer;

        public string InstanceID { get; set; }

        public WorkflowModel WorkflowModel
        {
            get
            {
                WorkflowModel wfModel = new WorkflowModel();
                wfModel.LoadModelXml(this.GetInstanceXml());
                return wfModel;
            }
            //set { wfModel = value; }
        }


        public ProcessInstance(string instanceID)
        {
            if (!String.IsNullOrWhiteSpace(instanceID))
            {
                this.InstanceID = instanceID;
            }
            else
            {
                logger.Warn("The instanceID is null or empty.");
            }
            this.databaseLayer = new DatabaseAccess();
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Executes function is used to start an instance of a workflow.
        ///             A new entry into the table engineAlerts is created. This way
        ///             on the next heart beat the processing is started. </summary>
        ///
        /// <remarks>   Fleckj, 18.03.2015. </remarks>
        ///
        /// <param name="inputVarSerialised">       Any initial parameters you want to pass to the workflow instance.
        ///                                     Use a Hashtable as type. </param>
        /// <param name="storeInputParameter">  (Optional) true to store input parameter in the instances table in column [inputParams]. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        #endregion
        public bool Execute(string inputVarSerialised, bool storeInputParameter = false)
        {
            //inputVarSerialised["instanceID"] = this.InstanceID;

            this.databaseLayer.UpdateInstanceStatus(this.InstanceID, EnumWorkflowInstanceStatus.Executing);
            this.CreateEngineAlert("start", inputVarSerialised, "");

            return false;
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Creates engine alert. An engine alert is technically a row in the table engineAlerts
        ///             The WF engine checks this table for work to do. Every alert tells the WF engine
        ///             where to start processing a workflow. (e.g. at the start or from a specific activity (step).
        ///             
        ///             The WF engine always tries to process as many activities as possible up to an activity that
        ///             cannot be done whith interaction from outside (human or other software).
        ///             
        ///             For a workflow to continue after such an activity and outside (outside the WF engine) process
        ///             has to add another row into the engineAlerts table.
        ///             </summary>
        ///
        /// <remarks>   Fleckj, 18.03.2015. </remarks>
        ///
        /// <param name="startStep">        The start step. </param>
        /// <param name="inputParameter">   Any initial parameters you want to pass to the workflow
        ///                                 instance. Use a Hashtable as type. </param>
        /// <param name="returnValue">      The return value. </param>
        ///
        /// <returns>  The amount of rows effected in the database. This is an indicator if the function has worked. 
        ///            Any return value different from 1 means a problem has occured. </returns> 
        #endregion
        private void CreateEngineAlert(string startStep, string inputParameter, string returnValue)
        {
            this.databaseLayer.CreateEngineAlert(startStep, this.InstanceID, "", inputParameter, EnumAlertTypes.Normal);
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets the instance XML string from the database for the given instance (this.instanceID). </summary>
        ///
        /// <remarks>   Fleckj, 18.03.2015. </remarks>
        ///
        /// <returns>   String with xml. </returns>
        #endregion

        public string GetInstanceXml()
        {
            return this.databaseLayer.GetInstanceXml(this.InstanceID);
        }

        public void UpdateInstanceStatus(EnumWorkflowInstanceStatus newStatus)
        {
            this.databaseLayer.UpdateInstanceStatus(this.InstanceID, newStatus);
        }

        public void UpdateInstanceXmlContent(string instXml)
        {
            this.databaseLayer.UpdateInstanceXml(this.InstanceID, instXml);
        }

        public void UpdateInstanceXmlStatus(EnumWorkflowInstanceStatus newStatus, string waitingSteps)
        {
            // set status=executionstatus to newstatus, updatedOn=now, waitSteps
            this.databaseLayer.UpdateInstanceStatus(this.InstanceID, newStatus);
        }

        public string HashTableToXML(Hashtable inputVars)
        {
            XDocument xDocument = XDocument.Parse("<hashtable/>");
            XElement xElement = xDocument.XPathSelectElement("//hashtable");
            foreach (DictionaryEntry dictionaryEntry in inputVars)
            {
                XElement xElement1 = new XElement("item");
                xElement.Add(xElement1);
                xElement1.SetAttributeValue("key", dictionaryEntry.Key.ToString());
                xElement1.SetAttributeValue("value", dictionaryEntry.Value.ToString());
            }
            return xDocument.ToString();
        }


    }
}

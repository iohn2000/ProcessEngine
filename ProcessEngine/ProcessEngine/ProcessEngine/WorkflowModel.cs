using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.Serialiser;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   An object represenation of an instance or definition xpdl xml 
    ///             for a specific workflow. (--> clsUIModel)</summary>
    ///
    /// <remarks>   Fleckj, 06.02.2015. </remarks>
    #endregion
    public class WorkflowModel
    {
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string SETTING_DefaultErrorActivity = "ProcessEngine.EDP.DefaultErrorActivity.Name";

        /// <summary>
        /// the workflow in XDocument formats
        /// </summary>
        public XDocument wfModel;

        public string InstanceID
        {
            get
            {
                return this.GetWorkflowAttribteValue("id");
            }
            set
            {
                XElement statusElement = this.wfModel.XPathSelectElement("workflow");
                if (statusElement != null)
                {
                    statusElement.SetAttributeValue("id", value);
                }
            }
        }

        /// <summary>
        /// get the configured error activity INSTANCE
        /// returns emty if nothing is configured
        /// </summary>
        public string ErrorActivityInstance
        {
            get
            {
                string errAc = this.GetWorkflowAttribteValue("errorActivity");
                if (errAc == null)
                    errAc = "";
                return errAc;
            }

        }

        /// <summary>
        /// get the default error activity DEFINITION from app settings
        /// </summary>
        public string DefaultErrorActivityDefinition
        {
            get
            {
                //nothing configured use app.config value
                string defaultErrActivity;
                try
                {
                    defaultErrActivity = ConfigurationManager.AppSettings[SETTING_DefaultErrorActivity];
                }
                catch (ConfigurationErrorsException ex)
                {
                    defaultErrActivity = "";
                    logger.Error("Could not find application setting key. Key =" + SETTING_DefaultErrorActivity, ex);
                }
                return defaultErrActivity;
            }
        }


        /// <summary>
        /// set wf instance to demo mode
        /// </summary>
        public bool IsDemoMode
        {
            get
            {
                XElement statusElement = this.wfModel.XPathSelectElement("workflow");
                if (statusElement != null && statusElement.HasAttributes)
                {
                    XAttribute id = statusElement.Attribute("demoMode");
                    if (id != null)
                    {
                        bool de;
                        bool.TryParse(id.Value, out de);
                        return de;
                    }
                    else
                        return false;
                }
                return false;
            }
            set
            {
                XElement statusElement = this.wfModel.XPathSelectElement("workflow");
                if (statusElement != null)
                {
                    statusElement.SetAttributeValue("demoMode", value.ToString().ToLower());
                }
            }
        }

        /// <summary>
        /// get parent workflow if exists, otherwise null
        /// </summary>
        public string ParentWorkflow
        {
            get
            {
                DataLayer.DatabaseAccess db = new DataLayer.DatabaseAccess();
                string parent = db.GetParentWorkflow(this.InstanceID);
                return parent;
            }
            set { }
        }

        /// <summary>
        /// the name of the core datahelper that is linked to this workflow definition
        /// </summary>
        public string DataHelperName
        {
            get
            {
                return this.GetWorkflowAttribteValue("dataHelperName");
            }

        }
        /// <summary>
        /// get nextactivity to run when back from subworkflow
        /// </summary>
        public string NextActivity
        {
            get
            {
                DataLayer.DatabaseAccess db = new DataLayer.DatabaseAccess();
                string nextAc = db.GetNextActivity(this.InstanceID);
                return nextAc;
            }
            set { }
        }

        public EnumWorkflowInstanceStatus GetStatus()
        {
            string statValue = "";

            XElement statusElement = this.wfModel.XPathSelectElement("workflow");
            if (statusElement != null && statusElement.HasAttributes)
            {
                EnumWorkflowInstanceStatus execStat;
                XAttribute a = statusElement.Attribute("status");
                if (a != null)
                    statValue = statusElement.Attribute("status").Value;
                else
                {
                    statValue = EnumWorkflowInstanceStatus.Undefined.ToString();
                    statusElement.SetAttributeValue("status", statValue);
                }
                if (Enum.TryParse(statValue, out execStat))
                {
                    if (Enum.IsDefined(typeof(EnumWorkflowInstanceStatus), execStat))
                    {
                        return execStat;
                    }
                }
            }
            return EnumWorkflowInstanceStatus.Undefined;
        }
        /// <summary>
        /// set the status of workflow 
        /// </summary>
        /// <param name="val"></param>
        public void SetStatus(EnumWorkflowInstanceStatus val)
        {
            XElement statusElement = this.wfModel.XPathSelectElement("workflow");
            if (statusElement != null)
                statusElement.SetAttributeValue("status", val.ToString());
        }


        /// <summary>
        /// 
        /// </summary>
        public WorkflowModel()
        {
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   parse XML string. </summary>
        ///
        /// <remarks>   Fleckj, 17.11.2015. </remarks>
        ///
        /// <param name="wfModel">  The wf model. </param>
        #endregion
        public void LoadModelXml(string wfModel)
        {
            List<WorkflowError> errors = new List<WorkflowError>();
            bool hasErrors = false;
            try
            {
                this.wfModel = XDocument.Parse(wfModel, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

                //StreamReader streamReader = new StreamReader(Shared.Files.GetFileHelper.WorkflowValidationSchemaPath);

                //XmlReader xmlReader = XmlReader.Create(streamReader);

                //XmlSchemaSet schemas = new XmlSchemaSet();
                //schemas.Add("", xmlReader);


                //string validationMessage = string.Empty;
                //this.wfModel.Validate(schemas, (o, e) =>
                //{
                //    hasErrors = true;
                //    WorkflowError wfErr = new WorkflowError(e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
                //    errors.Add(wfErr);
                //});

                //if (hasErrors)
                //{
                //    string errMsg = string.Format("The Schema Validation failed. Message: {0}", validationMessage);
                //    throw new WorkflowException(errMsg, errors, WorkflowErrorType.NotWellformed);
                //}
            }
            catch (Exception ex)
            {
                if (!hasErrors)
                {
                    WorkflowError wfErr = new WorkflowError(ex.Message, -1, -1);
                    errors.Add(wfErr);
                    //throw new WorkflowException("error loading xml", ex, WorkflowErrorType.NotWellformed);
                    throw new WorkflowException("error loading xml", errors, WorkflowErrorType.NotWellformed);
                }
                else
                {
                    throw ex;
                }
            }
        }


        /// <summary>   Gets activity from instance ID. </summary>
        ///
        /// <remarks>   Fleckj, 17.11.2015. </remarks>
        ///
        /// <param name="activityInstanceID">   instance name of activity </param>
        ///
        /// <returns>   The activity or null if it doesnt exist. </returns>
        public Activity GetActivityFromInstance(string activityInstanceID)
        {
            string xquery = "/workflow/activities/activity[@instance='{0}']";
            XElement statusElement = this.wfModel.XPathSelectElement(string.Format(xquery, activityInstanceID));
            if (statusElement != null)
            {
                return new Activity(statusElement);
            }
            else
                return null;
        }

        /// <summary>
        /// returns the activity in the workflow marked as start activity
        /// </summary>
        /// <returns>null if no found or start activity</returns>
        public Activity GetStartActivityFromInstance()
        {
            DataLayer.DatabaseAccess db = new DataLayer.DatabaseAccess();
            var wfActivities = this.GetAllActivities();
            var dbActivities = db.GetAllActivityDefinitons();
            foreach (var wfA in wfActivities)
            {
                bool isStart = false;
                isStart = dbActivities.Exists(m => m.WFAD_IsStartActivity == true && m.WFAD_ID == wfA.Id);
                if (isStart)
                {
                    return wfA;
                }
            }
            return null;
        }

        /// <summary>
        /// returns the start activiry for a new workflow
        /// the activity marked as start activity in database
        /// </summary>
        /// <returns>null if no starzt activity found</returns>
        public WFEActivityDefinition GetStartActivityForNewWorkflow()
        {
            DataLayer.DatabaseAccess db = new DataLayer.DatabaseAccess();
            var dbActivities = db.GetAllActivityDefinitons().Find(m => m.WFAD_IsStartActivity == true);
            return dbActivities;
        }

        /// <summary>
        /// returns all activities from workflow
        /// </summary>
        /// <returns></returns>
        public List<Activity> GetAllActivities()
        {
            List<Activity> allActivities = new List<ProcessEngine.Activity>();
            string xquery = "/workflow/activities/activity";
            var activityElements = this.wfModel.XPathSelectElements(xquery);
            foreach (var aElement in activityElements)
            {
                allActivities.Add(new Activity(aElement));
            }

            return allActivities;
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Get the next highest Id of the current workflow
        ///             Scope is over all execution iteration from all activities. </summary>
        ///
        /// <remarks>   Fleckj, 19.02.2015. </remarks>
        ///
        /// <returns>   The next execution identifier. </returns>
        #endregion
        public int GetNextExecutionId()
        {
            return this.wfModel.Descendants("activity").Elements<XElement>("execution").Where<XElement>((XElement el) =>
            {
                if (el.Attribute("id") == null)
                {
                    return false;
                }
                return !string.IsNullOrWhiteSpace(el.Attribute("id").Value);
            }).DefaultIfEmpty<XElement>().Max<XElement>((XElement i) =>
            {
                if (i == null)
                {
                    return 0;
                }
                return int.Parse(i.Attribute("id").Value);
            }) + 1;
        }

        /// <summary>
        /// Generates variables tags from all existing activity properties (=activity variables)
        /// This overwrites any values in existing variable tags. 
        /// --> only call this when designing the workflow, not during execution
        /// </summary>
        /// <returns>returns a sorted list of updated variables</returns>
        public void BuildXmlVariables_FromProperties()
        {
            SortedList<string, Variable> returnList = new SortedList<string, Variable>(StringComparer.InvariantCultureIgnoreCase);
            IEnumerable<XElement> allActivityVariables;

            allActivityVariables = this.wfModel.XPathSelectElements("//property");
            if (allActivityVariables != null)
            {
                foreach (XElement v in allActivityVariables)
                {
                    try
                    {
                        string uniqueName;
                        string varValue;
                        EnumVariableDirection direction;
                        EnumVariablesDataType tmpType;

                        this.ExtraktVariableProperties(v, out uniqueName, out varValue, out direction, out tmpType);

                        this.UpdateWorkflowVariableInXmlModel(uniqueName, varValue, tmpType, direction);
                    }
                    catch (Exception ex)
                    {
                        throw new BaseException(ErrorCodeHandler.E_XML_INVALID_XPATH, ex);
                    }

                }
            }
            else
            {
                logger.Info("Could not create variables for wf xml model. there were no //property tags in xml model");
            }
        }

        public void ExtraktVariableProperties(XElement v, out string varValue, out string uniqueName, out EnumVariableDirection direction, out EnumVariablesDataType tmpType)
        {
            string activityInstanceNr;
            string vName;

            vName = v.Attribute("name").Value;

            if (v.Name.LocalName.ToLowerInvariant().Equals("property"))
                activityInstanceNr = v.Parent.Parent.Attribute("nr").Value + ".";
            else
                activityInstanceNr = "";

            uniqueName = activityInstanceNr + vName;

            // this can contain strings or xml nodes
            // needs different ways to extract value

            if (v.HasElements)
            {

                varValue = string.Concat(v.Nodes());

            }
            else
                varValue = v.Value;


            direction = ActivityProperty.ConvertToType(v.Attribute("direction").Value);

            string dType = v.Attribute("dataType") != null ? v.Attribute("dataType").Value : "stringType"; // default to string

            tmpType = Variable.ConvertToEnumDataType(dType);

        }

        /// <summary>
        /// [starts-with(@name,'0.')]
        /// or empty for all
        /// </summary>
        /// <returns>returns the all workflow variables defined in xml model</returns>
        public IEnumerable<Variable> GetPunktVariables(string xpathCondition)
        {
            List<Variable> result = new List<Variable>();
            List<string> resultKeys = new List<string>();

            try
            {
                IEnumerable<XElement> allWorkflowVariables;
                allWorkflowVariables = this.wfModel.XPathSelectElements("/workflow/variables/variable" + xpathCondition);

                if (allWorkflowVariables != null)
                {
                    int i = 0;
                    foreach (XElement v in allWorkflowVariables)
                    {
                        i++;

                        string uniqueName;
                        string varValue;
                        EnumVariableDirection direction;
                        EnumVariablesDataType varType;

                        this.ExtraktVariableProperties(v, out varValue, out uniqueName, out direction, out varType);

                        if (resultKeys.Contains(uniqueName))
                        {

                            Variable bla = result.Find(x => x.Name == uniqueName);


                            bla.VarValue = varValue;
                            bla.DataType = varType;
                            bla.Direction = direction;

                        }
                        else
                        {

                            //add new
                            Variable newVar = new Variable(uniqueName, varValue, varType, direction);
                            result.Add(newVar);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_XML_GENERALERROR, ex);
            }

            return result;
        }

        /// <summary>
        /// Updates the workflow variable.
        /// If not exists then create the xml tag
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="variableValue"></param>
        /// <param name="dataType"></param>
        /// <param name="direction"></param>
        public void UpdateWorkflowVariableInXmlModel(string variableName, string variableValue, EnumVariablesDataType dataType, EnumVariableDirection direction)
        {
            XElement elVariables = this.wfModel.XPathSelectElement("/workflow/variables");

            if (elVariables == null) //variables section doesnt exist (yet) --> create it
            {
                XElement newVarsSection = new XElement("variables");
                this.wfModel.XPathSelectElement("workflow").Add(newVarsSection);
                elVariables = this.wfModel.XPathSelectElement("/workflow/variables");
            }

            XElement newVariable = elVariables.XPathSelectElement("variable[@name='" + variableName + "']");
            if (newVariable != null)//update
            {

                string varValue;
                string uniqueName;
                EnumVariableDirection d;
                EnumVariablesDataType t;
                this.ExtraktVariableProperties(newVariable, out varValue, out uniqueName, out d, out t);

                this.addValueToVariable(newVariable, variableValue);
                newVariable.SetAttributeValue("dataType", t.ToString());
                newVariable.SetAttributeValue("direction", d.ToString());
            }
            else //new
            {
                XElement newVar = new XElement("variable",
                    new XAttribute("name", variableName),
                    new XAttribute("direction", direction.ToString()),
                    new XAttribute("dataType", dataType.ToString()));
                //newVar.Value = variableValue;
                this.addValueToVariable(newVar, variableValue);
                elVariables.Add(newVar);
            }

        }

        private void addValueToVariable(XElement var, string varValue)
        {

            try
            {
                if (!string.IsNullOrWhiteSpace(varValue))
                {
                    XElement xml = XElement.Parse(varValue);
                    var.Nodes().Remove();
                    var.Add(xml);
                }
            }
            catch (Exception e)
            {
                // not xml
                var.Value = varValue;
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Updates the workflow variables described by dataFields. </summary>
        ///
        /// <remarks>   Fleckj, 18.11.2015. </remarks>
        ///
        /// <param name="dataFields">   The data fields. </param>
        #endregion
        public void UpdateWorkflowVariablesInXmlModel(SortedList<string, Variable> dataFields)
        {
            foreach (KeyValuePair<string, Variable> v in dataFields)
            {
                this.UpdateWorkflowVariableInXmlModel(v.Value.Name, v.Value.VarValue, v.Value.DataType, v.Value.Direction);
            }
        }

        public bool HasWaitingSteps()
        {
            IEnumerable<XElement> ws = this.wfModel.XPathSelectElements("//execution[@stepExecStatus='" + EnumStepState.Wait.ToString() + "']");
            if (ws != null && ws.Count() > 0)
                return true;
            else
                return false;
        }

        public string GetWaitingSteps()
        {
            return "";
        }

        /// <summary>
        /// convert string to obj and add values of variables to wf variables
        /// </summary>
        /// <param name="inputParameters"></param>
        public void AddInputVariables(string inputParameters)
        {
            if (string.IsNullOrWhiteSpace(inputParameters))
                return;
            else
            {
                List<WorkflowMessageVariable> wfVariables = XmlSerialiserHelper.DeserialiseFromXml<List<WorkflowMessageVariable>>(inputParameters);
                if (wfVariables != null)
                {
                    foreach (WorkflowMessageVariable v in wfVariables)
                    {
                        this.UpdateWorkflowVariableInXmlModel("0." + v.VarName, v.VarValue, EnumVariablesDataType.stringType, EnumVariableDirection.both);
                    }
                }
            }
        }

        /// <summary>
        /// returns a workflow xml string formatted
        /// </summary>
        /// <returns></returns>
        public string GetWorkflowXml()
        {
            //brutalo method - creating a new xdocument will format with line breaks
            var tmpDoc = XDocument.Parse(this.wfModel.ToString(SaveOptions.None), LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
            return tmpDoc.ToString(SaveOptions.None);
        }

        private static string PrettyXml(string xml)
        {
            var stringBuilder = new StringBuilder();

            var element = XElement.Parse(xml);

            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// adds activity that is a subworkflow activity
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="subWorkflowDefinitionID"></param>
        /// <param name="subWorkflowVariablesXml"></param>
        public void AddSubworkflowActivity(Activity activity, string subWorkflowDefinitionID, string subWorkflowVariablesXml)
        {

            Dictionary<string, string> prefillValues = new Dictionary<string, string>()
            {
                { "subWorfklowDefinitionID", subWorkflowDefinitionID }
            };

            // just add activity as normal
            activity = this.AddActivity(activity, prefillValues);

            //add subworkflow variables as direction="subworkflow"


            // <root>
            //   <varname>varvalue</varname>
            //   <varname2>varvalue2</varname2>
            // </root>
            var subWFVars = XElement.Parse(subWorkflowVariablesXml).XPathSelectElements("child::*");
            foreach (var subWfVar in subWFVars)
            {
                // create name with correct instance number !
                string vName = activity.Nr + "." + subWfVar.Name.ToString();

                string vValue = subWfVar.Value;
                this.UpdateWorkflowVariableInXmlModel(vName, vValue, EnumVariablesDataType.stringType, EnumVariableDirection.subworkflow);
            }

        }

        /// <summary>
        /// adds activity to workflow model and inserts workflow variables resulting from new activity properties
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="prefillVariableValues">use this to pre-fill variables, provide a dictionary with variable name and value</param>
        public Activity AddActivity(Activity activity, Dictionary<string, string> prefillVariableValues = null)
        {
            // get activities an find the last number

            List<XElement> elements = this.wfModel.XPathSelectElements("/workflow/activities/activity").ToList();

            int highestNumber = 0;
            foreach (XElement element in elements)
            {
                int number = int.Parse(element.Attribute("nr").Value);

                if (number > highestNumber)
                {
                    highestNumber = number;
                }
            }
            int newNumber = highestNumber + 1;
            // get activity definition and insert the activity to the xml

            activity.Nr = newNumber.ToString();
            //   activity.Instance = string.Format("{0}.{1}", newNumber, activity.Name.Split('.').Last());

            XElement activityRoot = this.wfModel.XPathSelectElement("/workflow/activities");
            activityRoot.Add(activity.ActivityNode);

            // get variables for the activity and insert the instances to the xml
            List<Variable> variables = Variable.GetVariablesFromActivity(activity);

            XElement variableRoot = this.wfModel.XPathSelectElement("/workflow/variables");
            foreach (Variable item in variables)
            {
                string prefillValue;
                if (prefillVariableValues != null)
                {
                    if (!prefillVariableValues.TryGetValue(item.Name, out prefillValue))
                        prefillValue = "";
                }
                else
                    prefillValue = "";

                this.UpdateWorkflowVariableInXmlModel(newNumber.ToString() + "." + item.Name, prefillValue, item.DataType, item.Direction);
                //XElement newVariable = item.GetInstanceNode(newNumber);
            }

            return activity;
        }

        private string GetWorkflowAttribteValue(string attributeName)
        {
            XElement statusElement = this.wfModel.XPathSelectElement("workflow");
            if (statusElement != null && statusElement.HasAttributes)
            {
                XAttribute id = statusElement.Attribute(attributeName);
                if (id != null)
                {
                    return id.Value;
                }
                else
                    return null;
            }
            return null;
        }
    }
}

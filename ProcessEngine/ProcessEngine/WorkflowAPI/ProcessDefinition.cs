using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.DLLConfiguration;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using Kapsch.IS.ProcessEngine.Shared.Interfaces;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   The ProcessDefinition represents a workflow definition as defined 
    ///             in an XPDL tool like 'Together Workflow Editor' and stored in the table [definitions]. </summary>
    ///
    /// <remarks>   Fleckj, 18.03.2015. </remarks>
    #endregion
    public class ProcessDefinition
    {
        /// <summary>   Unique identifier for the workflow definition. </summary>
        private IDataAccess databaseLayer;
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Fleckj, 18.03.2015. </remarks>
        ///
        /// <param name="wfDefID">    Identifier for the definition. </param>
        /// <param name="db">        Class to access the data layer.</param>
        #endregion
        public ProcessDefinition()
        {
            this.databaseLayer = new DatabaseAccess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataAccess"></param>
        public ProcessDefinition(IDataAccess dataAccess)
        {
            this.databaseLayer = dataAccess;
        }



        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Creates an instance of a workflow for a given <paramref name="workflowInstanceID"/>.
        ///             A new row in the database table [instances] is created and a new object of type <see cref="ProcessInstance"/>
        ///             is created.</summary>
        ///
        /// <remarks>   Fleckj, 18.03.2015. </remarks>
        ///
        /// <param name="workflowDefinitionID"> Name of the instance. </param>
        /// <param name="workflowInstanceID"> Name of the instance. </param>
        /// <param name="parentWorkflowInstanceID"> if this is a subworkflow being created, write the parent WF  </param>
        /// <param name="nextActivity"> if this is a subworkflow being created, remember next activitry in parent to call when subwf finished </param>
        /// <returns>   The new instance class. </returns>
        #endregion
        public ProcessInstance CreateWorkflowInstance(string workflowDefinitionID, string workflowInstanceID, string parentWorkflowInstanceID = "", string nextActivity = "")
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            logger.Debug("SWPD0: Start ProcessDefinition.CreateWorkflowInstance: " + sw.ElapsedMilliseconds);

            ProcessInstance newWfInstance = null;
            string newXmlWorkflow = String.Empty;

            // create instance in db
            if (String.IsNullOrWhiteSpace(workflowInstanceID))
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "A new workflow instance ID cannot be null or empty");
            if (String.IsNullOrWhiteSpace(workflowDefinitionID))
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "A new workflowDefinitionID cannot be null or empty");

            try
            {
                DatabaseAccess databaseAccess = new DatabaseAccess();
                WFEWorkflowDefinition wdf = databaseAccess.GetWorkflowDefinition(workflowDefinitionID, false); // get latest version if workflow that isn't checked out
                logger.Debug("SWPD1: Got WorkflowDefinition "+ workflowDefinitionID + "ProcessDefinition.CreateWorkflowInstance: " + sw.ElapsedMilliseconds);
                if (wdf == null)
                {
                    string errMsg = string.Format("Could not find Workflow Definition {0}", workflowDefinitionID);
                    throw new BaseException(ErrorCodeHandler.E_WF_CREATE_INSTANCE, errMsg);
                } else
                {
                    WorkflowInstance wfi = new WorkflowInstance();
                    DateTime jetzt = DateTime.Now;
                    wfi.WFI_ID = workflowInstanceID;
                    wfi.WFI_WFD_ID = workflowDefinitionID;
                    wfi.WFI_Xml = wdf.WFD_Definition;
                    //TODO : maybe add the version if definition used to instance ?  wdf.WFD_Version
                    wfi.WFI_Status = EnumWorkflowInstanceStatus.NotStarted.ToString();
                    wfi.WFI_CurrentActivity = "start";
                    wfi.WFI_ParentWF = parentWorkflowInstanceID;
                    wfi.WFI_NextActivity = nextActivity;
                    wfi.WFI_Created = jetzt;
                    wfi.WFI_Updated = jetzt;

                    newWfInstance = new ProcessInstance(workflowInstanceID);

                    // update the instanceID in xml model 
                    WorkflowModel wfm = new WorkflowModel();
                    wfm.LoadModelXml(wfi.WFI_Xml);
                    wfm.InstanceID = workflowInstanceID;
                    wfi.WFI_Xml = wfm.wfModel.ToString(System.Xml.Linq.SaveOptions.None);

                    // and save to db
                    logger.Debug("SWPD2: ProcessDefinition before saving instance to DB " + sw.ElapsedMilliseconds);
                    databaseAccess.AddWorkflowInstance(wfi);
                    logger.Debug("SWPD3: ProcessDefinition after saving instance to DB " + sw.ElapsedMilliseconds);
                    logger.Debug(string.Format("Workflow {0} successfully created.", workflowInstanceID));
                    sw.Reset();

                }
                return newWfInstance;

            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "A new workflow instance cannot be created. workflowInstanceID=" + workflowDefinitionID, ex);
            }
        }

        /// <summary>
        /// counts active workflow instances from wf def
        /// throws BasException
        /// </summary>
        /// <param name="workflowID"></param>
        /// <returns></returns>
        public int GetProcessInstanceCountFromWorkflowID(string workflowID)
        {
            return this.databaseLayer.GetActiveWorkflowProcesses(workflowID);
        }

        /// <summary>
        /// get all workflow definitions
        /// only show the one row per workflowdefinition if there are 1+ versions.
        ///   show latest version number that isn't checked out with 'checkedOutBy' guid from person (version with nr == -1)
        /// throws exception
        /// </summary>
        /// <returns></returns>
        public List<WFEWorkflowDefinition> GetAllWorkflowDefinitions(bool noxml=true)
        {
            return this.databaseLayer.GetAllWorkflowDefinitions(onlyactive: true);//new List<WFEWorkflowDefinition>();
        }

        /// <summary>
        /// create a new workflow definition
        /// CheckedOutBy is empty (= -1)
        /// throws exception
        /// </summary>
        /// <param name="workflowName">ID of the workflow, must not be null</param>
        /// <param name="workflowDescription">must not be null</param>
        /// <param name="validFrom">null means valid from now</param>
        /// <param name="validTo">null means valid until forever</param>
        /// <returns>new object ProcessDefinition</returns>
        public WFEWorkflowDefinition CreateWorkflowDefinition(string workflowName, string workflowDescription, DateTime? validFrom, DateTime? validTo, bool isdemoMode = false)
        {
            WFEWorkflowDefinition newDefinition = new WFEWorkflowDefinition()
            {
                WFD_ID = Engine.Prefix_WorkflowDefinition + "_" + Guid.NewGuid().ToString("N"),
                WFD_Definition = string.Format("<workflow id='' dataHelperName='' name='{0}' status='new' demoMode='{1}'><variables></variables><activities></activities></workflow>", workflowName, isdemoMode.ToString().ToLower()),
                WFD_Description = workflowDescription,
                WFD_Name = workflowName,
                WFD_ValidFrom = validFrom,
                WFD_ValidTo = validTo,
                WFD_Version = 0,
                WFD_CheckedOutBy = null,
            };

            WorkflowModel m = new WorkflowModel();
            m.LoadModelXml(newDefinition.WFD_Definition);
            m.InstanceID = newDefinition.WFD_ID;
            m.IsDemoMode = true;
            m.SetStatus(EnumWorkflowInstanceStatus.NotStarted);
            //m.AddActivity(new Activity(this.GetActivityDefinitionTemplate("start")));
            var start = m.GetStartActivityForNewWorkflow();
            m.AddActivity(new Activity(this.GetActivityDefinitionTemplate(start.WFAD_ID)));
            newDefinition.WFD_Definition = m.GetWorkflowXml();

            newDefinition = this.databaseLayer.CreateNewWorkflowDefinition(newDefinition);
            return newDefinition;
        }

        /// <summary>
        /// already checked out? -> exception
        /// </summary>
        /// <param name="wfGuid"></param>
        /// <param name="userGuid"></param>
        internal WFEWorkflowDefinition CheckoutWorkflowDefinition(Guid wfGuid, string userGuid)
        {
            // see notes : take latest version with checkout null
            // wird schon angelegt; already checked out? -> exception
            var toBeCheckedout = this.GetWorkflowDefinitionByGUID(wfGuid);
            // see if this version has been checked out already
            var checkedOutWorfklow = this.databaseLayer.GetWorkflowDefinition(toBeCheckedout.WFD_ID, true);
            if (checkedOutWorfklow == null)
            {
                // do the checkout, create new row with version = -1 and checkoutBy guid set
                WFEWorkflowDefinition newWfDef = new WFEWorkflowDefinition()
                {
                    Guid = Guid.NewGuid(),
                    WFD_Created = toBeCheckedout.WFD_Created,
                    WFD_CheckedOutBy = userGuid,
                    WFD_Version = -1,
                    WFD_ID = toBeCheckedout.WFD_ID,
                    WFD_Name = toBeCheckedout.WFD_Name,
                    WFD_Definition = toBeCheckedout.WFD_Definition,
                    WFD_Description = toBeCheckedout.WFD_Description,
                };
                newWfDef = this.databaseLayer.CreateNewWorkflowDefinition(newWfDef);
                return newWfDef;
            }
            else
            {
                string msg = string.Format("This workflowdefinition is already checked-out! Guid = {0} , ID = {1}",
                    toBeCheckedout.Guid.ToString(),
                    toBeCheckedout.WFD_ID);
                logger.Warn(msg);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }
        }

        /// <summary>
        /// create new row with version -1, write checkedout by into version active
        /// </summary>
        /// <param name="workflowID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public WFEWorkflowDefinition CheckoutWorkflowDefinition(string workflowID, string userID)
        {
            //
            // set checked out by
            //
            WFEWorkflowDefinition wfActive = this.GetWorkflowDefinitionByID(workflowID, false);
            WFEWorkflowDefinition wfChecked = this.GetWorkflowDefinitionByID(workflowID, true);
            if (wfChecked != null)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "This workflow is already checked out. WorkflowID =  " + workflowID);
            }
            else
            {
                wfActive.WFD_CheckedOutBy = userID;
                this.databaseLayer.UpdateWorkflowDefinitionObject(wfActive);
                //
                // create new row
                //
                // do the checkout, create new row with version = -1 and checkoutBy guid set
                WFEWorkflowDefinition newWfDef = new WFEWorkflowDefinition()
                {
                    Guid = Guid.NewGuid(),
                    WFD_Created = wfActive.WFD_Created,
                    WFD_Version = -1,
                    WFD_ID = wfActive.WFD_ID,
                    WFD_Name = wfActive.WFD_Name,
                    WFD_Definition = wfActive.WFD_Definition,
                    WFD_Description = wfActive.WFD_Description,
                };
                newWfDef = this.databaseLayer.CreateNewWorkflowDefinition(newWfDef, true);
                return wfActive;
            }
        }

        /// <summary>
        /// checkin a workflow
        /// only same user that checked out can checkin
        /// version nr gets increased
        /// </summary>
        /// <param name="workflowID"></param>
        /// <param name="userID"></param>
        public void CheckinWorkflowDefinition(string workflowID, string userID)
        {
            WFEWorkflowDefinition wfChecked = null;
            WFEWorkflowDefinition wfLatest = null;

            wfChecked = this.GetWorkflowDefinitionByID(workflowID, true);
            wfLatest = this.GetWorkflowDefinitionByID(workflowID, false);

            if (wfLatest == null)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Can't find a workflow with id = " + workflowID);
            }

            if (wfChecked == null)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Can't find a checked-out version for workflow with id = " + workflowID);
            }
            if (wfLatest.WFD_CheckedOutBy != userID)
            {
                string msg = string.Format("Can't checkin workflow with Id = {0}. User : '{1}' has not check out this workflow. Only user : '{2}' can checkin worflow",
                    wfLatest.WFD_ID, userID, wfLatest.WFD_CheckedOutBy);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }

            //
            // update latest : set checkedout auf null
            //
            wfLatest.WFD_CheckedOutBy = null;
            this.databaseLayer.UpdateWorkflowDefinitionObject(wfLatest);

            //
            // update checked out
            //
            wfChecked.WFD_CheckedOutBy = null;
            wfChecked.WFD_Version = wfLatest.WFD_Version + 1;
            this.databaseLayer.UpdateWorkflowDefinitionObject(wfChecked);

        }

        /// <summary>
        /// 
        /// </summary>
        public void UndoCheckout(string workflowID, string userID)
        {
            WFEWorkflowDefinition wfChecked = null;
            WFEWorkflowDefinition wfLatest = null;

            wfChecked = this.GetWorkflowDefinitionByID(workflowID, true);
            wfLatest = this.GetWorkflowDefinitionByID(workflowID, false);

            if (wfLatest == null)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Can't find a workflow with id = " + workflowID);
            }

            if (wfChecked == null)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Can't find a checked out version for workflow with id = " + workflowID);
            }
            if (wfLatest.WFD_CheckedOutBy != userID)
            {
                string msg = string.Format("Can't undo the checkout for the workflow with Id = {0}. User : '{1}' has not check out this workflow. Only user : '{2}' can undo checkout worflow",
                    wfLatest.WFD_ID, userID, wfLatest.WFD_CheckedOutBy);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }

            wfLatest.WFD_CheckedOutBy = null;
            this.databaseLayer.UpdateWorkflowDefinitionObject(wfLatest);

            //delete row with latest
            this.databaseLayer.DeletWorkflowByGUID(wfChecked.Guid);
        }

        /// <summary>
        /// only save to checked out version if userId match
        /// throws a WorkflowException
        /// </summary>
        /// <param name="workflowID"></param>
        /// <param name="workflowXml"></param>
        /// <param name="userId"></param>
        public void SaveWorkflowXmlDefinitionOnly(string workflowID, string workflowXml, string userId, bool isWebservice = false)
        {
            #region check if xml is well formed

            //
            // check and throw exception in case
            //
            ProcessDefinition.CheckForWellFormedXml(workflowXml, isWebservice);
            //
            // TODO call every validation rules for every activity
            //
            //this.RunValidationRulesOnActivities(workflowXml);

            #endregion

            WFEWorkflowDefinition wfChecked = null;
            WFEWorkflowDefinition wfLatest = null;

            wfChecked = this.GetWorkflowDefinitionByID(workflowID, true);
            wfLatest = this.GetWorkflowDefinitionByID(workflowID, false);

            if (wfLatest == null)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Can't find a workflow with id = " + workflowID);
            }
            if (wfChecked == null)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Can't find a checked-out version for workflow with id = " + workflowID);
            }
            if (wfLatest.WFD_CheckedOutBy != userId)
            {
                string msg = string.Format("Can't save the workflow with Id = {0}. The User : '{1}' has not checked out this workflow. Only user : '{2}' can update the workflow",
                    wfLatest.WFD_ID, userId, wfLatest.WFD_CheckedOutBy);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }
            this.databaseLayer.UpdateWorkflowDefinitionXml(wfChecked.Guid, XDocument.Parse(workflowXml).ToString(SaveOptions.None));
        }

        private Tuple<Dictionary<string, object>, Dictionary<string, MethodInfo>> LoadActivityDlls(ActivityDLLConfigurationSection dllConfig)
        {
            Dictionary<string, object> dllCache = new Dictionary<string, object>();
            Dictionary<string, MethodInfo> methodCache = new Dictionary<string, MethodInfo>();

            object activityObject = null;
            MethodInfo activityMethod = null;

            string fullDLLPath = "";
            string baseDir = ConfigurationManager.AppSettings["DLLStepsBaseDir"];

            foreach (ActivityDLLElement item in dllConfig.ActivityDLLs)
            {
                // build path
                if (!Path.IsPathRooted(item.DLLPath))
                {
                    if (!string.IsNullOrWhiteSpace(baseDir))
                        fullDLLPath = Path.Combine(baseDir, item.DLLPath);
                    else
                    {
                        string exePath = Path.GetDirectoryName((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath);
                        fullDLLPath = Path.Combine(exePath, item.DLLPath);
                    }
                }
                try
                {
                    Assembly activityAssembly = Assembly.LoadFrom(fullDLLPath);
                    var it = typeof(IActivityValidator);
                    foreach (Type acType in activityAssembly.GetTypes().Where(it.IsAssignableFrom).ToList())
                    {
                        activityObject = Activator.CreateInstance(acType);
                        activityMethod = acType.GetMethods()
                            .Where(m => m.Name == "Validate")
                            .Where(m =>
                                {
                                    if (m.GetParameters().First().ParameterType == typeof(Activity))
                                        return true;
                                    else
                                        return false;
                                }
                            ).First();

                        dllCache[acType.FullName] = activityObject;
                        methodCache[acType.FullName] = activityMethod;
                    }
                }
                catch (Exception ex)
                {
                    throw new BaseException(ErrorCodeHandler.E_WF_GENERAL,
                        string.Format("Error loading acttivity DLL : '{0}'", fullDLLPath), ex);
                }
            }

            return Tuple.Create(dllCache, methodCache);
        }

        private void RunValidationRulesOnActivities(string workflowXml)
        {
            ActivityDLLConfigurationSection dllConfig;
            Dictionary<string, object> dicDLLCache;
            Dictionary<string, MethodInfo> dicMethodCache;

            dllConfig = ConfigurationManager.GetSection("ActivityDLLsConfig") as ActivityDLLConfigurationSection;
            Tuple<Dictionary<string, object>, Dictionary<string, MethodInfo>> returnValue = this.LoadActivityDlls(dllConfig);
            dicDLLCache = returnValue.Item1;
            dicMethodCache = returnValue.Item2;

            try
            {
                WorkflowModel wfModel = new WorkflowModel();
                wfModel.LoadModelXml(workflowXml);
                foreach (Activity ac in wfModel.GetAllActivities())
                {
                    string nameSpace = ac.Id;
                    object oBaseObject = null;
                    MethodInfo oMethod = null;


                    if (!dicDLLCache.TryGetValue(nameSpace, out oBaseObject))
                    {
                        string msg = string.Format("Cannot find activity {0} in dicDLLCache", nameSpace);
                        logger.Error(msg);
                        StringWriter sw = new StringWriter();
                        ObjectDumper.Write(dicDLLCache, 5, sw);
                        logger.Debug(sw.ToString());
                        throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
                    }

                    if (!dicMethodCache.TryGetValue(nameSpace, out oMethod))
                    {
                        string msg = string.Format("Cannot find activity {0} in dicMethodCache", nameSpace);
                        logger.Error(msg);
                        StringWriter sw = new StringWriter();
                        ObjectDumper.Write(dicDLLCache, 5, sw);
                        logger.Debug(sw.ToString());
                        throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
                    }

                    object[] arguments = new object[] { ac };

                    StepReturn stepResult = (StepReturn)oMethod.Invoke(oBaseObject, BindingFlags.Public, null, arguments, CultureInfo.CurrentCulture);
                    if (stepResult.StepState == EnumStepState.ErrorStop)
                    {
                        stepResult.ReturnValue = "error";
                    }

                    //return stepResult;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// check workflow xml if valid
        /// 
        /// </summary>
        /// <param name="workflowXml"></param>
        /// <param name="isWebservice"></param>
        public static void CheckForWellFormedXml(string workflowXml, bool isWebservice = false)
        {
            List<WorkflowError> errors = new List<WorkflowError>();
            try
            {


                XDocument wfModel = XDocument.Parse(workflowXml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("",
                    XmlReader.Create(
                        new StreamReader(Shared.Files.GetFileHelper.GetWorkflowValidationSchemaPath(isWebservice)))
                        );

                wfModel.Validate(schemas, (o, e) =>
                {
                    WorkflowError wfErr = new WorkflowError(e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
                    errors.Add(wfErr);
                });


            }
            catch (XmlException e)
            {
                string errMsg = string.Format("The Xml Validation failed.");
                WorkflowError wfErr = new WorkflowError(e.Message, e.LineNumber, e.LinePosition);
                errors.Add(wfErr);

                throw new WorkflowException(errMsg, errors, WorkflowErrorType.NotWellformed);
            }
            catch (Exception ex)
            {
                //     errors.Add(new WorkflowError())
                throw new WorkflowException("General error in loading Xml", ex, WorkflowErrorType.NotWellformed);
            }


            if (errors.Count > 0)
            {
                string errMsg = string.Format("The Schema Validation failed.");
                throw new WorkflowException(errMsg, errors, WorkflowErrorType.NotWellformed);
            }
        }

        /// <summary>
        /// Save workflow meta data
        /// </summary>
        /// <param name="workflowID"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="validFrom"></param>
        /// <param name="validTo"></param>
        /// <param name="userId"></param>
        public void SaveWorkflowMetaData(string workflowID, string name, string description, DateTime? validFrom, DateTime? validTo, string userId)
        {
            WFEWorkflowDefinition wfChecked = null;
            WFEWorkflowDefinition wfLatest = null;

            wfChecked = this.GetWorkflowDefinitionByID(workflowID, true);
            wfLatest = this.GetWorkflowDefinitionByID(workflowID, false);

            if (wfLatest == null)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Can't find a workflow with id = " + workflowID);
            }
            if (wfChecked == null)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Can't find a checked-out version for workflow with id = " + workflowID);
            }
            if (wfLatest.WFD_CheckedOutBy != userId)
            {
                string msg = string.Format("Can't save the workflow with Id = {0}. The User : '{1}' has not checked out this workflow. Only user : '{2}' can update the workflow",
                    wfLatest.WFD_ID, userId, wfLatest.WFD_CheckedOutBy);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }

            WFEWorkflowDefinition wfdefPure = this.databaseLayer.GetWorkflowDefinitionByGuid(wfChecked.Guid);

            if (wfdefPure != null)
            {
                wfdefPure.WFD_Name = name;
                wfdefPure.WFD_Description = description;
                wfdefPure.WFD_ValidFrom = validFrom;
                wfdefPure.WFD_ValidTo = validTo;
                this.databaseLayer.UpdateWorkflowDefinitionObject(wfdefPure);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityID"></param>
        /// 
        public XElement GetActivityDefinitionTemplate(string activityID)
        {
            WFEActivityDefinition acDef = this.databaseLayer.GetActivityDefinitionTemplate(activityID);
            XElement xEl = XElement.Parse(acDef.WFAD_ConfigTemplate);
            return xEl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Dictionary (Id, Name)</returns>
        public List<WFEActivityDefinition> GetAllActivities()
        {
            return this.databaseLayer.GetAllActivityDefinitons();
        }

        /// <summary>
        /// throws exception
        /// </summary>
        /// <param name="wfDefId"></param>
        /// <param name="getCheckedOutVersion">if true gets the checked out version of this workflow; false is default and will get get the latest version of workflow</param>
        /// <returns>a workflowdefinition or null if nothing is found</returns>
        internal WFEWorkflowDefinition GetWorkflowDefinitionByID(string wfDefId, bool getCheckedOutVersion = false)
        {
            WFEWorkflowDefinition wfDef = this.databaseLayer.GetWorkflowDefinition(wfDefId, getCheckedOutVersion);
            return wfDef;
        }

        /// <summary>
        /// if userId equals checkedOutBy field return the checked out version
        /// else return aktive version
        /// </summary>
        /// <param name="wfDefId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public WFEWorkflowDefinition GetWorkflowDefinitionByID(string wfDefId, string userId)
        {
            WFEWorkflowDefinition wfLatest = this.databaseLayer.GetWorkflowDefinition(wfDefId, false);

            if (wfLatest.WFD_CheckedOutBy == userId)
            {
                // wf is checked out return version with -1
                WFEWorkflowDefinition wfChecked = this.databaseLayer.GetWorkflowDefinition(wfDefId, true);

                if (wfChecked != null)
                {
                    // set infos from latest version to currently editing workflow
                    wfChecked.WFD_CheckedOutBy = wfLatest.WFD_CheckedOutBy;
                    wfChecked.WFD_Version = wfLatest.WFD_Version;
                    return wfChecked;
                }
                else
                    return wfLatest;
            }
            else
                return wfLatest;
        }

        /// <summary>
        /// get wf def by guid
        /// </summary>
        /// <param name="theGuid"></param>
        /// <returns></returns>
        public WFEWorkflowDefinition GetWorkflowDefinitionByGUID(Guid theGuid)
        {
            WFEWorkflowDefinition wfDef = this.databaseLayer.GetWorkflowDefinitionByGuid(theGuid);
            return wfDef;
        }

        /// <summary>
        /// nur der activities teil
        /// </summary>
        /// <param name="workflowDefintionID"></param>
        /// <returns></returns>
        public XElement GetWorkflowAtivities(string workflowDefintionID)
        {
            // create func in workflowmodel
            return null;
        }

        /// <summary>
        /// nur der var teil
        /// </summary>
        /// <param name="workflowDefintionID"></param>
        /// <returns></returns>
        public XElement GetWorkflowVariables(string workflowDefintionID)
        {
            return null;
        }

        /// <summary>
        /// add activity variables for idActivity to xml model
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="idActivity"></param>
        /// <returns></returns>
        public string CreateWorkflowXmlWithActivity(string xml, string idActivity)
        {
            WorkflowModel model = new WorkflowModel();
            model.LoadModelXml(xml);
            model.AddActivity(new Activity(this.GetActivityDefinitionTemplate(idActivity)));

            //XDocument.Parse(model.wfModel.ToString()).ToString()

            return model.GetWorkflowXml();
        }

        /// <summary>
        /// adds a subworkflow activity plus all the InputVariables (0.) of the given subWorkflowDefinition
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="idActivity">id of the subworkflow activity</param>
        /// <param name="subWorkflowDefinitionID">id of workflow that is called by subworkflow activity</param>
        /// <param name="subWorkflowVariables"></param>
        /// <returns>the new workflow xml as string</returns>
        public string CreateWorkflowXmlWithActivity(string xml, string idActivity, string subWorkflowDefinitionID, string subWorkflowVariables)
        {
            WorkflowModel model = new WorkflowModel();
            model.LoadModelXml(xml);

            model.AddSubworkflowActivity(
                activity: new Activity(this.GetActivityDefinitionTemplate(idActivity)),
                subWorkflowDefinitionID: subWorkflowDefinitionID,
                subWorkflowVariablesXml: subWorkflowVariables
                );

            return model.GetWorkflowXml();
        }
    }
}

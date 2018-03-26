using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using ProcessEngine.Shared.Enums;
using System;
using System.Data.Entity;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.WFActivity.NavisionTicket
{

    public class NavisionTicketActivityWait : BaseEDPAsyncActivity, IActivityValidator, IProcessStep
    {
        private EngineContext engineContext;
        private WFEAsyncWaitItem waitItemWFE;
        private Variable pollingIntervalSeconds;
        private bool alertExists;
        private bool isDemoMode;
        private string linkedActivityInstance;
        private XDocument awiCfgDoc;
        private string navStatus;
        private string navOldStatus;
        private string ticketNumber = "";
        private string timeoutString;
        private bool isEmpty;
        private bool isNoChange;
        private bool changeToAngelegt;
        private bool changeToBeendet;
        private bool changeToStorno;
        private DatabaseAccess database;
        private DbContextTransaction transaction;

        public NavisionTicketActivityWait() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn PostInitialize(EngineContext engineContext, BaseAsyncRequestResult baseResult)
        {
            StepReturn result = new StepReturn("PostInitialize not implemented.", EnumStepState.ErrorStop);
            return result;
        }

        public override StepReturn Initialize(EngineContext engineContext)
        {
            DatabaseAccess db = new DatabaseAccess();
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            try
            {
                this.isDemoMode = base.isDemoModeOn(engineContext);
                this.pollingIntervalSeconds = base.GetProcessedActivityVariable(engineContext, "pollingIntervalSeconds");
                this.alertExists = db.PollingAlertExists(engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID);

                this.linkedActivityInstance = engineContext.CurrenActivity.WaitInstanceID;
                this.waitItemWFE = db.GetWaitItem(engineContext.WorkflowModel.InstanceID, this.linkedActivityInstance);
                if (this.waitItemWFE == null)
                {
                    string errMsg = string.Format("{0} Cannot find WaitItem: instanceID:'{1}', linkedActivity:'{2}' ",
                        base.getWorkflowLoggingContext(engineContext), engineContext.WorkflowModel.InstanceID, this.linkedActivityInstance);
                    logger.Error(errMsg);
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, errMsg);
                }
                if (string.IsNullOrWhiteSpace(this.waitItemWFE.AWI_Config))
                {
                    string errMsg = string.Format("{0} WaitItem: instanceID:'{1}', linkedActivity:'{2}' has empty AWI_Config.",
                        base.getWorkflowLoggingContext(engineContext), engineContext.WorkflowModel.InstanceID, this.linkedActivityInstance);
                    logger.Error(errMsg);
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, errMsg);
                }

                try
                {
                    this.awiCfgDoc = XDocument.Parse(this.waitItemWFE.AWI_Config);
                }
                catch (XmlException xmlEx)
                {
                    string errMsg = string.Format("{0} WaitItem: instanceID:'{1}', linkedActivity:'{2}' error parsing the AWI_Config xml.\r\nError was:{3}\r\nXml was:\r\n{4}",
                        base.getWorkflowLoggingContext(engineContext), engineContext.WorkflowModel.InstanceID, this.linkedActivityInstance, xmlEx.Message, this.waitItemWFE.AWI_Config);
                    logger.Error(errMsg);
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, errMsg);
                }
                catch (Exception ex)
                {
                    string errMsg = string.Format("{0} WaitItem: instanceID:'{1}', linkedActivity:'{2}' general parse error.",
                        base.getWorkflowLoggingContext(engineContext), engineContext.WorkflowModel.InstanceID, this.linkedActivityInstance);
                    logger.Error(errMsg + Environment.NewLine + ex.Message);
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, errMsg);
                }

                this.navStatus = awiCfgDoc.XPathSelectElement("/root/item[@name='status']").Attribute("value").Value;
                this.navOldStatus = awiCfgDoc.XPathSelectElement("/root/item[@name='oldStatus']").Attribute("value").Value;
                this.ticketNumber = awiCfgDoc.XPathSelectElement("/root/item[@name='ticketNumber']").Attribute("value").Value;
                this.timeoutString = awiCfgDoc.XPathSelectElement("/root/item[@name='timeoutPeriodBeendet']").Attribute("value").Value;
                this.isEmpty = string.IsNullOrWhiteSpace(this.navStatus) && string.IsNullOrWhiteSpace(this.ticketNumber);
                this.isNoChange = !string.IsNullOrWhiteSpace(this.navStatus) && this.navStatus == this.navOldStatus;
                this.changeToAngelegt = this.navStatus.ToLowerInvariant() == EnumNavisionTicketStatus.ANGELEGT.ToString().ToLowerInvariant() && this.navStatus != this.navOldStatus;
                this.changeToBeendet = this.navStatus.ToLowerInvariant() == EnumNavisionTicketStatus.BEENDET.ToString().ToLowerInvariant() && this.navStatus != this.navOldStatus;
                this.changeToStorno = navStatus.ToLowerInvariant() == EnumNavisionTicketStatus.STORNO.ToString().ToLowerInvariant() && navStatus != navOldStatus;
            }
            catch (BaseException bEx)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " unexpected error", bEx);
                result.StepState = EnumStepState.ErrorToHandle;
                result.ReturnValue = bEx.Message;
                result.DetailedDescription = bEx.ToString();
            }
            catch (Exception ex)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " unexpected error", ex);
                result.StepState = EnumStepState.ErrorToHandle;
                result.ReturnValue = ex.Message;
                result.DetailedDescription = ex.ToString();
            }
            finally
            {
                db.Dispose();
            }

            // uiui böse
            result = this.doRun(engineContext);
            return result;
        }


        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn retStep = new StepReturn("", EnumStepState.Complete);
            return retStep;
        }
        /// <summary>
        /// 1) first time here create an engine alert with "polling" flag and -schedule and leave
        ///    only poll from second time onwards (alert already exsits)ummary>
        ///    
        /// 2) get polling result
        /// 
        /// 3) 
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public StepReturn doRun(EngineContext engineContext)
        {
            this.database = new DatabaseAccess();
            this.transaction = database.StartTransaction();

            this.engineContext = engineContext;
            StepReturn retStep = new StepReturn("ok", EnumStepState.Wait);

            try
            {
                if (base.AddAlertIfFirstTime(engineContext, this.pollingIntervalSeconds))
                {
                    //
                    // step 1) get polling result
                    //
                    //var navWaitStatus = this.getNavTicketPollingResult();
                    AsyncNavReqResult navWaitStatus;
                    BaseAsyncRequestResult resultDummy;
                    bool isResultAvail = this.isResultAvailable(null, out resultDummy); //updates AWIItem, transactional
                    navWaitStatus = (AsyncNavReqResult) resultDummy;

                    logger.Debug(string.Format("{0} : polling result = [ state={1} ]", base.getWorkflowLoggingContext(engineContext), navWaitStatus.NavisionTicketStatus.ToString()));
                    //
                    // step 2) handle return status
                    //
                    switch (navWaitStatus.NavisionTicketStatus)
                    {
                        case EnumNavTicketPollingStatus.angelegt:
                            //update engine alert -> last polling date
                            this.database.UpdateLastPolling(this.transaction, engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID, DateTime.Now);
                            logger.Debug(string.Format("{0} : updated Engine Alert.", base.getWorkflowLoggingContext(engineContext)));
                            engineContext.SetActivityVariable("ticketNumber", this.ticketNumber); // = ticketNumber
                            logger.Debug(
                                string.Format("{0} : Navision ticketNumber after status angelegt={1}.",
                                    base.getWorkflowLoggingContext(engineContext),
                                    this.ticketNumber
                                )
                            );

                            engineContext.SetActivityVariable("returnStatus", EnumNavTicketPollingStatus.angelegt.ToString());
                            retStep = new StepReturn("", EnumStepState.Wait);
                            this.transaction.Commit();
                            break;

                        case EnumNavTicketPollingStatus.storno:
                        case EnumNavTicketPollingStatus.beendet:
                            this.database.FinishEngineAlert(this.transaction, engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID);
                            logger.Debug(string.Format("{0} : finished Engine Alert.", base.getWorkflowLoggingContext(engineContext)));
                            this.database.FinishWaitItem(this.transaction, this.waitItemWFE.AWI_ID);
                            logger.Debug(string.Format("{0} : finished Wait Item.", base.getWorkflowLoggingContext(engineContext)));
                            engineContext.SetActivityVariable("returnStatus", navWaitStatus.NavisionTicketStatus.ToString());
                            retStep = new StepReturn("", EnumStepState.Complete);
                            this.transaction.Commit();
                            break;

                        // tell workflow to wait
                        case EnumNavTicketPollingStatus.empty:
                        case EnumNavTicketPollingStatus.nochange:
                            //update engine alert -> last polling date
                            this.database.UpdateLastPolling(this.transaction, engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID, DateTime.Now);
                            logger.Debug(string.Format("{0} : updated Engine Alert.", base.getWorkflowLoggingContext(engineContext)));
                            logger.Debug(
                                string.Format("{0} : no change since last polling. current status='{1}'; Navision ticketNumber={2}.",
                                    base.getWorkflowLoggingContext(engineContext),
                                    this.navStatus,
                                    this.ticketNumber
                                )
                            );
                            retStep = new StepReturn("", EnumStepState.Wait);
                            this.transaction.Commit();
                            break;

                        case EnumNavTicketPollingStatus.timeoutAngelegt:
                            this.database.FinishEngineAlert(this.transaction, engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID);
                            logger.Debug(string.Format("{0} : finished Engine Alert because of timeoutAngelegt.", base.getWorkflowLoggingContext(engineContext)));
                            engineContext.SetActivityVariable("returnStatus", EnumNavTicketPollingStatus.timeoutAngelegt.ToString());
                            logger.Debug(
                                string.Format("{0} : timoutAngelegt occured. Webeinmeldung was not converted to Incident in time. Workflow set to Pause mode.",
                                    base.getWorkflowLoggingContext(engineContext)
                                )
                            );
                            retStep = new StepReturn("", EnumStepState.Paused); // stop polling, wait for user to activate again
                            this.transaction.Commit();
                            break;

                        case EnumNavTicketPollingStatus.timeoutBeendet:
                            this.database.FinishEngineAlert(this.transaction, engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID);
                            logger.Debug(string.Format("{0} : finished Engine Alert because of timeoutBeendet.", base.getWorkflowLoggingContext(engineContext)));
                            engineContext.SetActivityVariable("returnStatus", EnumNavTicketPollingStatus.timeoutAngelegt.ToString());
                            logger.Debug(
                                 string.Format("{0} : timeoutBeendet occured. Incident was not beendet or storno in time. Workflow set to Pause mode. current status='{1}'; Navision ticketNumber={2}.",
                                     base.getWorkflowLoggingContext(engineContext)
                                     , this.navStatus
                                     , this.ticketNumber
                                 )
                            );
                            retStep = new StepReturn("", EnumStepState.Paused); // stop polling, wait for user to activate again
                            this.transaction.Commit();
                            break;

                        default:
                        case EnumNavTicketPollingStatus.error:
                            retStep = new StepReturn("", EnumStepState.ErrorStop);
                            this.transaction.Rollback();
                            break;

                    }
                }

            }//end try
            catch (Exception ex)
            {
                this.transaction.Rollback();
                return this.logErrorAndReturnStepState(engineContext, ex, "Error in NavisionTicketActivityWait", EnumStepState.ErrorStop);
            }
            finally
            {
                this.database.Dispose();
                this.transaction.Dispose();
            }

            return retStep;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        public override StepReturn HandleReminder(EngineContext engineContext, BaseAsyncRequestResult baseResult, bool resultAvailable)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        /// <summary>
        /// go check if rückmelde WebSrv has written something (new) into table
        /// </summary>
        /// <returns></returns>
        public override bool isResultAvailable(string AsyncRequestID, out BaseAsyncRequestResult AsyncRequestResult)
        {
            if (this.waitItemWFE != null)
            {
                //
                // check if task is overdue
                //
                if (this.waitItemWFE.AWI_DueDate != null && DateTime.Now > this.waitItemWFE.AWI_DueDate.Value)
                {
                    if (string.IsNullOrWhiteSpace(this.navStatus))
                    {
                        // remove duedate
                        this.waitItemWFE.AWI_DueDate = null;
                        this.database.UpdateWaitItem(this.transaction, engineContext.WorkflowModel.InstanceID, this.linkedActivityInstance, this.waitItemWFE);
                        AsyncRequestResult = new AsyncNavReqResult("timeoutAngelegt", EnumNavTicketPollingStatus.timeoutAngelegt);
                        return true;
                    }

                    if (!string.IsNullOrWhiteSpace(this.ticketNumber))
                    {
                        //
                        // ticket is overdue
                        // set status timeoutBeendet
                        // in case ticket has been finished (beendet) ignore duedate and finish polling/ticket
                        //
                        if (string.Equals(this.navStatus, EnumNavisionTicketStatus.BEENDET.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            AsyncRequestResult = new AsyncNavReqResult(this.navStatus, EnumNavTicketPollingStatus.beendet);
                        }
                        else if (string.Equals(this.navStatus, EnumNavisionTicketStatus.STORNO.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            AsyncRequestResult = new AsyncNavReqResult(this.navStatus, EnumNavTicketPollingStatus.storno);
                        }
                        else
                        {
                            AsyncRequestResult = new AsyncNavReqResult("", EnumNavTicketPollingStatus.timeoutBeendet);
                        }
                        return true;
                    }
                }

                //
                //  as long as nothing happened but not a timout
                //
                if (this.isEmpty)
                {
                    AsyncRequestResult = new AsyncNavReqResult("", EnumNavTicketPollingStatus.empty);
                    return true;
                }
                //
                // nochange
                //
                if (this.isNoChange)
                {
                    AsyncRequestResult = new AsyncNavReqResult(this.navStatus, EnumNavTicketPollingStatus.nochange);
                    return true;
                }
                //
                // change von irgendwas auf ANGELEGT
                //
                if (this.changeToAngelegt)
                {
                    // set old status for change tracking
                    awiCfgDoc.XPathSelectElement("/root/item[@name='oldStatus']").Attribute("value").SetValue(navStatus);

                    // set second timeout (duedate) for ticket finished (beendet or storno)
                    DateTime timeoutBeendet;
                    if (!DateTime.TryParse(this.timeoutString, out timeoutBeendet))
                    {
                        timeoutBeendet = DateTime.Now.AddDays(7);
                    }
                    //
                    // save to db
                    //
                    this.waitItemWFE.AWI_DueDate = timeoutBeendet;
                    this.waitItemWFE.AWI_Config = awiCfgDoc.ToString(SaveOptions.None);
                    this.database.UpdateWaitItem(this.transaction, engineContext.WorkflowModel.InstanceID, linkedActivityInstance, this.waitItemWFE);
                    AsyncRequestResult = new AsyncNavReqResult(this.ticketNumber, EnumNavTicketPollingStatus.angelegt);
                    return true;
                }

                //
                // chg auf BEENDET
                //
                if (this.changeToBeendet)
                {
                    awiCfgDoc.XPathSelectElement("/root/item[@name='oldStatus']").Attribute("value").SetValue(navStatus);
                    this.database.UpdateWaitItem(this.transaction, engineContext.WorkflowModel.InstanceID, linkedActivityInstance, this.waitItemWFE);

                    AsyncRequestResult = new AsyncNavReqResult(navStatus, EnumNavTicketPollingStatus.beendet);
                    return true;
                }
                //
                // chg auf STORNO
                //
                if (this.changeToStorno)
                {
                    awiCfgDoc.XPathSelectElement("/root/item[@name='oldStatus']").Attribute("value").SetValue(navStatus);
                    this.database.UpdateWaitItem(this.transaction, engineContext.WorkflowModel.InstanceID, linkedActivityInstance, this.waitItemWFE);

                    AsyncRequestResult = new AsyncNavReqResult(navStatus, EnumNavTicketPollingStatus.storno);
                    return true;
                }

                AsyncRequestResult = new AsyncNavReqResult("falling through", EnumNavTicketPollingStatus.nochange);
                return true;

            }
            else // no wait item found
            {
                string errMsg = string.Format("{0} error finding linked NavisionTaskActivity (linkedTo = '{1}')",
                    base.getWorkflowLoggingContext(engineContext), linkedActivityInstance);
                logger.Error(errMsg);
                AsyncRequestResult = new AsyncNavReqResult(errMsg, EnumNavTicketPollingStatus.error);
                return false;
            }

        }

        #region helpers

        #endregion

        #region Validation
        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }
        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}

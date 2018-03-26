using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Serialiser;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.WFActivity.HandleChangeEquipments
{
    public class HandleChangeEquipmentsActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        // needed 0.Variables, bzw. activity properties
        private Variable ChangeEquipmentInfos;
        private Variable RequestingPersonEmploymentGuid;
        private Variable EffectedPersonEmploymentGuid;
        private Variable NewCostCenterGuid;
        private string RequestingPersonGuid = null;
        private Variable wfVariableTargetDate;
        private DateTime TargetDate;
        private bool ChangeHasNewEmployment;
        private string NewEmploymentType;
        private Variable NewLocation;

        private string resultedEqMoves = "";
        private Variable NewEmploymentGuid;

        private EngineContext engineContextField;

        public HandleChangeEquipmentsActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        { }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            EmploymentHandler emplH = new EmploymentHandler();
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            try
            {
                this.wfVariableTargetDate = base.GetProcessedActivityVariable(engineContext, "TargetDate", false);
                if (!string.IsNullOrWhiteSpace(this.wfVariableTargetDate.VarValue))
                {
                    this.TargetDate = this.wfVariableTargetDate.GetDateValue().Value;
                }
                else
                {
                    string errMsg = " Error parsing TargetDate date: " + this.wfVariableTargetDate.VarValue ?? "" + " continue without Error2Handle";
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + errMsg);
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, errMsg);
                }


                this.RequestingPersonEmploymentGuid = base.GetProcessedActivityVariable(engineContext, "RequestingPersonEmploymentGuid", false);
                this.EffectedPersonEmploymentGuid = base.GetProcessedActivityVariable(engineContext, "EffectedPersonEmploymentGuid", false);

                this.NewCostCenterGuid = base.GetProcessedActivityVariable(engineContext, "NewCostCenterGuid", true);
                this.NewLocation = base.GetProcessedActivityVariable(engineContext, "GuidLocation", true);

                Variable chgHasNewEmplVariable;
                //checking this change is done to a new employment or we do just change an existant one
                chgHasNewEmplVariable = base.GetProcessedActivityVariable(engineContext, "ChangeHasNewEmployment", true);
                if (chgHasNewEmplVariable != null)
                {
                    try
                    {
                        this.ChangeHasNewEmployment = (bool)chgHasNewEmplVariable.GetBooleanValue();
                    }
                    catch (Exception ex)
                    {
                        string msg = "Exception thrown when trying to get workflowvariable ChangeHasNewEmployment";
                        return this.logErrorAndReturnStepState(engineContext, ex, msg + ": " + ex.Message, EnumStepState.ErrorToHandle);
                    }
                }
                else
                {
                    string msg = "error trying to get workflowvariable ChangeHasNewEmployment";
                    return this.logErrorAndReturnStepState(engineContext, null, msg, EnumStepState.ErrorToHandle);
                }

                if (ChangeHasNewEmployment)
                {
                    this.NewEmploymentGuid = base.GetProcessedActivityVariable(engineContext, "NewEmploymentGuid", false);
                    EMDEmployment newEmpl = (EMDEmployment)emplH.GetObject<EMDEmployment>(this.NewEmploymentGuid.GetStringValue());
                    this.NewEmploymentType = newEmpl.ET_Guid;
                }

                this.ChangeEquipmentInfos = base.GetProcessedActivityVariable(engineContext, "ChangeEquipmentInfos", true);

                EMDEmployment reqEmpl = (EMDEmployment)emplH.GetObject<EMDEmployment>(this.RequestingPersonEmploymentGuid.VarValue);
                if (reqEmpl != null)
                {
                    this.RequestingPersonGuid = reqEmpl.P_Guid;
                }
                // header of csv list
                this.resultedEqMoves = "obreGuid;eqdeGuid;eqName;action;keep;available|||";
            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext, bEx, bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error trying to get workflowvariables";
                return this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            string resultedWorkflow = "";
            bool isEqAvailable = false;
            //WorkflowHandler wfH = new WorkflowHandler();
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            this.engineContextField = engineContext;

            CoreTransaction transaction = new CoreTransaction();
            transaction.Begin();
            try
            {
                EquipmentDefinitionHandler eqdeH = new EquipmentDefinitionHandler(transaction);
                //
                // list of EQ infos
                //
                if (this.ChangeEquipmentInfos.VarValue.StartsWith("\"")) this.ChangeEquipmentInfos.VarValue = this.ChangeEquipmentInfos.VarValue.Remove(0, 1);
                if (this.ChangeEquipmentInfos.VarValue.EndsWith("\"")) this.ChangeEquipmentInfos.VarValue = this.ChangeEquipmentInfos.VarValue.Remove(this.ChangeEquipmentInfos.VarValue.Length - 1, 1);
                if (this.ChangeEquipmentInfos != null)
                {
                    XElement xElEqInfo = XElement.Parse(this.ChangeEquipmentInfos.VarValue);
                    var removeEqInfos = xElEqInfo.XPathSelectElements("/RemoveEquipmentInfo");

                    foreach (var eqInfoElement in removeEqInfos)
                    {
                        RemoveEquipmentInfo changeEQInfos = XmlSerialiserHelper.DeserialiseFromXml<RemoveEquipmentInfo>(eqInfoElement.ToString());
                        DateTime tDate = changeEQInfos.DateOfAction != null ? changeEQInfos.DateOfAction.Value : DateTime.Now;

                        FilterManager firuM = new FilterManager(changeEQInfos.EquipmentDefinitionGuid);
                        isEqAvailable = true; //remove check here sin firuM.CheckRule(this.firuCrit);

                        if (ChangeHasNewEmployment)
                        {
                            // NEW EMPLOYMENT 

                            if (changeEQInfos.DoKeep) //only do change anything if a new employment was created
                            {
                                // keep : YES

                                if (isEqAvailable)
                                {
                                    //eq available -> start change workflow for EQ as subworkflow
                                    logger.Debug("New Employment => Found EQ to be kept => Start Change Workflow for " + changeEQInfos.ObreGuid);
                                    resultedWorkflow = StartChangeKeepEquipmentWorkflow(engineContext, transaction, changeEQInfos);
                                }
                                else
                                {
                                    //eq NOT available
                                    logger.Debug("New Employment => Found EQ to be kept => not available => Start Remove Workflow for " + changeEQInfos.ObreGuid);
                                    resultedWorkflow = StartRemoveEquipmentWorkflow(transaction, changeEQInfos);
                                }
                            }
                            else
                            {
                                // keep : NO
                                logger.Debug("new Employment => Found EQ to be ignored and kept on old employment for " + changeEQInfos.ObreGuid);
                                // no new employment and keep is set => just keep equipment and do nothing
                                resultedWorkflow = "ignore";
                            }
                        }
                        else
                        {
                            // NO NEW EMPLOYMENT 
                            if (changeEQInfos.DoKeep == false)
                            {
                                // DO NOT KEEP
                                // no new employment but no keep => remove Equipment
                                logger.Debug("No new Employment but no keep => Found EQ to be removed for " + changeEQInfos.ObreGuid);
                                resultedWorkflow = StartRemoveEquipmentWorkflow(transaction, changeEQInfos);
                            }
                            else
                            {
                                // DO KEEP
                                if (isEqAvailable)
                                {
                                    // no new employment but keep => keep Equipment
                                    logger.Debug("No new Employment and keep => Found EQ to be changed for " + changeEQInfos.ObreGuid + " on old employment as new");
                                    resultedWorkflow = StartChangeKeepEquipmentWorkflow(engineContext, transaction, changeEQInfos);
                                }
                                else
                                {
                                    // no new employment and keep but not available => remove Equipment
                                    logger.Debug("No new Employment => Found to be kept => not available => remove" + changeEQInfos.ObreGuid + " on old employment as new");
                                    resultedWorkflow = StartRemoveEquipmentWorkflow(transaction, changeEQInfos);
                                }
                            }
                        }
                        // record result for this EQ
                        EMDEquipmentDefinition eqde = (EMDEquipmentDefinition)eqdeH.GetObject<EMDEquipmentDefinition>(changeEQInfos.EquipmentDefinitionGuid);
                        this.addEqResult(changeEQInfos, isEqAvailable, resultedWorkflow, eqde.Name);
                    }
                }

                transaction.Commit();
            }
            catch (BaseException bEx)
            {
                transaction.Rollback();
                return this.logErrorAndReturnStepState(engineContext, bEx, bEx.Message + " rollback transaction!", EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                string msg = "error trying to get workflowvariables";
                return this.logErrorAndReturnStepState(engineContext, ex, msg + " rollback transaction!", EnumStepState.ErrorToHandle);
            }


            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("", EnumStepState.Complete);

            // write variable with csv list of resulted actions
            engineContext.SetActivityVariable("ResultEqHandling", this.resultedEqMoves, true);

            return result;
        }

        #region Helpers

        /// <summary>
        /// KEEP Equipment process - Starts the workflow for an equipment change process
        /// Catches Error if no workflow mapping was found and sets the Status to ObjectRelationStatus.ACTIVE
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="transaction"></param>
        /// <param name="changeEQInfo"></param>
        /// <returns></returns>
        private string StartChangeKeepEquipmentWorkflow(EngineContext engineContext, CoreTransaction transaction, RemoveEquipmentInfo changeEQInfo)
        {
            //WorkflowHandler wfH = new WorkflowHandler();
            StepReturn result = new StepReturn("Complete", EnumStepState.Complete);

            ObreChangeWorkflowMessage obreChangeWfMsg;
            try
            {
                if (ChangeHasNewEmployment)
                { //Change to new employment
                    obreChangeWfMsg = WorkflowMessageHelper.GetObreChangeWorkflowMessage(
                       transaction: transaction,
                       userGuid: this.RequestingPersonGuid, // requesting person PERS guid
                       effectedPersonEmploymentGuid: this.EffectedPersonEmploymentGuid.VarValue,
                       obreGuid: changeEQInfo.ObreGuid,
                       equipmentDefinitionGuid: changeEQInfo.EquipmentDefinitionGuid,
                       requestingPersEMPLGuid: this.RequestingPersonEmploymentGuid.VarValue,
                       targetDate: this.TargetDate,
                       newEmploymentGuid: this.NewEmploymentGuid.VarValue,
                       businessCase: base.BusinessCase,
                       changeType: base.ChangeType);

                }
                else
                { //Change to old employment
                    obreChangeWfMsg = WorkflowMessageHelper.GetObreChangeWorkflowMessage(
                       transaction: transaction,
                       userGuid: this.RequestingPersonGuid, // requesting person PERS guid
                       effectedPersonEmploymentGuid: this.EffectedPersonEmploymentGuid.VarValue,
                       obreGuid: changeEQInfo.ObreGuid,
                       equipmentDefinitionGuid: changeEQInfo.EquipmentDefinitionGuid,
                       requestingPersEMPLGuid: this.RequestingPersonEmploymentGuid.VarValue,
                       targetDate: this.TargetDate,
                       newEmploymentGuid: this.EffectedPersonEmploymentGuid.VarValue,
                       businessCase: base.BusinessCase,
                       changeType: base.ChangeType); //<-- use the actual employment guid                
                }


                //get all 0.Vars into dictionary
                Dictionary<string, string> allNullVars = new Dictionary<string, string>();
                allNullVars = this.GetNullVariablesIntoDictionary();

                obreChangeWfMsg.CreateWorkflowInstance(allNullVars, this.RequestingPersonGuid, "HandleChangeEquipmentsActivity.StartChangeEquipmentWorkflow()");
            }
            catch (BaseException ex)
            {
                ObjectRelationHandler obreHandler = new ObjectRelationHandler(transaction, this.RequestingPersonGuid);
                EMDObjectRelation emdObre = (EMDObjectRelation)obreHandler.GetObject<EMDObjectRelation>(changeEQInfo.ObreGuid);
                emdObre.Status = (byte)ObjectRelationStatus.STATUSITEM_ACTIVE;
                obreHandler.UpdateObject(emdObre);


                logger.Debug("new Employment => Found EQ to be ignored and kept on old employment for " + changeEQInfo.ObreGuid);
            }
            catch (Exception ex)
            {
                logger.Debug("new Employment => Found EQ to be ignored and kept on old employment for " + changeEQInfo.ObreGuid);
            }

            return "change";
        }


        private void addEqResult(RemoveEquipmentInfo chgEQInfos, bool? isAvailable, string resultedWorkflow, string eqName)
        {
            // 0=obreGuid;1=eqdeGuid;2=eqName;3=action;4=keep;5=available\r\n
            string lineTemplate = "{0};{1};{2};{3};{4};{5}|||";
            // get eqdeName
            string newLine = string.Format(lineTemplate,
                chgEQInfos.ObreGuid,
                chgEQInfos.EquipmentDefinitionGuid,
                eqName,
                resultedWorkflow,
                chgEQInfos.DoKeep.ToString().ToLower(),
                isAvailable != null ? isAvailable.ToString().ToLower() : "n/a"
                );
            this.resultedEqMoves += newLine;
        }

        private string StartRemoveEquipmentWorkflow(CoreTransaction transaction, RemoveEquipmentInfo changeEQInfos)
        {
            string result = "";

            ObreRemoveWorkflowMessage obreRemoveWfMsg;
            obreRemoveWfMsg = WorkflowMessageHelper.GetObreRemoveWorkflowMessage(
                            transaction: transaction,
                            userGuid: this.RequestingPersonGuid,
                            effectedPersonEmploymentGuid: this.EffectedPersonEmploymentGuid.VarValue,
                            obreGuid: changeEQInfos.ObreGuid,
                            equipmentDefinitionGuid: changeEQInfos.EquipmentDefinitionGuid,
                            requestingPersEMPLGuid: this.RequestingPersonEmploymentGuid.VarValue,
                            targetDate: this.TargetDate,
                            businessCase: base.BusinessCase,
                            doKeep: false, // doKeep for ObreRemoveWorkflowMessage is only available in case of offboarding
                            changeType: base.ChangeType);


            //get all 0.Vars into dictionary
            Dictionary<string, string> allNullVars = new Dictionary<string, string>();
            allNullVars = this.GetNullVariablesIntoDictionary();

            obreRemoveWfMsg.CreateWorkflowInstance(allNullVars, this.RequestingPersonGuid, "HandleChangeEquipmentsActivity.StartChangeEquipmentWorkflow()");

            //WorkflowMessageData workflowMessageDataItem = WfHelper.GetWorkflowMessageData(obreRemoveWfMsg);
            //wfH.CreateNewWorkflowInstance(workflowMessageDataItem);

            result = "remove";
            return result;
        }

        /// <summary>
        /// removes any 0. string if at the start of variable
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetNullVariablesIntoDictionary()
        {
            Dictionary<string, string> allNullVars = new Dictionary<string, string>();
            var nullPunkts = (List<Variable>)this.engineContextField.WorkflowModel.GetPunktVariables("[starts-with(@name,'0.')]");
            foreach (var item in nullPunkts)
            {
                if (item.Name.StartsWith("0."))
                    item.Name = item.Name.Substring(2);

                allNullVars.Add(item.Name, item.VarValue);
            }
            return allNullVars;
        }

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

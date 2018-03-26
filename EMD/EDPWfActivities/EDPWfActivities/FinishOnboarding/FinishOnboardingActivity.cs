using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
//using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.Serialiser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.WFActivity.FinishOnboarding
{
    public class FinishOnboardingActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        private EMDOrgUnitRole orro = null;
        private List<EMDContact> newContacts = null;
        private string RequestingPersonGuid = null;

        public FinishOnboardingActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            EmploymentHandler emplH = new EmploymentHandler();

            // get requesting PERS Guid
            Variable tmp = base.GetProcessedActivityVariable(engineContext, "RequestingPersonEmploymentGuid", true);
            if (tmp != null)
            {
                EMDEmployment reqEmpl = (EMDEmployment) emplH.GetObject<EMDEmployment>(tmp.VarValue);
                if (reqEmpl != null)
                {
                    this.RequestingPersonGuid = reqEmpl.P_Guid;
                }
            }
            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            #region input vars
            /* 0.RequestingPersonEmploymentGuid"  
               0.EffectedPersonEmploymentGuid"   
               0.EffectedAccountGuid"            
               0.EffectedOrgUnitGuid"            
               0.ContactsXdoc"                   
               0.EmailType        
               0.NewEquipmentInfos  
              
             */
            #endregion

            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            Variable emplGuid = base.GetProcessedActivityVariable(engineContext, "EffectedPersonEmploymentGuid");

            

            try
            {
                EmploymentHandler emplH = new EmploymentHandler();
                //
                // add contacts to employment
                //
                //ret = this.createEmplContacts(engineContext, emplGuid, ret);
                //
                // Org. Unit Role
                //
                result = this.createOrgUnitRole(engineContext, emplGuid, result);
                //
                // Create an email for person if not exists
                //
                result = this.createMainMailForPerson(engineContext, emplGuid, result);
                //

                //
                // removed b/c gui controller creates users (12.1.2017, chris,wolfg.)
                // ret = this.createUserIdsForEmployment(engineContext, emplGuid, ret);
                //

                // persnr
                //
                result = this.handlePersNumber(engineContext, emplGuid, emplH, result);
                //
                // start subwf for equipments (NewEquipmentInfos)
                //
                Variable newEquipmentInfos = engineContext.GetWorkflowVariable("0.NewEquipmentInfos");
                if (newEquipmentInfos != null)
                {
                    if (!String.IsNullOrEmpty(newEquipmentInfos.VarValue))
                        result = this.StartSubWorkflowsEquipmentAdd(engineContext, emplGuid, result);
                }
                //
                // set employment status
                //
                this.setEmploymentStatus(engineContext, emplH, emplGuid);
                //
                //fill ReturnStatus variable
                //
                engineContext.SetActivityVariable("returnStatus", "Complete");
            }
            catch (Exception ex)
            {
                engineContext.SetActivityVariable("returnStatus", "Error");
                return base.logErrorAndReturnStepState(engineContext, ex, "Error in FinishOnboarding Activity", EnumStepState.ErrorStop);
            }

            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        /// <summary>
        /// create 3 AD users, and mark 00 and 99 as reserved
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="emplGuid"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        //private StepReturn createUserIdsForEmployment(EngineContext engineContext, Variable emplGuid, StepReturn ret)
        //{
        //    CoreTransaction tr = new CoreTransaction();
        //    try
        //    {
        //        UserManager userMgr = new UserManager();
        //        EMDEmployment empl = (EMDEmployment) new EmploymentHandler().GetObject<EMDEmployment>(emplGuid.VarValue);
        //        userMgr.CreateNewUsersToEmployment(empl);
        //        // now sync userid to main empl

        //        tr.Begin();
        //        userMgr.SynchronizeMainEmploymentUsers(emplGuid.VarValue, emplGuid.VarValue, tr);
        //        tr.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        tr.Rollback();
        //        string msg = string.Format("error trying to create Users.");
        //        logger.Error(msg, ex);
        //        ret.ReturnValue = base.getWorkflowLoggingContext(engineContext) + " Error in FinishOnboarding Activity";
        //        ret.StepState = EnumStepState.ErrorStop;
        //        engineContext.SetActivityVariable("ReturnStatus", "Error");
        //        return ret;
        //    }
        //    return ret;
        //}

        private StepReturn handlePersNumber(EngineContext engineContext, Variable emplGuid, EmploymentHandler emplH, StepReturn ret)
        {
            bool isUnder = false;
            string childEnteGuid;
            int emtyCat;
            string countryA2;
            EnterpriseLocationHandler enloH = new EnterpriseLocationHandler();
            EnterpriseHandler enteH = new EnterpriseHandler();

            EMDEmployment empl = (EMDEmployment) emplH.GetObject<EMDEmployment>(emplGuid.VarValue);

            try
            {
                //emtyCategory
                EMDEmploymentType emty = (EMDEmploymentType) new EmploymentTypeHandler().GetObject<EMDEmploymentType>(empl.ET_Guid);
                emtyCat = emty.ETC_ID;

                //countryA2
                EMDEnterpriseLocation enlo = (EMDEnterpriseLocation) enloH.GetObject<EMDEnterpriseLocation>(empl.ENLO_Guid);
                EMDLocation loca = (EMDLocation) new LocationHandler().GetObject<EMDLocation>(enlo.L_Guid);
                EMDCountry cory = (EMDCountry) new CountryHandler().GetObject<EMDCountry>(loca.CTY_Guid);
                countryA2 = cory.Code_A2;
                childEnteGuid = enlo.E_Guid;
            }
            catch (Exception ex)
            {
                string msg = string.Format("error trying to determine if pers task is needed.");
                logger.Error(msg, ex);
                ret.ReturnValue = base.getWorkflowLoggingContext(engineContext) + " Error in FinishOnboarding Activity";
                ret.StepState = EnumStepState.ErrorStop;
                engineContext.SetActivityVariable("ReturnStatus", "Error");
                return ret;
            }

            // parentEnte can be empty or null
            try
            {
                Variable parentEnteGuid = base.GetProcessedActivityVariable(engineContext, "ParentEnteGuid");
                if (!string.IsNullOrWhiteSpace(parentEnteGuid.VarValue))
                    isUnder = enteH.IsEnterpriseUnderParent(parentEnteGuid.VarValue, childEnteGuid, -1);
                else
                    isUnder = false;
            }
            catch (Exception)
            {
                isUnder = false;
            }

            bool NeedPersNrTask = (emtyCat == ((int) EnumEmploymentTypeCategory.Internal))
                                  & (countryA2 == "AT")
                                  & (isUnder == true);

            if (NeedPersNrTask == true)
            {
                // set output variable so workflow can branch off
                engineContext.SetActivityVariable("NeedPersNrTask", "true");
            }
            else
            {
                engineContext.SetActivityVariable("NeedPersNrTask", "false");
                // get persnr from empl
                if (string.IsNullOrWhiteSpace(empl.PersNr))
                {
                    empl.PersNr = empl.EP_ID.ToString();
                }
                else
                {
                    // leave persnr as is
                }
            }
            return ret;
        }

        private StepReturn createMainMailForPerson(EngineContext engineContext, Variable emplGuid, StepReturn ret)
        {

            PersonManager persM = new PersonManager(this.RequestingPersonGuid, "onboarding workflow");

            string pGuid = "";
            Type propType;
            pGuid = new EntityQuery().Query("P_Guid@@" + emplGuid.VarValue, out propType).ToString();
            PersonHandler persH = new PersonHandler();

            // get employmentType for generation E-Mail Adress
            EMDEmployment effectedEmployment = (EMDEmployment) new EmploymentHandler().GetObject<EMDEmployment>(emplGuid.VarValue);
            EMDEmploymentType effectedEmploymentType = (EMDEmploymentType) new EmploymentTypeHandler().GetObject<EMDEmploymentType>(effectedEmployment.ET_Guid);


            EMDPerson emdPers = (EMDPerson) persH.GetObject<EMDPerson>(pGuid);

            if (emdPers != null)
            {
                if (string.IsNullOrWhiteSpace(emdPers.MainMail))
                {
                    try
                    {
                        string oldEmail = emdPers.MainMail;

                        Variable emailType;
                        try
                        {
                            emailType = base.GetProcessedActivityVariable(engineContext, "EmailType");
                        }
                        catch (Exception)
                        {
                            emailType = new Variable("something", "", EnumVariablesDataType.stringType, EnumVariableDirection.input);
                        }
                        bool isExernalEmail = emailType.VarValue.ToString().ToLower() == "extern";
                        string tempMail = persM.CreateMainMailForPerson(ref emdPers, effectedEmploymentType, null, null, null, isExernalEmail);

                        if (tempMail != oldEmail)
                        {
                            //emdPers.MainMail = tempMail;

                            persH.UpdateObject(emdPers);

                        }
                    }
                    catch (Exception)
                    {
                        string msg = base.getWorkflowLoggingContext(engineContext) + "Cannot update person : " + emdPers.Guid + "(" + emdPers.FamilyName + ")";
                        logger.Error(msg);
                        this.RollBackOnboardingContacts(newContacts);
                        this.RollBackOrgUnitRole(orro);
                        ret.StepState = EnumStepState.ErrorStop;
                        ret.ReturnValue = msg;
                    }
                }
                else
                {
                    // do nothing, person already has an email, doesnt change
                }
            }
            else
            {
                // error
                string msg = base.getWorkflowLoggingContext(engineContext) + "Cannot find a Person for Employment : " + emplGuid.VarValue;
                logger.Error(msg);
                ret.StepState = EnumStepState.ErrorStop;
                ret.ReturnValue = msg;
            }
            return ret;
        }

        private StepReturn createEmplContacts(EngineContext engineContext, Variable emplGuid, StepReturn ret)
        {
            Variable contactXml = base.GetProcessedActivityVariable(engineContext, "ContactsXdoc");

            if (contactXml.VarValue.Length < 20)
                return ret;

            if (contactXml != null && emplGuid != null)
            {
                try
                {
                    ContactManager contaMgr = new ContactManager();
                    //remove quotes if there
                    if (contactXml.VarValue.StartsWith("\""))
                        contactXml.VarValue = contactXml.VarValue.Remove(0, 1);
                    if (contactXml.VarValue.EndsWith("\""))
                        contactXml.VarValue = contactXml.VarValue.Remove(contactXml.VarValue.Length - 1, 1);

                    newContacts = contaMgr.AddContactsToEmploymentFromXmlString(emplGuid.VarValue, contactXml.VarValue);
                }
                catch (Exception ex)
                {
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error trying to write Contacts for Employment:" + emplGuid.VarValue + "\r\nContact Data were :\r\n" + contactXml.VarValue, ex);
                    ret.ReturnValue = "Activity Error";
                    ret.StepState = EnumStepState.ErrorStop;
                    return ret;
                }
            }
            else
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in FinishOnboarding Activity.Cannot read ContactsXdoc and EffectedPersonEmploymentGuid");
                ret.ReturnValue = "Activity Error";
                ret.StepState = EnumStepState.ErrorStop;
                return ret;
            }
            return ret;
        }

        private StepReturn createOrgUnitRole(EngineContext engineContext, Variable emplGuid, StepReturn ret)
        {
            try
            {
                Variable orgUnit = base.GetProcessedActivityVariable(engineContext, "EffectedOrgUnitGuid");
                OrgUnitRoleHandler orroH = new OrgUnitRoleHandler();

                string query = string.Format("R_ID = {0} AND EP_Guid = \"{1}\"", OrgUnitRoleHandler.ROLE_ID_PERSON, emplGuid.VarValue);
                var orroList = orroH.GetObjects<EMDOrgUnitRole, OrgUnitRole>(query);
                if (orroList == null || orroList.Count == 0)
                {
                    this.orro = orroH.AddOrgUnitRoleToEmployment(emplGuid.VarValue, orgUnit.VarValue, OrgUnitRoleHandler.ROLE_ID_PERSON);
                }
                else
                {
                    // do nothing, org unit already exists
                }

            }
            catch (Exception)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in FinishOnboarding Activity. Cannot read EffectedOrgUnitGuid");

                this.RollBackOnboardingContacts(this.newContacts);

                ret.ReturnValue = "Activity Error";
                ret.StepState = EnumStepState.ErrorStop;
                return ret;
            }
            return ret;
        }

        private StepReturn StartSubWorkflowsEquipmentAdd(EngineContext engineContext, Variable emplGuid, StepReturn ret)
        {
            //WorkflowHandler wfH = new WorkflowHandler();
            EmploymentManager emplMgr = new EmploymentManager();
            Variable newEquipmentInfos = engineContext.GetWorkflowVariable("0.NewEquipmentInfos");
            Variable reqPersEmplguid = engineContext.GetWorkflowVariable("0.RequestingPersonEmploymentGuid");

            DateTime targetDate;
            try
            {
                Variable tmp = base.GetProcessedActivityVariable(engineContext, "TargetDate");
                targetDate = tmp.GetDateValue().Value;
            }
            catch (Exception)
            {
                targetDate = DateTime.Now;
            }


            if (newEquipmentInfos != null && emplGuid != null)
            {
                CoreTransaction transaction = new CoreTransaction();
                transaction.Begin();
                try
                {
                    EquipmentManager eqMgr = new EquipmentManager();
                    //remove quotes if there
                    if (newEquipmentInfos.VarValue.StartsWith("\""))
                        newEquipmentInfos.VarValue = newEquipmentInfos.VarValue.Remove(0, 1);
                    if (newEquipmentInfos.VarValue.EndsWith("\""))
                        newEquipmentInfos.VarValue = newEquipmentInfos.VarValue.Remove(newEquipmentInfos.VarValue.Length - 1, 1);

                    XElement xElEqInfo = XElement.Parse(newEquipmentInfos.VarValue);
                    var lstEqInfos = xElEqInfo.XPathSelectElements("/NewEquipmentInfo");

                    List<NewEquipmentInfo> newEQList = new List<NewEquipmentInfo>();
                    // create typed list
                    foreach (var eqInfoElement in lstEqInfos)
                    {
                        NewEquipmentInfo newEqInfo = XmlSerialiserHelper.DeserialiseFromXml<NewEquipmentInfo>(eqInfoElement.ToString());
                        newEQList.Add(newEqInfo);
                    }

                    // link packages to employment
                    var distinctPackages = newEQList.Select(x => x.FromTemplateGuid).Distinct().ToList();
                    foreach (var packGuid in distinctPackages)
                    {
                        emplMgr.AddPackageToEmployment(transaction: transaction,
                            employmentGuid: emplGuid.VarValue,
                            packageGuid: packGuid,
                            requestingPersEMPLGuid: reqPersEmplguid.VarValue);
                    }

                    List<ObreAddWorkflowMessage> workflowsToStart = new List<ObreAddWorkflowMessage>();

                    // add each eqde
                    foreach (var eqInfoElement in newEQList)
                    {
                        // add obre and return wf variables
                        ObreAddWorkflowMessage variables = emplMgr.GetWorkflowVariablesForNewEquipmentInstanceEmployment(
                            transaction: transaction,
                            effectedPersonEmploymentGuid: emplGuid.VarValue,
                            eqdeGuid: eqInfoElement.EqdeGuid,
                            ortyGuid: eqInfoElement.OrtyGuid,
                            fromTemplateGuid: eqInfoElement.FromTemplateGuid,
                            requestingPersEMPLGuid: reqPersEmplguid.VarValue,
                            targetDate: targetDate);

                        workflowsToStart.Add(variables);
                    }
                    transaction.Commit();

                    //
                    // start workflows oly after obres are created
                    //
                    ObjectRelationHandler obreH = new ObjectRelationHandler();
                    WorkflowMessageData workflowMessageDataItem = new WorkflowMessageData();
                    foreach (ObreAddWorkflowMessage variables in workflowsToStart)
                    {
                        try
                        {
                            //workflowMessageDataItem = WfHelper.GetWorkflowMessageData(variables);
                            //wfH.CreateNewWorkflowInstance(workflowMessageDataItem);
                            variables.CreateWorkflowInstance(this.RequestingPersonGuid, "FinishOnboardingActivity.StartSubWorkflowsEquipmentAdd()");
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error trying to start onboarding workflows:" + emplGuid.VarValue + "\r\nWorkflowVariables were :\r\n" + workflowMessageDataItem.WorkflowVariables, ex);
                                EMDObjectRelation obre = (EMDObjectRelation) obreH.GetObject<EMDObjectRelation>(variables.ObreGuid);
                                obre.Status = 0; // not set
                                obreH.UpdateObject(obre);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error trying to create Equipments:" + emplGuid.VarValue + "\r\nContact Data were :\r\n" + newEquipmentInfos.VarValue, ex);
                    ret.ReturnValue = "Activity Error";
                    ret.StepState = EnumStepState.ErrorStop;
                    ret.DetailedDescription = ex.ToString();
                    return ret;
                }
            }
            else
            {
                string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error in FinishOnboarding Activity. Cannot read NewEquipmentInfos and EffectedPersonEmploymentGuid";
                logger.Error(errMsg);
                ret.ReturnValue = "Activity Error";
                ret.StepState = EnumStepState.ErrorStop;
                ret.DetailedDescription = errMsg;
                return ret;
            }

            return ret;
        }

        private void setEmploymentStatus(EngineContext engineContext, EmploymentHandler emplH, Variable emplGuid)
        {
            EMDEmployment obre = (EMDEmployment) emplH.GetObject<EMDEmployment>(emplGuid.VarValue);
            obre.Status = 50;
            emplH.UpdateObject(obre, historize: true);
            engineContext.SetActivityVariable("returnStatus", "ok");
        }

        private void RollBackOrgUnitRole(EMDOrgUnitRole orro)
        {
            new OrgUnitRoleHandler().DeleteObject(orro, historize: true);
        }

        private void RollBackOnboardingContacts(List<EMDContact> newContacts)
        {
            if (newContacts.Count < 1)
                return;

            ContactHandler cotaH = new ContactHandler();
            for (int idx = 0; idx < newContacts.Count; idx++)
            {
                cotaH.DeleteObject(newContacts[idx], historize: true);
            }
        }

        #region  Validation
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

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

using Kapsch.IS.EDP.WFActivity.ITAutomationWebService;
using Kapsch.IS.ITAutomation.Shared.XML;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.EDPExports.Entities;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.DB;


namespace Kapsch.IS.EDP.WFActivity.ITAutomation
{
    public class UpdateUserInExportDBTableEDPDataForIT: BaseEDPActivity, IProcessStep, IActivityValidator
    {
        #region Variables

        /// <summary>
        /// Name of the task which is connected to the script started by IT Automation Service
        /// </summary>
        public virtual string TaskName { get { return "UpdateUserInExportDBTableEDPDataForIT"; } }

        /// <summary>
        /// Name of the XML Property for the UserID 
        /// </summary>
        public const string VAR_USERID = "UserID";

        private string UserID;
        

        //private ITAutomationWebService.ServiceITAutomationClient serviceClient;
        //private ITAutomationWebService.ServiceITAutomationClient ServiceClient
        //{
        //    get
        //    {
        //        if (this.serviceClient == null)
        //        {
        //            this.serviceClient = new ITAutomationWebService.ServiceITAutomationClient();
        //        }

        //        return this.serviceClient;
        //    }
        //}
        #endregion

        #region constructor

        public UpdateUserInExportDBTableEDPDataForIT() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
        }
        #endregion

        #region ActivityMethods
        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            try
            {
                Variable tmp;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_USERID, false);
                this.UserID = tmp.VarValue;
            }
            catch (BaseException bEx)
            {
                string msg = "error trying to get Workflow-variables";
                return this.logErrorAndReturnStepState(engineContext, bEx, msg + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error trying to get Workflow-variables";
                return this.logErrorAndReturnStepState(engineContext, ex, msg + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            //set WorkflowmodelInstance as client reference so we can connect a WF to IT AutomationScript
            String clientReference = engineContext.WorkflowModel.InstanceID;
            try
            {
                int moreThanOneMainEmployment = 0;
                DateTime myNow = DateTime.Now;
                PersonManager persManager = new PersonManager();
                EMDPerson person = persManager.GetPersonByUserId(this.UserID);

                EmploymentManager emplManager = new EmploymentManager();
                
                List<EMDEmployment> emplList = emplManager.GetEmploymentsForPerson(person.Guid, true);

                List<EMDEmployment> emplsActive = emplList.Where(item => item.P_Guid == person.Guid && item.LeaveFrom > DateTime.Now && item.LeaveTo > DateTime.Now).ToList();
                List<EMDEmployment> emplsInActive = emplList.Where(item => item.P_Guid == person.Guid && item.LeaveFrom < DateTime.Now && item.LeaveTo > DateTime.Now).ToList();

                ObjectFlagManager objManager = new ObjectFlagManager();

                EDPDataForIT exportItem = null;

                if (emplsInActive.Count > 0)
                {
                    exportItem = CreateExportItem("inactive", person, emplsInActive[0], myNow);
                }
                else if (emplsActive.Count > 0)
                {
                    EMDEmployment mainEmployment = null;
                    foreach (EMDEmployment empl in emplsActive)
                    {
                        //List<EMDObjectFlag> mainEmpl = mainEmpls.Where(item => item.Obj_Guid == empl.Guid).Cast<EMDObjectFlag>().ToList();
                        List<EMDObjectFlag> mainEmpl = objManager.ObjectFlagsByTypeAndObjectGuid(EnumObjectFlagType.MainEmployment, empl.Guid, true);
                        
                        int mainEmplCounter = mainEmpl.Count();
                        if (mainEmplCounter > 1)
                        {
                            //hasMainEmpl = true;
                            moreThanOneMainEmployment += 1;
                            logger.Warn(String.Format("More than on main-employment found for {0}", person.Guid));
                        }
                        else if (mainEmplCounter == 1)
                        {
                            //logger.Error(String.Format("Main-employment found for {0}", person.Guid));
                        }
                        mainEmployment = empl;
                    }
                    if (mainEmployment == null)
                    {
                        mainEmployment = emplsActive[0];
                    }

                    exportItem = CreateExportItem("active", person, mainEmployment, myNow);
                }
                
                //else if (emplsPrepare.Count > 0)
                //{
                //    newRow = createDataRow("prepare", emplsPrepare[0], newRow, myNow, true);
                //}
                //else if (emplsClosed.Count > 0)
                //{
                //    newRow = createDataRow("closed", emplsClosed[0], newRow, myNow, false);
                //}

                //dtOutput.Rows.Add(newRow);

                EDPDataForITHandler exportHandler = new EDPDataForITHandler("EMD_Export");
                exportHandler.CreateOrUpdateItem(exportItem);
                result.ReturnValue = string.Format("Successfully updated user {0} in EDPExport table EDPDataForIT", UserID);
                logger.Debug("resulting ok");
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Error in Activity UpdateUserInExportDBTableEDPDataForIT for {0}", clientReference); 
                result = this.logErrorAndReturnStepState(engineContext, ex, errMsg, EnumStepState.NotCompleted);
            }

            logger.Info(this.TaskName + " Run results: " + result.ReturnValue);
            return result;
        }

        private EDPDataForIT CreateExportItem(string status,EMDPerson person, EMDEmployment empl, DateTime created)
        {
            EDPDataForIT exportItem = new EDPDataForIT();
            exportItem.UserID = this.UserID;

            if ((empl.Entry >= created || empl.FirstWorkDay >= created) && (empl.Exit >= created || empl.LastDay >= created) && ((empl.LeaveFrom > created && empl.LeaveTo > created) || (empl.LeaveFrom < created && empl.LeaveTo < created)))
            {
                status = "prepare";
            }
            else if (((empl.LeaveFrom > created && empl.LeaveTo > created) || (empl.LeaveFrom < created && empl.LeaveTo < created)) && (empl.Entry < created || empl.FirstWorkDay < created) && (empl.Exit > created && empl.LastDay > created))
            {
                status = "active";
            }
            else if (empl.LeaveFrom < created && empl.LeaveTo > created)
            {
                status = "inactive";
            }
            else
            {
                status = "closed";
            }

            exportItem.Status = status;
            //List<EMDPerson> foundPersons = personList.Where(item => item.Guid == empl.P_Guid).ToList();

            exportItem.PersonalNr = empl.PersNr;
            exportItem.EmploymentID = empl.EP_ID;
            exportItem.created = created;

            exportItem.UserID = this.UserID;
            exportItem.FirstName= person.FirstName;
            exportItem.FamilyName = person.FamilyName;
            exportItem.DisplayName = person.Display_FamilyName + " " + person.Display_FirstName;
            exportItem.PersonID = person.P_ID;
            exportItem.Gender = person.Sex;

            UserHandler userHandler = new UserHandler();
            List<EMDUser> userList = new List<EMDUser>();
            userHandler.DeliverInActive = true;
            userList = userHandler.GetObjects<EMDUser, User>().Cast<EMDUser>().ToList();

            EMDUser user = userList.Where(item => item.Guid == person.USER_GUID).FirstOrDefault();
            if (user != null)
            {
                exportItem.UserType = ((EnumUserType)user.UserType).ToString();
                exportItem.UserStatus = ((EnumUserStatus)user.Status).ToString();
            }

            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
            List<EMDEnterpriseLocation> enloList = new List<EMDEnterpriseLocation>();
            enloHandler.DeliverInActive = true;
            enloList = enloHandler.GetObjects<EMDEnterpriseLocation, EnterpriseLocation>().Cast<EMDEnterpriseLocation>().ToList();

            EMDEnterpriseLocation enlo = enloList.Where(item => item.Guid == empl.ENLO_Guid).FirstOrDefault();
            if (enlo != null)
            {
                LocationHandler locaHandler = new LocationHandler();
                List<EMDLocation> locaList = new List<EMDLocation>();
                locaHandler.DeliverInActive = true;
                locaList = locaHandler.GetObjects<EMDLocation, Location>().Cast<EMDLocation>().ToList();

                EMDLocation loca = locaList.Where(item => item.Guid == enlo.L_Guid).FirstOrDefault();
                if (loca != null && loca.EL_ID != null)
                {
                    exportItem.ObjID = (int)loca.EL_ID;
                }

                EnterpriseHandler enteHandler = new EnterpriseHandler();
                List<EMDEnterprise> enteList = new List<EMDEnterprise>();
                enteHandler.DeliverInActive = true;
                enteList = enteHandler.GetObjects<EMDEnterprise, Enterprise>().Cast<EMDEnterprise>().ToList();

                EMDEnterprise ente = enteList.Where(item => item.Guid == enlo.E_Guid).FirstOrDefault();
                if (ente != null)
                {
                    exportItem.CompanyShortName = ente.NameShort;
                }
            }

            EmploymentTypeHandler emplTypeHandler = new EmploymentTypeHandler();
            List<EMDEmploymentType> emplTypeList = new List<EMDEmploymentType>();
            emplTypeHandler.DeliverInActive = true;
            emplTypeList = emplTypeHandler.GetObjects<EMDEmploymentType, EmploymentType>().Cast<EMDEmploymentType>().ToList();

            EMDEmploymentType emplType = emplTypeList.Where(item => item.Guid == empl.ET_Guid).FirstOrDefault();
            if (emplType != null)
            {
                exportItem.EmploymentTypeID = emplType.ET_ID;
            }

            exportItem.Direct = string.Empty;
            exportItem.Mobile = string.Empty;
            exportItem.Phone = string.Empty;
            exportItem.EFax = string.Empty;
            exportItem.Room = string.Empty;

            //EMDContact contDirect = contList.Where(item => item.EP_Guid == empl.Guid && item.C_CT_ID == ContactTypeHandler.DIRECTDIAL).FirstOrDefault();
            //if (contDirect != null)
            //{
            //    target["Direct"] = contDirect.Text;
            //}

            //EMDContact contMobile = contList.Where(item => item.EP_Guid == empl.Guid && item.C_CT_ID == ContactTypeHandler.MOBILE).FirstOrDefault();
            //if (contMobile != null)
            //{
            //    target["Mobile"] = contMobile.Text;
            //}

            //EMDContact contPhone = contList.Where(item => item.EP_Guid == empl.Guid && item.C_CT_ID == ContactTypeHandler.PHONE).FirstOrDefault();
            //if (contPhone != null)
            //{
            //    target["Phone"] = contPhone.Text;
            //}

            //EMDContact contEFax = contList.Where(item => item.EP_Guid == empl.Guid && item.C_CT_ID == ContactTypeHandler.DIRECTEFAX).FirstOrDefault();
            //if (contEFax != null)
            //{
            //    target["eFax"] = contEFax.Text;
            //}

            //EMDContact contRoom = contList.Where(item => item.EP_Guid == empl.Guid && item.C_CT_ID == ContactTypeHandler.ROOM).FirstOrDefault();
            //if (contRoom != null)
            //{
            //    target["Room"] = contRoom.Text;
            //}

            return exportItem;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            // setup return object 
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            result.ReturnValue = "Successfully updated User: " + UserID;
            engineContext.SetActivityVariable("ResultMessage", result.ReturnValue, true);
            return result;
        }
        #endregion



        #region Validation

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }

        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

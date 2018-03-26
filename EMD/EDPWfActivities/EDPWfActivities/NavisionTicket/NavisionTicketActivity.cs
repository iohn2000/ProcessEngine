using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.WFActivity.NavTicketSrv;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.NavisionTicket
{
    public class NavisionTicketActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        //http://s900x250/KSMP_GenV1/KSMP_GenV1.asmx

        private Variable periodAngelegt;
        private Variable periodBeendet;
        private DateTime dueDateAngelegt;
        private string strWaitItemConfig;
        private Variable vAuftraggeberName;
        private Variable vAuftraggeberTelefon;
        private Variable vAuftragsbeschreibung;
        private Variable vEinsatzart;
        private Variable vEmail;
        private Variable vEinsatzartBeschreibung;
        private Variable vLieferungAnPLZ;
        private Variable vLieferungAnLand;
        private Variable vAuftragsbeschreibung3;
        private Variable vQuellsystemNummer;
        private Variable vEmploymentGuid;
        private Variable vPrioritaet;
        //private Variable vAuftraggeberAm;
        //private Variable vAuftraggeberUm;
        private Variable vGeschaeftsfall;
        private StepReturn navisionResponse;
        private int LocationID = 0;

        public NavisionTicketActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }


        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            // setup return object 
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                EntityQuery entityQuery = new EntityQuery();
                Type propType;

                this.periodAngelegt = base.GetProcessedActivityVariable(engineContext, "timeoutPeriodAngelegt", false);
                this.periodBeendet = base.GetProcessedActivityVariable(engineContext, "timeoutPeriodBeendet", false);
                //get all parameter for needed for navision ticket
                this.vAuftraggeberName = base.GetProcessedActivityVariable(engineContext, "AuftraggeberName", false);
                this.vAuftraggeberTelefon = base.GetProcessedActivityVariable(engineContext, "AuftraggeberTelefon", false);
                this.vAuftragsbeschreibung = base.GetProcessedActivityVariable(engineContext, "Auftragsbeschreibung", false);
                this.vEinsatzart = base.GetProcessedActivityVariable(engineContext, "Einsatzart", false);
                this.vEmail = base.GetProcessedActivityVariable(engineContext, "Email", false);
                this.vEinsatzartBeschreibung = base.GetProcessedActivityVariable(engineContext, "EinsatzartBeschreibung", false);
                this.vLieferungAnPLZ = base.GetProcessedActivityVariable(engineContext, "LieferungAnPLZ", false);
                this.vLieferungAnLand = base.GetProcessedActivityVariable(engineContext, "LieferungAnLand", false);
                this.vAuftragsbeschreibung3 = base.GetProcessedActivityVariable(engineContext, "Auftragsbeschreibung3", false);
                this.vQuellsystemNummer = base.GetProcessedActivityVariable(engineContext, "QuellsystemNummer", false);
                this.vEmploymentGuid = base.GetProcessedActivityVariable(engineContext, "EmploymentGuid", false);
                this.vPrioritaet = base.GetProcessedActivityVariable(engineContext, "Prioritaet", false);
                this.vGeschaeftsfall = base.GetProcessedActivityVariable(engineContext, "Geschaeftsfall", false);

                // calc location ID
                //  ( L_EL_ID * 10000 ) + E_ID 
                string e_id_new = entityQuery.Query("E_ID_new@@E_Guid@@ENLO_Guid@@" + this.vEmploymentGuid.VarValue, out propType).ToString();
                string l_id = entityQuery.Query("EL_ID@@L_Guid@@ENLO_Guid@@" + this.vEmploymentGuid.VarValue, out propType).ToString();
                int e = int.Parse(e_id_new);
                int l = int.Parse(l_id);
                this.LocationID = (l * 10000) + e;

            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext,bEx,bEx.Message,EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error trying to get workflowvariables";
                return this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
            }

            try
            {
                this.dueDateAngelegt = this.calcDueDate(engineContext, this.periodAngelegt, 30);
                XDocument xmlWaitItemConfig = new XDocument();
                XElement root = this.buildWaitItemXml(engineContext);
                xmlWaitItemConfig.Add(root);
                this.strWaitItemConfig = xmlWaitItemConfig.ToString(SaveOptions.None);

            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext,bEx,bEx.Message,EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error executing calcDueDate() or buildWaitItemXml()";
                return this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
            }

            return result;
        }



        /// <summary>
        /// 1) Create NAV Ticket
        /// 
        /// 2) Create a async wait item with
        ///    AWI_InstanceID
        ///    AWI_ActivityID
        ///    AWI_Status = Wait
        ///    AWI_DueDate = 2 versions : a) for ANGELEGT = 30 mins and b) BEENDET/STORNO = 2 Wochen??
        /// 
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public override StepReturn Run(EngineContext engineContext)
        {
            // setup return object 
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            base.database = new DatabaseAccess();

            DbContextTransaction transaction = this.database.StartTransaction();
            try
            {
                //
                // create AWI Item
                //
                int waitItemID = base.createAWIItem(engineContext, transaction, this.strWaitItemConfig, this.dueDateAngelegt);
                //
                //do NAV Ticket
                //
                this.navisionResponse = this.doNavisionTicket(engineContext, waitItemID);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    string msg = " : Error creating navision ticket.";
                    if (ex.InnerException != null)
                    {
                        msg += " inner Exception: " + ex.InnerException.Message;
                    }
                    result = this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
                }
                finally
                {
                    transaction.Rollback();
                }
            }
            finally
            {
                database.Dispose();
                transaction.Dispose();
            }

            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            // setup return object 
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            try
            {
                if (this.navisionResponse.StepState == EnumStepState.ErrorToHandle)
                {
                    // failed so return set ReturnValue accordingly
                    engineContext.SetActivityVariable("returnStatus", "error", createNewIfNotExist: true);
                    result.StepState = EnumStepState.ErrorToHandle;
                }
                //
                // all OK
                //
                engineContext.SetActivityVariable("returnStatus", "ok", createNewIfNotExist: true);
            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext,bEx,bEx.Message,EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error setting returnStatus variable.";
                return this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
            }
            return result;
        }

        #region Helpers
        /// <summary>
        /// AWI_Config = xml containing
        ///                         item (name = ticketnumber)
        ///                         item (name = status)
        ///                         item (name = oldStatus)  ...  to check for changes                         
        ///                         item (name = linkedActivity)
        ///                         item (name = timeoutPeriodBeendet)
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        private XElement buildWaitItemXml(EngineContext engineContext)
        {
            XElement root = new XElement("root");

            XElement item = new XElement("item");
            item.SetAttributeValue("name", "ticketNumber");
            item.SetAttributeValue("value", "");
            root.Add(item);

            item = new XElement("item");
            item.SetAttributeValue("name", "status");
            item.SetAttributeValue("value", "");
            root.Add(item);

            item = new XElement("item");
            item.SetAttributeValue("name", "oldStatus");
            item.SetAttributeValue("value", "");
            root.Add(item);

            item = new XElement("item");
            item.SetAttributeValue("name", "linkedActivity");
            item.SetAttributeValue("value", engineContext.CurrenActivity.Instance);
            root.Add(item);

            item = new XElement("item");
            item.SetAttributeValue("name", "timeoutPeriodBeendet");
            // set due date
            string beendetTimeout = this.calcDueDate(engineContext, periodBeendet, 10080).ToString("yyyy-MM-dd HH:mm:ss"); // 1 week default
            item.SetAttributeValue("value", beendetTimeout);
            root.Add(item);
            return root;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        private DateTime calcDueDate(EngineContext engineContext, Variable angelegtPeriod, int defaultPeriod)
        {
            DateTime dueDate = DateTime.Now;
            int timePeriod;

            if (angelegtPeriod != null)
                int.TryParse(angelegtPeriod.VarValue, out timePeriod);
            else
                timePeriod = defaultPeriod;
            dueDate = dueDate.AddMinutes(timePeriod);
            return dueDate;
        }
        private StepReturn doNavisionTicket(EngineContext engineContext, int waitItemID)
        {
            // call ticket service
            StepReturn navResponse = this.CreateNavisionTicket(
                autraggebername: this.vAuftraggeberName.VarValue,
                auftraggeberTelefon: this.vAuftraggeberTelefon.VarValue,
                auftragsBeschreibung: this.vAuftragsbeschreibung.VarValue,
                auftragsBeschreibung3: this.vAuftragsbeschreibung3.VarValue,
                einsatzartBeschreibung: this.vEinsatzartBeschreibung.VarValue,
                email: this.vEmail.VarValue,
                lieferungAnLand: this.vLieferungAnLand.VarValue,
                lieferungAnPLZ: this.vLieferungAnPLZ.VarValue,
                locationID: this.LocationID.ToString(),
                quellSystemNummer: this.vQuellsystemNummer.VarValue,
                partnerTicketID: waitItemID.ToString(),
                priority: this.vPrioritaet.VarValue,
                geschaeftsfall: this.vGeschaeftsfall.VarValue,
                engineContext: engineContext
                );
            return navResponse;
        }
        #endregion

        #region NAV Ticket Functions
        private StepReturn CreateNavisionTicket(string autraggebername, string auftraggeberTelefon, string auftragsBeschreibung,
            string auftragsBeschreibung3, string einsatzartBeschreibung,
            string email, string lieferungAnLand, string lieferungAnPLZ,
            string locationID, string quellSystemNummer, string partnerTicketID,
            string priority, string geschaeftsfall, EngineContext engineContext)
        {
            KSMP_GenV1SoapClient service = new KSMP_GenV1SoapClient();
            TicketRequestTicketRequest request = new TicketRequestTicketRequest();
            TicketRequestResponseTicketResponse response = new TicketRequestResponseTicketResponse();

            List<object> paramList = new List<object>();

            request.Quelle = "EMD-ENGINE";

            paramList.Add(CreateStringVal("AuftraggeberName", autraggebername));
            paramList.Add(CreateStringVal("AuftraggeberTelefon", auftraggeberTelefon));
            paramList.Add(CreateStringVal("Auftragsbeschreibung", auftragsBeschreibung));
            paramList.Add(CreateStringVal("Einsatzart", "PLAN"));
            paramList.Add(CreateStringVal("EMail", email));
            paramList.Add(CreateIntVal("Prioritaet", int.Parse(priority)));
            paramList.Add(CreateStringVal("EinsatzartBeschreibung", einsatzartBeschreibung));
            paramList.Add(CreateStringVal("PartnerTicketID", partnerTicketID));
            paramList.Add(CreateStringVal("Geschaeftsfall", geschaeftsfall));
            paramList.Add(CreateDateTimeVal("AuftraggeberAm", DateTime.Now.Date));
            paramList.Add(CreateDateTimeVal("AuftraggeberUm", DateTime.Now));
            paramList.Add(CreateStringVal("LieferungAnPLZ", lieferungAnPLZ));
            paramList.Add(CreateStringVal("LieferungAnLand", lieferungAnLand));
            paramList.Add(CreateStringVal("Auftragsbeschreibung3", auftragsBeschreibung3));
            paramList.Add(CreateStringVal("QuellsystemNummer", quellSystemNummer));
            paramList.Add(CreateStringVal("LocationID", locationID));

            request.Items = paramList.ToArray();

            string Err = "";
            try
            {
                response = service.TicketRequest(request);
                if (response.success == true)
                {
                    //Log("Navision Ticket created - no Error from Webservice", true);
                    //Log("EMALC - Message: " + Nav_Response.errorMsg.ToString(), true);
                    return new StepReturn("ok", EnumStepState.Complete);
                }
                else
                {
                    Err = "Navision Ticket could not be created - Error: " + response.errorMsg.ToString();
                    return base.logErrorAndReturnStepState(engineContext, null, Err, EnumStepState.ErrorToHandle);
                }
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Unexpected Error trying to create Navision ticket: {0}", ex.ToString());
                return base.logErrorAndReturnStepState(engineContext, ex, errMsg, EnumStepState.ErrorToHandle);
            }
        }


        private TicketRequestTicketRequestStringVal CreateStringVal(string aName, String aValue)
        {
            TicketRequestTicketRequestStringVal sVal1 = new TicketRequestTicketRequestStringVal();
            sVal1.Name = aName;
            sVal1.Value = aValue;

            return sVal1;

        }
        private TicketRequestTicketRequestDateTimeVal CreateDateTimeVal(string aName, DateTime aValue)
        {
            TicketRequestTicketRequestDateTimeVal sVal1 = new TicketRequestTicketRequestDateTimeVal();
            sVal1.Name = aName;
            sVal1.Value = aValue;

            return sVal1;

        }
        private TicketRequestTicketRequestIntVal CreateIntVal(string aName, int aValue)
        {
            TicketRequestTicketRequestIntVal sVal1 = new TicketRequestTicketRequestIntVal();
            sVal1.Name = aName;
            sVal1.Value = aValue;

            return sVal1;
        }
        private TicketRequestTicketRequestAttachment CreateAttachementValue(string fileId, string fileId2, string fileName, long size)
        {
            TicketRequestTicketRequestAttachment att = new TicketRequestTicketRequestAttachment();
            att.FileID = fileId;
            att.FileID2 = fileId2;
            att.FileName = fileName;
            att.FileSize = size;

            return att;
        }
        private TicketRequestTicketRequestAttachment CreateAttachementValue(string fileName, byte[] contents)
        {
            TicketRequestTicketRequestAttachment att = new TicketRequestTicketRequestAttachment();
            att.FileID = "";
            att.FileID2 = "";
            att.FileName = fileName;
            att.FileSize = contents.Length;
            att.FileContents = contents;

            return att;
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

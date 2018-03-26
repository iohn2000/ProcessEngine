using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.Serialiser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.Core.Utils
{
    /// <summary>
    /// A helper method which creates serialized messages for the Processengine
    /// </summary>
    public class WorkflowMessageHelper
    {
        protected static IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Sets the Status of the equipment to ObjectRelationStatus.STATUSITEM_INPROGRESS
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="effectedPersonEmploymentGuid"></param>
        /// <param name="obreGuid"></param>
        /// <param name="requestingPersEMPLGuid"></param>
        /// <returns>WorkflowMessage Object for the Webservice</returns>
        public static ObreRemoveWorkflowMessage GetObreRemoveWorkflowMessage(
            string effectedPersonEmploymentGuid,
            string requestingPersEMPLGuid,
            DateTime targetDate,
            EnumEmploymentChangeType changeType,
            EnumBusinessCase businessCase,

            string userGuid, 
            string obreGuid, 
            string equipmentDefinitionGuid, 
            bool doKeep,
            CoreTransaction transaction
            )
        {
            ObjectRelationHandler obreHandler = new ObjectRelationHandler(transaction, userGuid, "from GetObreRemoveWorkflowMessage");


            EMDObjectRelation emdObre = (EMDObjectRelation)obreHandler.GetObject<EMDObjectRelation>(obreGuid);
            emdObre.Status = (byte)ObjectRelationStatus.STATUSITEM_QUEUED;
            obreHandler.UpdateObject(emdObre);


            // code here to add msg to queue for workflow
            // get mapping for eq
            ProcessMappingHandler prmpH = new ProcessMappingHandler(userGuid);

            //returns a list of variables to be filled
            FilterCriteria c = EmploymentManager.GetFilterCriteria(effectedPersonEmploymentGuid);

            ObreRemoveWorkflowMessage removeEquipmentWorkflowMessage = new ObreRemoveWorkflowMessage();
            removeEquipmentWorkflowMessage = prmpH.GetWorkflowMapping<ObreRemoveWorkflowMessage>(equipmentDefinitionGuid, c);


            removeEquipmentWorkflowMessage.RequestingPersonEmploymentGuid = requestingPersEMPLGuid;
            removeEquipmentWorkflowMessage.EffectedPersonEmploymentGuid = effectedPersonEmploymentGuid;
            removeEquipmentWorkflowMessage.TargetDate = targetDate;
            removeEquipmentWorkflowMessage.ObreGuid = obreGuid;
            removeEquipmentWorkflowMessage.DoKeep = doKeep;

            removeEquipmentWorkflowMessage.BusinessCase = businessCase;
            removeEquipmentWorkflowMessage.ChangeType = changeType;

            return removeEquipmentWorkflowMessage;
        }

        public static ObreChangeWorkflowMessage GetObreChangeWorkflowMessage(
            string effectedPersonEmploymentGuid,
            string requestingPersEMPLGuid,
            DateTime targetDate,
            EnumEmploymentChangeType changeType,
            EnumBusinessCase businessCase,
            string userGuid, 
            string obreGuid, 
            string equipmentDefinitionGuid, 
            string newEmploymentGuid = null,
            CoreTransaction transaction = null)
        {
            ObjectRelationHandler obreHandler = new ObjectRelationHandler(transaction, userGuid);

            EMDObjectRelation emdObre = (EMDObjectRelation)obreHandler.GetObject<EMDObjectRelation>(obreGuid);
            emdObre.Status = (byte)ObjectRelationStatus.STATUSITEM_INPROGRESS;
            obreHandler.UpdateObject(emdObre);


            // code here to add msg to queue for workflow
            // get mapping for eq
            ProcessMappingHandler prmpH = new ProcessMappingHandler(userGuid);

            //returns a list of variables to be filled
            FilterCriteria c = EmploymentManager.GetFilterCriteria(effectedPersonEmploymentGuid);

            ObreChangeWorkflowMessage changeEquipmentWorkflowMessage = new ObreChangeWorkflowMessage();
            changeEquipmentWorkflowMessage = prmpH.GetWorkflowMapping<ObreChangeWorkflowMessage>(equipmentDefinitionGuid, c);

            changeEquipmentWorkflowMessage.NewEmploymentGuid = newEmploymentGuid;
            changeEquipmentWorkflowMessage.RequestingPersonEmploymentGuid = requestingPersEMPLGuid;
            changeEquipmentWorkflowMessage.EffectedPersonEmploymentGuid = effectedPersonEmploymentGuid;
            changeEquipmentWorkflowMessage.TargetDate = targetDate;
            changeEquipmentWorkflowMessage.ObreGuid = obreGuid;

            changeEquipmentWorkflowMessage.BusinessCase = businessCase;
            changeEquipmentWorkflowMessage.ChangeType = changeType;

            return changeEquipmentWorkflowMessage;
        }

        private static string BuildSerializedDefaultEquipmentString(List<DefaultEquipmentInfo> defaultEquipmentInfo)
        {
            XDocument xEqs = new XDocument();
            XElement rootEq = new XElement("DefaultEquipments");
            foreach (DefaultEquipmentInfo nEq in defaultEquipmentInfo)
            {
                XElement e = SerializeDefaultEquipment(nEq);
                rootEq.Add(e);
            }
            xEqs.Add(rootEq);

            return string.Concat(xEqs.Nodes());
        }

        public static XElement SerializeDefaultEquipment(DefaultEquipmentInfo cont)
        {
            XElement contact = new XElement("DefaultEquipmentInfo");

            try
            {
                String xStr = XmlSerialiserHelper.SerialiseIntoXmlString(cont);
                contact = XElement.Parse(xStr);
            }
            catch (Exception exc)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Could not serialize given DefaultEquipmentInfo", exc);
            }

            return contact;
        }


        #region Change Employment



        /// <summary>
        /// Create a EmplChangeWorkflowMessage with set EnumEmploymentChangeType (Organisation or Pause)
        /// </summary>
        /// <param name="effectedPersonEmploymentGuid"></param>
        /// <param name="requestingPersEMPLGuid"></param>
        /// <param name="targetDate"></param>
        /// <param name="changeType"></param>
        /// <param name="businessCase"></param>
        /// <param name="guidCostCenter"></param>
        /// <param name="guidOrgunit"></param>
        /// <param name="guidSponsor"></param>
        /// <param name="personalNumber"></param>
        /// <param name="guidLocation"></param>
        /// <param name="approveEquipments"></param>
        /// <param name="equipmentInfos"></param>
        /// <param name="leaveFrom"></param>
        /// <param name="leaveTo"></param>
        /// <returns></returns>
        public static EmplChangeWorkflowMessage GetEmplSmallChangeWorkflowMessage(
            string effectedPersonEmploymentGuid, 
            string requestingPersEMPLGuid,
            DateTime targetDate,
            EnumEmploymentChangeType changeType,
            EnumBusinessCase businessCase,
            string guidCostCenter,
            string guidOrgunit,
            string guidSponsor,
            string personalNumber, 
            string guidLocation,
            bool approveEquipments,
            List<RemoveEquipmentInfo> equipmentInfos,
            bool moveAllRoles,
            DateTime? leaveFrom = null, 
            DateTime? leaveTo = null)
        {
            EquipmentManager eqdeMgr = new EquipmentManager();
            XDocument xEqs = new XDocument();
            XElement rootEq = new XElement("RemoveEquipments");
            foreach (RemoveEquipmentInfo nEq in equipmentInfos)
            {
                XElement e = eqdeMgr.SerializeToXml(nEq);
                rootEq.Add(e);
            }
            xEqs.Add(rootEq);

            // code here to add msg to queue for workflow
            // get mapping for eq
            ProcessMappingHandler prmpH = new ProcessMappingHandler();

            //returns a list of variables to be filled
            FilterCriteria filterCriteria = EmploymentManager.GetFilterCriteria(effectedPersonEmploymentGuid);


            EmplChangeWorkflowMessage emplChangeWorkflowMessage = prmpH.GetWorkflowMapping<EmplChangeWorkflowMessage>(null, filterCriteria);
            emplChangeWorkflowMessage.ApproveEquipments = approveEquipments;
            emplChangeWorkflowMessage.ChangeType = EnumEmploymentChangeType.Organisation;
            if (leaveFrom.HasValue || leaveTo.HasValue)
            {
                emplChangeWorkflowMessage.ChangeType = EnumEmploymentChangeType.Pause;
            }
            emplChangeWorkflowMessage.BusinessCase = businessCase;
            emplChangeWorkflowMessage.RequestingPersonEmploymentGuid = requestingPersEMPLGuid;
            emplChangeWorkflowMessage.EffectedPersonEmploymentGuid = effectedPersonEmploymentGuid;

            emplChangeWorkflowMessage.TargetDate = targetDate;
            emplChangeWorkflowMessage.GuidCostCenter = guidCostCenter;
            emplChangeWorkflowMessage.GuidOrgUnit = guidOrgunit;

            emplChangeWorkflowMessage.GuidSponsor = guidSponsor;
            emplChangeWorkflowMessage.PersonalNumber = string.IsNullOrEmpty(personalNumber) ? string.Empty : personalNumber;
            emplChangeWorkflowMessage.GuidLocation = string.IsNullOrEmpty(guidLocation) ? string.Empty : guidLocation; ;
            emplChangeWorkflowMessage.EquipmentInfos = string.Concat(xEqs.Nodes());

            emplChangeWorkflowMessage.DateLeaveFrom = leaveFrom;
            emplChangeWorkflowMessage.DateLeaveTo = leaveTo;
            emplChangeWorkflowMessage.MoveAllRoles = moveAllRoles;
            return emplChangeWorkflowMessage;
        }

        public static EmplChangeWorkflowMessage GetEmplSmallChangeWorkflowMessage(
         string effectedPersonEmploymentGuid, 
         string requestingPersEMPLGuid,
         DateTime targetDate,
         EnumEmploymentChangeType changeType,
         EnumBusinessCase businessCase,
         string new_empl_guid, 
         bool approveEquipments,
         List<RemoveEquipmentInfo> equipmentInfos)
        {
            EquipmentManager eqdeMgr = new EquipmentManager();
            XDocument xEqs = new XDocument();
            XElement rootEq = new XElement("RemoveEquipments");
            foreach (RemoveEquipmentInfo nEq in equipmentInfos)
            {
                XElement e = eqdeMgr.SerializeToXml(nEq);
                rootEq.Add(e);
            }
            xEqs.Add(rootEq);

            // code here to add msg to queue for workflow
            // get mapping for eq
            ProcessMappingHandler prmpH = new ProcessMappingHandler();

            //returns a list of variables to be filled
            FilterCriteria filterCriteria = EmploymentManager.GetFilterCriteria(effectedPersonEmploymentGuid);

            EmplChangeWorkflowMessage emplChangeWorkflowMessage = prmpH.GetWorkflowMapping<EmplChangeWorkflowMessage>(null, filterCriteria);
            emplChangeWorkflowMessage.ApproveEquipments = approveEquipments;
            emplChangeWorkflowMessage.ChangeType = changeType;
            emplChangeWorkflowMessage.BusinessCase = businessCase; 

            emplChangeWorkflowMessage.NewEmploymentGuid = new_empl_guid;
            emplChangeWorkflowMessage.RequestingPersonEmploymentGuid = requestingPersEMPLGuid;
            emplChangeWorkflowMessage.EffectedPersonEmploymentGuid = effectedPersonEmploymentGuid;

            emplChangeWorkflowMessage.TargetDate = targetDate;


            emplChangeWorkflowMessage.EquipmentInfos = string.Concat(xEqs.Nodes());


            return emplChangeWorkflowMessage;
        }

        #endregion
    }
}

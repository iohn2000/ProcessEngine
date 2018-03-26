using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.DB;

namespace Kapsch.IS.EDP.Core.Logic
{
    /// <summary>
    /// Manager for EquipmentDefinitionOwners
    /// </summary>
    public class EquipmentDefinitionOwnerManager : BaseManager
    {
        #region Constructors

        public EquipmentDefinitionOwnerManager()
            : base()
        {
        }

        public EquipmentDefinitionOwnerManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public EquipmentDefinitionOwnerManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public EquipmentDefinitionOwnerManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDEquipmentDefinitionOwner Get(string guid)
        {
            EquipmentDefinitionOwnerHandler handler = new EquipmentDefinitionOwnerHandler(this.Transaction);

            return (EMDEquipmentDefinitionOwner)handler.GetObject<EMDEquipmentDefinitionOwner>(guid);
        }

        public EMDEquipmentDefinitionOwner Delete(string guid)
        {
            EquipmentDefinitionOwnerHandler handler = new EquipmentDefinitionOwnerHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDEquipmentDefinitionOwner emdEquipmentDefinitionOwner = Get(guid);
            if (emdEquipmentDefinitionOwner != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDEquipmentDefinitionOwner)handler.DeleteObject<EMDEquipmentDefinitionOwner>(emdEquipmentDefinitionOwner);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The Country with guid: {0} was not found.", guid));
            }
        }

        /// <summary>
        /// Gets the owners of an equipmentDefinition
        /// </summary>
        /// <param name="eqde_guid">equipmentDefinition  guid</param>
        /// <returns><see cref="List{EMDEmployment}"/></returns>
        public List<EMDEmployment> GetOwnersForEquipmentDefinitionAsEmployees(string eqde_guid)
        {
            EquipmentDefinitionOwnerHandler handler = new EquipmentDefinitionOwnerHandler();
            EmploymentHandler emplHandler = new EmploymentHandler();

            return (from owners in handler.GetObjects<EMDEquipmentDefinitionOwner, EquipmentDefinitionOwner>("EQDE_GUID = \"" + eqde_guid + "\"").Cast<EMDEquipmentDefinitionOwner>() join empl in emplHandler.GetObjects<EMDEmployment, Employment>() on owners.EP_GUID equals empl.Guid select empl).Cast<EMDEmployment>().ToList();
        }

        public List<EMDEquipmentDefinitionOwner> GetOwnersForEquipmentDefinition(string eqde_guid)
        {
            EquipmentDefinitionOwnerHandler handler = new EquipmentDefinitionOwnerHandler();
            EmploymentHandler emplHandler = new EmploymentHandler();

            return handler.GetObjects<EMDEquipmentDefinitionOwner, EquipmentDefinitionOwner>("EQDE_GUID = \"" + eqde_guid + "\"").Cast<EMDEquipmentDefinitionOwner>().ToList();
        }

        /// <summary>
        /// Gets all owners that are active
        /// </summary>
        /// <returns><see cref="List{EMDEmployment}"/></returns>
        public List<EMDEmployment> GetAllOwners()
        {
            EquipmentDefinitionOwnerHandler handler = new EquipmentDefinitionOwnerHandler();
            EmploymentHandler emplHandler = new EmploymentHandler();


            List<EMDEmployment> empls =  (from owners in handler.GetObjects<EMDEquipmentDefinitionOwner, EquipmentDefinitionOwner>().Cast<EMDEquipmentDefinitionOwner>() join empl in emplHandler.GetObjects<EMDEmployment, Employment>() on owners.EP_GUID equals empl.Guid select empl).Cast<EMDEmployment>().ToList();
            return empls.Distinct().ToList();
        }



        /// <summary>
        /// Creates an equipmentDefinitionOwner
        /// </summary>
        /// <param name="emdEquipmentDefinitionOwner"></param>
        /// <returns><see cref="EMDEquipmentDefinitionOwner"/></returns>
        public EMDEquipmentDefinitionOwner Create(EMDEquipmentDefinitionOwner emdEquipmentDefinitionOwner)
        {
            EquipmentDefinitionOwnerHandler handler = new EquipmentDefinitionOwnerHandler();
            return (EMDEquipmentDefinitionOwner)handler.CreateObject<EMDEquipmentDefinitionOwner>(emdEquipmentDefinitionOwner);
        }

        /// <summary>
        /// Checks in the person is the owner of the  EquipmentDefinition
        /// </summary>
        /// <param name="eqde_guid">EquipmentDefinition Guid</param>
        /// <param name="pers_guid">Person Guid</param>
        /// <returns></returns>
        public bool IsPersonOwnerOfEquipment(string eqde_guid, string pers_guid)
        {
            EmploymentManager emplManager = new EmploymentManager();
            List<EMDEmployment> empls = emplManager.GetEmploymentsForPerson(pers_guid);
            bool isOwner = false;
            foreach (EMDEmployment empl in empls)
            {
                if (this.IsEmploymentOwnerOfEquipment(eqde_guid, empl.Guid))
                    isOwner = true;
            }
            return isOwner;
        }

        /// <summary>
        /// Checks in the employment is the owner of the  EquipmentDefinition
        /// </summary>
        /// <param name="eqde_guid">EquipmentDefinition Guid</param>
        /// <param name="empl_guid">Employment Guid</param>
        /// <returns>Boolean</returns>
        public bool IsEmploymentOwnerOfEquipment(string eqde_guid, string empl_guid)
        {
            EquipmentDefinitionOwnerHandler equipmentDefinitionOwnerHandler = new EquipmentDefinitionOwnerHandler();
            EquipmentDefinitionHandler equipmentDefinitionHandler = new EquipmentDefinitionHandler();
            if ((from owners in equipmentDefinitionOwnerHandler.GetObjects<EMDEquipmentDefinitionOwner, EquipmentDefinitionOwner>("EP_GUID = \"" + empl_guid + "\" && EQDE_GUID = \"" + eqde_guid + "\"").Cast<EMDEquipmentDefinitionOwner>() join eq in equipmentDefinitionHandler.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>() on owners.EQDE_Guid equals eq.Guid select eq).Cast<EMDEquipmentDefinition>().Count() > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets the equipmentDefinitions for a owner 
        /// </summary>
        /// <param name="pers_guid">Person guid</param>
        /// <returns><see cref="List{EMDEquipmentDefinition}"/></returns>
        public List<EMDEquipmentDefinition> GetEquipmentDefinitionsForOwner(string pers_guid)
        {
            EmploymentManager emplManager = new EmploymentManager();
            List<EMDEmployment> empls = emplManager.GetEmploymentsForPerson(pers_guid);
            EquipmentDefinitionOwnerManager equipmentOwnerManager = new EquipmentDefinitionOwnerManager();
            List<EMDEquipmentDefinition> equipmentDefinitions = new List<EMDEquipmentDefinition>();
            foreach (EMDEmployment empl in empls)
            {
                equipmentDefinitions.AddRange(equipmentOwnerManager.GetEquipmentDefinitionsForOwnerEmployment(empl.Guid));
            }

            return equipmentDefinitions;

        }

        /// <summary>
        /// Gets a list of equipments that the employment is the owner of
        /// </summary>
        /// <param name="empl_guid">Employment guid</param>
        /// <returns><see cref="List{EMDEquipmentDefinition}"/></returns>
        public List<EMDEquipmentDefinition> GetEquipmentDefinitionsForOwnerEmployment(string empl_guid)
        {
            EquipmentDefinitionOwnerHandler handler = new EquipmentDefinitionOwnerHandler();
            EquipmentDefinitionHandler equipmentDefinitionHandler = new EquipmentDefinitionHandler();

            return (from owners in handler.GetObjects<EMDEquipmentDefinitionOwner, EquipmentDefinitionOwner>("EP_GUID = \"" + empl_guid + "\"").Cast<EMDEquipmentDefinitionOwner>() join eq in equipmentDefinitionHandler.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>() on owners.EQDE_Guid equals eq.Guid select eq).Cast<EMDEquipmentDefinition>().ToList();
        }

        public void UpdateOwnersForEquipment(string eqde_guid, List<string> configuredGuids)
        {
            List<EMDEquipmentDefinitionOwner> oldOwners = this.GetOwnersForEquipmentDefinition(eqde_guid);
            //remove employments that are no longer owners
            foreach (EMDEquipmentDefinitionOwner oldOwner in oldOwners.ToList())
            {
                bool oldOwnerIsNewOwner = false;
                foreach (string newOwnerGuid in configuredGuids)
                {
                    if (newOwnerGuid == oldOwner.EP_GUID)
                    {
                        oldOwnerIsNewOwner = true;
                    }
                }
                if (!oldOwnerIsNewOwner)
                {
                    this.Delete(oldOwner.Guid);
                }
            }


            //Add new owners
            foreach (string newOwnerGuid in configuredGuids)
            {
                bool isCurrentOwner = false;
                foreach (EMDEquipmentDefinitionOwner oldOwner in oldOwners.ToList())
                {
                    if (newOwnerGuid == oldOwner.EP_GUID)
                    {
                        isCurrentOwner = true;
                    }
                }
                if (!isCurrentOwner)
                {
                    EMDEquipmentDefinitionOwner newOwner = new EMDEquipmentDefinitionOwner();
                    newOwner.EP_GUID = newOwnerGuid;
                    newOwner.EQDE_Guid = eqde_guid;
                    newOwner.ActiveTo = EMDEquipmentDefinitionOwner.INFINITY;
                    this.Create(newOwner);
                }
            }

        }
    }
}

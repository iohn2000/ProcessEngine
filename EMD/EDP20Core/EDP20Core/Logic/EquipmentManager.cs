using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.DB;
using System.Xml.Linq;
using Kapsch.IS.Util.Serialiser;
using Kapsch.IS.Util.ErrorHandling;
using System.Collections;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ReflectionHelper;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class EquipmentManager
        : BaseManager
    {
        #region Constructors

        public EquipmentManager()
            : base()
        {
        }

        public EquipmentManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public EquipmentManager(string guid_ModifiedBy, string modifiyComment = null)
            : base(guid_ModifiedBy, modifiyComment)
        {
        }

        public EquipmentManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDEquipmentDefinition Get(string guid)
        {
            EquipmentDefinitionHandler eqDefinitionHandler = new EquipmentDefinitionHandler(this.Transaction);

            return (EMDEquipmentDefinition)eqDefinitionHandler.GetObject<EMDEquipmentDefinition>(guid);
        }

        public EMDEquipmentDefinition Delete(string guid)
        {
            EquipmentDefinitionHandler eqDefinitionHandler = new EquipmentDefinitionHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            PersonManager persManager = new PersonManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDEquipmentDefinition emdEquipmentDefinition = Get(guid);
            if (emdEquipmentDefinition != null)
            {
                CategoryEntityManager categoryEntityManager = new CategoryEntityManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                List<EMDCategoryEntity> linkedcategoryEntities = categoryEntityManager.GetCategoryEntitiesForEntity(guid, EnumCategoryType.EquipmentDefinition);

                linkedcategoryEntities.ForEach(item =>
                {
                    categoryEntityManager.Delete(item.Guid);
                });

                EquipmentDefinitionOwnerManager equipmentDefinitionOwnerManager = new EquipmentDefinitionOwnerManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                List<EMDEquipmentDefinitionOwner> linkedOwners = equipmentDefinitionOwnerManager.GetOwnersForEquipmentDefinition(guid);

                linkedOwners.ForEach(item =>
                {
                    equipmentDefinitionOwnerManager.Delete(item.Guid);
                });

                EquipmentDefinitionPriceManager equipmentDefinitionPriceManager = new EquipmentDefinitionPriceManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                List<EMDEquipmentDefinitionPrice> linkedPrices = equipmentDefinitionPriceManager.GetObjectsForEquipment(guid, true);

                linkedPrices.ForEach(item =>
                {
                    equipmentDefinitionPriceManager.Delete(item.Guid);
                });

                Hashtable hashTable = eqDefinitionHandler.GetRelatedEntities(guid);
                ObjectRelationManager obreManager = new ObjectRelationManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                List<EMDObjectRelation> obres = obreManager.GetAllEQsLinkedToEmploymentsForEquipmentDefinition(guid);
                if (obres != null)
                {
                    foreach (EMDObjectRelation relation in obres)
                    {
                        EMDPerson person = persManager.GetPersonByEmployment(relation.Object1);
                        if (person != null)
                        {
                            hashTable.Add(string.Format("{0} - {1} {2}", relation.Object1, person.FamilyName, person.FirstName), 1);
                        }
                        else
                        {
                            hashTable.Add(relation.Object1, 1);
                        }
                    }

                }

                if (hashTable.Count == 0)
                {
                    return (EMDEquipmentDefinition)eqDefinitionHandler.DeleteObject<EMDEquipmentDefinition>(emdEquipmentDefinition);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The Equipment-Definition with guid: {0} was not found.", guid));
            }
        }

        /// <summary>
        /// Task ID 809
        /// </summary>
        /// <param name="filterCrit"></param>
        /// <returns></returns>
        public List<EMDEquipmentDefinition> GetFilteredListOfEquipmentDefinitions(FilterCriteria filterCrit)
        {
            // loop through all equipments
            EquipmentDefinitionHandler eqH = new EquipmentDefinitionHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            FilterRuleHandler frh = new FilterRuleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<EMDEquipmentDefinition> allowedEQDefs = new List<EMDEquipmentDefinition>();

            List<IEMDObject<EMDEquipmentDefinition>> allEQDefs
                = (List<IEMDObject<EMDEquipmentDefinition>>)eqH.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>("1=1");

            //find rule for each container
            foreach (var item in allEQDefs)
            {
                FilterManager fm = new FilterManager(item.Guid, this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                bool allowed = fm.CheckRule(item.Guid, filterCrit);
                if (allowed)
                {
                    EMDEquipmentDefinition newEQDef = (EMDEquipmentDefinition)eqH.GetObject<EMDEquipmentDefinition>(item.Guid);
                    allowedEQDefs.Add(newEQDef);
                }
            }
            return allowedEQDefs;
        }

        public XElement SerializeToXml(NewEquipmentInfo cont)
        {
            XElement contact = new XElement("NewEquipmentInfo");

            try
            {
                String xStr = XmlSerialiserHelper.SerialiseIntoXmlString(cont);
                contact = XElement.Parse(xStr);
            }
            catch (Exception exc)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Could not serialize given NewEquipmentInfo", exc);
            }

            return contact;
        }

        public XElement SerializeToXml(RemoveEquipmentInfo cont)
        {
            XElement contact = new XElement("RemoveEquipmentInfo");

            try
            {
                String xStr = XmlSerialiserHelper.SerialiseIntoXmlString(cont);
                contact = XElement.Parse(xStr);
            }
            catch (Exception exc)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Could not serialize given RemoveEquipmentInfo", exc);
            }

            return contact;
        }

        /// <summary>
        /// check if for given employment the max number of requests for given eqdes is reached 
        /// </summary>
        /// <param name="eqdeGuidList">List of EQDEs to check if still allowed to request for given employment</param>
        /// <param name="emplGuid"></param>
        /// <returns>Dictiondary of eqdes with Tuple(bool,int) -> (isMaxReached, amountRequestedAlready, maxRequestCount) </bool></returns>
        public Dictionary<string, Tuple<bool, int, int>> GetRequestCountForEquipments(List<string> eqdeGuidList, string emplGuid)
        {
            Dictionary<string, Tuple<bool, int, int>> result = new Dictionary<string, Tuple<bool, int, int>>();
            //
            // get all existing equipments OBRE for employment
            //
            List<EMDEquipmentInstance> configuredEQs = new EmploymentManager().GetConfiguredListOfEquipmentIntancesForEmployment(emplGuid);
            var q = from item in configuredEQs
                    group item by item.GetEquipmentDefinition().Guid into g
                    let count = g.Count()
                    orderby count descending
                    where count > 0
                    select new { eqdeGuid = g.Key, Count = count };
            var eqdeCountList = q.ToList();

            foreach (string eqdeGuid in eqdeGuidList)
            {
                Tuple<bool, int, int> outValue;
                bool alreadyInDictionary = result.TryGetValue(eqdeGuid, out outValue);


                if (!alreadyInDictionary)
                {
                    int maxRequests = 0;
                    EMDEquipmentDefinition eqde;
                    //
                    // get configures max requests for this equipment
                    //
                    EquipmentDefinitionHandler eqdeH = new Entities.EquipmentDefinitionHandler();
                    eqde = (EMDEquipmentDefinition)eqdeH.GetObject<EMDEquipmentDefinition>(eqdeGuid);
                    var cfg = eqde.GetEquipmentDefinitionConfig();
                    maxRequests = cfg.MaxNumberAllowedEquipments;
                    //
                    // find this eqde in duplicate list
                    //
                    var dummy = eqdeCountList.Find(dub => dub.eqdeGuid == eqdeGuid);
                    int actualAmountRequested = dummy != null ? dummy.Count : 0;
                    //
                    // add result eqde -> (isMaxRequestReached, ActualRequests)
                    //
                    result.Add(eqdeGuid, new Tuple<bool, int, int>(actualAmountRequested >= maxRequests, actualAmountRequested, maxRequests));
                }
                else
                {
                    // eqdeGuid should only exist once in the list
                    // if duplicate no need to add again
                }
            }
            return result;
        }

        public string GetEquipmentDefinitionGuidFromObre(string obreGuid)
        {
            string result = null;

            try
            {
                ObjectRelationHandler obreH = new ObjectRelationHandler();
                EMDObjectRelation obre = (EMDObjectRelation)obreH.GetObject<EMDObjectRelation>(obreGuid);

                ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler();
                string objectNumber = ortyH.GetObjectNumberWithPrefix(obre.ORTYGuid, "EQDE");

                // now get eqde guid out of obre
                result = (string)ReflectionHelper.GetPropValue(obre, objectNumber);
                if (result == null)
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "could not get eqde from obre:" + obreGuid, ex);
            }

            return result;
        }

        /// <summary>
        /// Gets the EquipmentDefinition for an ObjectRelation Guid
        /// </summary>
        /// <param name="obreGuid"></param>
        /// <returns></returns>
        public EMDEquipmentDefinition GetEquipmentDefinitionFromObre(string obreGuid)
        {
            EMDEquipmentDefinition result = null;

            try
            {
                ObjectRelationHandler obreH = new ObjectRelationHandler();
                EMDObjectRelation obre = (EMDObjectRelation)obreH.GetObject<EMDObjectRelation>(obreGuid);
                result = new EquipmentManager().Get(obre.Object2);
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "could not get eqde from obre:" + obreGuid, ex);
            }

            return result;
        }

        /// <summary>
        /// Gets all equipments with the specified clientReferenceSystemForPrice
        /// </summary>
        /// <param name="clientReferenceSystemForPrice"><see cref="EnumClientReferenceSystemForPrice"/></param>
        /// <returns><see cref="List{EMDEquipmentDefinition}"/></returns>
        public List<EMDEquipmentDefinition> GetEquipmentDefinitionsForClientReferenceSystemForPrice(EnumClientReferenceSystemForPrice clientReferenceSystemForPrice)
        {
            EquipmentDefinitionHandler equipmentDefinitionHandler = new EquipmentDefinitionHandler();
            return equipmentDefinitionHandler.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>("ClientReferenceSystemForPrice = " + (int)clientReferenceSystemForPrice).Cast<EMDEquipmentDefinition>().ToList();
        }

        /// <summary>
        /// Gets all equipment definitiony
        /// </summary>
        /// <param name="deliverInActive"></param>
        /// <returns><seealso cref="List{EMDEquipmentDefinition}"/></returns>
        public List<EMDEquipmentDefinition> GetAllEquipmentDefinitions(bool deliverInActive = false)
        {
            EquipmentDefinitionHandler equipmentDefinitionHandler = new EquipmentDefinitionHandler();
            equipmentDefinitionHandler.DeliverInActive = deliverInActive;
            return equipmentDefinitionHandler.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>().Cast<EMDEquipmentDefinition>().ToList();
        }

    }
}

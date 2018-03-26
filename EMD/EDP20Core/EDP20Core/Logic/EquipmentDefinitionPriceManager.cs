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
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.EDP.Core.Entities.PriceInformation;
using Kapsch.IS.EDP.DataAccess.DB;
using Kapsch.IS.EDP.DataAccess.Entities;

namespace Kapsch.IS.EDP.Core.Logic
{
    /// <summary>
    /// Manager to handle EquipmentDefinitionPrice informations
    /// </summary>
    public class EquipmentDefinitionPriceManager : BaseManager
    {
        #region Constructors
        public EquipmentDefinitionPriceManager()
            : base()
        {
        }

        public EquipmentDefinitionPriceManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public EquipmentDefinitionPriceManager(string guid_ModifiedBy, string modifiyComment = null)
            : base(guid_ModifiedBy, modifiyComment)
        {
        }

        public EquipmentDefinitionPriceManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }
        #endregion Constructors
        /// <summary>
        /// Gets the active equipmentDefinitionPrice for an equipmentDefinition
        /// </summary>
        /// <param name="eqdeGuid"></param>
        /// <returns>Returns the active <see cref="EMDEquipmentDefinitionPrice"/> if one is found in the database, otherwise it returns null.</returns>
        /// <exception cref="BaseException">Throws exception if more the one price information is active</exception>
        public EMDEquipmentDefinitionPrice GetPriceForEquipmentDefinition(string eqdeGuid, bool isFuture = false)
        {
            //TODO: Implement Future
            EquipmentDefinitionHandler eqdeHandler = new EquipmentDefinitionHandler();
            eqdeHandler.DeliverInActive = false;
            string query = string.Format("EQDE_Guid= \"{0}\"", eqdeGuid);
            List<EMDEquipmentDefinitionPrice> listprices = eqdeHandler.GetObjects<EMDEquipmentDefinitionPrice, EquipmentDefinitionPrice>(query).Cast<EMDEquipmentDefinitionPrice>().ToList();
            EMDEquipmentDefinitionPrice price = null;
            if (listprices != null)
            {
                if (listprices.Count == 1)
                {
                    price = listprices.FirstOrDefault();
                }
                else if (listprices.Count > 1)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Found to much active prices for this EquipmentDefinition."); //+guids

                }
            }
            return price;
        }

        public EMDEquipmentDefinitionPrice GetLastPriceForEquipmentDefinition(string eqdeGuid, bool isFuture = false)
        {
            EquipmentDefinitionHandler eqdeHandler = new EquipmentDefinitionHandler(new CoreTransaction());
            eqdeHandler.DeliverInActive = true;
            eqdeHandler.Historical = true;

            string query = string.Format("EQDE_Guid= \"{0}\"", eqdeGuid);
            List<EMDEquipmentDefinitionPrice> listprices = eqdeHandler.GetObjects<EMDEquipmentDefinitionPrice, EquipmentDefinitionPrice>(query).Cast<EMDEquipmentDefinitionPrice>().OrderByDescending(a => a.ValidFrom).ToList();
            EMDEquipmentDefinitionPrice price = null;
            if (listprices != null)
            {
                if (listprices.Count > 1)
                {
                    price = listprices[1]; //.FirstOrDefault(a => a.Guid != a.HistoryGuid);
                }
                else if (listprices.Count > 1)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Found to much active prices for this EquipmentDefinition."); //+guids

                }
            }
            return price;
        }


        /// <summary>
        /// Gets an equipmentDefinitionPrice by its guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Returns the found <see cref="EMDEquipmentDefinitionPrice"/></returns>
        public EMDEquipmentDefinitionPrice Get(string guid)
        {
            EquipmentDefinitionPriceHandler eqDefinitionPriceHandler = new EquipmentDefinitionPriceHandler(this.Transaction);

            return (EMDEquipmentDefinitionPrice)eqDefinitionPriceHandler.GetObject<EMDEquipmentDefinitionPrice>(guid);
        }

        /// <summary>
        /// Creates a new price item for an given EQDE. Internally WriteOrModifyEquipmentDefinitionPrice is called.
        /// </summary>
        /// <param name="price"></param>
        public EMDEquipmentDefinitionPrice Create(EMDEquipmentDefinitionPrice price)
        {
            return this.WriteOrModifyEquipmentDefinitionPrice(price);
        }

        /// <summary>
        /// Deletes the equipmentDefinitionPrice with the guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Returns the deleted <see cref="EMDEquipmentDefinitionPrice"/></returns>
        /// <exception cref="RelatedEntitiesException">Throws exception if related entities are found</exception>
        /// <exception cref="EntityNotFoundException">Throws exception if the  EquipmentDefinitionPrice is not found</exception>
        public EMDEquipmentDefinitionPrice Delete(string guid)
        {
            EquipmentDefinitionPriceHandler handler = new EquipmentDefinitionPriceHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            EMDEquipmentDefinitionPrice emdEquipmentDefinitionPrice = Get(guid);
            if (emdEquipmentDefinitionPrice != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    if (emdEquipmentDefinitionPrice.ActiveFrom > DateTime.Now)
                    {
                        EMDEquipmentDefinitionPrice other = GetObjectsForEquipment(emdEquipmentDefinitionPrice.EQDE_Guid, true).Cast<EMDEquipmentDefinitionPrice>()
                                                  .Where(c => c.Guid != emdEquipmentDefinitionPrice.Guid)
                                                  .SingleOrDefault(); //There are only zero or one additional equipmentDefinitionPrices allowed

                        if (other.ActiveFrom < emdEquipmentDefinitionPrice.ActiveFrom) //other is earlier than emdEquipmentDefinitionPrice
                        {
                            other.ActiveTo = EMDEquipmentDefinitionPrice.INFINITY;
                            handler.UpdateObject(other, allowChangeActive: true);
                        }
                    }
                    return (EMDEquipmentDefinitionPrice)handler.DeleteObject<EMDEquipmentDefinitionPrice>(emdEquipmentDefinitionPrice);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The EquipmentDefinitionPrice with guid: {0} was not found.", guid));
            }
        }



        /// <summary>
        /// Writes the given EMDEquipmentDefinitionPrice to the database. If there is already a EquipmentDefinitionPrice one of the following cases occurs:
        /// <ul>
        /// <li><b>more than or equal to two:</b> An <see cref="BaseException"/> is thrown.</li>
        /// <li><b>exactly one:</b> The <see cref="EMDObject{T}.ActiveTo"/> of the earlier (whether it is the currently written or the existing EquipmentDefinitionPrice) is set to the later EquipmentDefinitionPrice <see cref="EMDObject{T}.ActiveFrom"/> and the new EquipmentDefinitionPrice is written to the database.
        /// In case the existing EquipmentDefinitionPrice is the earlier one ist is updated in the database.</li>
        /// <li><b>no other EquipmentDefinitionPrice:</b>The given EquipmentDefinitionPrice is written to the database.</li>
        /// </ul>
        /// </summary>
        /// <param name="emdEquipmentDefinitionPrice">EquipmentDefinitionPrice to write. Mustn't be null and the <see cref="EMDEquipmentDefinitionPrice.EQDE_Guid"/> has to be set.</param>
        /// <returns>The newly written EquipmentDefinitionPrice, now containing a GUID and some other auto-filled properties.</returns>
        /// <exception cref="BaseException">Is thrown with error-code <see cref="ErrorCodeHandler.E_EDP_BUSINESS_LOGIK"/> if there are to many EquipmentDefinitionPrice for this EquipmentDefinition.</exception>
        /// <seealso cref="EMDObjectHandler.CreateObject{T}(IEMDObject{T}, string, bool)"/>
        public EMDEquipmentDefinitionPrice WriteOrModifyEquipmentDefinitionPrice(EMDEquipmentDefinitionPrice emdEquipmentDefinitionPrice)
        {
            using (EquipmentDefinitionHandler equipmentDefinitionHandler = new EquipmentDefinitionHandler(base.Transaction, base.Guid_ModifiedBy, base.ModifyComment))

            using (EquipmentDefinitionPriceHandler equipmentDefinitionPriceHandler = new EquipmentDefinitionPriceHandler(base.Transaction, base.Guid_ModifiedBy, base.ModifyComment))
            {
                emdEquipmentDefinitionPrice.FillEmptyDates();

                EMDEquipmentDefinition emdEquipmentDefinition = (EMDEquipmentDefinition)equipmentDefinitionHandler.GetObject<EMDEquipmentDefinition>(emdEquipmentDefinitionPrice.EQDE_Guid);
                IEnumerable<EMDEquipmentDefinitionPrice> equipmentDefPrices = GetObjectsForEquipment(emdEquipmentDefinition.Guid, futurize: true).Cast<EMDEquipmentDefinitionPrice>(); //Handler ändern

                EMDEquipmentDefinitionPrice current = null;

                if (equipmentDefPrices.Count() >= 2)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Found to much EquipmentDefinitionPrices for equipmentDefinition {0}", emdEquipmentDefinitionPrice.EQDE_Guid));
                }
                else if (equipmentDefPrices.Count() == 1)
                {
                    EMDEquipmentDefinitionPrice other = equipmentDefPrices.First();
                    if (other.ActiveFrom > emdEquipmentDefinitionPrice.ActiveFrom)
                    {
                        emdEquipmentDefinitionPrice.ActiveTo = other.ActiveFrom;
                    }
                    else
                    {
                        current = other;
                    }
                }
                if (current != null)
                {
                    equipmentDefinitionPriceHandler.DeleteObject(current, dueDate: emdEquipmentDefinitionPrice.ActiveFrom);
                }

                equipmentDefinitionPriceHandler.CreateObject(emdEquipmentDefinitionPrice, datesAreSet: true);
            }
            return emdEquipmentDefinitionPrice;
        }

        public string CreateOrUpdate(EMDEquipmentDefinitionPrice emdEquipmentDefinitionPrice)
        {
            if (!EMDEquipmentDefinitionPrice.IsEMDGuid(emdEquipmentDefinitionPrice.Guid))
            {
                if (emdEquipmentDefinitionPrice.Price != 0 && emdEquipmentDefinitionPrice.BillingPeriod != 0)
                {
                    this.WriteOrModifyEquipmentDefinitionPrice(emdEquipmentDefinitionPrice);
                }
            }
            else
            {
                this.Update(emdEquipmentDefinitionPrice);
            }

            return emdEquipmentDefinitionPrice.Price.ToString();
        }

        public List<EMDEquipmentDefinitionPrice> GetObjectsForEquipment(String eqde_guid, bool futurize = false)
        {
            EquipmentDefinitionPriceHandler equipmentDefinitionPriceHandler = new EquipmentDefinitionPriceHandler();
            List<EMDEquipmentDefinitionPrice> prices = equipmentDefinitionPriceHandler.GetActiveObjectsInInterval<EMDEquipmentDefinitionPrice, EquipmentDefinitionPrice>(DateTime.Now, (futurize ? EMDEquipmentDefinitionPrice.INFINITY : DateTime.Now),
                "EQDE_Guid=\"" + eqde_guid + "\"").Cast<EMDEquipmentDefinitionPrice>().ToList();
            return prices;
        }

        public EMDEquipmentDefinitionPrice GetEquipmentDefinitionPriceForEquipment(string eqde_guid, bool isFuture = false)
        {
            List<EMDEquipmentDefinitionPrice> prices = null;
            EquipmentDefinitionPriceHandler equipmentDefinitionPriceHandler = new EquipmentDefinitionPriceHandler();
            if (isFuture)
            {
                DateTime today = DateTime.Now;
                prices = equipmentDefinitionPriceHandler.GetActiveObjectsInInterval<EMDEquipmentDefinitionPrice, EquipmentDefinitionPrice>(new DateTime(today.Year, today.Month, today.Day).AddDays(1), EMDEquipmentDefinitionPrice.INFINITY, "EQDE_Guid = \"" + eqde_guid + "\"").FindAll(a => a.ActiveFrom > DateTime.Now);
            }
            else
            {
                prices = equipmentDefinitionPriceHandler.GetObjects<EMDEquipmentDefinitionPrice, EquipmentDefinitionPrice>("EQDE_Guid = \"" + eqde_guid + "\"").Cast<EMDEquipmentDefinitionPrice>().ToList();
            }

            if (prices.Count == 1) return (EMDEquipmentDefinitionPrice)prices.First();
            else if (prices.Count > 1)
            {
                IISLogger logger = ISLogger.GetLogger("EquipmentDefinitionPriceManager");
                logger.Warn(String.Format("Data error: more than one EquipmentDefinitionPrice for EQDE_Guid {0}", eqde_guid));
                return (EMDEquipmentDefinitionPrice)prices[0];
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Updates the given EMDEquipmentDefinitionPrice in the database and also adapts the current active EMDEquipmentDefinitionPrice if the updated EMDEquipmentDefinitionPrice is active in the future.
        /// </summary>
        /// <param name="emdEquipmentDefinitionPrice">EMDEquipmentDefinitionPrice to update. Mustn't be null.</param>
        /// <returns>Updated EMDEquipmentDefinitionPrice</returns>
        /// <exception cref="BaseException">Is thrown with error-code <see cref="ErrorCodeHandler.E_EDP_BUSINESS_LOGIK"/> if there is more than one additional EMDEquipmentDefinitionPrice for the equipmentDefinition.</exception>
        /// <seealso cref="EMDObjectHandler.UpdateObject{T}(IEMDObject{T}, bool, bool, bool))"/>
        private EMDEquipmentDefinitionPrice Update(EMDEquipmentDefinitionPrice emdEquipmentDefinitionPrice)
        {
            using (EquipmentDefinitionPriceHandler equipmentDefinitionPriceHandler = new EquipmentDefinitionPriceHandler())
            {
                if (emdEquipmentDefinitionPrice.ActiveFrom > DateTime.Now)
                {
                    //Find currently active EquipmentDefinitionPrice
                    try
                    {
                        EMDEquipmentDefinitionPrice activeCont = GetObjectsForEquipment(emdEquipmentDefinitionPrice.EQDE_Guid)
                                                                        .Cast<EMDEquipmentDefinitionPrice>()
                                                                        .Where(c => c.Guid != emdEquipmentDefinitionPrice.Guid)
                                                                        .SingleOrDefault();

                        if (activeCont != null && !DateTimeHelper.IsDateTimeEqual(activeCont.ActiveTo, emdEquipmentDefinitionPrice.ActiveFrom))
                        {
                            //Change ActiveTo
                            equipmentDefinitionPriceHandler.DeleteObject(activeCont, dueDate: emdEquipmentDefinitionPrice.ActiveFrom);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "The equipment (" + emdEquipmentDefinitionPrice.EQDE_Guid + ") has more than one additional EquipmentDefinitionPrice", ex);
                    }
                }
                return equipmentDefinitionPriceHandler.UpdateObject(emdEquipmentDefinitionPrice, allowChangeActive: true) as EMDEquipmentDefinitionPrice;
            }
        }

        public List<EMDEquipmentDefinitionPrice> GetAllObjects()
        {
            EquipmentDefinitionPriceHandler equipmentDefinitionPriceHandler = new EquipmentDefinitionPriceHandler();
            List<EMDEquipmentDefinitionPrice> prices = equipmentDefinitionPriceHandler.GetObjects<EMDEquipmentDefinitionPrice, EquipmentDefinitionPrice>().Cast<EMDEquipmentDefinitionPrice>().ToList();
            return prices;
        }

        /// <summary>
        /// Get the price info for a specific system and priceReferenceId
        /// </summary>
        /// <param name="clientReferenceSystem"></param>
        /// <param name="idReferencePrice"></param>
        /// <returns></returns>
        public ExternalPriceInfoExtended GetPriceInfo(EnumClientReferenceSystemForPrice clientReferenceSystem, string idReferencePrice)
        {
            ExternalPriceInfoExtended externalPriceInfo = null;

            switch (clientReferenceSystem)
            {
                case EnumClientReferenceSystemForPrice.KBCAccountingItem:

                    ExternalPriceInfo priceInfo = DatabaseConnection.ExternalPriceInfoKbcAccounting.GetPrice(idReferencePrice);
                    if (priceInfo != null)
                    {
                        externalPriceInfo = new ExternalPriceInfoExtended();
                        externalPriceInfo = new ExternalPriceInfoExtended().Initialize(priceInfo);
                    }
                    break;
            }

            return externalPriceInfo;
        }

        /// <summary>
        /// Updates all prices for equipments
        /// </summary>
        /// <param name="clientReferenceSystem"></param>
        public void UpdateAllPricesFromExternalSystem(EnumClientReferenceSystemForPrice clientReferenceSystem)
        {
            ExternalPriceInfoExtended externalPriceInfo = null;

            switch (clientReferenceSystem)
            {
                case EnumClientReferenceSystemForPrice.KBCAccountingItem:

                    // get price infos from external system an make a lookup for all relevant prices
                    List<ExternalPriceInfo> priceInfos = DatabaseConnection.ExternalPriceInfoKbcAccounting.GetPrices();
                    List<ExternalPriceInfoExtended> externalPriceInfosExtended = ExternalPriceInfoExtended.Map(priceInfos);
                    UpdatePriceInfosList(clientReferenceSystem, externalPriceInfosExtended);
                    break;
            }
        }

        /// <summary>
        /// Updates a single equipmentDefintion Price
        /// </summary>
        /// <param name="eqdeGuid"></param>
        /// <param name="clientReferenceSystem"></param>
        /// <param name="idClientReference"></param>
        /// <returns></returns>
        public ExternalPriceInfoExtended UpdatePriceInfo(string eqdeGuid, EnumClientReferenceSystemForPrice clientReferenceSystem, string idClientReference)
        {
            EMDEquipmentDefinitionPrice equipmentDefinitionPrice = GetEquipmentDefinitionPriceForEquipment(eqdeGuid);
            if (equipmentDefinitionPrice == null)
            {
                equipmentDefinitionPrice = new EMDEquipmentDefinitionPrice()
                {
                    EQDE_Guid = eqdeGuid,
                    BillingPeriod = (int)ClientReferenceSystemPriceAttribute.GetAttribute(clientReferenceSystem).Period
                };
            }
            ExternalPriceInfoExtended externalPriceInfoExtended = GetPriceInfo(clientReferenceSystem, idClientReference);
            if (equipmentDefinitionPrice.Price != externalPriceInfoExtended.Price)
            {
                equipmentDefinitionPrice.Price = externalPriceInfoExtended.Price;
                equipmentDefinitionPrice.BillingPeriod = (int)externalPriceInfoExtended.BillingPeriod;
                CreateOrUpdate(equipmentDefinitionPrice);
            }

            return externalPriceInfoExtended;
        }

        private void UpdatePriceInfosList(EnumClientReferenceSystemForPrice clientReferenceSystem, List<ExternalPriceInfoExtended> externalPriceInfosExtended)
        {
            EquipmentDefinitionHandler equipmentDefinitionHandler = new EquipmentDefinitionHandler();
            CoreTransaction coreTransaction = new CoreTransaction();

            List<EMDEquipmentDefinition> foundEquipmentDefinitions = equipmentDefinitionHandler.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>(string.Format("ClientReferenceSystemForPrice = {0}", (int)clientReferenceSystem)).Cast<EMDEquipmentDefinition>().ToList();


            foreach (EMDEquipmentDefinition equipmentDefinition in foundEquipmentDefinitions)
            {
                EMDEquipmentDefinitionPrice equipmentDefinitionPrice = GetEquipmentDefinitionPriceForEquipment(equipmentDefinition.Guid);

                if (equipmentDefinitionPrice == null)
                {
                    equipmentDefinitionPrice = new EMDEquipmentDefinitionPrice()
                    {
                        EQDE_Guid = equipmentDefinition.Guid
                    };
                }

                ExternalPriceInfoExtended externalPriceInfoExtended = externalPriceInfosExtended.SingleOrDefault(a => a.IdClientReference == equipmentDefinition.ClientReferenceIDForPrice);

                if (externalPriceInfoExtended != null)
                {
                    if (equipmentDefinitionPrice.Price != externalPriceInfoExtended.Price)
                    {
                        equipmentDefinitionPrice.Price = externalPriceInfoExtended.Price;
                        equipmentDefinitionPrice.BillingPeriod = (int)externalPriceInfoExtended.BillingPeriod;
                        CreateOrUpdate(equipmentDefinitionPrice);
                    }
                }
            }
        }
    }
}

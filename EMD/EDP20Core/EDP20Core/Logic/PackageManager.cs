using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kapsch.IS.EDP.Core.Utils;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Data.SqlClient;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class PackageManager
        : BaseManager
    {
        //Get logger
        internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string NAME_EMPLOMENT_PACKAGE = "Employment Package";


        #region Constructors

        public PackageManager()
            : base()
        {
        }

        public PackageManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public PackageManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public PackageManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        /// <summary>
        /// use case : create package
        /// </summary>
        /// <param name="newOBCO"></param>
        /// <param name="filterRuleSubSets"></param>
        /// <returns></returns>
        public EMDObjectContainer CreatePackage(EMDObjectContainer newOBCO, List<FilterRuleSubSetForCriteria> filterRuleSubSets)
        {
            //all of this has to be within one transaction
            //   TransactionHandler th = TransactionHandler.Instance;
            CoreTransaction ta = new CoreTransaction();

            try
            {
                ta.Begin();
                // 1a) create objcontainer
                ObjectContainerHandler och = new ObjectContainerHandler(ta, this.Guid_ModifiedBy, this.ModifyComment);
                newOBCO = (EMDObjectContainer)och.CreateObject(newOBCO);
                // 1b) BaseContainer ?

                // 2a) create filter for it
                FilterManager fm = new FilterManager(newOBCO.Guid, ta, this.Guid_ModifiedBy, this.ModifyComment);
                fm.CreateFilterRule(newOBCO.Guid, filterRuleSubSets, ta);

                ta.Commit();
            }
            catch (Exception ex)
            {
                ta.Rollback();
                BaseException be = new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Exception thrown while creating Package.", ex);
                logger.Error("Error created ObjectContainer. Transaction rolled back.", ex);
                throw be;
            }

            return newOBCO;
        }

        /// <summary>
        /// Task ID 806
        /// </summary>
        /// <returns></returns>
        public EMDObjectContainer UpdatePackage(EMDObjectContainer updateOBCO, List<FilterRuleSubSetForCriteria> filterRuleSubSets)
        {
            //all of this has to be within one transaction
            //TransactionHandler th = TransactionHandler.Instance;
            CoreTransaction ta = new CoreTransaction();

            try
            {
                ta.Begin();
                // 1a) update objcontainer
                ObjectContainerHandler obcoH = new ObjectContainerHandler(ta, this.Guid_ModifiedBy, this.ModifyComment);
                obcoH.UpdateObject(updateOBCO);
                // 1b) BaseContainer ?


                if (filterRuleSubSets != null)
                {
                    // 2a) update filter for it
                    // delete old rules
                    FilterManager fm = new FilterManager(updateOBCO.Guid, this.Guid_ModifiedBy, this.ModifyComment);
                    fm.DeleteFilterRule(ta);
                    // create new ones
                    fm.CreateFilterRule(updateOBCO.Guid, filterRuleSubSets, ta);
                }

                ta.Commit();
            }
            catch (Exception ex)
            {
                ta.Rollback();
                logger.Error("Error upating Package(ObjectContainer). Transaction rolled back.", ex);
            }

            return updateOBCO;
        }

        /// <summary>
        /// update, add or delete base packages depending on tickbox
        /// throws BaseException, does the logging
        /// </summary>
        /// <param name="packaageGuid"></param>
        /// <param name="doEnterpriseBasePack"></param>
        /// <param name="doLocationBasePack"></param>
        public void UpdateBasePackage(string packageGuid, bool doEnterpriseBasePack, bool doLocationBasePack, List<FilterRuleSubSetForCriteria> filterSet)
        {
            BaseContainerHandler bacoH = new Entities.BaseContainerHandler(this.Transaction);
            //check if basecontainer for packageGuid exists
            //a) ENTE
            //b) LOCA
            bool baseEnteExists = bacoH.BaseContainerExitsForPackage(packageGuid, "ENTE");
            bool baseLocaExists = bacoH.BaseContainerExitsForPackage(packageGuid, "LOCA");
            this.DoUpdateBasePackage(packageGuid, baseEnteExists, doEnterpriseBasePack, filterSet, "ENTE");
            this.DoUpdateBasePackage(packageGuid, baseLocaExists, doLocationBasePack, filterSet, "LOCA");
        }

        private void DoUpdateBasePackage(string packageGuid, bool alreadyExists, bool doPack, List<FilterRuleSubSetForCriteria> filterSet, string bacoPrefix)
        {
            // a) delete if it exits & doPack = false
            // b) create if not exits & doPack = true
            // c) update if exists & doPack = true;
            // d) do nothing is not exits & doPack = false

            // a)
            if (alreadyExists && !doPack)
            {
                CoreTransaction transi = new CoreTransaction();
                transi.Begin();
                try
                {
                    BaseContainerHandler bacoH = new BaseContainerHandler(transi, this.Guid_ModifiedBy, this.ModifyComment);
                    var baseCoToDelete = bacoH.GetBaseContainer(packageGuid, bacoPrefix);
                    FilterManager firuM = new FilterManager(baseCoToDelete.Guid, transi, this.Guid_ModifiedBy, this.ModifyComment);
                    bacoH.DeleteObject(baseCoToDelete);
                    firuM.DeleteFilterRule(transi);
                    transi.Commit();
                }
                catch (BaseException bEx)
                {
                    transi.Rollback();
                    throw bEx;
                }
                catch (Exception ex)
                {
                    transi.Rollback();
                    string errmsg = string.Format("error trying to delete base container. obcoguid={0} bacoPrefix={1}", packageGuid, bacoPrefix);
                    logger.Error(errmsg, ex);
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errmsg, ex);
                }
            }

            // b)
            if (!alreadyExists && doPack)
            {
                this.CreateBasePackage(packageGuid, bacoPrefix, filterSet);
            }

            // c)
            if (alreadyExists && doPack)
            {
                CoreTransaction transi = new CoreTransaction();
                transi.Begin();
                try
                {
                    BaseContainerHandler bacoH = new BaseContainerHandler(transi, this.Guid_ModifiedBy, this.ModifyComment);
                    var baseCoToDelete = bacoH.GetBaseContainer(packageGuid, bacoPrefix);
                    FilterManager firuM = new FilterManager(baseCoToDelete.Guid, this.Guid_ModifiedBy, this.ModifyComment);
                    bacoH.DeleteObject(baseCoToDelete);
                    firuM.DeleteFilterRule(transi);

                    this.CreateBasePackage(packageGuid, bacoPrefix, filterSet, transi);
                    transi.Commit();
                }
                catch (BaseException bEx)
                {
                    transi.Rollback();
                    throw bEx;
                }
                catch (Exception ex)
                {
                    transi.Rollback();
                    string errmsg = string.Format("error trying to update base container. obcoguid={0} bacoPrefix={1}", packageGuid, bacoPrefix);
                    logger.Error(errmsg, ex);
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errmsg, ex);
                }
            }
        }

        /// <summary>
        /// packageGuid = objectContainerGuid
        /// </summary>
        /// <param name="packageGuid"></param>
        public void DeletePackage(string packageGuid, CoreTransaction transi)
        {


            FilterManager firuM = new FilterManager(packageGuid, transi, this.Guid_ModifiedBy, this.ModifyComment);

            ObjectContainerHandler objectContainerHandler = new ObjectContainerHandler(transi, this.Guid_ModifiedBy, this.ModifyComment);
            var emdPackage = objectContainerHandler.GetObject<EMDObjectContainer>(packageGuid);


            if (emdPackage != null)
            {
                Hashtable hashTable = objectContainerHandler.GetRelatedEntities(packageGuid);

                if (hashTable.Count == 0)
                {
                    objectContainerHandler.DeleteObject<EMDObjectContainer>(emdPackage);
                    firuM.DeleteFilterRule(transi);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("No package found for: {0}", packageGuid));
            }
        }

        public void DeletePackage(string packageGuid)
        {
            CoreTransaction transi = new CoreTransaction();
            transi.Begin();
            try
            {
                this.DeletePackage(packageGuid, transi);
                transi.Commit();
            }
            catch (BaseException bex)
            {
                transi.Rollback();
                string errmsg = "Error deleting Package(ObjectContainer). Transaction rolled back. packagguid = " + packageGuid;
                logger.Error(errmsg, bex);
                throw bex;
            }
            catch (Exception ex)
            {
                transi.Rollback();
                string errmsg = "Error deleting Package(ObjectContainer). Transaction rolled back. packagguid = " + packageGuid;
                logger.Error(errmsg, ex);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errmsg, ex);
            }
        }

        /// <summary>
        /// throws BaseExections and does the logging
        /// </summary>
        /// <param name="packageGuid"></param>
        public void DeleteBasePackage(string packageGuid)
        {
            CoreTransaction transi = new CoreTransaction();
            transi.Begin();
            try
            {
                this.DeleteBasePackage(packageGuid, transi);
                transi.Commit();
            }
            catch (BaseException bEx)
            {
                transi.Rollback();
                throw bEx;
            }
            catch (Exception ex)
            {
                transi.Rollback();
                string errMsg = string.Format("error trying to delete basecontainer items. package guid is: '{0}'", packageGuid);
                logger.Error(errMsg, ex);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errMsg, ex);
            }
        }

        public void DeleteBasePackage(string packageGuid, CoreTransaction transi)
        {
            BaseContainerHandler bacoH = new BaseContainerHandler(transi, this.Guid_ModifiedBy, this.ModifyComment);
            // get all base packages items for this package
            var bacoDeleteList = bacoH.GetObjects<EMDBaseContainer, BaseContainer>("OBCOGuid = \"" + packageGuid + "\"");
            foreach (var bacoGone in bacoDeleteList)
            {
                FilterManager firuM = new FilterManager(bacoGone.Guid, transi, this.Guid_ModifiedBy, this.ModifyComment);
                bacoH.DeleteObject((EMDBaseContainer)bacoGone);
                firuM.DeleteFilterRule(transi);
            }
        }

        /// <summary>
        /// Task ID 805
        /// use case : Update EQ Defs for Package
        /// </summary>
        /// <param name="objectContainerGUID"></param>
        /// <param name="equipmentDefinitionGUIDs"></param>
        public void UpdateEquipmentDefinitionsForPackage(string objectContainerGUID, List<string> equipmentDefinitionGUIDs)
        {
            if (objectContainerGUID == null)
                throw new ArgumentNullException("objectContainerGUID", "objectContainerGUID must not be null.");

            if (equipmentDefinitionGUIDs == null)
                throw new ArgumentNullException("equipmentDefinitionGUIDs", "equipmentDefinitionGUIDs must not be null.");

            if (equipmentDefinitionGUIDs.Count < 1)
                throw new ArgumentException("List equipmentDefinitionGUIDs must have 1+ entries", "equipmentDefinitionGUIDs");

            //update (add, remove) EQDEs
            //Framework.TransactionHandler th = Framework.TransactionHandler.Instance;
            Framework.CoreTransaction ta = new CoreTransaction();

            try
            {
                ta.Begin();
                ObjectContainerContentHandler occH = new ObjectContainerContentHandler(ta, this.Guid_ModifiedBy, this.ModifyComment);

                List<IEMDObject<EMDObjectContainerContent>> existingContent
                        = (List<IEMDObject<EMDObjectContainerContent>>)occH.GetObjects<EMDObjectContainerContent, ObjectContainerContent>
                        ("OC_Guid = \"" + objectContainerGUID + "\"", null);

                existingContent.ForEach(item =>
                    {
                        EMDObjectContainerContent tmp = (EMDObjectContainerContent)item;
                        occH.DeleteObject(tmp, historize: true);
                    });

                equipmentDefinitionGUIDs.ForEach(item =>
                    {
                        EMDObjectContainerContent tmp = new EMDObjectContainerContent();
                        tmp.OC_Guid = objectContainerGUID;
                        tmp.ObjectGuid = item;
                        tmp = (EMDObjectContainerContent)occH.CreateObject(tmp);
                    });
                ta.Commit();
            }
            catch (Exception ex)
            {
                ta.Rollback();
                BaseException be = new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error created ObjectContainerContent. Transaction rolled back.", ex);
                logger.Error("Error created ObjectContainerContent. Transaction rolled back.", ex);
                throw be;
            }

        }

        /// <summary>
        /// use case : GetEquipmentDefinitions By Package ID
        /// </summary>
        /// <param name="packageGuid"></param>
        /// <returns></returns>
        public List<EMDObjectContainerContent> GetEquipmentDefinitionsForPackage(string packageGuid)
        {
            ObjectContainerContentHandler occH = new ObjectContainerContentHandler(this.Transaction);

            List<IEMDObject<EMDObjectContainerContent>> existingContent
                = (List<IEMDObject<EMDObjectContainerContent>>)occH.GetObjects<EMDObjectContainerContent, ObjectContainerContent>
                ("OC_Guid = \"" + packageGuid + "\"", null);

            List<EMDObjectContainerContent> tmp = new List<EMDObjectContainerContent>();
            existingContent.ForEach(item => tmp.Add((EMDObjectContainerContent)item));
            return tmp;
        }

        /// <summary>
        /// Task ID 807
        /// use case : get list of packages with filter applied (package manager)
        /// </summary>
        /// <returns></returns>
        public List<EMDObjectContainer> GetFilteredListofPackages(FilterCriteria filterCriteria = null)
        {
            // loop through all objcontainers
            ObjectContainerHandler och = new ObjectContainerHandler(this.Transaction);
            FilterRuleHandler frh = new FilterRuleHandler(this.Transaction);
            List<EMDObjectContainer> allowedObjContainers = new List<EMDObjectContainer>();

            List<IEMDObject<EMDObjectContainer>> allObjContGuids
                        = (List<IEMDObject<EMDObjectContainer>>)och.GetObjects<EMDObjectContainer, ObjectContainer>("1=1");

            //find rule for each container
            foreach (var item in allObjContGuids)
            {
                if (filterCriteria != null)
                {
                    FilterManager fm = new FilterManager(item.Guid, this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                    bool allowed = fm.CheckRule(item.Guid, filterCriteria);
                    if (allowed)
                    {
                        EMDObjectContainer newContainer = (EMDObjectContainer)och.GetObject<EMDObjectContainer>(item.Guid);
                        allowedObjContainers.Add(newContainer);
                    }
                }
                else
                {
                    EMDObjectContainer newContainer = (EMDObjectContainer)och.GetObject<EMDObjectContainer>(item.Guid);
                    allowedObjContainers.Add(newContainer);
                }
            }
            return allowedObjContainers;
        }

        /// <summary>
        /// TASK 862
        /// </summary>
        /// <returns></returns>
        public List<EMDObjectContainer> GetListOfPackagesByTemplateRuleSet(List<FilterRuleSubSetForCriteria> filterRuleSubSets)
        {
            //TODO fix this after demo meeting
            return this.GetFilteredListofPackages(null);
        }

        /// <summary>
        /// TASKID 809
        /// linke seite manage eq packages
        /// </summary>
        /// <param name="filterCriteria"></param>
        /// <returns></returns>
        public List<EMDEquipmentDefinition> GetFilteredListOfEquipmentDefinitions(FilterCriteria filterCriteria)
        {
            EquipmentDefinitionHandler eqdh = new EquipmentDefinitionHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<EMDEquipmentDefinition> allowedEqDefs = new List<EMDEquipmentDefinition>();
            

            //var allEQDEs = eqdh.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>();
            var allEQDEs = eqdh.GetAllEquipmentDefinitions_DIRECT();


            List<string> allEQDefsGuids = new List<string>();
            foreach (var item in allEQDEs)
            {
                allEQDefsGuids.Add(item.Guid);
            }

            //List<string> allEQDefsGuids = allEQDEs.ConvertAll(new Converter<List<EMDEquipmentDefinition>, string>(
            //    delegate (List<EMDEquipmentDefinition> inp)
            //    {
            //        return inp.Guid.ToString();
            //    }
            //    ));

            //FilterManager fm = new FilterManager(allEQDefsGuids);
            //allEQDefsGuids.ForEach(eqGuid =>
            //    {
            //        bool allowed = fm.CheckRule(eqGuid, filterCriteria);
            //        if (allowed)
            //        {
            //            EMDEquipmentDefinition newEqDef = (EMDEquipmentDefinition)allEQDEs.Find(m => m.Guid == eqGuid);
            //            allowedEqDefs.Add(newEqDef);
            //        }
            //    });

            ConcurrentBag<EMDEquipmentDefinition> bag = new ConcurrentBag<EMDEquipmentDefinition>(); 
            FilterManager fm = new FilterManager(allEQDefsGuids);
            Parallel.ForEach(allEQDefsGuids, eqGuid =>
            {

                bool allowed = fm.CheckRule(eqGuid, filterCriteria);
                if (allowed)
                {
                    EMDEquipmentDefinition newEqDef = (EMDEquipmentDefinition)allEQDEs.Find(m => m.Guid == eqGuid);
                    bag.Add(newEqDef);

                }
            });

            //NOT parallel no batch mode
            //allEQDefsGuids.ForEach(eqGuid =>
            //    {
            //        FilterManager fm = new FilterManager(eqGuid);
            //        bool allowed = fm.CheckRule(filterCriteria);
            //        if (allowed)
            //        {
            //            EMDEquipmentDefinition newEqDef = (EMDEquipmentDefinition)allEQDEs.Find(m => m.Guid == eqGuid);
            //            allowedEqDefs.Add(newEqDef);
            //        }
            //    });

            return bag.ToList(); 
        }

        /// <summary>
        /// Task ID 854, rechte Seite
        /// </summary>
        /// <param name="objectContainerGuid"></param>
        /// <returns></returns> 
        public List<EMDEquipmentDefinition> GetConfiguredEquipmentDefinitionsForPackage(string objectContainerGuid)
        {
            ObjectContainerContentHandler obccH = new ObjectContainerContentHandler(this.Transaction);
            EquipmentDefinitionHandler eqdeH = new EquipmentDefinitionHandler(this.Transaction);
            List<EMDEquipmentDefinition> configuredEQDEs = new List<EMDEquipmentDefinition>();

            List<IEMDObject<EMDObjectContainerContent>> allEqDefsForPackage = (List<IEMDObject<EMDObjectContainerContent>>)
                obccH.GetObjects<EMDObjectContainerContent, ObjectContainerContent>("OC_Guid = \"" + objectContainerGuid + "\"");

            foreach (var eqDef in allEqDefsForPackage)
            {
                EMDObjectContainerContent cc = (EMDObjectContainerContent)eqDef;
                EMDEquipmentDefinition eqde = (EMDEquipmentDefinition)eqdeH.GetObject<EMDEquipmentDefinition>(cc.ObjectGuid);
                configuredEQDEs.Add(eqde);
            }

            return configuredEQDEs;
        }

        /// <summary>
        /// Task ID 853, linke Seite
        /// </summary>
        /// <param name="objectContainerGuid"></param>
        /// <returns></returns>
        public List<EMDEquipmentDefinition> GetAvailableEquipmentDefinitionsForPackage(string packageGuid, FilterCriteria filterCrit = null)
        {
            List<EMDEquipmentDefinition> availableEQDEs = new List<EMDEquipmentDefinition>();
            List<EMDEquipmentDefinition> diff = new List<EMDEquipmentDefinition>();
            EquipmentDefinitionHandler eqdeH = new EquipmentDefinitionHandler(this.Transaction);

            List<EMDEquipmentDefinition> configured = this.GetConfiguredEquipmentDefinitionsForPackage(packageGuid);

            List<IEMDObject<EMDEquipmentDefinition>> allEqDefs = (List<IEMDObject<EMDEquipmentDefinition>>)
                            eqdeH.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>("1=1");


            if (filterCrit != null)
            {
                foreach (var item in allEqDefs)
                {
                    //TODO we need a RuleSet for each EquipmentDefinitions
                    FilterManager fm = new FilterManager(item.Guid, this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                    bool allowed = fm.CheckRule(item.Guid, filterCrit);
                    if (allowed)
                        availableEQDEs.Add((EMDEquipmentDefinition)item);
                }
            }
            else
            {
                foreach (var item in allEqDefs)
                    availableEQDEs.Add((EMDEquipmentDefinition)item);
            }

            // remove the already configured
            foreach (EMDEquipmentDefinition confItem in configured)
            {
                var mussWeg = availableEQDEs.Single(p => p.Guid == confItem.Guid);
                availableEQDEs.Remove(mussWeg);
            }

            //TODO delete this remove the old migration eqs
            foreach (EMDEquipmentDefinition confItem in configured)
            {
                if (confItem.Q_ID > 0)
                    availableEQDEs.Remove(confItem);
            }

            return availableEQDEs;
        }

        /// <summary>
        /// Create BasePackageInformation for Package based on Lists (which are converted to FilterRules)
        /// </summary>
        /// <param name="OBCO_Guid"></param>
        /// <param name="baseTypePrefix"></param>
        /// <param name="ENTEGuids"></param>
        /// <param name="LOCAGuids"></param>
        /// <param name="EMTYGuids"></param>
        public void CreateBasePackage(string OBCO_Guid, string baseTypePrefix, List<string> ENTEGuids, List<string> LOCAGuids, List<string> EMTYGuids, List<string> USERGuids, bool invertFlag, bool enteIsInherited = false)
        {
            string baseFilterAction = BaseFilterAction.DENYALL;
            if (invertFlag)
                baseFilterAction = BaseFilterAction.ALLOWALL;

            List<FilterRuleSubSetForCriteria> subSets = new List<FilterRuleSubSetForCriteria>();
            if (ENTEGuids != null && ENTEGuids.Count > 0)
                subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.Company,
                                                            baseFilterAction,
                                                            ENTEGuids, enteIsInherited)
               );

            if (LOCAGuids != null && LOCAGuids.Count > 0)
                subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.Location,
                                                            baseFilterAction,
                                                            LOCAGuids)
               );

            if (EMTYGuids != null && EMTYGuids.Count > 0)
                subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.EmploymentType,
                                                            baseFilterAction,
                                                            EMTYGuids)
               );

            if (USERGuids != null && USERGuids.Count > 0)
                subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.UserType,
                                                            baseFilterAction,
                                                            USERGuids)
               );

            CreateBasePackage(OBCO_Guid, baseTypePrefix, subSets);
        }

        public void CreateBasePackage(string OBCOGuid, string baseTypePrefix, List<FilterRuleSubSetForCriteria> filterRuleSubSets)
        {
            CoreTransaction bptransaction = new CoreTransaction();
            bptransaction.Begin();
            try
            {
                this.CreateBasePackage(OBCOGuid, baseTypePrefix, filterRuleSubSets, bptransaction);
                bptransaction.Commit();
            }
            catch (Exception ex)
            {
                bptransaction.Rollback();
                BaseException be = new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error created SetAsBasePackage. Transaction rolled back.", ex);
                throw be;
            }
        }

        public void CreateBasePackage(string OBCOGuid, string baseTypePrefix, List<FilterRuleSubSetForCriteria> filterRuleSubSets, CoreTransaction bptransaction)
        {
            BaseContainerHandler bacoH = new BaseContainerHandler(bptransaction, this.Guid_ModifiedBy, this.ModifyComment);
            bacoH.transaction = bptransaction;

            EMDBaseContainer baco = new EMDBaseContainer();
            baco.OBCOGuid = OBCOGuid;
            baco.BACOPrefix = baseTypePrefix;
            baco = (EMDBaseContainer)bacoH.CreateObject(baco);

            FilterManager fm = new FilterManager(baco.Guid, bptransaction, this.Guid_ModifiedBy, this.ModifyComment);
            fm.CreateFilterRule(baco.Guid, filterRuleSubSets, bptransaction);
        }

        /// <summary>
        /// Get BasePackageInformation for Package based on FilterRule
        /// </summary>
        /// <param name="baseTypePrefix">ENTE ... Enterprise; LOCA ... Location </param>
        /// <param name="ENTEGuid"></param>
        /// <param name="LOCAGuid"></param>
        /// <param name="EMTYGuid"></param>
        /// <returns></returns>
        public List<EMDObjectContainer> GetBasePackages(string baseTypePrefix, string ENTEGuid, string LOCAGuid, string EMTYGuid, List<string> userTypeIds)
        {

            FilterCriteria filterCriteria = new FilterCriteria();
            filterCriteria.Company = ENTEGuid;
            filterCriteria.EmploymentType = EMTYGuid;
            filterCriteria.Location = LOCAGuid;
            filterCriteria.UserTypeIds = userTypeIds;

            return GetBasePackages(baseTypePrefix, filterCriteria);
        }

        public List<EMDObjectContainer> GetBasePackages(string baseTypePrefix, FilterCriteria filterCriteria)
        {
            List<EMDObjectContainer> fList;

            if (string.IsNullOrWhiteSpace(baseTypePrefix))
            {
                ArgumentNullException nullArg = new ArgumentNullException("baseTypePrefix", "null is not allowed for baseTypePrefix.");
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "null is not allowed for baseTypePrefix.", nullArg);
            }

            ObjectContainerHandler obcoH = new ObjectContainerHandler(this.Transaction);
            BaseContainerHandler bacoH = new BaseContainerHandler(this.Transaction);

            string filterString = null;
            if (!string.IsNullOrEmpty(baseTypePrefix))
            {

                filterString = string.Format("BACOPrefix == \"{0}\"", baseTypePrefix);
            }


            //hole eine Liste aller BaseContainer
            List<IEMDObject<EMDBaseContainer>> baseContainerFiltered = (List<IEMDObject<EMDBaseContainer>>)
                bacoH.GetObjects<EMDBaseContainer, BaseContainer>(filterString);

            if (baseContainerFiltered.Count > 0)
            {
                fList = new List<EMDObjectContainer>();
                foreach (var item in baseContainerFiltered)
                {
                    //prüfe ob die angegebene FilterRule auf das BasePackage passt.
                    FilterManager fm = new FilterManager(item.Guid, this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                    bool isAMatch = fm.CheckRule(item.Guid, filterCriteria);

                    if (isAMatch)
                    {
                        //suche das Package auf das dieser BaseContainer zeigt und hänge es in die Liste
                        EMDObjectContainer obco = (EMDObjectContainer)obcoH.GetObject<EMDObjectContainer>((((EMDBaseContainer)item).OBCOGuid));
                        fList.Add(obco);
                    }
                }
            }
            //stelle im Fehlerfall sicher, dass zumindest eine lange Liste zurückgeliefert wird.
            else
                fList = new List<EMDObjectContainer>();

            return fList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obre_guid"></param>
        /// <param name="empl_guid"></param>
        /// <returns>null if obre was not found</returns>
        public EMDObjectRelation MoveEquipmentToEmploymentPackage(string obre_guid, string empl_guid)
        {
            EMDObjectRelation result = null;

            ObjectRelationHandler obreH = new ObjectRelationHandler(this.Transaction, null, this.ModifyComment);

            if (!(String.IsNullOrEmpty(obre_guid) && String.IsNullOrEmpty(empl_guid)))
            {

                EMDObjectRelation obre = (EMDObjectRelation)obreH.GetObject<EMDObjectRelation>(obre_guid);
                if (obre != null)
                {
                    // employment has changed - obre umhängen (und auf EmploymentPackage hängen)
                    obre.ORTYGuid = EnumObjectRelationTypeAttribute.GetGuid(EnumObjectRelationType.EquipmentByEmploymentPackage);
                    obre.Object1 = empl_guid;
                    obre.FromTemplateGuid = empl_guid;
                    obre = (EMDObjectRelation)obreH.UpdateObject(obre);
                    result = obre;
                }
            }

            return result;
        }

        public EMDObjectRelation SetEquipmentStatus(string obre_guid, int processStatus, bool doKeep = false)
        {
            EMDObjectRelation result = null;

            // Handle the special remove case for an employment who get the equipment as a gift
            // DoKeep is only set in the OffboardingActivity and not in the ChangeActivity
            if (processStatus == ProcessStatus.STATUSITEM_REMOVED && doKeep)
            {
                processStatus = EquipmentStatus.STATUSITEM_REMOVED_GIFTEDTOEMPLOYMENT;
            }


            ObjectRelationHandler obreH = new ObjectRelationHandler(this.Transaction, null, this.ModifyComment);
            if (!String.IsNullOrEmpty(obre_guid))
            {
                EMDObjectRelation obre = (EMDObjectRelation)obreH.GetObject<EMDObjectRelation>(obre_guid);
                if (obre != null)
                {
                    obre.Status = Convert.ToByte(processStatus);
                    obre = (EMDObjectRelation)obreH.UpdateObject(obre, historize: (processStatus != ProcessStatus.STATUSITEM_REMOVED));
                    result = obre;
                }
                if (processStatus == ProcessStatus.STATUSITEM_REMOVED)
                {
                    obreH.DeleteObject(obre);
                }

            }
            return result;
        }

    }
}


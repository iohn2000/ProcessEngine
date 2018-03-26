using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using Kapsch.IS.EDP.Core.Entities.Enums;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class EmploymentManager : BaseManager
    {
        protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Does just the checks and does not start the workflows, and also does not write to EDP-database
        /// Only implemented for DoOffboarding
        /// </summary>
        public bool ReadOnlyMode { get; set; } = false;

        #region Constructors

        public EmploymentManager()
            : base()
        {
        }

        public EmploymentManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public EmploymentManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public EmploymentManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDEmployment GetEmployment(string guid)
        {
            EmploymentHandler eph = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            return (EMDEmployment)eph.GetObject<EMDEmployment>(guid);


        }

        public EMDEmployment Update(EMDEmployment employment)
        {
            EmploymentHandler eph = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            return (EMDEmployment)eph.UpdateObject<EMDEmployment>(employment);
        }



        /// <summary>
        /// returns a list of all active employments
        /// </summary>
        /// <returns></returns>
        public List<EMDEmployment> GetList(string whereClause)
        {
            return new EmploymentHandler(this.Transaction).GetObjects<EMDEmployment, Employment>(whereClause).Cast<EMDEmployment>().ToList();
        }

        /// <summary>
        /// Gets all active employments with status "active"
        /// </summary>
        /// <returns><seealso cref="List{EMDEmployment}"/></returns>
        public List<EMDEmployment> GetActiveEmployments()
        {
            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction);
            return employmentHandler.GetObjects<EMDEmployment, Employment>("Status = 50").Cast<EMDEmployment>().ToList();
        }

        /// <summary>
        /// Delivers only active OR active and removed employments
        /// </summary>
        /// <param name="pers_guid"></param>
        /// <param name="deliverRemoved">if true >> Delivers active and removed</param>
        /// <returns></returns>
        public List<EMDEmployment> GetEmploymentsForPerson(string pers_guid, bool deliverRemoved = false)
        {
            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction);
            employmentHandler.DeliverInActive = deliverRemoved;
            //List<IEMDObject<EMDEmployment>> employments = eph.GetObjects<EMDEmployment,Employment>("P_GUID=\"" + pers_guid + "\"");
            List<IEMDObject<EMDEmployment>> employments = null;
            if (deliverRemoved)
            {
                employments = employmentHandler.GetObjects<EMDEmployment, Employment>("P_GUID=\"" + pers_guid + "\"");
            }
            else
            {
                employments = employmentHandler.GetObjects<EMDEmployment, Employment>("P_GUID=\"" + pers_guid + "\" && Status != 70");
            }

            return employments.Cast<EMDEmployment>().ToList();
        }

        internal static ObjectCache cache = MemoryCache.Default;

        //Create a custom Timeout of 10 seconds
        private static CacheItemPolicy policy = new CacheItemPolicy();

        //Create a custom Timeout for extended cache
        private static CacheItemPolicy policyExtended = new CacheItemPolicy();

        /// <summary>
        /// Gets the list of EMDPersonEmployments for the provided employment-guids
        /// </summary>
        /// <param name="empl_guids">List of the employment guids</param>
        /// <param name="deliverInActive"></param>
        /// <returns><seealso cref="List{EMDPersonEmployment}"/></returns>
        public List<EMDPersonEmployment> GetPersonEmploymentsForEmployees(List<string> empl_guids, bool deliverInActive = false)
        {
            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction);
            PersonHandler persHandler = new PersonHandler(this.Transaction);
            employmentHandler.DeliverInActive = deliverInActive;
            persHandler.DeliverInActive = deliverInActive;

            var empls = (from empl in employmentHandler.GetObjects<EMDEmployment, Employment>().Cast<EMDEmployment>().Where(item => empl_guids.Contains(item.Guid)).ToList()
                         join pers in persHandler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>().ToList() on empl.P_Guid equals pers.Guid
                         select new { pers, empl });


            List<EMDPersonEmployment> employments = new List<EMDPersonEmployment>();
            foreach (var item in empls)
            {
                employments.Add(new EMDPersonEmployment(item.pers, item.empl));
            }
            return employments;
        }

        /// <summary>
        /// Gets the list of EMDPersonEmployments for the provided EMDEmployments
        /// </summary>
        /// <param name="emdEmployments">List of the EMDEmployment <seealso cref="EMDEmployment"/></param>
        /// <param name="deliverInActive"></param>
        /// <returns><seealso cref="List{EMDPersonEmployment}"/></returns>
        public List<EMDPersonEmployment> GetPersonEmploymentsForEmployees(List<EMDEmployment> emdEmployments, bool deliverInActive = false)
        {
            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction);
            PersonHandler persHandler = new PersonHandler(this.Transaction);
            employmentHandler.DeliverInActive = deliverInActive;
            persHandler.DeliverInActive = deliverInActive;

            var empls = (from empl in emdEmployments
                         join pers in persHandler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>().ToList() on empl.P_Guid equals pers.Guid
                         select new { pers, empl });


            List<EMDPersonEmployment> employments = new List<EMDPersonEmployment>();
            foreach (var item in empls)
            {
                employments.Add(new EMDPersonEmployment(item.pers, item.empl));
            }
            return employments;
        }

        /// <summary>
        /// Gets a list of PersonEmployment (no database mapping) cached for 5 minutes
        /// </summary>
        /// <param name="deliverInActive"></param>
        /// <returns></returns>
        public List<EMDPersonEmployment> GetAllPersonEmployments(bool deliverInActive = false)
        {
            List<EMDPersonEmployment> employments = cache.Get("ListOfEMDPersonEmployments") as List<EMDPersonEmployment>;

            if (employments != null)
            {
                return employments;
            }

            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction);
            PersonHandler persHandler = new PersonHandler(this.Transaction);
            employmentHandler.DeliverInActive = deliverInActive;
            persHandler.DeliverInActive = deliverInActive;

            var empls = (from empl in employmentHandler.GetObjects<EMDEmployment, Employment>().Cast<EMDEmployment>().ToList()
                         join pers in persHandler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>().ToList() on empl.P_Guid equals pers.Guid
                         select new { pers, empl });

            if (!deliverInActive)
            {
                empls = empls.Where(item => item.empl.IsLegalActive == true);
            }

            employments = new List<EMDPersonEmployment>();
            foreach (var item in empls)
            {
                employments.Add(new EMDPersonEmployment(item.pers, item.empl));
            }

            // add PersonEmploymentsList to Cache
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5);
            cache.Add("ListOfEMDPersonEmployments", employments, policy);

            return employments;
        }

        /// <summary>
        /// Gets a specific personemployment from a cached Dictionary
        /// This method is performance optimized and caches 5 minutes
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <returns></returns>
        public EMDPersonEmployment GetPersonEmployment(string emplGuid, bool deliverInActive = false)
        {
            Dictionary<string, EMDPersonEmployment> dictionary = cache.Get("DictOfEMDPersonEmployments") as Dictionary<string, EMDPersonEmployment>;

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, EMDPersonEmployment>();

                foreach (EMDPersonEmployment employment in GetAllPersonEmployments(deliverInActive))
                {
                    dictionary.Add(employment.Empl.Guid, employment);
                }

                // add PersonEmploymentsList to Cache
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5);
                cache.Add("DictOfEMDPersonEmployments", dictionary, policy);
            }

            EMDPersonEmployment foundEmpl = null;
            dictionary.TryGetValue(emplGuid, out foundEmpl);

            return foundEmpl;
        }

        /// <summary>
        /// Returns all reasons as a set of <see cref="EnumOffboardingDeclined"/>) why offboarding cannot be started.
        /// </summary>
        /// <param name="emplGuid">Employment Guid to check</param>
        /// <returns>returns an empty set if no reasons exists not to start offboarding</returns>
        public HashSet<EnumOffboardingDeclined> CheckIfOffboardingAllowed(string emplGuid)
        {
            HashSet<EnumOffboardingDeclined> reasonsNotToOffb = new HashSet<EnumOffboardingDeclined>();
            // check if this employment is a main employment and has other employments
            EMDEmployment emplToCheck = this.GetEmployment(emplGuid);
            EMDEmployment mainEmpl = this.GetMainEploymentForPerson(emplToCheck.P_Guid);

            bool isMainEmployment = false;
            if (mainEmpl != null)
            {
                if (emplToCheck.Guid == mainEmpl.Guid)
                {
                    isMainEmployment = true;
                }
            }

            // employment to check is not main empl so problems with offboarding
            if (isMainEmployment)
            {
                List<EMDEmployment> allEmployments = this.GetEmploymentsForPerson(emplToCheck.P_Guid);
                // is the only employment so its ok to offboard
                if (allEmployments.Count() > 1)
                {
                    reasonsNotToOffb.Add(EnumOffboardingDeclined.OtherEmploymentsExist);
                }
            }
            return reasonsNotToOffb;
        }

        public List<EMDEmployment> GetEmploymentsByEnterprise(string enteGuid)
        {
            List<EMDEmployment> matchedEmployments = new List<EMDEmployment>();

            // get enterprise locations for enterprises
            EnterpriseLocationHandler enterpriseLocationHandler = new EnterpriseLocationHandler(this.Transaction);
            List<IEMDObject<EMDEnterpriseLocation>> enterpriseLocations = (List<IEMDObject<EMDEnterpriseLocation>>)enterpriseLocationHandler.GetObjects<EMDEnterpriseLocation, EnterpriseLocation>("E_Guid = \"" + enteGuid + "\"", null).ToList();

            // get employments in enterprise locations
            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction);
            List<IEMDObject<EMDEmployment>> employments = (List<IEMDObject<EMDEmployment>>)enterpriseLocationHandler.GetObjects<EMDEmployment, Employment>().ToList();

            foreach (EMDEmployment item in employments.Cast<EMDEmployment>())
            {
                if (enterpriseLocations.Exists(c => c.Guid == item.ENLO_Guid))
                {
                    matchedEmployments.Add(item);
                }
            }

            return matchedEmployments;
        }

        public FilterCriteria GetFilterCriteriaFromEmployment_DIRECT(SqlConnection sqlConnection, string employmentGuid)
        {
            FilterCriteria filterCriteria = new FilterCriteria();


            string getEmplQuery = string.Format("select top 1 ENLO_Guid,ET_Guid from employment where guid = '{0}'", employmentGuid);
            SqlCommand sqlCommand = new SqlCommand(getEmplQuery, sqlConnection);
            var reader = sqlCommand.ExecuteReader();
            string ENLO_Guid = "";
            string ET_Guid = "";
            if (reader.Read())
            {
                ENLO_Guid = reader.GetString(0);
                ET_Guid = reader.GetString(1);
            }
            reader.Close();

            //Hauptkostenstelle costcenter <---> employment
            string acc_guid = new AccountManager().GetAccountGuidForEmployment_DIRECT(sqlConnection, employmentGuid);
            if (acc_guid != null)
            {
                filterCriteria.CostCenter = acc_guid;
            }

            string getEnloQuery = string.Format("select top 1 E_Guid,L_Guid from EnterpriseLocation where guid = '{0}'", ENLO_Guid);
            SqlCommand sqlCommand2 = new SqlCommand(getEnloQuery, sqlConnection);
            var reader2 = sqlCommand2.ExecuteReader();
            if (reader2.Read())
            {
                filterCriteria.Company = reader2.GetString(0);//.E_Guid;
                filterCriteria.Location = reader2.GetString(1);//.L_Guid;
                filterCriteria.EmploymentType = ET_Guid;// empl.ET_Guid;
                filterCriteria.UserTypeIds = new List<string>();
            }
            reader2.Close();

            string getUserTypes = string.Format("select UserType from [User] where EMPL_Guid = '{0}' and guid = historyguid", employmentGuid);
            SqlCommand sqlCommand3 = new SqlCommand(getUserTypes, sqlConnection);
            var reader3 = sqlCommand3.ExecuteReader();
            while (reader3.Read())
            {
                filterCriteria.UserTypeIds.Add(((EnumUserType)reader3.GetByte(0)).ToString());
            }

            reader3.Close();

            return filterCriteria;
        }

        /// <summary>
        /// Task ID 804
        /// </summary>
        /// <param name="employmentGuid"></param>
        /// <returns></returns>
        public FilterCriteria GetFilterCriteriaFromEmployment(string employmentGuid)
        {
            EmploymentHandler eph = new EmploymentHandler(this.Transaction);
            EMDEmployment empl = (EMDEmployment)eph.GetObject<EMDEmployment>(employmentGuid);

            EmploymentAccountHandler eacch = new EmploymentAccountHandler(this.Transaction);

            //Hauptkostenstelle costcenter <---> employment
            EMDAccount emdAccount = new AccountManager(this.Transaction).GetAccountForEmployment(employmentGuid);

            string acc_guid = string.Empty;
            if (emdAccount != null)
            {
                acc_guid = emdAccount.Guid;
            }

            FilterCriteria filterCriteria = new FilterCriteria();

            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler(this.Transaction);
            EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(empl.ENLO_Guid);

            filterCriteria.Company = enlo.E_Guid;
            filterCriteria.Location = enlo.L_Guid;
            filterCriteria.EmploymentType = empl.ET_Guid;
            filterCriteria.UserTypeIds = new List<string>();
            List<EMDUser> users = Manager.UserManager.GetEmploymentUsers(employmentGuid);
            // TODO: woller filterCriteria.UstyGuid
            foreach (EMDUser user in users)
            {
                filterCriteria.UserTypeIds.Add(((EnumUserType)user.UserType).ToString());
            }

            if (!string.IsNullOrWhiteSpace(acc_guid))
                filterCriteria.CostCenter = acc_guid;

            return filterCriteria;
        }


        public EMDObjectRelation AddEquipmentToEmployment(CoreTransaction transaction, string effectedPersonEmploymentGuid, string eqdeGuid, string requestingPersEMPLGuid)
        {
            ObjectRelationHandler obreH = new ObjectRelationHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);
            ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler(transaction);

            EMDEquipmentDefinition equipmentDefinition = new EquipmentManager(transaction).Get(eqdeGuid);
            EquipmentDefinitionConfig equipmentDefinitionConfig = equipmentDefinition.GetEquipmentDefinitionConfig();

            List<EMDObjectRelation> existingEquipmentInstances = obreH.GetObjects<EMDObjectRelation, ObjectRelation>("Object1 = \"" + effectedPersonEmploymentGuid + "\" AND Object2 = \"" + eqdeGuid + "\" AND Status <= 50", null).Cast<EMDObjectRelation>().ToList();

            //   List <EMDEquipmentInstance> foundEquipments = (from a in existingEmploymentEquipments where !string.IsNullOrEmpty(a.Guid) && a.GetEquipmentDefinition().Guid == equipmentDefinition.Guid select a).ToList();
            int foundEquipments = existingEquipmentInstances == null ? 0 : existingEquipmentInstances.Count;
            EMDObjectRelation obreNew = new EMDObjectRelation();
            if (foundEquipments < equipmentDefinitionConfig.MaxNumberAllowedEquipments)
            {
                string fromTemplateGuid = effectedPersonEmploymentGuid;
                string ortyGuid = ortyH.GetGuidForRelationName(ObjectRelationTypeList.EquipmentByEmploymentPackage);

                // create OBRE
                obreNew.Object1 = effectedPersonEmploymentGuid;
                obreNew.Object2 = eqdeGuid;
                obreNew.FromTemplateGuid = fromTemplateGuid;
                obreNew.Status = (byte)EquipmentStatus.STATUSITEM_QUEUED;
                obreNew.ORTYGuid = ortyGuid;
                obreNew.Data = "<root></root>";
                obreH.CreateObject(obreNew);
            }
            else
            {
                throw new Exception(string.Format("The Equipment has a maximum of {0} allowed equipments configured. Add not possible.", equipmentDefinitionConfig?.MaxNumberAllowedEquipments));
            }

            return obreNew;
        }

        /// <summary>
        /// Task ID 810
        /// </summary>
        /// <param name="packageGuid">the Package GUID; emtpy or null for Empployment Package</param>
        /// <param name="employmentGuid"></param>
        /// <returns></returns>
        public void AddPackageToEmployment(CoreTransaction transaction, string employmentGuid, string packageGuid, string requestingPersEMPLGuid)
        {
            ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler(transaction);
            ObjectRelationHandler obreH = new ObjectRelationHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);

            // xTODO 
            // a) das packge (obco) dem empl zuordnen -->  OBRT und OBRE verwenden
            // b) jede EQDE im objcontainer via obre dem employment zuordnen

            // a)
            EMDObjectRelation obrePack = new EMDObjectRelation();
            obrePack.Object1 = packageGuid;
            obrePack.Object2 = employmentGuid;
            obrePack.Status = (byte)ObjectRelationStatus.STATUSITEM_ACTIVE;
            obrePack.ORTYGuid = ortyH.GetGuidForRelationName(ObjectRelationTypeList.PackageByEmployment);
            obrePack.Data = "<root></root>";
            obrePack.FromTemplateGuid = packageGuid;
            obreH.CreateObject(obrePack);

            // b)
            if (string.IsNullOrWhiteSpace(packageGuid))
            {
                string msg = string.Format("ObjectContainer Guid cannot be null or emtpy");
                logger.Error(msg);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg);
            }
        }


        /// <summary>
        /// Task ID 810
        /// </summary>
        /// <param name="packageGuid">the Package GUID; emtpy or null for Empployment Package</param>
        /// <param name="employmentGuid"></param>
        /// <returns></returns>
        public List<EMDObjectRelation> AddPackageToEmploymentWithEquipments(CoreTransaction transaction, string employmentGuid, string packageGuid, string requestingPersEMPLGuid)
        {
            List<EMDObjectRelation> equipmentInstances = new List<EMDObjectRelation>();

            ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler(transaction);
            ObjectRelationHandler obreH = new ObjectRelationHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);

            List<EMDObjectRelation> existingPackages = obreH.GetObjects<EMDObjectRelation, ObjectRelation>("Object1 = \"" + packageGuid + "\" AND Object2 = \"" + employmentGuid + "\" AND Status <= 50", null).Cast<EMDObjectRelation>().ToList();
            int foundPackages = existingPackages == null ? 0 : existingPackages.Count;

            if (foundPackages > 0)
            {
                throw new Exception("The requested Package is already configured!");
            }

            // xTODO 
            // a) das packge (obco) dem empl zuordnen -->  OBRT und OBRE verwenden
            // b) jede EQDE im objcontainer via obre dem employment zuordnen

            // a)
            EMDObjectRelation obrePack = new EMDObjectRelation();
            obrePack.Object1 = packageGuid;
            obrePack.Object2 = employmentGuid;
            obrePack.Status = (byte)ObjectRelationStatus.STATUSITEM_ACTIVE;
            obrePack.ORTYGuid = ortyH.GetGuidForRelationName(ObjectRelationTypeList.PackageByEmployment);
            obrePack.Data = "<root></root>";
            obrePack.FromTemplateGuid = packageGuid;
            obreH.CreateObject(obrePack);

            // b)
            if (string.IsNullOrWhiteSpace(packageGuid))
            {
                string msg = string.Format("ObjectContainer Guid cannot be null or emtpy");
                logger.Error(msg);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg);
            }

            ObjectContainerContentHandler obccH = new ObjectContainerContentHandler(this.Guid_ModifiedBy);
            List<EMDObjectContainerContent> allObjContContent = obccH.GetObjects<EMDObjectContainerContent, ObjectContainerContent>("OC_Guid = \"" + packageGuid + "\"", null).Cast<EMDObjectContainerContent>().ToList();

            string ortyGuid = ortyH.GetGuidForRelationName(ObjectRelationTypeList.EquipmentByPackage);
            List<EMDEquipmentInstance> existingEmploymentEquipments = GetConfiguredListOfEquipmentIntancesForEmployment(employmentGuid);

            foreach (EMDObjectContainerContent objectContent in allObjContContent)
            {
                // check if the maximum of Equipments is configured for the employment
                // EMDObjectContainerContent.ObjectGuid => EquipmentDefinition
                EMDEquipmentDefinition equipmentDefinition = new EquipmentManager(transaction).Get(objectContent.ObjectGuid);
                EquipmentDefinitionConfig equipmentDefinitionConfig = equipmentDefinition.GetEquipmentDefinitionConfig();

                List<EMDObjectRelation> existingEquipmentInstances = obreH.GetObjects<EMDObjectRelation, ObjectRelation>("Object1 = \"" + employmentGuid + "\" AND Object2 = \"" + objectContent.ObjectGuid + "\" AND Status <= 50", null).Cast<EMDObjectRelation>().ToList();

                //   List <EMDEquipmentInstance> foundEquipments = (from a in existingEmploymentEquipments where !string.IsNullOrEmpty(a.Guid) && a.GetEquipmentDefinition().Guid == equipmentDefinition.Guid select a).ToList();
                int foundEquipments = existingEquipmentInstances == null ? 0 : existingEquipmentInstances.Count;

                // check if there are more equipments possible for this employment
                if (foundEquipments < equipmentDefinitionConfig.MaxNumberAllowedEquipments)
                {

                    EMDObjectRelation obreNew = new EMDObjectRelation();

                    // create OBRE
                    obreNew.Object1 = employmentGuid;
                    obreNew.Object2 = objectContent.ObjectGuid;
                    obreNew.FromTemplateGuid = packageGuid;
                    obreNew.Status = (byte)EquipmentStatus.STATUSITEM_QUEUED;
                    obreNew.ORTYGuid = ortyGuid;
                    obreNew.Data = "<root></root>";
                    obreH.CreateObject(obreNew);

                    equipmentInstances.Add(obreNew);
                }
            }



            return equipmentInstances;
        }

        /// <summary>
        /// Task ID 843
        /// </summary>
        /// <param name="employmentGuid"></param>
        /// <returns></returns>
        public List<EMDObjectContainer> GetAvailableListOfPackagesForEmployment(string employmentGuid)
        {
            FilterCriteria ruleCriteria = this.GetFilterCriteriaFromEmployment(employmentGuid);
            PackageManager pMgr = new PackageManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            return pMgr.GetFilteredListofPackages(ruleCriteria);
        }

        /// <summary>
        /// Task ID 844
        /// </summary>
        /// <param name="employmentGuid"></param>
        /// <returns></returns>
        public List<EMDPackageInstance> GetConfiguredListOfPackagesForEmployment(string employmentGuid)
        {
            List<EMDPackageInstance> configuredPackInstances = new List<EMDPackageInstance>();
            List<EMDObjectRelation> allPackLinks = new List<EMDObjectRelation>();
            // via OBRT und empl guid die packages finden
            // package wird über OBRE mit tpye ort.Key = "PackageByEmployment"; verknüpft
            ObjectRelationHandler obreH = new ObjectRelationHandler(this.Transaction);
            ObjectContainerHandler obcoH = new ObjectContainerHandler(this.Transaction);

            // find what field is used for employment obj1 or obj2 go looking in OBRT
            ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler(this.Transaction);

            string employmentKey = ortyH.FindObjectField(ObjectRelationTypeList.PackageByEmployment, new EMDEmployment().Prefix);
            string packageKey = ortyH.FindObjectField(ObjectRelationTypeList.PackageByEmployment, new EMDObjectContainer().Prefix);
            string ortyGuid = ortyH.GetGuidForRelationName(ObjectRelationTypeList.PackageByEmployment);
            string query = string.Format("ORTYGuid = \"{0}\" && {1} = \"{2}\"", ortyGuid, employmentKey, employmentGuid);

            var x = (List<IEMDObject<EMDObjectRelation>>)obreH.GetObjects<EMDObjectRelation, ObjectRelation>(query);

            foreach (var item in x)
                allPackLinks.Add((EMDObjectRelation)item);

            foreach (var pack in allPackLinks)
            {
                EMDObjectRelation packOBRE = (EMDObjectRelation)pack;
                string packGuid = ReflectionHelper.GetPropValue(packOBRE, packageKey).ToString();
                // get objectcontainer (package)
                EMDObjectContainer obco = (EMDObjectContainer)obcoH.GetObject<EMDObjectContainer>(packGuid);

                EMDPackageInstance i = new EMDPackageInstance();
                i.ObjectRelation = packOBRE;
                i.ObjectRelationGuid = packOBRE.Guid;
                i.Package = obco;
                i.PackageName = obco.Name;
                i.PackageDescription = obco.Description;
                i.PackageStatus = packOBRE.Status;
                configuredPackInstances.Add(i);
            }
            return configuredPackInstances;
        }





        /// <summary>
        /// Updates all set parameters (not null) for the effectedEmplGuid or newEmplGuid
        /// This method is called from ProcessEngine on the DAY OF ACTION
        /// The changeType is used for logging and business case checks
        /// </summary>
        /// <param name="changeType">Change Type for Business process</param>
        /// <param name="requestingEmplGuid">Employment GUID from Requestor</param>
        /// <param name="effectedEmplGuid">Employment GUID to update or remove</param>
        /// <param name="newEmplGuid">Employment GUID in case of Enterprise-Change or EmploymentType-Change</param>
        /// <param name="guid_ente"></param>
        /// <param name="guid_acco"></param>
        /// <param name="guid_emty"></param>
        /// <param name="guid_orgu"></param>
        /// <param name="guid_dist">DistributionGroupType</param>
        /// <param name="guid_loca"></param>
        /// <param name="guid_sponsor"></param>
        /// <param name="moveAllRoles">update orgunit-roles from existing orgunit to new if guid_orgu is set</param>
        /// <param name="emailType">intern or extern create a new email on the person depending on type intern/extern</param>
        /// <param name="leaveToDate"></param>
        /// <param name="personellNumber">for changed enterprise and employment type only</param>
        /// <param name="kccData">not used now - additional data for employment</param>
        public void DoChange(EnumEmploymentChangeType changeType, string requestingEmplGuid, string effectedEmplGuid, string newEmplGuid,
            string guid_acco, string guid_emty, string guid_orgu, string guid_dist, string guid_loca,
            string guid_sponsor, bool moveAllRoles,
            string personellNumber, string kccData, bool? isExternalEmail, DateTime? leaveFrom, DateTime? leaveTo)
        {
            logger.Info(string.Format("Start EmploymentChange:{0} for employment:{1}, RequestingEmployment:{2}", changeType.ToString(), effectedEmplGuid, requestingEmplGuid));
            // check business validity
            switch (changeType)
            {
                case EnumEmploymentChangeType.Enterprise:
                case EnumEmploymentChangeType.EmploymentType:
                    if (string.IsNullOrWhiteSpace(newEmplGuid))
                    {
                        throw new Exception(string.Format("DoChange failed for effectedEmplGuid:{0}, because parameter 'newEmplGuid' is null for changeType:{1}", effectedEmplGuid, changeType.ToString()));
                    }


                    break;
                default:
                    break;
            }
            EMDPerson requestingPerson = new PersonManager().GetPersonByEmployment(requestingEmplGuid);
            EMDPerson effectedPerson = new PersonManager().GetPersonByEmployment(effectedEmplGuid);

            CoreTransaction coreTransaction = new CoreTransaction();
            PersonHandler personHandler = new PersonHandler(coreTransaction, requestingPerson.Guid, "do Change called from core/workflow process");
            EmploymentHandler employmentHandler = new EmploymentHandler(coreTransaction, requestingPerson.Guid, "do Change called from core/workflow process");
            EmploymentAccountHandler employmentAccountHandler = new EmploymentAccountHandler(coreTransaction, requestingPerson.Guid, "do Change called from core/workflow process");
            EnterpriseLocationHandler enterpriseLocationHandler = new EnterpriseLocationHandler(coreTransaction, requestingPerson.Guid, "do Change called from core/workflow process");
            OrgUnitRoleHandler orgUnitRoleHandler = new OrgUnitRoleHandler(coreTransaction, requestingPerson.Guid, "do Change called from core/workflow process");
            DistributionGroupHandler distributionGroupTypeHandler = new DistributionGroupHandler(coreTransaction, requestingPerson.Guid, "do Change called from core/workflow process");

            AccountManager accountManager = new AccountManager(coreTransaction, requestingPerson.Guid, "do Change called from core/workflow process");
            OrgUnitRoleManager orgUnitRoleManager = new OrgUnitRoleManager(coreTransaction, requestingPerson.Guid, "do Change called from core/workflow process");
            EnterpriseLocationManager enloManager = new EnterpriseLocationManager(coreTransaction, requestingPerson.Guid, "do Change called from core/workflow process");

            EMDEmployment effectedEmployment = (EMDEmployment)employmentHandler.GetObject<EMDEmployment>(effectedEmplGuid);

            bool isNewEmployment = false;
            EMDEmployment editEmployment = null;

            if (!string.IsNullOrWhiteSpace(newEmplGuid))
            {
                isNewEmployment = true;
                editEmployment = (EMDEmployment)employmentHandler.GetObject<EMDEmployment>(newEmplGuid);

            }
            else
            {
                editEmployment = (EMDEmployment)employmentHandler.GetObject<EMDEmployment>(effectedEmplGuid);

            }

            EMDEmploymentType effectedEmploymentType = (EMDEmploymentType)new EmploymentTypeHandler().GetObject<EMDEmploymentType>(effectedEmployment.ET_Guid);

            bool hasPersonChanges = false;

            try
            {
                coreTransaction.Begin();

                if (!isNewEmployment)
                {

                    // check for Account (Costcenter)
                    if (!string.IsNullOrEmpty(guid_acco))
                    {
                        bool keepCostCenter = accountManager.EmploymentIsInCostCenter(editEmployment.Guid, guid_acco);
                        if (keepCostCenter == false)
                        {
                            DoChangeCostCenter(editEmployment.Guid, guid_acco, employmentAccountHandler);
                        }

                    }

                    // check for EmploymentType
                    if (!string.IsNullOrEmpty(guid_emty))
                    {
                        if (editEmployment.ET_Guid != guid_emty)
                        {
                            editEmployment.ET_Guid = guid_emty;
                        }
                    }

                    // check for DistributiongroupType
                    if (!string.IsNullOrEmpty(guid_dist))
                    {
                        if (editEmployment.DGT_Guid != guid_dist)
                        {
                            EMDDistributionGroup distributionGroup = (EMDDistributionGroup)distributionGroupTypeHandler.GetObject<EMDDistributionGroup>(guid_dist);
                            if (distributionGroup != null)
                            {
                                editEmployment.DGT_ID = distributionGroup.DGT_ID;
                            }

                            editEmployment.DGT_Guid = guid_dist;
                        }
                    }

                    // check for Orgunit
                    if (!string.IsNullOrEmpty(guid_orgu))
                    {
                        bool keepOrgUnit = orgUnitRoleManager.IsEmploymentInOrgUnitWithPersonRole(editEmployment.Guid, guid_orgu);
                        if (keepOrgUnit == false)
                        {
                            DoChangeOrgUnit(guid_orgu, moveAllRoles, orgUnitRoleHandler, editEmployment);
                        }
                    }

                    // check for sponsor
                    if (!string.IsNullOrEmpty(guid_sponsor))
                    {
                        if (editEmployment.Sponsor_Guid != guid_sponsor)
                        {
                            editEmployment.Sponsor_Guid = guid_sponsor;
                        }
                    }

                    // check for Location
                    if (!string.IsNullOrEmpty(guid_loca))
                    {
                        EMDEnterpriseLocation currentEnterpriseLocation = (EMDEnterpriseLocation)enterpriseLocationHandler.GetObject<EMDEnterpriseLocation>(effectedEmployment.ENLO_Guid);

                        if (currentEnterpriseLocation == null)
                        {
                            throw new Exception(string.Format("The current Employment {0} doesn't have an existing enterprise location for enlo_guid:{1}. Fix the error manually and run the method DoChange again.", effectedEmplGuid, effectedEmployment.ENLO_Guid));
                        }

                        EMDEnterpriseLocation newEnterpriseLocation = new EnterpriseLocationManager().Get(currentEnterpriseLocation.E_Guid, guid_loca);

                        if (newEnterpriseLocation == null)
                        {
                            throw new Exception(string.Format("The current Employment {0} can't move to new Location because there is no Enterprise Location for ente_guid:{1} and loca_guid{2}. Fix the error manually and run the method DoChange again.", effectedEmplGuid, currentEnterpriseLocation.E_Guid, guid_loca));
                        }

                        if (newEnterpriseLocation.Guid != editEmployment.ENLO_Guid)
                        {
                            editEmployment.ENLO_Guid = newEnterpriseLocation.Guid;
                        }
                    }


                    // check personell number
                    if (!string.IsNullOrEmpty(personellNumber))
                    {
                        if (editEmployment.PersNr != personellNumber)
                        {
                            editEmployment.PersNr = personellNumber;
                        }
                    }

                    //check Pause
                    if (leaveFrom.HasValue && leaveTo.HasValue)
                    {
                        if (editEmployment.LeaveFrom != leaveFrom.Value && editEmployment.LeaveTo != leaveTo.Value)
                        {
                            editEmployment.LeaveFrom = leaveFrom.Value;
                            editEmployment.LeaveTo = leaveTo.Value;
                        }
                    }
                    else
                    {
                        if (editEmployment.LeaveFrom != EMDEmployment.INFINITY)
                        {
                            editEmployment.LeaveFrom = EMDEmployment.INFINITY;
                        }

                        if (editEmployment.LeaveTo != EMDEmployment.INFINITY)
                        {
                            editEmployment.LeaveTo = EMDEmployment.INFINITY;
                        }
                    }
                }

                if (hasPersonChanges)
                {
                    personHandler.UpdateObject(effectedPerson);
                }

                employmentHandler.UpdateObject(editEmployment);

                coreTransaction.Commit();
            }
            catch (Exception ex)
            {
                coreTransaction.Rollback();
                string msg = string.Format("Cannot DoChange for employment for guid:{0}" + effectedEmplGuid);
                throw new Exception(msg, ex);
            }

        }

        /// <summary>
        /// do change orgunit for small changes
        /// </summary>
        /// <param name="guid_orgu"></param>
        /// <param name="moveAllRoles"></param>
        /// <param name="orgUnitRoleHandler"></param>
        /// <param name="effectedEmployment"></param>
        /// <param name="editEmployment"></param>
        private void DoChangeOrgUnit(string guid_orgu, bool moveAllRoles, OrgUnitRoleHandler orgUnitRoleHandler, EMDEmployment editEmployment)
        {
            EMDRole personRole = null;
            try
            {
                personRole = (EMDRole)new RoleHandler().GetRoleById(RoleHandler.PERSON);
            }
            catch (Exception)
            {
                throw new Exception("PersonRole not found");
            }

            OrgUnitRoleManager orgUnitRoleManager = new OrgUnitRoleManager(orgUnitRoleHandler.Transaction, orgUnitRoleHandler.Guid_ModifiedBy, orgUnitRoleHandler.ModifyComment);

            //Get Current OrgUnit
            EMDOrgUnitRole our = (EMDOrgUnitRole)orgUnitRoleHandler.GetOrgUnitRole(editEmployment.Guid, personRole.Guid);

            //List<EMDOrgUnitRole> effectedOrgunitRoles = orgUnitRoleManager.GetOrgUnitRolesForEmploymentinOrgUnit(guid_orgu, editEmployment.Guid);
            List<EMDOrgUnitRole> effectedOrgunitRoles = orgUnitRoleManager.GetOrgUnitRolesForEmploymentinOrgUnit(our.O_Guid, editEmployment.Guid);
            foreach (EMDOrgUnitRole effectedOrgunitRole in effectedOrgunitRoles)
            {
                logger.Debug("checking effectedOrgunitRole: " + effectedOrgunitRole.Guid);
                if (effectedOrgunitRole.R_Guid == personRole.Guid || moveAllRoles == true)
                {
                    logger.Debug("MoveRoleToOrgUnit - effectedOrgunitRole: " + effectedOrgunitRole + ", guid_orgu: " + guid_orgu);
                    orgUnitRoleManager.MoveRoleToOrgUnit(effectedOrgunitRole, guid_orgu);
                }
            }
        }

        /// <summary>
        /// Do change costcenter to a new costcenter for a given employment (no new employment)
        /// </summary>
        /// <param name="effectedEmplGuid"></param>
        /// <param name="guid_acco"></param>
        /// <param name="employmentAccountHandler"></param>
        /// <param name="effectedEmployment"></param>
        /// <param name="isNewEmployment"></param>
        private void DoChangeCostCenter(string effectedEmplGuid, string guid_acco, EmploymentAccountHandler employmentAccountHandler)
        {
            List<EMDEmploymentAccount> effectedEmploymentAccounts = employmentAccountHandler.GetObjects<EMDEmploymentAccount, EmploymentAccount>(string.Format("EP_GUID = \"{0}\"", effectedEmplGuid)).Cast<EMDEmploymentAccount>().ToList();

            if (effectedEmploymentAccounts.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The current Employment {0} has {1} EmploymentAccounts. Fix the error manually and run the method DoChange again.", effectedEmplGuid, effectedEmploymentAccounts.Count.ToString()));
            }

            if (effectedEmploymentAccounts.Count == 1)
            {
                EMDEmploymentAccount currentEmploymentAccount = effectedEmploymentAccounts[0];
                // check if EmploymentAccount has changed
                if (currentEmploymentAccount.AC_Guid != guid_acco)
                {
                    currentEmploymentAccount.AC_Guid = guid_acco;
                    employmentAccountHandler.UpdateObject(currentEmploymentAccount);
                }
            }
        }

        /// <summary>
        /// Task ID 845
        /// </summary>
        /// <param name="employmentGuid"></param>
        /// <returns></returns>
        public List<EMDEquipmentInstance> GetConfiguredListOfEquipmentIntancesForEmployment(string employmentGuid, bool deliverInActive = false)
        {
            //TODO write function to get all eq via obj rel , calc status, ...

            // 2 steps : a) get all Equipments that are employment package = 11
            //           b) get all EQs with "normal" package

            List<EMDEquipmentInstance> eqs4employment = new List<EMDEquipmentInstance>();
            ObjectRelationTypeHandler objectRelationHandler = new ObjectRelationTypeHandler(this.Transaction);
            objectRelationHandler.DeliverInActive = deliverInActive;
            string employmentKey, eqdeKey;

            // 2)
            employmentKey = objectRelationHandler.FindObjectField(ObjectRelationTypeList.EquipmentByEmploymentPackage, new EMDEmployment().Prefix);
            eqdeKey = objectRelationHandler.FindObjectField(ObjectRelationTypeList.EquipmentByEmploymentPackage, new EMDEquipmentDefinition().Prefix);

            // 2a) get key for OBRE employment by empployment package, 40,11
            eqs4employment.AddRange(this.getEQInstancesForEmploymentPackage(employmentGuid, employmentKey, eqdeKey, deliverInActive));

            // 2b) get key for OBRE employment by normal package
            eqs4employment.AddRange(this.getEquipmentByPackage(employmentGuid, deliverInActive));

            return eqs4employment;
        }

        private List<EMDEquipmentInstance> getEQInstancesForEmploymentPackage(string employmentGuid, string employmentKey, string eqdeKey, bool deliverInActive = false)
        {
            ObjectRelationHandler obreH = new ObjectRelationHandler(this.Transaction);
            obreH.DeliverInActive = deliverInActive;
            EquipmentDefinitionHandler eqdeH = new EquipmentDefinitionHandler(this.Transaction);
            ObjectContainerHandler obcoH = new ObjectContainerHandler(this.Transaction);

            List<EMDEquipmentInstance> newEQinstances = new List<EMDEquipmentInstance>();

            string where = string.Format("{0} = \"{1}\" && FromTemplateGuid = \"{1}\"", employmentKey, employmentGuid);

            var obreForEmploymentPackages = (List<IEMDObject<EMDObjectRelation>>)obreH.GetObjects<EMDObjectRelation, ObjectRelation>(where);

            foreach (var item in obreForEmploymentPackages)
            {
                newEQinstances.Add(this.buildEQInstance(eqdeH, obcoH, eqdeKey, item));
            }

            return newEQinstances;
        }

        /// <summary>
        /// Process EQ Status		Beschreibung	
        /// 
        ///NotSet       0		
        ///Ordered     10	Not Queued by Queue Manager	
        ///Queued      20	Queued by QueueManager	
        ///InProgress  30  In Progress by Workflow	
        ///Active      40  Attached to the employment	
        ///TimeOut     50  Timed out process	
        ///Declined    60  Declined by approver	
        ///Removed     70  Removed from employment	status auf 70, aber nicht valid from to invalid setzen --> beim Lesen aufpassen
        ///
        ///PackEQ Status (EQ is … )			
        ///NotSet               0		
        ///Packaged            10  EQ and Package linked	
        ///EmploymentPackage   11  EQ in Employment Package	
        ///RemovedFromPackage  20  Has EQ but no longer in package	, package still linked
        ///EligibleFor         30  Has not EQ but still package exists	demo_todo : 6) 7) the same
        ///FromUnlinkedPackage 40  Has EQ but no longer linked package	durchstreichen (is in a removed package)
        ///FromFormerPackage   50  Has EQ but no package deleted	
        /// </summary>
        /// <param name="employmentGuid"></param>
        /// <returns></returns>
        private List<EMDEquipmentInstance> getEquipmentByPackage(string employmentGuid, bool deliverInActive = false)
        {
            List<EMDEquipmentInstance> returnEquipmentInstanceList = new List<EMDEquipmentInstance>();
            List<EMDObjectRelation> allLinkedOBREEquipments = new List<EMDObjectRelation>();
            List<EMDObjectRelation> allLinkedOBREPackages = new List<EMDObjectRelation>();

            ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler(this.Transaction);
            ortyH.DeliverInActive = deliverInActive;
            ObjectRelationHandler obreH = new ObjectRelationHandler(this.Transaction);
            obreH.DeliverInActive = deliverInActive;
            ObjectContainerContentHandler obccH = new ObjectContainerContentHandler(this.Transaction);
            obccH.DeliverInActive = deliverInActive;
            ObjectContainerHandler obcoH = new ObjectContainerHandler(this.Transaction);
            obcoH.DeliverInActive = deliverInActive;
            EquipmentDefinitionHandler eqdeH = new EquipmentDefinitionHandler(this.Transaction);
            eqdeH.DeliverInActive = deliverInActive;

            PackageEquipmentStatus packEqStatusList = new PackageEquipmentStatus();



            string employmentKey, where, objContainerKey, equipmentKey;

            int packageStatus = PackageEquipmentStatus.STATUSITEM_NOTSET;

            objContainerKey = ortyH.FindObjectField(ObjectRelationTypeList.PackageByEmployment, new EMDObjectContainer().Prefix);
            employmentKey = ortyH.FindObjectField(ObjectRelationTypeList.PackageByEmployment, new EMDEquipmentDefinition().Prefix);
            equipmentKey = ortyH.FindObjectField(ObjectRelationTypeList.EquipmentByPackage, new EMDEquipmentDefinition().Prefix);

            allLinkedOBREEquipments = this.getAllEQsLinkedToEmploymentByPackage(employmentGuid, ortyH, obreH);
            allLinkedOBREPackages = this.getAllPackagesLinkedToEmployment(employmentGuid, ortyH, obreH);

            // Packstatus 10,11,20,40,50 can be found by loop over all linked EQs (obre) 
            foreach (EMDObjectRelation obre in allLinkedOBREEquipments)
            {
                packageStatus = PackageEquipmentStatus.STATUSITEM_NOTSET;

                EMDObjectContainer obco = (EMDObjectContainer)obcoH.GetObject<EMDObjectContainer>(obre.FromTemplateGuid);
                if (obco == null)
                {
                    string msg = "OBCO for OBRE: " + obre.Guid + " with FromTemplateGuid = " + obre.FromTemplateGuid + " results in NULL.";
                    logger.Warn(msg);
                    //TODO umhängen von EQs auf employment package
                    packageStatus = packEqStatusList.GetProcessStatusItemByShortName("EmploymentPackage").StatusNumber;
                }
                else
                {
                    if (obco.ValidityStatus != EMDObjectContainer.VALIDITY_STATUS_VALID)
                    {
                        // FromFormerPackage   50  Has EQ but no package deleted
                        // check if fromTemplateGuid is a package that has been deleted (as in valid from/to invalid)
                        packageStatus = packEqStatusList.GetProcessStatusItemByShortName("FromFormerPackage").StatusNumber;
                    }
                    else
                    {
                        // dont look at eq with status removed or declined
                        if (obre.Status != EquipmentStatus.STATUSITEM_REMOVED & obre.Status != EquipmentStatus.STATUSITEM_DECLINED)
                        {
                            packageStatus = this.calculateEqPackageStatus(allLinkedOBREPackages, obccH, ortyH, obcoH, eqdeH, obre);
                        }
                    }
                }

                string eqdeGuid = ReflectionHelper.GetPropValue(obre, equipmentKey).ToString();
                EMDEquipmentDefinition eqde = (EMDEquipmentDefinition)eqdeH.GetObject<EMDEquipmentDefinition>(eqdeGuid);
                EMDEquipmentInstance newEqInstance = null;
                bool isInstanceAdded = false;
                if (packageStatus > PackageEquipmentStatus.STATUSITEM_NOTSET)
                {

                    // status could be calculated
                    string packageName = string.Empty;
                    if (packageStatus != PackageEquipmentStatus.STATUSITEM_EMPLOYMENTPACKAGE)
                    {

                        if (obco != null)
                        {
                            packageName = obco.Name;
                        }

                        newEqInstance = this.fillEQInstanceItem(packageStatus, obre, packageName, eqde);
                    }
                    else
                    {
                        packageName = PackageManager.NAME_EMPLOMENT_PACKAGE;
                        newEqInstance = this.fillEQInstanceItem(packageStatus, obre, packageName, eqde);
                    }


                    if (newEqInstance != null)
                    {
                        isInstanceAdded = true;
                        returnEquipmentInstanceList.Add(newEqInstance);
                    }
                }

                // Check if the instance was added - never loose an active instance
                if (!isInstanceAdded)
                {
                    returnEquipmentInstanceList.Add(this.fillEQInstanceItem(PackageEquipmentStatus.STATUSITEM_NOTSET, obre, string.Empty, eqde));
                }
            }




            //
            // Packstatus 30 : loop over linked packages , has no EQ but package
            //   -) non existent
            foreach (EMDObjectRelation packageOBRE in allLinkedOBREPackages)
            {
                // get all ids of equipmentdefs of this linked package

                string obcoGuid = ReflectionHelper.GetPropValue(packageOBRE, objContainerKey).ToString();
                where = string.Format("OC_Guid = \"{0}\"", obcoGuid);
                var tmpList = (List<IEMDObject<EMDObjectContainerContent>>)obccH.GetObjects<EMDObjectContainerContent, ObjectContainerContent>(where);
                List<EMDObjectContainerContent> allLinkedPackageContent = new List<EMDObjectContainerContent>();
                foreach (var item in tmpList) allLinkedPackageContent.Add((EMDObjectContainerContent)item);

                // loop all EQs in package (container) and check whether this eq is part of the package (eqInPackage=>true)
                foreach (var obccItem in allLinkedPackageContent)
                {
                    packageStatus = -1;
                    // check if there is a linked EQ for this objet container element
                    EMDObjectRelation obre = null;

                    foreach (EMDObjectRelation linkedObreEqde in allLinkedOBREEquipments)
                    {
                        // see if you find match between OBCC item and OBREeqde

                        string obreEqdeGuid = ReflectionHelper.GetPropValue(linkedObreEqde, equipmentKey).ToString();
                        string obrePackageGuid = linkedObreEqde.FromTemplateGuid;

                        bool matchingPackage = obccItem.OC_Guid == obrePackageGuid;
                        bool matchingEQDE = obccItem.ObjectGuid == obreEqdeGuid;
                        bool obccItemHasLinkedObre = matchingPackage & matchingEQDE;

                        //if i found the first (doesnt matter how many else) stop loop b/c its not a Status 30
                        if (obccItemHasLinkedObre)
                        {
                            obre = linkedObreEqde;
                            break;
                        }
                    }

                    if (obre == null && packageOBRE.Status == PackageStatus.STATUSITEM_ACTIVE)
                    {
                        // eq in package doesnt exist as linked equipment
                        packageStatus = packEqStatusList.GetProcessStatusItemByShortName("EligibleFor").StatusNumber;
                    }
                    else
                    {
                        // the status is already set, no further steps necessary
                        break;
                    }

                    // This part of code is only called if the user hasn't an equipment and should show the status

                    EMDObjectContainer obco;
                    EMDEquipmentDefinition eqde;

                    if (obre != null) // get obco and eqde from given obre eqdeGuid
                    {
                        obco = (EMDObjectContainer)obcoH.GetObject<EMDObjectContainer>(obre.FromTemplateGuid);
                        string eqdeGuid = ReflectionHelper.GetPropValue(obre, equipmentKey).ToString();
                        eqde = (EMDEquipmentDefinition)eqdeH.GetObject<EMDEquipmentDefinition>(eqdeGuid);
                    }
                    else
                    {
                        // here obre == null --> Status EligibleFor
                        // use obco from obcc item
                        // use obcc.ObjectGuid to find eqde 
                        obco = (EMDObjectContainer)obcoH.GetObject<EMDObjectContainer>(obccItem.OC_Guid);
                        eqde = (EMDEquipmentDefinition)eqdeH.GetObject<EMDEquipmentDefinition>(obccItem.ObjectGuid);
                        obre = new EMDObjectRelation();
                        obre.Status = EquipmentStatus.STATUSITEM_NOTSET;
                        obre.Guid = null;
                    }

                    if (packageStatus > -1 && packageStatus < int.MaxValue)
                    {
                        // status could be calculated
                        EMDEquipmentInstance newEqInstance = this.fillEQInstanceItem(packageStatus, obre, obco.Name, eqde);

                        // only to be sure, that the equipment isn't already in the list >> remove it first and add the equipment with the correct status
                        if (obre.Guid != null)
                        {
                            EMDEquipmentInstance existingInstance = returnEquipmentInstanceList.FirstOrDefault(a => a.ObjectRelationGuid == newEqInstance.ObjectRelationGuid);
                            if (existingInstance != null)
                            {
                                returnEquipmentInstanceList.Remove(existingInstance);
                            }
                        }
                        returnEquipmentInstanceList.Add(newEqInstance);


                    }
                    else if (packageStatus == -1)
                    {
                        packageStatus = EquipmentStatus.STATUSITEM_ERROR;

                        EMDEquipmentInstance newEqInstance = this.fillEQInstanceItem(packageStatus, obre, obco.Name, eqde);

                        // only to be sure, that the equipment isn't already in the list >> remove it first and add the equipment with the correct status
                        if (obre.Guid != null)
                        {
                            EMDEquipmentInstance existingInstance = returnEquipmentInstanceList.FirstOrDefault(a => a.ObjectRelationGuid == newEqInstance.ObjectRelationGuid);
                            if (existingInstance != null)
                            {
                                returnEquipmentInstanceList.Remove(existingInstance);
                            }
                        }
                        returnEquipmentInstanceList.Add(newEqInstance);



                        StringWriter swr = new StringWriter();
                        ObjectDumper.Write(newEqInstance, 3, swr);
                        logger.Warn("Could not correctly calculate Equipmentstatus for :" + swr.ToString());
                    }

                }//end each obcc
            }//end each package

            return returnEquipmentInstanceList;
        }



        private EMDEquipmentInstance fillEQInstanceItem(int packageStatus, EMDObjectRelation obre, string packageName, EMDEquipmentDefinition eqde)
        {
            EMDEquipmentInstance newEqInstance = new EMDEquipmentInstance();

            newEqInstance.EquipmentName = eqde.Name;
            newEqInstance.ObjectRelationGuid = obre.Guid;
            newEqInstance.PackageName = packageName;
            newEqInstance.PackageStatus = packageStatus;
            newEqInstance.ProcessStatus = obre.Status;
            newEqInstance.ActiveFrom = obre.ActiveFrom;
            newEqInstance.SetEquipmentDefinition(eqde);

            return newEqInstance;
        }

        private int calculateEqPackageStatus(List<EMDObjectRelation> allOBREPackagesLinks, ObjectContainerContentHandler obccH,
            ObjectRelationTypeHandler ortyH, ObjectContainerHandler obcoH, EquipmentDefinitionHandler eqdeH, EMDObjectRelation obreEQ)
        {
            bool eqInPackage = false;
            int packageStatus = -1;

            string objContainerKey = ortyH.FindObjectField(ObjectRelationTypeList.PackageByEmployment, new EMDObjectContainer().Prefix);
            string employmentKey = ortyH.FindObjectField(ObjectRelationTypeList.EquipmentByPackage, new EMDEquipmentDefinition().Prefix);

            // iterate all packages of linked to the employment (also removed ones)
            foreach (EMDObjectRelation packageOBRE in allOBREPackagesLinks)
            {
                // check whether linked package is removed by state
                bool isPackageRemoved = (packageOBRE.Status == PackageStatus.STATUSITEM_REMOVED);

                // check whether equipment is in an actual linked package
                bool fromTemplateInLinkedPackages = allOBREPackagesLinks.Exists(m => ReflectionHelper.GetPropValue(m, objContainerKey).ToString() == obreEQ.FromTemplateGuid);
                if (!fromTemplateInLinkedPackages)
                    return PackageEquipmentStatus.STATUSITEM_REMOVEDFROMPACKAGE; // 20

                // get all ids of equipmentdefs of this linked package
                string obcoGuid = ReflectionHelper.GetPropValue(packageOBRE, objContainerKey).ToString();
                string where = string.Format("OC_Guid = \"{0}\"", obcoGuid);
                var tmpList = (List<IEMDObject<EMDObjectContainerContent>>)obccH.GetObjects<EMDObjectContainerContent, ObjectContainerContent>(where);
                List<EMDObjectContainerContent> allPackContent = new List<EMDObjectContainerContent>();

                foreach (var item in tmpList)
                    allPackContent.Add((EMDObjectContainerContent)item);

                eqInPackage = false;

                // loop all EQs in package (container) and check whether this eq is part of the package (eqInPackage=>true)
                foreach (var obccItem in allPackContent)
                {
                    //eqde = (EMDEquipmentDefinition) eqdeH.GetObject<EMDEquipmentDefinition>(((EMDObjectContainerContent) obccItem).ObjectGuid);
                    eqInPackage = ReflectionHelper.GetPropValue(obreEQ, employmentKey).ToString() == obccItem.ObjectGuid; // eq is in a package (contentcontainer)
                    if (eqInPackage)
                        break;
                }

                if (eqInPackage)
                {
                    //check if actual package is fromtemplate => 10, 40
                    if (obcoGuid == obreEQ.FromTemplateGuid)
                    {
                        if (!isPackageRemoved)
                            return PackageEquipmentStatus.STATUSITEM_PACKAGED; // 10
                        else
                            return PackageEquipmentStatus.STATUSITEM_FROMUNLINKEDPACKAGE; // 40
                    }
                }
            }
            return packageStatus;
        }

        private List<EMDObjectRelation> getAllEQsLinkedToEmploymentByPackage(string employmentGuid, ObjectRelationTypeHandler ortyH, ObjectRelationHandler obreH)
        {
            List<EMDObjectRelation> allOBREEquipmentLinks = new List<EMDObjectRelation>();

            // get all equipments linked to employment (all obre even ones with removed status)
            string employmentKey = ortyH.FindObjectField(ObjectRelationTypeList.EquipmentByPackage, new EMDEmployment().Prefix);
            string ortyGuid = ortyH.GetGuidForRelationName(ObjectRelationTypeList.EquipmentByPackage);
            string where = string.Format("{0} = \"{1}\" && ORTYGuid = \"{2}\"", employmentKey, employmentGuid, ortyGuid);

            var tmpList = (List<IEMDObject<EMDObjectRelation>>)obreH.GetObjects<EMDObjectRelation, ObjectRelation>(where);

            foreach (var item in tmpList)
                allOBREEquipmentLinks.Add((EMDObjectRelation)item);

            return allOBREEquipmentLinks;
        }

        private List<EMDObjectRelation> getAllPackagesLinkedToEmployment(string employmentGuid, ObjectRelationTypeHandler ortyH, ObjectRelationHandler obreH)
        {
            // get all packages linked to employment
            List<EMDObjectRelation> allOBREPackagesLinks = new List<EMDObjectRelation>();

            string employmentKey = ortyH.FindObjectField(ObjectRelationTypeList.PackageByEmployment, new EMDEmployment().Prefix);
            string objContainerKey = ortyH.FindObjectField(ObjectRelationTypeList.PackageByEmployment, new EMDObjectContainer().Prefix);
            string ortyGuid = ortyH.GetGuidForRelationName(ObjectRelationTypeList.PackageByEmployment);
            string where = string.Format("{0} = \"{1}\" && ORTYGuid = \"{2}\"", employmentKey, employmentGuid, ortyGuid);

            //// switch so that i get deleted objects
            //bool oldH = obreH.Historical;
            //bool oldD = obreH.DeliverInActive;
            //obreH.Historical = true;
            //obreH.DeliverInActive = false;

            var tmpList = (List<IEMDObject<EMDObjectRelation>>)obreH.GetObjects<EMDObjectRelation, ObjectRelation>(where);
            foreach (var item in tmpList)
                allOBREPackagesLinks.Add((EMDObjectRelation)item);

            //// switch back
            //obreH.Historical = oldH;
            //obreH.DeliverInActive = oldD;

            return allOBREPackagesLinks;
        }

        private EMDEquipmentInstance buildEQInstance(EquipmentDefinitionHandler eqdeH,
                                                        ObjectContainerHandler obcoH, string eqdeKey, IEMDObject<EMDObjectRelation> item)
        {
            EMDEquipmentInstance i = new EMDEquipmentInstance();
            EMDObjectRelation obre = (EMDObjectRelation)item;
            string eqdeGuid = ReflectionHelper.GetPropValue(obre, eqdeKey).ToString();
            EMDEquipmentDefinition eqdeDef = (EMDEquipmentDefinition)eqdeH.GetObject<EMDEquipmentDefinition>(eqdeGuid);

            if (!string.IsNullOrEmpty(obre.Data))
            {
                XDocument xDocument = XDocument.Parse(obre.Data);

                i.IdWorkflowInstance = xDocument.Descendants("IdWorkflowInstance").FirstOrDefault()?.Value;
                i.TechnicalException = xDocument.Descendants("TechnicalException").FirstOrDefault()?.Value;
            }

            i.PackageName = ObjectRelationTypeList.EquipmentByEmploymentPackage;
            i.ObjectRelationGuid = item.Guid;
            i.EquipmentName = eqdeDef.Name;
            i.ProcessStatus = obre.Status;
            i.PackageStatus = PackageEquipmentStatus.STATUSITEM_EMPLOYMENTPACKAGE;
            i.SetEquipmentDefinition(eqdeDef);

            i.ValidFrom = obre.ValidFrom;
            i.ValidTo = obre.ValidTo;
            i.Created = obre.Created;
            i.ActiveFrom = obre.ActiveFrom;
            i.ActiveTo = obre.ActiveTo;
            i.Modified = obre.Modified;

            return i;
        }

        /// <summary>
        /// Task ID 846
        /// Get Available List Of EquipmentDefinitions For Employment
        /// </summary>
        /// <param name="employmentGuid"></param>
        /// <returns></returns>
        [Obsolete("Use GetAvailableEquipmentsForEmployment(), it has been replaced with a quicker and bug fixed function.", false)]
        public List<EMDEquipmentDefinition> GetAvailableListOfEquipmentDefinitionsForEmployment(string employmentGuid)
        {
            // TODO GetAvailableListOfEquipmentDefinitionsForEmployment
            // result = alle EQDE die emply noch nicht hat
            //        = Alle möglichen EQ 
            // a)               minus  (alle von packages die empl schon hat) 
            // b)               minus  (all einzelnen EQ ohne package die schon da) 
            // c)               plus   (EQ die mehrfach erlaubt sind)

            // a)
            List<EMDEquipmentInstance> configuredEQs = this.GetConfiguredListOfEquipmentIntancesForEmployment(employmentGuid);

            // b)
            PackageManager packMgr = new PackageManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            FilterCriteria filterCriteria = GetFilterCriteriaFromEmployment(employmentGuid);
            var allEqdeList = packMgr.GetFilteredListOfEquipmentDefinitions(filterCriteria);

            // c) eq mehrfach
            var q = from item in configuredEQs
                    group item by item.GetEquipmentDefinition().Guid into g
                    let count = g.Count()
                    orderby count descending
                    where count > 1
                    select new { eqdeGuid = g.Key, Count = count };

            var eqdeCountList = q.ToList();

            //minus plus ...
            foreach (var item in configuredEQs)
            {
                if (item.ProcessStatus != PackageEquipmentStatus.STATUSITEM_ELIGIBLEFOR)
                {
                    // user has this EQ already -> check if user is allowed more than 1 remove it
                    // OBRE mit selbiger EQDE Guid am selben employment

                    //
                    // get configures max requests for this equipment
                    //
                    var cfg = item.GetEquipmentDefinition().GetEquipmentDefinitionConfig();
                    int maxRequests = 1;
                    maxRequests = cfg.MaxNumberAllowedEquipments;

                    //
                    // find this eqde in duplicate list
                    //
                    var dummy = eqdeCountList.Find(dub => dub.eqdeGuid == item.GetEquipmentDefinition().Guid);
                    int actualAmountRequested = dummy != null ? dummy.Count : 0;

                    //
                    // if eqde is alreaday requested up to max requests remove from list
                    //
                    if (actualAmountRequested >= maxRequests)
                    {
                        EMDEquipmentDefinition eq = allEqdeList.FirstOrDefault(e => e.Guid == item.GetEquipmentDefinition().Guid);
                        if (eq != null)
                            allEqdeList.Remove(eq);
                    }
                }
                else
                {
                    // if user doesnt have EQ but could .... leave it in the list
                }
            }

            return allEqdeList;
        }



        /// <summary>
        /// throws exception
        /// </summary>
        /// <param name="effectedPersonEmploymentGuid"></param>
        /// <param name="eqdeGuid"></param>
        public ObreAddWorkflowMessage GetWorkflowVariablesForNewEquipmentInstanceEmployment(CoreTransaction transaction, string effectedPersonEmploymentGuid, string eqdeGuid, string ortyGuid, string fromTemplateGuid, string requestingPersEMPLGuid, DateTime targetDate)
        {
            ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler(transaction);
            ObjectRelationHandler obreH = new ObjectRelationHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDObjectRelation obreNew = new EMDObjectRelation();

            if (fromTemplateGuid == null)
                fromTemplateGuid = effectedPersonEmploymentGuid;

            if (ortyGuid == null)
            {
                ortyGuid = ortyH.GetGuidForRelationName(ObjectRelationTypeList.EquipmentByEmploymentPackage);
            }

            // create OBRE
            obreNew.Object1 = effectedPersonEmploymentGuid;
            obreNew.Object2 = eqdeGuid;
            obreNew.FromTemplateGuid = fromTemplateGuid;
            obreNew.Status = (byte)EquipmentStatus.STATUSITEM_QUEUED;
            obreNew.ORTYGuid = ortyGuid;
            obreNew.Data = "<root></root>";
            obreH.CreateObject(obreNew);


            // code here to add msg to queue for workflow
            // get mapping for eq
            ProcessMappingHandler prmpH = new ProcessMappingHandler(transaction);

            //returns a list of variables to be filled
            FilterCriteria c = EmploymentManager.GetFilterCriteria(effectedPersonEmploymentGuid);

            ObreAddWorkflowMessage obreAddWorkflowMessage = prmpH.GetWorkflowMapping<ObreAddWorkflowMessage>(eqdeGuid, c);

            obreAddWorkflowMessage.RequestingPersonEmploymentGuid = requestingPersEMPLGuid;
            obreAddWorkflowMessage.EffectedPersonEmploymentGuid = effectedPersonEmploymentGuid;
            obreAddWorkflowMessage.ObreGuid = obreNew.Guid;
            obreAddWorkflowMessage.TargetDate = targetDate;



            return obreAddWorkflowMessage;


        }

        public ObreAddWorkflowMessage GetWorkflowVariablesForExistingEquipmentInstanceEmployment(CoreTransaction transaction, string effectedPersonEmploymentGuid, string requestingPersEMPLGuid, string guidObreNew, string guidEquipmentDefinition, DateTime targetDate)
        {
            // code here to add msg to queue for workflow
            // get mapping for eq
            ProcessMappingHandler prmpH = new ProcessMappingHandler(transaction);

            //returns a list of variables to be filled
            FilterCriteria c = EmploymentManager.GetFilterCriteria(effectedPersonEmploymentGuid);

            ObreAddWorkflowMessage obreAddWorkflowMessage = prmpH.GetWorkflowMapping<ObreAddWorkflowMessage>(guidEquipmentDefinition, c);

            obreAddWorkflowMessage.RequestingPersonEmploymentGuid = requestingPersEMPLGuid;
            obreAddWorkflowMessage.EffectedPersonEmploymentGuid = effectedPersonEmploymentGuid;
            obreAddWorkflowMessage.ObreGuid = guidObreNew;
            obreAddWorkflowMessage.TargetDate = targetDate;

            return obreAddWorkflowMessage;
        }











        public static FilterCriteria GetFilterCriteria(string employmentGuid)
        {
            try
            {
                FilterCriteria c = new FilterCriteria();
                EMDEmployment empl = (EMDEmployment)new EmploymentHandler().GetObject<EMDEmployment>(employmentGuid);
                EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)new EnterpriseLocationHandler().GetObject<EMDEnterpriseLocation>(empl.ENLO_Guid);

                string where = string.Format("EP_Guid = \"{0}\" ", empl.Guid);
                var tmpList = (List<IEMDObject<EMDEmploymentAccount>>)new EmploymentAccountHandler().GetObjects<EMDEmploymentAccount, EmploymentAccount>(where);
                if (tmpList != null && tmpList.Count > 0)
                    c.CostCenter = tmpList[0].Guid;
                else
                {
                    ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType).Warn("cannot find an employment account for :" + employmentGuid);
                }
                c.Company = enlo.E_Guid;
                c.EmploymentType = empl.ET_Guid;
                c.Location = enlo.L_Guid;
                return c;
            }
            catch (Exception ex)
            {
                string msg = "cannot build FilterCriteria from given eff.pers.empl.guid (" + employmentGuid + ")";
                ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType).Error(msg, ex);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg, ex);
            }
        }

        /// <summary>
        /// Task ID 851
        /// </summary>
        /// <param name="employmentGuid"></param>
        /// <param name="objectContainerGuid"></param>
        public void RemovePackageFromEmployment(string employmentGuid, string packageGuid)
        {
            ObjectContainerHandler obcoH = new ObjectContainerHandler(this.Transaction);
            ObjectRelationHandler obreH = new ObjectRelationHandler(this.Transaction);
            ObjectRelationTypeHandler ortyH = new ObjectRelationTypeHandler(this.Transaction);

            string employmentKey = ortyH.FindObjectField(ObjectRelationTypeList.PackageByEmployment, new EMDEmployment().Prefix);
            string objContainerKey = ortyH.FindObjectField(ObjectRelationTypeList.PackageByEmployment, new EMDObjectContainer().Prefix);
            string ortyGuid = ortyH.GetGuidForRelationName(ObjectRelationTypeList.PackageByEmployment);
            string where = string.Format("{0} = \"{1}\" && {2} = \"{3}\" && ORTYGuid = \"{4}\" && Status <= 50 ", employmentKey, employmentGuid, objContainerKey, packageGuid, ortyGuid);

            var tmp1 = (List<IEMDObject<EMDObjectRelation>>)obreH.GetObjects<EMDObjectRelation, ObjectRelation>(where);
            List<EMDObjectRelation> allOBREPackagesLinks = new List<EMDObjectRelation>();
            tmp1.ForEach(item => allOBREPackagesLinks.Add((EMDObjectRelation)item));
            if (allOBREPackagesLinks.Count == 1)
            {
                allOBREPackagesLinks[0].Status = (byte)ObjectRelationStatus.STATUSITEM_REMOVED;
                obreH.UpdateObject(allOBREPackagesLinks[0], historize: true);
            }
            else
            {
                string msg = string.Format("invalid amount of package relations found. query was '{0}'", where);
                logger.Error(msg);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg);
            }
        }

        /// <summary>
        /// Task ID 852
        /// </summary>
        /// <param name="employmentGuid"></param>
        /// <param name="objectRelationGuid"></param>
        public void RemoveEquipmentInstanceFromEmployment(string employmentGuid, string objectRelationGuid)
        {
            throw new Exception("No yet known. Set Remove status or delete item");

            /* Alter Code
            ObjectRelationHandler obreH = new ObjectRelationHandler();

            EMDObjectRelation obre = (EMDObjectRelation)obreH.GetObject<EMDObjectRelation>(objectRelationGuid);
            obre.Status = (byte)ObjectRelationStatus.STATUSITEM_REMOVED;
            obreH.UpdateObject(obre);
            */
        }

        /// <summary>
        /// Gets the mainEmployment for a user
        /// </summary>
        /// <param name="personGuid"></param>
        /// <returns>null if no mainEmpl found</returns>
        public EMDEmployment GetMainEploymentForPerson(string personGuid)
        {
            DateTime now = DateTime.Now;

            if (this.Transaction == null)
            {
                this.Transaction = new CoreTransaction();
            }

            EMD_Entities emdEntities = this.Transaction.dbContext;

            EMDEmployment mainEmployment = null;

            List<Employment> employments = (from e in emdEntities.Employment
                                            join f in emdEntities.ObjectFlag on e.Guid equals f.Obj_Guid
                                            where
                                            e.Guid == e.HistoryGuid &&
                                            e.ActiveTo > now &&
                                            e.Status != ProcessStatus.STATUSITEM_REMOVED &&
                                            f.Guid == f.HistoryGuid &&
                                            f.ActiveTo > now &&
                                            f.ValidTo > now &&
                                            f.FlagType == EnumObjectFlagType.MainEmployment.ToString() &&
                                            f.ValidTo > now &&
                                            f.ActiveTo > now &&
                                            e.P_Guid == personGuid
                                            select e
                       ).Distinct().ToList();

            if (employments.Count > 1)
            {
                string msg = string.Format("Invalid amount of main employments found for personGuid '{0}'", personGuid);
                logger.Error(msg);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg);
            }

            if (employments.Count == 1)
            {
                Employment sourceEmployment = employments[0];
                mainEmployment = new EMDEmployment();
                ReflectionHelper.CopyProperties(ref sourceEmployment, ref mainEmployment);
            }

            return mainEmployment;
        }

        /// <summary>
        /// Returns true if there is any employment with a set mainflag
        /// This rule is also valid for employments with process-status "removed", because they are not allow to have the main-flag set
        /// </summary>
        /// <param name="persGuid"></param>
        /// <returns></returns>
        public bool HasMainEmployments(string persGuid)
        {
            return GetMainEploymentForPerson(persGuid) != null;
        }

        /// <summary>
        /// returns true if the given employment is the main employment
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <returns></returns>
        public bool IsMainEmployment(string emplGuid)
        {
            EMDEmployment employment = GetEmployment(emplGuid);
            EMDEmployment mainEmployment = GetMainEploymentForPerson(employment.P_Guid);
            if (mainEmployment != null)
            {
                return employment.Guid == mainEmployment.Guid;
            }
            return false;
        }

        /// <summary>
        /// Sets the main flag for an employment
        /// Throws an exception if the person has already a main-empl set
        /// </summary>
        /// <param name="emplGuid"></param>
        public void SetMainEmploymentFlag(string emplGuid)
        {
            EMDPerson person = new PersonManager(this.Transaction).GetPersonByEmployment(emplGuid);
            EMDEmployment employment = GetMainEploymentForPerson(person.Guid);
            if (employment != null)
            {
                string msg = string.Format("The person-user:{0} has already an active mainemployment:{1}. You can't set a new with: {2}", person.UserID, employment.Guid, emplGuid);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg);
            }

            ObjectFlagManager objectFlagManager = new ObjectFlagManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            objectFlagManager.SetMainEmployment(emplGuid, true);
        }

        /// <summary>
        /// Removes the main flag for an employment
        /// </summary>
        /// <param name="emplGuid"></param>
        public void RemoveMainEmploymentFlag(string emplGuid)
        {
            if (IsMainEmployment(emplGuid))
            {
                ObjectFlagManager objectFlagManager = new ObjectFlagManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                objectFlagManager.SetMainEmployment(emplGuid, false);
            }
        }


        /// <summary>
        /// Update the employment-flag for to new ID, updates all underlying users with AD User
        /// </summary>
        /// <param name="employmentGuidOld"></param>
        /// <param name="employmentGuidNew"></param>
        public void ChangeMainEmployment(string employmentGuidOld, string employmentGuidNew)
        {
            DateTime now = DateTime.Now;

            bool hasTransaction = this.Transaction != null;
            if (!hasTransaction)
            {
                this.Transaction = new CoreTransaction();
            }

            UserManager userManager = new UserManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            try
            {
                // Search for EMDEmployment objects to change
                EMDEmployment oldMainEmployment = GetEmployment(employmentGuidOld);
                EMDEmployment newMainEmployment = GetEmployment(employmentGuidNew);


                if (oldMainEmployment != null && newMainEmployment != null)
                {
                    ObjectFlagManager objectFlagManager = new ObjectFlagManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                    if (!hasTransaction)
                    {
                        this.Transaction.Begin();
                    }

                    userManager.MoveEmploymentUserAndSynchronizePerson(employmentGuidOld, employmentGuidNew);

                    objectFlagManager.SetMainEmployment(newMainEmployment.Guid, true);

                    if (!hasTransaction)
                    {
                        this.Transaction.Commit();
                    }
                }
            }
            catch (BaseException baseException)
            {
                if (!hasTransaction)
                {
                    this.Transaction.Rollback();
                }
                throw baseException;
            }
            catch (Exception ex)
            {
                if (!hasTransaction)
                {
                    this.Transaction.Rollback();
                }
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, ex);
            }

        }

        public bool IsEmploymentOfPerson(string userId, string empl_guid)
        {
            EMDEmployment viewedEmplloyment = this.GetEmployment(empl_guid);
            if (viewedEmplloyment == null)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No employment found for " + empl_guid);
            }
            PersonManager persManager = new PersonManager(this.Transaction);
            return persManager.IsItSelf(userId, viewedEmplloyment.P_Guid);
        }

        public EMDEmployment GetCostcenterManager(string employmentToCheckFor)
        {
            EmploymentHandler emplHandler = new EmploymentHandler();
            return (EMDEmployment)emplHandler.GetObject<EMDEmployment>(employmentToCheckFor);
        }

        public EMDEmployment GetCostcenterManager(EMDEmployment employmentToCheckFor)
        {
            try
            {
                // Kostenstellenleiter für Employment & Accounts suchen
                AccountHandler accountHandler = new AccountHandler(this.Transaction);
                EmploymentAccountHandler employmentAccountHandler = new EmploymentAccountHandler(this.Transaction);

                var account = (from acc in Manager.AccountManager.GetAccounts()
                               join emplAcc in employmentAccountHandler.GetObjects<EMDEmploymentAccount, EmploymentAccount>().Cast<EMDEmploymentAccount>().ToList() on acc.Guid equals emplAcc.AC_Guid
                               where emplAcc.EP_Guid == employmentToCheckFor.Guid
                               select new { acc }).FirstOrDefault();


                if (account.acc.Responsible != null)
                {
                    return accountHandler.GetResponsible(account.acc.Responsible);
                }
            }
            catch (BaseException ex)
            {
                IISLogger logger = ISLogger.GetLogger("EmploymentManager");
                logger.Warn("function IsCostcenterManager - Error :" + ex.Message + " " + ex.StackTrace);

            }
            return null;
        }

        public bool IsCostcenterManager(List<EMDEmployment> employmentsFromUser, EMDEmployment employmentToCheckFor)
        {
            if (employmentsFromUser.Count > 0)
            {
                try
                {
                    // Kostenstellenleiter für Employment & Accounts suchen
                    AccountHandler accountHandler = new AccountHandler(this.Transaction);
                    EmploymentAccountHandler employmentAccountHandler = new EmploymentAccountHandler(this.Transaction);

                    var accounts = (from acc in EDP.Core.Logic.Manager.AccountManager.GetAccounts()
                                    join emplAcc in employmentAccountHandler.GetObjects<EMDEmploymentAccount, EmploymentAccount>().Cast<EMDEmploymentAccount>().ToList() on acc.Guid equals emplAcc.AC_Guid
                                    where emplAcc.EP_Guid == employmentToCheckFor.Guid
                                    select new { acc }).ToList();


                    List<string> costCenterManagerEmplGuids = new List<string>();
                    foreach (var item in accounts)
                    {
                        costCenterManagerEmplGuids.Add(item.acc.Responsible);
                    }

                    //Passt die Employment-Guid überein?
                    foreach (EMDEmployment empl in employmentsFromUser)
                    {
                        foreach (string costCenterManagerEmplGuid in costCenterManagerEmplGuids)
                        {
                            if (empl.Guid.Equals(costCenterManagerEmplGuid))
                                return true;
                        }
                    }
                }
                catch (BaseException ex)
                {
                    IISLogger logger = ISLogger.GetLogger("EmploymentManager");
                    logger.Warn("function IsCostcenterManager - Error :" + ex.Message + " " + ex.StackTrace);
                }
            }
            return false;
        }

        public bool IsAssistence(List<EMDEmployment> employmentsFromUser, EMDEmployment employmentToCheckFor)
        {
            if (employmentsFromUser.Count > 0)
            {
                try
                {
                    // Kostenstellenleiter für Employment & Accounts suchen
                    AccountHandler accountHandler = new AccountHandler(this.Transaction);
                    EmploymentAccountHandler employmentAccountHandler = new EmploymentAccountHandler(this.Transaction);
                    GroupHandler groupHandler = new GroupHandler(this.Transaction);
                    GroupMemberHandler groupMemberHandler = new GroupMemberHandler(this.Transaction);
                    AccountGroupHandler accountGroupHandler = new AccountGroupHandler(this.Transaction);

                    var foundItems = (from grpmbr in groupMemberHandler.GetObjects<EMDGroupMember, GroupMember>().Cast<EMDGroupMember>().ToList()
                                      join grp in groupHandler.GetObjects<EMDGroup, Group>().Cast<EMDGroup>().ToList() on grpmbr.G_Guid equals grp.Guid
                                      join accgrp in accountGroupHandler.GetObjects<EMDAccountGroup, AccountGroup>().Cast<EMDAccountGroup>().ToList() on grp.Guid equals accgrp.G_Guid
                                      join acc in EDP.Core.Logic.Manager.AccountManager.GetAccounts() on accgrp.AC_Guid equals acc.Guid
                                      join emplAcc in employmentAccountHandler.GetObjects<EMDEmploymentAccount, EmploymentAccount>().Cast<EMDEmploymentAccount>().ToList() on acc.Guid equals emplAcc.AC_Guid
                                      where emplAcc.EP_Guid == employmentToCheckFor.Guid
                                      select new { grpmbr }).ToList();


                    List<string> costCenterManagerEmplGuids = new List<string>();
                    foreach (EMDEmployment empl in employmentsFromUser)
                    {
                        foreach (var item in foundItems)
                        {
                            if (item.grpmbr.EP_Guid.Equals(empl.Guid))
                                return true;
                        }
                    }

                }
                catch (BaseException ex)
                {
                    IISLogger logger = ISLogger.GetLogger("EmploymentManager");
                    logger.Warn("function IsAssistence - Error :" + ex.Message + " " + ex.StackTrace);
                }
            }
            return false;
        }

        public List<EMDEmployment> GetEmploymentsInEnterprise(string enteGuid)
        {
            EnterpriseLocationManager enloManager = new EnterpriseLocationManager();
            EmploymentHandler emplHandler = new EmploymentHandler();
            List<string> enloGuids = enloManager.GetEnterpriseLocationGuidsForEnterprise(enteGuid);

            string where = "";
            enloGuids.ForEach(enlo =>
            {
                where += "ENLO = \"" + enlo + "\" || ";
            });

            if (where.Length > 3)
            {
                where = where.Substring(0, where.Length - 3);
            }
            List<EMDEmployment> employmentsInEnterprise = emplHandler.GetObjects<EMDEmployment, Employment>(where).Cast<EMDEmployment>().ToList();
            return employmentsInEnterprise;
        }
        public List<EMDEmployment> GetAssistence(string emplGuid)
        {
            List<EMDEmployment> assistence = new List<EMDEmployment>();
            try
            {
                EmploymentHandler emplHandler = new EmploymentHandler(this.Transaction);
                EMDEmployment empl = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(emplGuid);
                return GetAssistence(empl);
            }
            catch (BaseException ex)
            {
                IISLogger logger = ISLogger.GetLogger("EmploymentManager - GetAssistence");
                logger.Warn("function IsAssistence - Error :" + ex.Message + " " + ex.StackTrace);
            }
            catch (Exception ex)
            {
                IISLogger logger = ISLogger.GetLogger("EmploymentManager - GetAssistence");
                logger.Warn("function IsAssistence - Error :" + ex.Message + " " + ex.StackTrace);
                throw;
            }
            return assistence;
        }

        /// <summary>
        /// Gets all assistance employments, which have an active employment in the systems, for a specific employment
        /// </summary>
        /// <param name="employment"></param>
        /// <returns></returns>
        public List<EMDEmployment> GetAssistence(EMDEmployment employment)
        {
            List<EMDEmployment> result = new List<EMDEmployment>();
            EMD_Entities emdDataContext = new EMD_Entities();

            try
            {
                // Kostenstellenleiter für Employment & Accounts suchen
                EmploymentHandler emplHandler = new EmploymentHandler(this.Transaction);

                var foundItems = (from grpmbr in emdDataContext.GroupMember
                                  join grp in emdDataContext.Group on grpmbr.G_Guid equals grp.Guid
                                  join accgrp in emdDataContext.AccountGroup on grp.Guid equals accgrp.G_Guid
                                  join acc in emdDataContext.Account on accgrp.AC_Guid equals acc.Guid
                                  join emplAcc in emdDataContext.EmploymentAccount on acc.Guid equals emplAcc.AC_Guid
                                  where emplAcc.EP_Guid == employment.Guid
                                  && grpmbr.ActiveTo > DateTime.Now && grpmbr.ValidTo > DateTime.Now && grpmbr.ValidFrom < DateTime.Now && grpmbr.ActiveFrom < DateTime.Now
                                  && grp.ActiveTo > DateTime.Now && grp.ValidTo > DateTime.Now && grp.ValidFrom < DateTime.Now && grp.ActiveFrom < DateTime.Now
                                  && accgrp.ActiveTo > DateTime.Now && accgrp.ValidTo > DateTime.Now && accgrp.ValidFrom < DateTime.Now && accgrp.ActiveFrom < DateTime.Now
                                  && acc.ActiveTo > DateTime.Now && acc.ValidTo > DateTime.Now && acc.ValidFrom < DateTime.Now && acc.ActiveFrom < DateTime.Now
                                  && emplAcc.ActiveTo > DateTime.Now && emplAcc.ValidTo > DateTime.Now && emplAcc.ValidFrom < DateTime.Now && emplAcc.ActiveFrom < DateTime.Now
                                  select new { grpmbr }).ToList();


                foreach (var item in foundItems)
                {
                    EMDEmployment empl = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(item.grpmbr.EP_Guid);
                    if (empl != null && empl.IsSystemActive)
                    {
                        result.Add(empl);
                    }
                }
            }
            catch (BaseException ex)
            {
                IISLogger logger = ISLogger.GetLogger("EmploymentManager - GetAssistence");
                logger.Warn("function IsAssistence - Error :" + ex.Message + " " + ex.StackTrace);
            }
            catch (Exception ex)
            {
                IISLogger logger = ISLogger.GetLogger("EmploymentManager - GetAssistence");
                logger.Warn("function IsAssistence - Error :" + ex.Message + " " + ex.StackTrace);
                throw;
            }
            return result;
        }

        /// <summary>
        /// Find  the previous history employment for a given employment
        /// the criteria is the "latest.ValidFrom equals previous.ValidTo" 
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <returns>an employment previous to current emplyoment</returns>
        public EMDEmployment GetPreviousEmploymentFromHistory(EMDEmployment currentEmpl)
        {
            EMDEmployment result = null;

            EmploymentHandler emplH = new EmploymentHandler();
            emplH.Historical = true;
            emplH.DeliverInActive = false; // ?? what is this

            //var emplList = emplH.GetObjects<EMDEmployment, Employment>("").ToList();
            try
            {
                result = (from empl in emplH.GetObjects<EMDEmployment, Employment>().Cast<EMDEmployment>().ToList()
                          where empl.HistoryGuid == currentEmpl.Guid & empl.ValidTo == currentEmpl.ValidFrom
                          select empl).Single();

                return result;
            }
            catch (Exception ex)
            {
                IEDPLogger logger = EDPLogger.GetLogger("EmploymentManager - GetPreviousEmploymentFromHistory");
                logger.Error(ex.Message + "\r\n" + ex.StackTrace);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error calling GetPreviousEmploymentFromHistory()", ex);
            }
        }

        /// <summary>
        /// (re)sets the ExitDay and LastDay Dates to the values of previous employment
        /// </summary>
        /// <param name="currentEmplGuid"></param>
        /// <returns>current employment with reset ExitDay and LastDay</returns>
        public EMDEmployment SetExitandLastDayToPreviousEmployment(string currentEmplGuid)
        {
            EMDEmployment result;
            result = this.GetEmployment(currentEmplGuid);

            EMDEmployment prevEmployment = this.GetPreviousEmploymentFromHistory(result);



            result.SetExitAndLastDay(prevEmployment.Exit.Value, prevEmployment.LastDay);

            result = this.Update(result);

            return result;
        }

        /// <summary>
        /// returns all available equipements to can (still) be added to given employment.
        /// this functions take filter rules and MaxNumberAllowedEquipments into account
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <returns>List of EMDEquipmentDefs or empty list of none available</returns>
        public List<EMDEquipmentDefinition> GetAvailableEquipmentsForEmployment(string emplGuid)
        {
            // original code
            // c packages = empMngr.GetAvailableListOfEquipmentDefinitionsForEmployment(empl_guid);

            List<EMDEquipmentDefinition> availableList = new List<EMDEquipmentDefinition>();

            EmploymentManager emplMgr = new EmploymentManager();
            PackageManager packMgr = new PackageManager();

            //EquipmentManager eqdeMgr = new EquipmentManager();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["EMD_Direct"].ConnectionString);
            sqlConnection.Open();

            // get all eqde allowed according to filter rules
            sw.Restart();
            FilterCriteria filterCriteria = emplMgr.GetFilterCriteriaFromEmployment_DIRECT(sqlConnection, emplGuid);
            //Console.WriteLine("after GetFilterCriteriaFromEmployment_DIRECT(), duration:" + sw.ElapsedMilliseconds);

            //FilterCriteria ff = emplMgr.GetFilterCriteriaFromEmployment(emplGuid);
            //Console.WriteLine("after GetFilterCriteriaFromEmployment(), duration:" + sw.ElapsedMilliseconds);

            sw.Restart();
            Console.Write("started GetFilteredListOfEquipmentDefinitions() ... ");
            var allEqdeList = packMgr.GetFilteredListOfEquipmentDefinitions(filterCriteria);
            Console.WriteLine("finished. duration:" + sw.ElapsedMilliseconds);

            sw.Restart();
            // load shit into Hashset in memory, a lot quicker than hundreds of sql queries
            HashSet<Tuple<string, int>> allMaxAllowed = this.GetMaxAllowedObresForEqde(sqlConnection);
            HashSet<Tuple<string, int>> obreCountAll = this.GetALLObreCountForEmployment(sqlConnection, emplGuid);

            foreach (EMDEquipmentDefinition eqdeItem in allEqdeList)
            {
                //sw.Restart();
                int maxAllowedEqde = allMaxAllowed.Single(p => p.Item1 == eqdeItem.Guid).Item2;
                //Console.WriteLine("after GetMaxAllowedObresForEqde(), elapsed:" + sw.ElapsedMilliseconds);

                // get obre count           
                //sw.Restart();
                //int foundEquipments = this.GetCountofObresForEqde(sqlConnection, emplGuid, eqdeItem.Guid);
                var tmp = obreCountAll.SingleOrDefault(p => p.Item1 == eqdeItem.Guid);
                int foundEquipments = 0;
                if (tmp != null) foundEquipments = tmp.Item2;
                //Console.WriteLine("duration GetCountofObresForEqde(), elapsed:" + sw.ElapsedMilliseconds);

                if (foundEquipments < maxAllowedEqde /*equipmentDefinitionConfig.MaxNumberAllowedEquipments*/)
                {
                    // allowed
                    //eqdeNames.Add(eqdeItem.Name);
                    availableList.Add(eqdeItem);
                }
                //else
                //{
                //    eqdeNamesNOT.Add(eqdeItem.Name);
                //}
            }
            //Console.WriteLine("after loop checking maxallowed, duration: " + sw.ElapsedMilliseconds);
            sqlConnection.Close();


            return availableList;
        }

        private HashSet<Tuple<string, int>> GetMaxAllowedObresForEqde(SqlConnection sqlConnection)
        {
            HashSet<Tuple<string, int>> allMaxAllowed = new HashSet<Tuple<string, int>>();

            string countQuery = "SELECT guid,config.value('(EquipmentDefinition/MaxNumberAllowedEquipments)[1]','int') FROM dbo.EquipmentDefinition where guid = historyguid";

            try
            {
                SqlCommand sqlCommand = new SqlCommand(countQuery, sqlConnection);
                var reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    allMaxAllowed.Add(Tuple.Create<string, int>(reader.GetString(0), reader.GetInt32(1)));
                }
                reader.Close();
                return allMaxAllowed;
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, ex);
            }
        }

        private HashSet<Tuple<string, int>> GetALLObreCountForEmployment(SqlConnection sqlConnection, string emplGuid)
        {
            HashSet<Tuple<string, int>> obreInstances = new HashSet<Tuple<string, int>>();

            //string countQuery = "SELECT guid,config.value('(EquipmentDefinition/MaxNumberAllowedEquipments)[1]','int') FROM dbo.EquipmentDefinition where guid = historyguid";
            string obreCountQuery = "select COUNT(Object2), Object2 from ObjectRelation where Object1 = '{0}'  AND Status <= 50 and guid = historyguid group BY object2";
            obreCountQuery = string.Format(obreCountQuery, emplGuid);
            try
            {
                SqlCommand sqlCommand = new SqlCommand(obreCountQuery, sqlConnection);
                var reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    obreInstances.Add(Tuple.Create<string, int>(reader.GetString(1), reader.GetInt32(0)));
                }
                reader.Close();
                return obreInstances;
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, ex);
            }
        }

        private int GetCountofObresForEqde(SqlConnection sqlConnection, string emplGuid, string eqdeGuid)
        {
            int amountObres = 0;
            string countQuery = "select count(*) from ObjectRelation where Object1 = '{0}' AND Object2 = '{1}' AND Status <= 50 and guid = historyguid";
            countQuery = string.Format(countQuery, emplGuid, eqdeGuid);
            try
            {
                SqlCommand sqlCommand = new SqlCommand(countQuery, sqlConnection);
                amountObres = (int)sqlCommand.ExecuteScalar();
                return amountObres;
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, ex);
            }
        }

    }
}

using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Collections.Generic;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Runtime.Caching;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Basic handler to get objects from the database.
    /// </summary>
    public abstract class EMDBaseObjectHandler : IDisposable
    {
        /// <summary>
        /// Logger for all derived classes. Removed since Core must not log. (so keep all code simple ;) )
        /// </summary>
        /// protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        //TODO: Make fields private

        /// <summary>
        /// Indicates whether a internal, local transaction or a external transaction is used. 
        /// </summary>
        public bool transactionIsLocal = false;

        /// <summary>The transaction to use for database-access. Either internal created or given by constructor. Backing field for <see cref="Transaction"/></summary>
        public CoreTransaction transaction = null;

        /// <summary>
        /// Indicates whether this handler also gets invalid (e.g. old versions) database-objects.
        /// </summary>
        public bool Historical { get; set; }

        /// <inheritdoc/>
        public CoreTransaction Transaction
        {
            get { return this.transaction; }
            set
            {
                if (this.transaction != value)
                {
                    if (value == null)
                    {
                        this.transaction = new CoreTransaction();
                        this.transactionIsLocal = true;
                    }
                    else
                    {
                        this.transaction = value;
                        this.transactionIsLocal = false;
                    }
                }
            }
        }

        /// GUID of the person ("PERS") which uses this handler to modify objects.
        public string Guid_ModifiedBy { get; set; }

        /// Modification comment for all changes made with this handler.
        public string ModifyComment { get; set; }

        /// <summary>
        /// Indicates whether this handler also gets inactive database-objects.
        /// </summary>
        public bool DeliverInActive { get; set; }

        #region Caching
        internal static ObjectCache cache = MemoryCache.Default;

        //Create a custom Timeout of 10 seconds
        private static CacheItemPolicy policy = new CacheItemPolicy();

        //Create a custom Timeout for extended cache
        private static CacheItemPolicy policyExtended = new CacheItemPolicy();

        internal List<string> GetCacheKeys(Type type)
        {

            List<string> cacheKeys = new List<string>();
            if (Configuration.ISCACHINGACTIVE)
            {
                IEnumerable<KeyValuePair<string, object>> en = cache.AsEnumerable();

                cacheKeys = (from x in en where x.Key.StartsWith(type.FullName) select x.Key).ToList();
            }
            return cacheKeys;
        }

        internal void RemoveCache(Type type)
        {
            if (Configuration.ISCACHINGACTIVE)
            {
                List<string> cacheKeys = GetCacheKeys(type);
                if (cacheKeys != null)
                {
                    foreach (string cacheKey in cacheKeys)
                    {
                        cache.Remove(cacheKey);
                    }
                }
            }
            Configuration.MODIFICATION_ID = Guid.NewGuid();
        }

        public void ClearCache()
        {
            if (Configuration.ISCACHINGACTIVE)
            {
                IEnumerable<KeyValuePair<string, object>> en = cache.AsEnumerable();

                List<string> cacheKeys = (from x in en select x.Key).ToList();

                if (cacheKeys != null)
                {
                    foreach (string cacheKey in cacheKeys)
                    {
                        cache.Remove(cacheKey);
                    }
                }
            }
            Configuration.MODIFICATION_ID = Guid.NewGuid();
        }
        #endregion


        #region Constructors

        /// <summary>
        /// Initializes a new instance of this handler with <see cref="transactionIsLocal"/>=<see langword="true"/> and no <see cref="Guid_ModifiedBy"/> and no <see cref="ModifyComment"/>.
        /// </summary>
        /// <param name="logtype">The logging type for log4j is the type of the specific handler-implementation.</param>
        public EMDBaseObjectHandler(Type logtype)
        {
            transactionIsLocal = true;
            CheckForLocalTransaction();
            Guid_ModifiedBy = null;
            ModifyComment = null;
        }

        /// <summary>
        /// Initializes a new instance of this handler with an external transaction for database access and no <see cref="Guid_ModifiedBy"/> and no <see cref="ModifyComment"/>.
        /// </summary>
        /// <param name="logtype">The logging type for log4j is the type of the specific handler-implementation.</param>
        /// <param name="transaction">Database-transaction to use for database-access.</param>
        public EMDBaseObjectHandler(Type logtype, Framework.CoreTransaction transaction)
        {
            if (transaction == null)
            {
                transactionIsLocal = true;
                CheckForLocalTransaction();
            }
            else
            {
                transactionIsLocal = false;
                this.transaction = transaction;
            }
            this.transaction.RollBackEvent += Transaction_RollBackEvent;
            Guid_ModifiedBy = null;
            ModifyComment = null;
        }

        /// <summary>
        /// Initializes a new instance of this handler with <see cref="transactionIsLocal"/>=<see langword="true"/>.
        /// </summary>
        /// <param name="logtype">The logging type for log4j is the type of the specific handler-implementation.</param>
        /// <param name="guid_ModifiedBy"><see cref="Guid_ModifiedBy"/></param>
        /// <param name="modifyComment"><see cref="ModifyComment"/></param>
        public EMDBaseObjectHandler(Type logtype, string guid_ModifiedBy, string modifyComment = null)
            : this(logtype)
        {
            this.Guid_ModifiedBy = guid_ModifiedBy;
            this.ModifyComment = modifyComment;
        }

        /// <summary>
        /// Initializes a new instance of this handler with an external transaction for database access.
        /// </summary>
        /// <param name="logtype">The logging type for log4j is the type of the specific handler-implementation.</param>
        /// <param name="transaction">Database-transaction to use for database-access.</param>
        /// <param name="guid_ModifiedBy"><see cref="Guid_ModifiedBy"/></param>
        /// <param name="modifyComment"><see cref="ModifyComment"/></param>
        public EMDBaseObjectHandler(Type logtype, Framework.CoreTransaction transaction, string guid_ModifiedBy, string modifyComment)
            : this(logtype, transaction)
        {
            this.Guid_ModifiedBy = guid_ModifiedBy;
            this.ModifyComment = modifyComment;
        }

        #endregion Constructors

        private void Transaction_RollBackEvent(object sender, EventArgs e)
        {
            ClearCache();
        }

        /// <summary>
        /// Gets the type of the database-object in the Entity-Framework (<see cref="DB"/>).
        /// </summary>
        /// <returns>Type of the database-object in the Entity-Framework (<see cref="DB"/>).</returns>
        public abstract Type GetDBObjectType();

        /// <summary>
        /// Gets an database-object as Entity-Framework-Type with the given GUID.
        /// </summary>
        /// <typeparam name="T">Entity-Framework-Type of the object.</typeparam>
        /// <param name="guid">GUID of the object to get.</param>
        /// <returns>Object with the given GUID, throws an exception if no such object exists.</returns>
        public T GetDBObject<T>(string guid)
        {
            CheckForLocalTransaction();

            var dbObject = Activator.CreateInstance(this.GetDBObjectType());
            dbObject = transaction.dbContext.Set(this.GetDBObjectType()).Find(guid);
            return (T)dbObject;
        }

        /// <summary>
        /// Get the database-object with the given GUID.
        /// </summary>
        /// <typeparam name="EMDType">Type of the specific object-implementation.</typeparam>
        /// <param name="guid">GUID of the object to get.</param>
        /// <returns>Object with the given GUID or <see langword="null"/> if and only if no such object exists.</returns>
        public IEMDObject<EMDType> GetObject<EMDType>(String guid)
        {
            //https://msdn.microsoft.com/en-us/data/jj573936.aspx

            string cacheKey = null;
            object dbObject = null;

            if (!string.IsNullOrWhiteSpace(guid) && Configuration.ISCACHINGACTIVE)
            {
                cacheKey = string.Format("{0}-GUID:{1}", typeof(EMDType).FullName, guid);
                dbObject = cache.Get(cacheKey);
            }

            if (dbObject == null)
            {
                CheckForLocalTransaction();
                dbObject = Activator.CreateInstance(this.GetDBObjectType());
                dbObject = transaction.dbContext.Set(this.GetDBObjectType()).Find(guid);

                if (!string.IsNullOrWhiteSpace(guid) && Configuration.ISCACHINGACTIVE)
                {
                    if (!Configuration.IsExtendedEntity(typeof(EMDType).Name))
                    {
                        policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES);
                        cache.Add(cacheKey, dbObject, policy);
                    }
                    else
                    {
                        policyExtended.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES_EXTENDED);
                        cache.Add(cacheKey, dbObject, policyExtended);
                    }
                }
            }

            return CreateDataFromDBObject<EMDType>(dbObject);
        }

        /// <summary>
        /// Gets all database-objects that match the given WHERE-clause.
        /// </summary>
        /// <typeparam name="EMDType">Type of the specific object-implementation.</typeparam>
        /// <typeparam name="EFType">Type of the object in the Entity-Framework.</typeparam>
        /// <param name="whereclause">SQL-WHERE-clause to filter the objects.</param>
        /// <param name="paging">Not implemented.</param>
        /// <returns>Returns a list of objects that match the given WHERE-clause or an empty-list if no objects are found.</returns>
        public List<IEMDObject<EMDType>> GetObjects<EMDType, EFType>(string whereclause = null, DatabasePaging paging = null) where EMDType : IEMDObject<EMDType>, new()
        {
            //http://stackoverflow.com/questions/2078914/creating-a-generict-type-instance-with-a-variable-containing-the-type

            string cacheKey = null;
            ConcurrentBag<IEMDObject<EMDType>> emdObjectBag = null;

            if (Configuration.ISCACHINGACTIVE)
            {
                cacheKey = string.Format("{0}-{1}-{2}-{3}-{4}", typeof(EMDType).FullName, this.Historical, this.DeliverInActive, whereclause != null ? whereclause.Replace(" ", string.Empty) : "nowhereclause", paging != null ? paging.ToString() : "noPaging");
                emdObjectBag = cache.Get(cacheKey) as ConcurrentBag<IEMDObject<EMDType>>;
            }


            if (emdObjectBag == null)
            {
                CheckForLocalTransaction();

                IPropertyCopier<EFType, EMDType> copier = PropertyCopierProvider.Request<EFType, EMDType>();// new FastPropertyCopier<EFType, EMDType>();

                if (paging != null)
                {
                    IQueryable<EFType> countquery = (IQueryable<EFType>)transaction.dbContext.Set(typeof(EFType)).AsQueryable();
                    countquery = (IQueryable<EFType>)AddHandlerClauses<EFType>(countquery, whereclause, paging, true);
                    paging.NrOfAllElements = countquery.Count();
                }


                // get objects
                IQueryable<EFType> query = (IQueryable<EFType>)transaction.dbContext.Set(typeof(EFType)).AsQueryable();
                query = (IQueryable<EFType>)AddHandlerClauses<EFType>(query, whereclause, paging, false);

                emdObjectBag = new ConcurrentBag<IEMDObject<EMDType>>();

                Parallel.ForEach(query, dbObject =>
                {
                    EMDType emdObject = CreateDataFromDBObject<EMDType, EFType>(dbObject, copier);
                    emdObjectBag.Add(emdObject);
                });

                if (Configuration.ISCACHINGACTIVE)
                {
                    if (!Configuration.IsExtendedEntity(typeof(EMDType).Name))
                    {
                        policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES);
                        cache.Add(cacheKey, emdObjectBag, policy);
                    }
                    else
                    {
                        policyExtended.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES_EXTENDED);
                        cache.Add(cacheKey, emdObjectBag, policyExtended);
                    }
                }
            }

            return emdObjectBag.ToList();
        }

        /// <summary>
        /// Returns all objects that are active in the given timespan (<paramref name="fromDate"/>, <paramref name="toDate"/>).
        /// </summary>
        /// <typeparam name="EMDType">Type of the specific object-implementation.</typeparam>
        /// <typeparam name="EFType">Type of the object in the Entity-Framework.</typeparam>
        /// <param name="fromDate">Start-date of the timespan.</param>
        /// <param name="toDate">End-date of the timespan.</param>
        /// <param name="whereclause">SQL-WHERE-clause to filter the objects.</param>
        /// <param name="paging">Not implemented</param>
        /// <returns>List of all objects in the given timespan that match the WHERE-clause.</returns>
        /// <seealso cref="GetObjects{EMDType, EFType}(string, DatabasePaging)"/>
        public List<EMDType> GetActiveObjectsInInterval<EMDType, EFType>(DateTime fromDate, DateTime toDate, string whereclause = null, DatabasePaging paging = null) where EMDType : IEMDObject<EMDType>, new()
        {
            //http://stackoverflow.com/questions/2078914/creating-a-generict-type-instance-with-a-variable-containing-the-type

            CheckForLocalTransaction();

            List<EFType> dbObjectList = new List<EFType>();

            IPropertyCopier<EFType, EMDType> copier = PropertyCopierProvider.Request<EFType, EMDType>();// new FastPropertyCopier<EFType, EMDType>();

            //select count for all objects without paging-clause
            if (paging != null)
            {
                IQueryable<EFType> countquery = (IQueryable<EFType>)transaction.dbContext.Set(typeof(EFType)).AsQueryable();
                countquery = (IQueryable<EFType>)AddIntervalHandlerClauses<EFType>(countquery, fromDate, toDate, whereclause, paging, true);
                paging.NrOfAllElements = countquery.Count();
            }

            //get objects
            IQueryable<EFType> query = (IQueryable<EFType>)transaction.dbContext.Set(typeof(EFType)).AsQueryable();
            query = this.AddIntervalHandlerClauses<EFType>(query, fromDate, toDate, whereclause, paging);
            dbObjectList = query.ToList();

            ConcurrentBag<EMDType> emdObjectBag = new ConcurrentBag<EMDType>();

            Parallel.ForEach(dbObjectList, dbObject =>
            {
                EMDType emdObject = this.CreateDataFromDBObject<EMDType, EFType>(dbObject, copier);
                emdObjectBag.Add(emdObject);
            });

            return emdObjectBag.ToList();
        }

        /// <summary>
        /// Returns a hash-table with existing entities related to the given object by its GUID.
        /// </summary>
        /// <remarks>
        /// <b>Warning:</b> Constraints/Relation that are not relevant for deleting are not returned. <br/>
        /// <ul>
        /// <li>If entities is of type EntityReference it is not relevant for deleting (e.g. PERS points at USER)</li>
        /// <li>Entities of type EntitiyCollection are relevant ! (e.g. EMPL points at PERS)</li>
        /// </ul>
        /// </remarks>
        /// <param name="guid">Object which related object are needed.</param>
        /// <returns>Hash-table of all related entities.</returns>
        public Hashtable GetRelatedEntities(String guid)
        {
            Hashtable references = new Hashtable();
            CheckForLocalTransaction();
            var DB = new EMD_Entities();

            MethodInfo method = this.GetType().GetMethod("GetDBObject");
            MethodInfo generic = method.MakeGenericMethod(this.GetDBObjectType());

            //var dbObject = Activator.CreateInstance(this.GetDBObjectType());
            var dbObject = generic.Invoke(this, new object[] { guid });

            if (dbObject == null) throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "GetRelatedEntities: dbobject for given guid " + guid + " was not found");

            bool canGetRelationshipManager;
            ObjectStateManager objStateManager = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)DB).ObjectContext.ObjectStateManager;
            RelationshipManager relationMgr;
            canGetRelationshipManager = objStateManager.TryGetRelationshipManager(dbObject, out relationMgr);

            if (canGetRelationshipManager)
            {
                List<IRelatedEnd> relatedEntities = relationMgr.GetAllRelatedEnds().ToList();
                foreach (var entities in relatedEntities)
                {
                    // if entities is of type EntityReference it is not relevant for deleting (e.g. PERS points at USER)
                    // entities of type EntitiyCollection are relevant ! (e.g. EMPL points at PERS)
                    // http://stackoverflow.com/a/983061/925
                    if (entities.GetType().Name.StartsWith("EntityReference")) // EntityReference´1
                        continue;

                    int i = 0;
                    IList list = ((IListSource)entities).GetList();
                    if (list.Count > 0)
                    {
                        var enumerator = entities.GetEnumerator();
                        var listDbObject = list[0];
                        PropertyInfo propertyInfoActiveTo = listDbObject.GetType().GetRuntimeProperty("ActiveTo");
                        PropertyInfo propertyInfoValidTo = listDbObject.GetType().GetRuntimeProperty("ValidTo");

                        //PropertyInfo piAF = listDbObject.GetType().GetRuntimeProperty("ActiveFrom");

                        foreach (var entity in entities)
                        {
                            //DateTime activeFrom = (DateTime) piAF.GetValue(entity);
                            DateTime activeTo = (DateTime)propertyInfoActiveTo.GetValue(entity);
                            DateTime validTo = (DateTime)propertyInfoValidTo.GetValue(entity);

                            // check both end dates for an active entry
                            // remember: ActiveTo should always be the greater item, but because of errors in business-logic or Migration, this mustn't be true

                            string typeName = listDbObject.GetType().Name;
                            if (!typeName.ToLower().Contains("employment_"))
                            {
                                if (activeTo > DateTime.Now && validTo > DateTime.Now)
                                {
                                    i++;
                                }
                            }
                        }
                        if (i > 0)
                        {
                            String name = list[0].GetType().Name;
                            if (name.Contains("_") && !name.ToLower().Contains("employment_"))
                            {
                                references.Add("EMD" + name.Substring(0, name.IndexOf('_')), i);
                            }
                        }
                    }
                }
            }
            else
            {
                // no db entities with relations defined (exist) for entity dbOject.Guid
                // this is not an error
                return new Hashtable();
            }
            return references;
        }

        /// <summary>
        /// Writes updates to an existing object to the database.
        /// </summary>
        /// <typeparam name="T">Type of the specific object-implementation.</typeparam>
        /// <param name="emdObject">Updated object.</param>
        /// <param name="historize">Indicates whether the old object should be kept in database as invalid object (for version history) or not. Default is <see langword="true"/>.</param>
        /// <param name="checkActiveTo">Indicates whether the method allows inactive objects to be updated or not. Default is <see langword="true"/>.</param>
        /// <returns>The object with auto-updated fields (e.g. <see cref="EMDObject{T}.Modified"/>).</returns>
        /// <exception cref="BaseException"> If <paramref name="checkActiveTo"/> is <see langword="true"/> and the given object is not active anymore or if either <see cref="EMDObject{T}.ActiveFrom"/> or <see cref="EMDObject{T}.ActiveTo"/> have changed.</exception>
        public abstract IEMDObject<T> UpdateObject<T>(IEMDObject<T> emdObject, bool historize = true, bool checkActiveTo = true, bool allowChangeActive = false);

        /// <summary>
        /// Writes a new object to the database.
        /// </summary>
        /// <typeparam name="T">Type of the specific object-implementation.</typeparam>
        /// <param name="emdObject">New object to be written.</param>
        /// <param name="guid">GUID of the object, if <see langword="null"/> (default) a new GUID is generated.</param>
        /// <param name="datesAreSet">Indicates whether the dates (e.g. <see cref="EMDObject{T}.Created"/>) are already set or need to be automatically filled in. Default is <see langword="false"/>.</param>
        /// <returns>The object with auto-filled-in fields.</returns>
        public abstract IEMDObject<T> CreateObject<T>(IEMDObject<T> emdObject, string guid = null, bool datesAreSet = false);

        /// <summary>
        /// Deletes the given object from the database.
        /// </summary>
        /// <typeparam name="T">Type of the specific object-implementation.</typeparam>
        /// <param name="emdObject">Object to be deleted.</param>
        /// <param name="historize">Indicates whether the object should be kept as invalid (for version history) or not. Default is <see langword="true"/>.</param>
        /// <param name="ignoreDependency">Indicates whether dependencies should be ignored or not. Default is <see langword="false"/>.</param>
        /// <param name="dueDate">If set specifies the date on which the entity is deleted. If null <see cref="DateTime.Now"/> is used.</param>
        /// <returns>The object with auto-updated fields (e.g. <see cref="EMDObject{T}.ValidTo"/>).</returns>
        /// <exception cref="BaseException">Thrown if <paramref name="ignoreDependency"/> is <see langword="false"/> and there are still dependencies.</exception>
        public abstract IEMDObject<T> DeleteObject<T>(IEMDObject<T> emdObject, bool historize = true, bool ignoreDependency = false, DateTime? dueDate = null);

        /// <summary>
        /// Activates an object (<see cref="EMDObject{T}.Activate"/>) and writes the changes to the database.
        /// </summary>
        /// <typeparam name="T">Type of the specific object-implementation.</typeparam>
        /// <param name="emdObject">Object to be activated.</param>
        /// <param name="historize">Indicates whether the old object should be kept in database as invalid object (for version history) or not. Default is <see langword="true"/>.</param>
        /// <seealso cref="SetInactive{T}(IEMDObject{T}, bool)"/>
        public abstract void SetActive<T>(IEMDObject<T> emdObject, bool historize = true);

        /// <summary>
        /// Deactivates an object (<see cref="EMDObject{T}.Deactivate"/>) and writes the changes to the database.
        /// </summary>
        /// <typeparam name="T">Type of the specific object-implementation.</typeparam>
        /// <param name="emdObject">Object to be deactivated.</param>
        /// <param name="historize">Indicates whether the old object should be kept in database as invalid object (for version history) or not. Default is <see langword="true"/>.</param>
        /// <exception cref="BaseException">Thrown if there are dependencies on this object in the database.</exception>
        /// <seealso cref="SetActive{T}(IEMDObject{T}, bool)"/>
        public abstract void SetInactive<T>(IEMDObject<T> emdObject, bool historize = true);

        /// <summary>
        /// Creates a EMD-object from an Entity-Framework-object.
        /// <seealso cref="CreateDataFromDBObject{EMDType, EFType}(EFType, IPropertyCopier{EFType, EMDType})"/>.
        /// </summary>
        /// <typeparam name="T">Type of the specific object-implementation.</typeparam>
        /// <param name="dbObject">The given Entity-Framework-object.</param>
        /// <returns>EMD-object with interface-type where all fields are copied from <paramref name="dbObject"/>.</returns>
        internal abstract IEMDObject<T> CreateDataFromDBObject<T>(Object dbObject);

        /// <summary>
        /// Creates a EMD-object from an Entity-Framework-object.
        /// <seealso cref="CreateDataFromDBObject{T}(object)"/>
        /// </summary>
        /// <remarks>
        /// In difference to <see cref="CreateDataFromDBObject{T}(object)"/> this implementation uses an given instance of <see cref="IPropertyCopier{S, T}"/> 
        /// to copy the fields which can offer a huge performance-boost. Also it returns an object of an specific type, not the Interface <see cref="IEMDObject{T}"/>.
        /// </remarks>
        /// <typeparam name="EMDType">Type of the specific object-implementation.</typeparam>
        /// <typeparam name="EFType">Type of the object in the Entity-Framework.</typeparam>
        /// <param name="dbObject">The given Entity-Framework-object</param>
        /// <param name="propCopier">Copier to copy all properties from one object to another using reflection.</param>
        /// <returns>EMD-object with specific type where all fields are copied from <paramref name="dbObject"/>.</returns>
        internal virtual EMDType CreateDataFromDBObject<EMDType, EFType>(EFType dbObject, IPropertyCopier<EFType, EMDType> propCopier) where EMDType : IEMDObject<EMDType>, new()
        {
            if (dbObject == null)
            {
                return default(EMDType);
            }
            else
            {
                EMDType emdObject = new EMDType();
                propCopier.CopyAll(dbObject, ref emdObject);
                emdObject.SetValidityStatus();
                return emdObject;
            }
        }

        /// <summary>
        /// Copies the field from a given Entity-Framework-object to a given EMD-object.
        /// <seealso cref="CreateDataFromDBObject{T}(object)"/>
        /// <seealso cref="CreateDataFromDBObject{EMDType, EFType}(EFType, IPropertyCopier{EFType, EMDType})"/>
        /// </summary>
        /// <typeparam name="T">Type of the specific object-implementation.</typeparam>
        /// <param name="dbObject">The given Entity-Framework-object as source.</param>
        /// <param name="emdObject">The EMD-object as target.</param>
        internal virtual void MapDataToDBObject<T>(ref object dbObject, ref IEMDObject<T> emdObject)
        {
            emdObject.SetValidityStatus();
            ReflectionHelper.CopyProperties(ref emdObject, ref dbObject);
        }

        /// <summary>
        /// Checks whether a transaction is present, if not creates a local transaction and sets <see cref="transactionIsLocal"/> to <see langword="true"/>.
        /// </summary>
        public void CheckForLocalTransaction()
        {
            if (transaction == null)
            {
                transactionIsLocal = true;
                //Framework.TransactionHandler th = Framework.TransactionHandler.Instance;
                transaction = new CoreTransaction();

                transaction.RollBackEvent += Transaction_RollBackEvent;
            }
        }

        /// <summary> 
        /// Applies the given WHERE-clause and the paging-clause (not implemented) to the given <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <typeparam name="S">Entity-Framework-type of the objects which are queried.</typeparam>
        /// <param name="query">Objects to apply the clauses to.</param>
        /// <param name="whereclause">SQL-clause to be applied. If <see langword="null"/> nothing will be filtered.</param>
        /// <param name="paging">Not implemented.</param>
        /// <param name="filteronly">Paging is not applied if <see langword="true"/>.</param>
        /// <returns>Remaining objects.</returns>
        internal virtual IQueryable<S> AddHandlerClauses<S>(IQueryable<S> query, string whereclause = null, DatabasePaging paging = null, bool filteronly = false)
        {
            //http://dynamiclinq.azurewebsites.net/GettingStarted

            if (paging != null)
                query = LinqHelper.filteryQuery(query, paging);
            if (whereclause != null)
                query = query.Where(whereclause);
            if (paging != null && filteronly != true)
                query = LinqHelper.pageQuery(query, paging);

            return query;
        }

        /// <summary>
        /// Applies the same clauses as <see cref="AddHandlerClauses{S}(IQueryable{S}, string, DatabasePaging, bool)"/>, but additionally adds a clause that only allows objects where
        /// <see cref="EMDObject{T}.ActiveTo"/> is between <paramref name="fromDate"/> and <paramref name="toDate"/>.
        /// </summary>
        /// <typeparam name="S">Entity-Framework-type of the objects which are queried.</typeparam>
        /// <param name="query">Objects to apply the clauses to.</param>
        /// <param name="fromDate">Start of the allowed timespan.</param>
        /// <param name="toDate">End of the allowed timespan.</param>
        /// <param name="whereclause">SQL-clause to be applied. If <see langword="null"/> nothing will be filtered.</param>
        /// <param name="paging">Not implemented.</param>
        /// <param name="filteronly">Paging is not applied if <see langword="true"/>.</param>
        /// <returns>Remaining objects.</returns>
        internal virtual IQueryable<S> AddIntervalHandlerClauses<S>(IQueryable<S> query, DateTime fromDate, DateTime toDate, string whereclause = null, DatabasePaging paging = null, bool filteronly = false)
        {
            if (paging != null)
                query = LinqHelper.filteryQuery(query, paging);
            if (whereclause != null)
                query = query.Where(whereclause);
            if (paging != null && filteronly != true)
                query = LinqHelper.pageQuery(query, paging);

            query = query.Where("ActiveFrom < @0 && ActiveTo > @1 && ValidFrom < DateTime.Now && ValidTo > DateTime.Now", toDate, fromDate);

            return query;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// If <paramref name="disposing"/> is <see langword="true"/> unhooks all events and disposes the local transaction.
        /// </summary>
        /// <param name="disposing">Determine whether this handler should be disposed or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (transaction != null)
                    {
                        // dispose managed state (managed objects).
                        transaction.RollBackEvent -= Transaction_RollBackEvent;
                        if (transactionIsLocal)
                            transaction.Dispose();
                        transaction = null;
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EMDBaseObjectHandler() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Safely disposes this handler. 
        /// <seealso cref="Dispose(bool)"/>
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // cTODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
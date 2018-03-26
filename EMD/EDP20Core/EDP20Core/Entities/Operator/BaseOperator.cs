using Kapsch.IS.EDP.Core.Entities.Operator.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Kapsch.IS.EDP.Core.Entities.Operator.Enum;
using Kapsch.IS.Util.Logging;
using System.Reflection;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Utils;
using System.Runtime.Caching;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using System.Linq.Expressions;
using System.Data.Entity.Validation;
using System.Data.Entity.Core.Objects;
using Kapsch.IS.EDP.Core.DB;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Collections;
using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;

namespace Kapsch.IS.EDP.Core.Entities.Operator
{

    [Obsolete("Only implementation-recommendation!", true)]
    public abstract class BaseOperator<EFType, EMDType>
        : IReadOperator<EFType, EMDType>, IWriteOperator<EFType, EMDType>, IDisposable
        where EMDType : IEMDObject<EMDType>, new()
        where EFType : class, new()
    {
        /// <summary>
        /// Backing field for <see cref="IsLocalTransaction"/>. Is only set in the getter of <see cref="Transaction"/>. Do not access this field!
        /// </summary>
        private bool isLocal = false;
        /// <summary>
        /// Indicates whether this operator uses a local (internal) transaction or an external one.
        /// </summary>
        protected bool IsLocalTransaction
        {
            get
            {
                return isLocal;
            }
        }

        /// <summary>
        /// Backing field for <see cref="Transaction"/>. Do not access this field.
        /// </summary>
        private CoreTransaction transaction = null;
        /// <summary>
        /// Gives access to the transaction used by this handler.
        /// </summary>
        /// <seealso cref="IsLocalTransaction"/>
        protected CoreTransaction Transaction
        {
            get
            {
                if (transaction == null)
                {
                    transaction = new CoreTransaction();
                    isLocal = true;
                }
                return transaction;
            }
        }
        /// <summary>
        /// Method that clears the internal cache in case of an Rollback. Needs to be hooked to <see cref="CoreTransaction.RollBackEvent"/>. This is done in the constructor.
        /// </summary>
        private void OnTransactionRollback(object sender, EventArgs e)
        {
            ClearCache();
        }

        /// <summary>
        /// Backing field for <see cref="GUID_ModifiedBy"/>. Do not access this field.
        /// </summary>
        private EMDGuid modifiedBy;
        /// <summary>
        /// GUID of the <see cref="EMDPerson"/> which uses this operator for modifications. 
        /// Has to be the GUID of a Person with the corresponding GUID-Prefix 'PERS' or <see langword="null"/> if the modifications are not made by a person.
        /// </summary>
        /// <exception cref="EntityNotAllowedException">Is thrown if the assigned value is not a Person-GUID.</exception>
        public EMDGuid GUID_ModifiedBy
        {
            get { return modifiedBy; }
            protected set
            {
                if (value == null || value.Prefix == "PERS") { modifiedBy = value; }
                else
                {
                    throw new EntityNotAllowedException(typeof(EMDPerson).FullName,
                                                        EnumEntityNotAllowedError.EntityNotAllowed, ErrorCodeHandler.E_EDP_ENTITY,
                                                        "ModifiedBy has to be a GUID for EMDPerson (Prefix 'PERS') but was '" + value.Guid + "'.");
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="WorkingDate"/>. Do not access this field.
        /// </summary>
        private DateTime? working = null;
        /// <summary>
        /// The date the operator uses for all active and valid queries. If not specified otherwise <see cref="DateTime.Now"/> is used.
        /// If set to a custom value all writing access to the database is locked and calling one of the corresponding methods will result in an <see cref="BaseException"/> with the 
        /// error-code <see cref="ErrorCodeHandler.E_EDP_SECURITY"/>.
        /// </summary>
        public DateTime WorkingDate
        {
            get
            {
                if (working == null)
                {
                    return DateTime.Now;
                }
                return working.Value;
            }
        }

        /// <summary>
        /// Comment describing the modifications made by this operator.
        /// </summary>
        public string ModificationComment { get; protected set; }

        /// <summary>
        /// The GUID-prefix of the entities handled by the specific Operator-Implementation. Trying to handle objects with a wrong GUID-prefix results in a <see cref="BaseException"/> with 
        /// the error-code <see cref="ErrorCodeHandler.E_EDP_ENTITY"/>.
        /// </summary>
        public abstract string EntityPrefix { get; }

        /// <summary>
        /// Expression used to determine whether a given database-record is valid or not.
        /// </summary>
        protected abstract Expression<Func<EFType, bool>> IsValid { get; }
        /// <summary>
        /// Expression used to determine whether a given database-object is active or not.
        /// </summary>
        protected abstract Expression<Func<EFType, bool>> IsActive { get; }

        /// <summary>
        /// Generates an expression to check if an database-record has a matching history-GUID.
        /// </summary>
        /// <param name="historyGuid">The history-GUID to match.</param>
        /// <returns>Expression used to determine whether a given database record has a matching history-GUID.</returns>
        protected abstract Expression<Func<EFType, bool>> HasHistoryGuid(EMDGuid historyGuid);
        /// <summary>
        /// Generates an expression to check if an database-object is at some point active in the given timespan.
        /// </summary>
        /// <param name="from">Start of the timespan.</param>
        /// <param name="to">End of the timespan.</param>
        /// <returns>Expression used to determine whether a given database object is active in given timespan.</returns>
        protected abstract Expression<Func<EFType, bool>> IsActiveIn(DateTime from, DateTime to);
        /// <summary>
        /// Generates an expression to check if an database-object is at some point valid in the given timespan.
        /// </summary>
        /// <param name="from">Start of the timespan.</param>
        /// <param name="to">End of the timespan.</param>
        /// <returns>Expression used to determine whether a given database object is valid in given timespan.</returns>
        protected abstract Expression<Func<EFType, bool>> IsValidIn(DateTime from, DateTime to);

        #region Constructor
        /// <summary>
        /// Constructs a new Operator.
        /// </summary>
        /// <param name="modifyComment">Comment describing the modifications made with this operator. Accessible via <see cref="ModificationComment"/> (read only).</param>
        /// <param name="modifiedBy">GUID of the person using this operator to modify the database or <see langword="null"/> if the modifications are not made by a person. 
        /// This has to be a Person-GUID or else an exception is thrown. Default is <see langword="null"/>.</param>
        /// <param name="operatorTransaction">Transaction used by this operator. 
        /// If <see langword="null"/> a local transaction is used to perform rollbacks on an error. Default is <see langword="null"/>.</param>
        /// <param name="workingDate">Used to set a WorkingDate for this operator (see <see cref="WorkingDate"/>). 
        /// Writing access to the database is only granted if it is <see langword="null"/>. Default is <see langword="null"/>.</param>
        public BaseOperator(string modifyComment, EMDGuid modifiedBy = null, CoreTransaction operatorTransaction = null, DateTime? workingDate = null)
        {
            transaction = operatorTransaction;
            GUID_ModifiedBy = modifiedBy;
            ModificationComment = modifyComment;
            working = workingDate;

            Transaction.RollBackEvent += OnTransactionRollback;
        }
        #endregion

        #region Caching
        protected static ObjectCache cache = MemoryCache.Default;

        //Create a custom Timeout of 10 seconds
        private static CacheItemPolicy policy = new CacheItemPolicy();

        //Create a custom Timeout for extended cache
        private static CacheItemPolicy policyExtended = new CacheItemPolicy();

        protected List<string> GetCacheKeys(Type type)
        {

            List<string> cacheKeys = new List<string>();
            if (Configuration.ISCACHINGACTIVE)
            {
                IEnumerable<KeyValuePair<string, object>> en = cache.AsEnumerable();

                cacheKeys = (from x in en where x.Key.StartsWith(type.FullName) select x.Key).ToList();
            }
            return cacheKeys;
        }

        protected void RemoveCache(Type type)
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

        #region Object conversion (EMDObjed <=> DBObject)

        /// <summary>
        /// Creates a new Object of <see cref="EMDType"/> and copies all properties of the provided database-object to it using an <see cref="IPropertyCopier{EFType, EMDType}"/> provided by <see cref="PropertyCopierProvider"/>.
        /// </summary>
        /// <param name="dbObject">Database object with the data.</param>
        /// <returns>EMDObject containing the same data as the database-object or <see langword="null"/> if the database-object was <see langword="null"/>.</returns>
        /// <overloads>Creates a new Object of <see cref="EMDType"/> and copies all properties of the provided database-object to it.</overloads>
        protected EMDType CreateEMDObjectFrom(EFType dbObject)
        {
            if (dbObject == null) return default(EMDType);

            EMDType emdObject = new EMDType();
            PropertyCopierProvider.QuickCopy(dbObject, ref emdObject);

            return emdObject;
        }

        /// <summary>
        /// Creates a new Object of <see cref="EMDType"/> and copies all properties of the provided database-object to it using the provided <see cref="IPropertyCopier{EFType, EMDType}"/>.
        /// </summary>
        /// <remarks>
        /// Use this for parallel operation as <see cref="PropertyCopierProvider"/> is not thread-safe.
        /// </remarks>
        /// <param name="dbObject">Database object with the data.</param>
        /// <param name="cp">PropertyCopier to use. Mustn't be <see langword="null"/>.</param>
        /// <returns>EMDObject containing the same data as the database-object or <see langword="null"/> if the database-object was <see langword="null"/>.</returns>
        protected EMDType CreateEMDObjectFrom(EFType dbObject, IPropertyCopier<EFType, EMDType> cp)
        {
            if (dbObject == null) return default(EMDType);

            EMDType emdObject = new EMDType();
            cp.CopyAll(dbObject, ref emdObject);

            return emdObject;
        }

        /// <summary>
        /// Creates a new Object of <see cref="EFType"/> and copies all properties of the provided database-object to it using an <see cref="IPropertyCopier{EMDType, EFType}"/> provided by <see cref="PropertyCopierProvider"/>.
        /// </summary>
        /// <param name="dbObject">Emd-object with the data.</param>
        /// <returns>Database-object containing the same data as the emd-object or <see langword="null"/> if the emd-object was <see langword="null"/>.</returns>
        /// <overloads>Creates a new Object of <see cref="EFType"/> and copies all properties of the provided database-object to it.</overloads>
        protected EFType CreateDBObjectFrom(EMDType emdObject)
        {
            if (emdObject == null) return default(EFType);

            EFType dbObject = new EFType();
            PropertyCopierProvider.QuickCopy(emdObject, ref dbObject);

            return dbObject;
        }

        /// <summary>
        /// Creates a new Object of <see cref="EFType"/> and copies all properties of the provided database-object to it using the provided <see cref="IPropertyCopier{EMDType, EFType}"/>.
        /// </summary>
        /// <param name="dbObject">Emd-object with the data.</param>
        /// <param name="cp">PropertyCopier to use. Mustn't be <see langword="null"/>.</param>
        /// <returns>Database-object containing the same data as the emd-object or <see langword="null"/> if the emd-object was <see langword="null"/>.</returns>
        protected EFType CreateDBObjectFrom(EMDType emdObject, IPropertyCopier<EMDType, EFType> cp)
        {
            if (emdObject == null) return default(EFType);

            EFType dbObject = new EFType();
            cp.CopyAll(emdObject, ref dbObject);

            return dbObject;
        }

        #endregion

        #region Database

        /// <summary>
        /// Gets the database-object with the provided GUID as primary-key from the database-table corresponding to <see cref="EFType"/>.
        /// </summary>
        /// <param name="guid">Primary key of the object to get.</param>
        /// <returns>Retrieved object or <see langword="null"/> if no such object exists.</returns>
        protected EFType GetFromDB(string guid)
        {
            return Transaction.dbContext.Set<EFType>().Find(guid);
        }

        /// <summary>
        /// Creates a new database-object from the given emd-object and writes it to the database.
        /// </summary>
        /// <param name="emdObj">emd-object to write. Needs to hava a valid GUID.</param>
        /// <exception cref="EntityValidationException">Is thrown if the given object violates database-constraints.</exception>
        protected void InsertIntoDB(EMDType emdObj)
        {
            try
            {
                EFType dbObj = CreateDBObjectFrom(emdObj);
                Transaction.dbContext.Set<EFType>().Add(dbObj);
            }
            catch (DbEntityValidationException ex)
            {
                throw new EntityValidationException(typeof(EMDType).FullName,
                                                    String.Format("The entity with the GUID '{0}' violated some database-constraints.", emdObj.Guid), ex);
            }
        }

        /// <summary>
        /// Updates the given emd-object in the database.
        /// </summary>
        /// <param name="emdObj">Object to update. Mustn't be <see langword="null"/>.</param>
        /// <exception cref="NullReferenceException">Is thrown if the object to update was not found in the database.</exception>
        protected void UpdateInDB(EMDType emdObj)
        {
            EFType dbObj = Transaction.dbContext.Set<EFType>().Find(emdObj.Guid);
            if (dbObj != null)
            {
                PropertyCopierProvider.QuickCopy(emdObj, ref dbObj);
                if (!Transaction.dbContext.Configuration.AutoDetectChangesEnabled)
                {
                    Transaction.dbContext.Entry(dbObj).State = System.Data.Entity.EntityState.Modified;
                }
            }
            else
            {
                throw new NullReferenceException(String.Format("Object with GUID '{0}' does not exist in the database", emdObj.Guid));
            }
        }

        #endregion

        #region Read Ops

        /// <inheritdoc/>
        public EMDType Get(EMDGuid guid)
        {
            if (guid.Prefix != this.EntityPrefix)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY,
                                        String.Format("Prefix '{0}' does not match the required Prefix '{1}' of {2}.", guid.Prefix, this.EntityPrefix, typeof(EMDType).Name));
            }

            string cacheKey = null;
            EFType dbObject = null;

            if (Configuration.ISCACHINGACTIVE)
            {
                cacheKey = String.Format("{0}:{1}-GUID:{2}", typeof(EMDType).FullName, (working != null ? WorkingDate.ToString() : "now"), guid);
                dbObject = (EFType)cache.Get(cacheKey);
            }

            if (dbObject == null) //Not in cache
            {
                dbObject = GetFromDB(guid);

                if (dbObject != null && Configuration.ISCACHINGACTIVE)
                {
                    if (Configuration.IsExtendedEntity(typeof(EMDType).Name))
                    {
                        policyExtended.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES_EXTENDED);
                        cache.Add(cacheKey, dbObject, policyExtended);
                    }
                    else
                    {
                        policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES);
                        cache.Add(cacheKey, dbObject, policy);
                    }
                }
            }
            return CreateEMDObjectFrom(dbObject);
        }

        /// <inheritdoc/>
        public List<EMDType> GetHistory(EMDGuid guid, HistorySearchTypeEnum searchType)
        {
            if (guid.Prefix != this.EntityPrefix)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY,
                                        String.Format("Prefix '{0}' does not match the required Prefix '{1}' of {2}.", guid.Prefix, this.EntityPrefix, typeof(EMDType).Name));
            }

            string cacheKey = null;
            List<EMDType> emdObjects = null;

            if (Configuration.ISCACHINGACTIVE)
            {
                cacheKey = string.Format("{0}:{1}-GUID:{2}-{3}", typeof(EMDType).FullName,
                                                             (working != null ? WorkingDate.ToString() : "now"),
                                                             guid, searchType);
                emdObjects = cache.Get(cacheKey) as List<EMDType>;
            }

            if (emdObjects == null)
            {
                Tuple<DateTime, DateTime> span = searchType.GetValiditySpan(WorkingDate);
                DateTime from = span.Item1;
                DateTime to = span.Item2;

                IQueryable<EFType> query = Transaction.dbContext.Set<EFType>().AsQueryable();
                query = query.Where(HasHistoryGuid(guid)).Where(IsValidIn(from, to)).Where(IsActive);

                IPropertyCopier<EFType, EMDType> cp = PropertyCopierProvider.Request<EFType, EMDType>();
                emdObjects = query.AsParallel().Select(dbObj => CreateEMDObjectFrom(dbObj, cp)).ToList();

                if (Configuration.ISCACHINGACTIVE)
                {
                    if (Configuration.IsExtendedEntity(typeof(EMDType).Name))
                    {
                        policyExtended.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES_EXTENDED);
                        cache.Add(cacheKey, emdObjects, policyExtended);
                    }
                    else
                    {
                        policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES);
                        cache.Add(cacheKey, emdObjects, policy);
                    }
                }
            }

            return emdObjects;
        }

        /// <inheritdoc/>
        public List<EMDType> GetMultiple(Expression<Func<EFType, bool>> filter = null)
        {
            string cacheKey = null;
            List<EMDType> emdObjects = null;

            if (Configuration.ISCACHINGACTIVE)
            {
                cacheKey = string.Format("{0}-{1}:{2}-WHERE:{3}", MethodBase.GetCurrentMethod().DeclaringType.FullName,
                                                                    typeof(EMDType).FullName,
                                                                    (working != null ? WorkingDate.ToString() : "now"),
                                                                    (filter != null ? filter.ToString() : "1=1"));
                emdObjects = cache.Get(cacheKey) as List<EMDType>;
            }

            if (emdObjects == null)
            {
                IQueryable<EFType> query = Transaction.dbContext.Set<EFType>().AsQueryable();
                query = query.Where(IsValid).Where(IsActive);
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                IPropertyCopier<EFType, EMDType> cp = PropertyCopierProvider.Request<EFType, EMDType>();
                emdObjects = query.AsParallel().Select(dbObj => CreateEMDObjectFrom(dbObj, cp)).ToList();

                if (Configuration.ISCACHINGACTIVE)
                {
                    if (Configuration.IsExtendedEntity(typeof(EMDType).Name))
                    {
                        policyExtended.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES_EXTENDED);
                        cache.Add(cacheKey, emdObjects, policyExtended);
                    }
                    else
                    {
                        policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES);
                        cache.Add(cacheKey, emdObjects, policy);
                    }
                }
            }

            return emdObjects;
        }

        /// <inheritdoc/>
        public List<EMDType> GetMultiple(DateTime from, DateTime to, Expression<Func<EFType, bool>> filter = null)
        {
            string cacheKey = null;
            List<EMDType> emdObjects = null;

            if (Configuration.ISCACHINGACTIVE)
            {
                cacheKey = string.Format("{0}-{1}:{2}-WHERE:{3}-{4}-{5}", MethodBase.GetCurrentMethod().DeclaringType.FullName,
                                                                    typeof(EMDType).FullName,
                                                                    (working != null ? WorkingDate.ToString() : "now"),
                                                                    (filter != null ? filter.ToString() : "1=1"), from, to);
                emdObjects = cache.Get(cacheKey) as List<EMDType>;
            }

            if (emdObjects == null)
            {
                IQueryable<EFType> query = Transaction.dbContext.Set<EFType>().AsQueryable();
                query = query.Where(IsValid).Where(IsActiveIn(from, to));
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                IPropertyCopier<EFType, EMDType> cp = PropertyCopierProvider.Request<EFType, EMDType>();
                emdObjects = query.AsParallel().Select(dbObj => CreateEMDObjectFrom(dbObj, cp))
                                               .Where(emdObj => emdObj != null).ToList();

                if (Configuration.ISCACHINGACTIVE)
                {
                    if (Configuration.IsExtendedEntity(typeof(EMDType).Name))
                    {
                        policyExtended.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES_EXTENDED);
                        cache.Add(cacheKey, emdObjects, policyExtended);
                    }
                    else
                    {
                        policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(Configuration.CACHINGTIMEINMINUTES);
                        cache.Add(cacheKey, emdObjects, policy);
                    }
                }
            }
            return emdObjects;
        }

        /// <summary>
        /// Gets the number of existing related entities to the given object.
        /// </summary>
        /// <remarks>
        /// <b>Warning:</b> Constraints/Relation that are not relevant for deleting are not returned. <br/>
        /// <ul>
        /// <li>If entities is of type EntityReference it is not relevant for deleting (e.g. PERS points at USER)</li>
        /// <li>Entities of type EntitiyCollection are relevant ! (e.g. EMPL points at PERS)</li>
        /// </ul>
        /// </remarks>
        /// <param name="emdObj">Emd-object which related entities are to be queried.</param>
        /// <returns>A read-only dictionary where the key is a string with the name of the emd-object of the related entities and the value is number of the existing entities of this type.
        /// Only types where the existing-entity-count is at least one are added to the dictionary.</returns>
        public IReadOnlyDictionary<string, int> GetRelatedEntitiesOf(EMDType emdObj)
        {
            EMDGuid guid = emdObj.Guid;
            if (guid.Prefix != this.EntityPrefix)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY,
                                        String.Format("Prefix '{0}' does not match the required Prefix '{1}' of {2}.", guid.Prefix, this.EntityPrefix, typeof(EMDType).Name));
            }

            Dictionary<string, int> result = new Dictionary<string, int>();

            EFType dbObj = GetFromDB(guid);
            if (dbObj == null) throw new BaseException(ErrorCodeHandler.E_DB_NOT_FOUND, "GetRelatedEntities: object with GUID '" + guid + "' was not found.");

            IObjectContextAdapter dbCtxAdapter = new EMD_Entities();

            ObjectStateManager objStateManager = dbCtxAdapter.ObjectContext.ObjectStateManager;
            RelationshipManager objRelationManager;
            bool hasRelationsManager = objStateManager.TryGetRelationshipManager(dbObj, out objRelationManager);

            if (hasRelationsManager)
            {
                foreach (IRelatedEnd related in objRelationManager.GetAllRelatedEnds())
                {
                    if (related is EntityReference) { continue; } //Not relevant for deletion
                    else
                    {
                        IList entities = ((IListSource)related).GetList();

                        if (entities.Count > 0)
                        {
                            Type relatedEntityType = entities[0].GetType();

                            if (!relatedEntityType.Name.Contains("employment_"))
                            {
                                int count = 0;

                                PropertyInfo piActiveTo = relatedEntityType.GetRuntimeProperty("ActiveTo");
                                PropertyInfo piValidTo = relatedEntityType.GetRuntimeProperty("ValidTo");

                                foreach (object entity in entities)
                                {
                                    DateTime activeTo = (DateTime)piActiveTo.GetValue(entity);
                                    DateTime validTo = (DateTime)piActiveTo.GetValue(entity);

                                    if (activeTo > WorkingDate && validTo > WorkingDate) { count++; }
                                }
                                if (count > 0)
                                {
                                    string typeName = relatedEntityType.Name;
                                    string key = "EMD" + typeName.Substring(0, typeName.IndexOf('_'));
                                    result.Add(key, count);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        #endregion

        #region Write Ops

        /// <inheritdoc/>
        public EMDType Delete(EMDType toDelete, DateTime? dueDate = default(DateTime?), bool force = false)
        {
            if (working == null) throw new BaseException(ErrorCodeHandler.E_EDP_SECURITY, "Invalid State. Writing access is not allowed with a custom WorkingDate");

            EMDGuid guid = toDelete.Guid;
            if (guid.Prefix != this.EntityPrefix)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY,
                                        String.Format("Prefix '{0}' does not match the required Prefix '{1}' of {2}.", guid.Prefix, this.EntityPrefix, typeof(EMDType).Name));
            }

            RemoveCache(typeof(EMDType));

            IReadOnlyDictionary<string, int> relations = (force ? null : GetRelatedEntitiesOf(toDelete));

            if (relations.Count == 0 || force)
            {
                DateTime due = (dueDate != null) ? dueDate.Value : WorkingDate;
                toDelete.DeactivateBy(due);

                try
                {
                    if (IsLocalTransaction) { Transaction.Begin(); }
                    UpdateInDB(toDelete);
                    if (IsLocalTransaction) { Transaction.Commit(); }
                }
                catch (Exception ex)
                {
                    if (IsLocalTransaction) Transaction.Rollback();
                    throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR,
                                            String.Format("Could not delete object with GUID '{0}'.", toDelete.Guid), ex);
                }

                NotifyObjectChanged(toDelete.Guid, ChangeType.Delete);
                return toDelete;
            }
            else
            {
                throw new DeleteWithRelationsException(toDelete.Guid, relations);
            }
        }

        /// <inheritdoc/>
        public EMDType Update(EMDType toUpdate)
        {
            if (working == null) throw new BaseException(ErrorCodeHandler.E_EDP_SECURITY, "Invalid State. Writing access is not allowed with a custom WorkingDate");

            EMDGuid guid = toUpdate.Guid;
            if (guid.Prefix != this.EntityPrefix)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY,
                                        String.Format("Prefix '{0}' does not match the required Prefix '{1}' of {2}.", guid.Prefix, this.EntityPrefix, typeof(EMDType).Name));
            }
            if (toUpdate.ActiveTo < WorkingDate)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY,
                                        String.Format("Update for object with GUID '{0}' is not allowed because ActiveTo ({1}) is past the working-date ({2}).",
                                                       guid,
                                                       DateTimeHelper.DateTimeToIso8601(toUpdate.ActiveTo),
                                                       DateTimeHelper.DateTimeToIso8601(WorkingDate)));
            }

            EMDType oldVers = Get(guid);

            if (oldVers.ActiveTo < WorkingDate)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY,
                                        String.Format("Update for object with GUID '{0}' is not allowed because the object is already deleted.", guid));
            }

            RemoveCache(typeof(EMDType));

            toUpdate.SetModifiedBy(GUID_ModifiedBy, ModificationComment);

            //Prep new version
            toUpdate.Guid = oldVers.Guid;
            toUpdate.HistoryGuid = oldVers.Guid;
            toUpdate.ValidFrom = WorkingDate;
            toUpdate.ValidTo = EMDObject<EMDType>.INFINITY;

            //Prep old version
            oldVers.Guid = oldVers.CreateDBGuid();
            oldVers.InvalidateBy(WorkingDate);

            //Database Action
            try
            {
                if (IsLocalTransaction) { Transaction.Begin(); }

                InsertIntoDB(oldVers); //Write old version to database with a new primary-key to keep track of changes
                UpdateInDB(toUpdate);

                if (IsLocalTransaction) { Transaction.Commit(); }
            }
            catch (Exception ex)
            {
                if (IsLocalTransaction) Transaction.Rollback();

                if (ex is EntityValidationException) { throw ex; }
                else
                {
                    throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR,
                        String.Format("Could not update object with GUID '{0}'.", toUpdate.Guid), ex);
                }
            }

            NotifyObjectChanged(toUpdate.Guid, ChangeType.Update);
            return toUpdate;
        }

        /// <inheritdoc/>
        public EMDType Write(EMDType toWrite, bool generateGuid = true)
        {
            if (working == null) throw new BaseException(ErrorCodeHandler.E_EDP_SECURITY, "Invalid State. Writing access is not allowed with a custom WorkingDate");

            RemoveCache(typeof(EMDType));

            #region GUID generation or check
            if (generateGuid) { toWrite.Guid = toWrite.CreateDBGuid(); }
            else
            {
                EMDGuid guid = toWrite.Guid; //Implicit cast with Regex-check
                if (guid.Prefix != EntityPrefix)
                {
                    throw new EntityNotAllowedException(typeof(EMDType).Name, EnumEntityNotAllowedError.EntityNotAllowed, ErrorCodeHandler.E_EDP_ENTITY,
                                                        String.Format("The Prefix of the GUID '{0}' does not match the entity-type-prefix of the handler ('{1}').", guid, EntityPrefix));
                }
            }
            toWrite.HistoryGuid = toWrite.Guid;
            #endregion

            toWrite.SetModifiedBy(GUID_ModifiedBy, ModificationComment);
            toWrite.FillEmptyDates(WorkingDate);

            try
            {
                if (IsLocalTransaction) { Transaction.Begin(); }
                InsertIntoDB(toWrite);
                if (IsLocalTransaction) { Transaction.Commit(); }
            }
            catch (Exception ex)
            {
                if (IsLocalTransaction) Transaction.Rollback();

                if (ex is EntityValidationException) { throw ex; }
                else
                {
                    throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR,
                        String.Format("Could not insert object with GUID '{0}' into the database.", toWrite.Guid), ex);
                }
            }
            NotifyObjectChanged(toWrite.Guid, ChangeType.Create);

            return toWrite;
        }

        #endregion

        #region Events

        /// <summary>
        /// Notifies subscribers of changes made with this operator. The notification includes the GUID of the object changed and the type of the change.
        /// </summary>
        /// <seealso cref="ObjectChangeArgs"/>
        public event EventHandler<ObjectChangeArgs> ObjectChanged;

        /// <summary>
        /// Used to trigger the <see cref="ObjectChanged"/>-event on all subscribers.
        /// </summary>
        /// <param name="guid">GUID of the object that was changed.</param>
        /// <param name="change">Type of the change.</param>
        protected virtual void NotifyObjectChanged(EMDGuid guid, ChangeType change)
        {
            ObjectChanged(this, new ObjectChangeArgs(guid, change));
        }

        /// <summary>
        /// Event arguments for the <see cref="ObjectChanged"/>-event.
        /// </summary>
        public class ObjectChangeArgs : EventArgs
        {
            /// <summary>
            /// GUID of the object that was changed.
            /// </summary>
            public EMDGuid ObjGuid { get; }

            /// <summary>
            /// Type of the change.
            /// </summary>
            public ChangeType Change { get; }

            /// <summary>
            /// Constructs a new object.
            /// </summary>
            public ObjectChangeArgs(EMDGuid guid, ChangeType change)
            {
                ObjGuid = guid;
                Change = change;
            }
        }

        /// <summary>
        /// Specifies the type of a change in scope of the <see cref="ObjectChanged"/>-event.
        /// </summary>
        public enum ChangeType
        {
            /// <summary>
            /// Indicates that an object was created in the database.
            /// </summary>
            Create = 0,
            /// <summary>
            /// Indicates that an object was updated in the database.
            /// </summary>
            Update = 1,
            /// <summary>
            /// Indicates that an object was deleted from the database.
            /// </summary>
            Delete = 2
        }

        #endregion

        /// <summary>
        /// Frees all resources and un-subscribes all listeners to the <see cref="ObjectChanged"/>-event.
        /// </summary>
        public void Dispose()
        {
            Transaction.RollBackEvent -= OnTransactionRollback;
            if (IsLocalTransaction)
            {
                Transaction.Dispose();
            }
            transaction = null;
            ObjectChanged = null;
        }
    }
}

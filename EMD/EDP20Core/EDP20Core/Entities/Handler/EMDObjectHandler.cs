using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Extends the <see cref="EMDBaseObjectHandler"/> with methods for creating, updating and deleting objects in the database.
    /// </summary>
    public abstract class EMDObjectHandler
        : EMDBaseObjectHandler
        , IEMDObjectHandler
    {
        /// <summary>
        /// ...
        /// </summary>
        public event EventHandler ObjectChanged;

        #region Constructors

        /// <inheritdoc/>
        public EMDObjectHandler(Type logtype) : base(logtype)
        {
            DeliverInActive = false;
        }

        ///<inheritdoc/>
        public EMDObjectHandler(Type logtype, CoreTransaction transaction) : base(logtype, transaction)
        {
            DeliverInActive = false;
        }

        /// <inheritdoc/>
        public EMDObjectHandler(Type logType, string guid_ModifiedBy, string modifyComment = null)
            : base(logType, guid_ModifiedBy, modifyComment)
        {
        }

        /// <inheritdoc/>
        public EMDObjectHandler(Type logType, CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(logType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        /// <summary>
        /// Validity clause for object-queries
        /// </summary>
        public string ValidClause = "ValidFrom < DateTime.Now && ValidTo > DateTime.Now";

        /// <summary>
        /// Active clause for object-queries
        /// </summary>
        public string ActiveClause = "ActiveFrom < DateTime.Now && ActiveTo > DateTime.Now";

        /// <inheritdoc/>
        public override IEMDObject<T> CreateObject<T>(IEMDObject<T> emdObject, string guid = null, bool datesAreSet = false)
        {
            RemoveCache(emdObject.GetType());

            if (transaction == null) CheckForLocalTransaction();

            if (transactionIsLocal) transaction.Begin();

            try
            {
                if (String.IsNullOrWhiteSpace(guid)) emdObject.Guid = emdObject.CreateDBGuid();
                else emdObject.Guid = guid; //TODO: Create exception for wrong guid-syntax
                emdObject.HistoryGuid = emdObject.Guid;
                if (emdObject.Guid_ModifiedBy == null && emdObject.ModifyComment == null)
                {
                    emdObject.SetModifiedBy(this.Guid_ModifiedBy, this.ModifyComment);
                }
                if (!datesAreSet)
                {
                    emdObject.SetAsNew();
                }
                else
                {
                    emdObject.FillEmptyDates();
                }
                InsertDBObject(emdObject);
            }
            catch (Exception e)
            {
                if (transactionIsLocal) transaction.Rollback();
                throw e;
            }

            if (transactionIsLocal) transaction.Commit();

            this.OnObjectChanged();
            return emdObject;
        }

        /// <inheritdoc/>
        public override IEMDObject<T> UpdateObject<T>(IEMDObject<T> emdObject, bool historize = true, bool checkActiveTo = true, bool allowChangeActive = false)
        {
            RemoveCache(emdObject.GetType());

            //check for changes in ActiveTo
            var dbObject = transaction.dbContext.Set(this.GetDBObjectType()).Find(emdObject.Guid);
            DateTime activeFrom = (DateTime)ReflectionHelper.GetPropValue(dbObject, "ActiveFrom");
            DateTime activeTo = (DateTime)ReflectionHelper.GetPropValue(dbObject, "ActiveTo");


            emdObject.SetModifiedBy(this.Guid_ModifiedBy, this.ModifyComment);


            if (!allowChangeActive && !DateTimeHelper.IsDateTimeEqual(emdObject.ActiveTo, activeTo))
            {
                string errmsg = "UpdateObject for object : " + emdObject.Guid + " is not allowed because ActiveTo has changed.";
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errmsg);
            }
            if (!allowChangeActive && !DateTimeHelper.IsDateTimeEqual(emdObject.ActiveFrom, activeFrom))
            {
                string errmsg = "UpdateObject for object : " + emdObject.Guid + " is not allowed because ActiveFrom has changed.";
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errmsg);
            }

            if (!checkActiveTo || emdObject.ActiveTo > DateTime.Now)
            {
                return DoUpdateObject(emdObject, historize, touchActiveTo: !allowChangeActive, touchActiveFrom: !allowChangeActive); //If change of active is allowed do not touch active to 
            }
            else
            {
                //UpdateObject not allowed if ActriveTo < now
                string errmsg = "UpdateObject for object : " + emdObject.Guid + " is not allowed. ActiveTo is in the past. (" + emdObject.ActiveTo.ToString("yyyy-MM-dd HH:mm:ss") + ")";
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errmsg);
            }

        }

        /// <summary>
        /// Does the real update without checking ActiveTo.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="emdObject"></param>
        /// <param name="historize"></param>
        /// <param name="touchActiveTo"> if set to false do not let SetAsUpdated function change the
        /// date, <see langword="true"/> = default (as it was so far)</param>
        /// <returns></returns>
        /// <exception cref="BaseException"></exception>
        private IEMDObject<T> DoUpdateObject<T>(IEMDObject<T> emdObject, bool historize = true, bool touchActiveTo = true, bool touchActiveFrom = true)
        {
            RemoveCache(emdObject.GetType());

            IEMDObject<T> oldObject = (IEMDObject<T>)this.GetObject<T>(emdObject.Guid);
            IEMDObject<T> objToReturn = null;

            if (transaction == null) CheckForLocalTransaction();

            if (transactionIsLocal) transaction.Begin();

            try
            {
                if (historize)
                {
                    emdObject.Guid = oldObject.Guid;
                    emdObject.ValidFrom = oldObject.ValidFrom;
                    oldObject.Guid = oldObject.CreateDBGuid();
                    oldObject.Invalidate();

                    emdObject.SetAsUpdated(oldObject, touchActiveTo, touchActiveFrom); // sets old ActiveFrom/To values --> no change of ActiveFrom/To via user is possible
                    emdObject.HistoryGuid = oldObject.HistoryGuid;

                    UpdateDBObject(emdObject);
                    InsertDBObject((EMDObject<T>)oldObject);
                    objToReturn = emdObject;
                }
                else
                {
                    if (emdObject.Guid_ModifiedBy == null && emdObject.ModifyComment == null)
                    {
                        emdObject.SetModified();
                    }
                    UpdateDBObject(emdObject);
                    objToReturn = emdObject;
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, "Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage, ex);
                    }
                }
            }
            catch (Exception e)
            {
                if (transactionIsLocal) transaction.Rollback();
                throw e;
            }

            if (transactionIsLocal) transaction.Commit();

            return objToReturn;
        }

        /// <summary>
        /// Do the update inside DB.
        /// </summary>
        /// <typeparam name="T">Type of the specific object-implementation.</typeparam>
        /// <param name="emdObject">Object to be updated.</param>
        /// <exception cref="BaseException">Thrown if the object does not exist in the database.</exception>
        internal void UpdateDBObject<T>(IEMDObject<T> emdObject)
        {
            RemoveCache(emdObject.GetType());

            var dbObject = transaction.dbContext.Set(this.GetDBObjectType()).Find(emdObject.Guid);
            if (dbObject != null)
            {
                MapDataToDBObject(ref dbObject, ref emdObject);

                if (!transaction.dbContext.Configuration.AutoDetectChangesEnabled)
                    transaction.dbContext.Entry(dbObject).State = System.Data.Entity.EntityState.Modified;

                transaction.saveChanges();
                OnObjectChanged();
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, "Trying to update a non existing DB object" + emdObject.Guid);
            }
        }

        /// <inheritdoc/>
        public override IEMDObject<T> DeleteObject<T>(IEMDObject<T> emdObject, bool historize = true, bool ignoreDependency = false, DateTime? dueDate = null)
        {
            RemoveCache(emdObject.GetType());

            //if ignoreDependency is true it is unnecessary to get the dependencies
            Hashtable relations = ignoreDependency ? null : this.GetRelatedEntities(emdObject.Guid);

            if (ignoreDependency || relations.Count == 0)
            {
                IEMDObject<T> objToReturn = null;
                if (transaction == null) CheckForLocalTransaction();

                if (transactionIsLocal) transaction.Begin();

                try
                {
                    if (historize)
                    {
                        DateTime due = (dueDate != null) ? dueDate.Value : DateTime.Now;
                        emdObject.InvalidateBy(due);
                        emdObject.DeactivateBy(due);
                        UpdateDBObject(emdObject);
                        objToReturn = emdObject;
                    }
                    else
                    {
                        DeleteDBObject((EMDObject<T>)emdObject);
                    }
                }
                catch (Exception e)
                {
                    if (transactionIsLocal) transaction.Rollback();
                    throw e;
                }

                if (transactionIsLocal) transaction.Commit();

                return (IEMDObject<T>)objToReturn;
            }
            else
            {
                //DeleteObject not allowed
                string errmsg = "DeleteObject for object : " + emdObject.Guid + " is not allowed. Related entities still exist.";
                StringWriter sw = new StringWriter();
                ObjectDumper.Write(relations, 4, sw);
                errmsg += Environment.NewLine + "Related Entities :" + sw.ToString();
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errmsg);
            }
        }

        /// <inheritdoc/>
        public override void SetActive<T>(IEMDObject<T> emdObject, bool historize = true)
        {
            RemoveCache(emdObject.GetType());

            emdObject.Activate();
            DoUpdateObject(emdObject, historize, touchActiveTo: false);
        }

        /// <inheritdoc/>
        public override void SetInactive<T>(IEMDObject<T> emdObject, bool historize = true)
        {
            RemoveCache(emdObject.GetType());

            Hashtable relations = this.GetRelatedEntities(emdObject.Guid);

            if (relations.Count == 0)
            {
                emdObject.Deactivate();
                DoUpdateObject(
                    emdObject: emdObject,
                    historize: historize,
                    touchActiveTo: false); // do not let SetAsUpdated function change the ActiveTo date
            }
            else
            {
                //set inactive not allowed
                string errmsg = "SetInactive for object : " + emdObject.Guid + " is not allowed. Related entities still exist.";
                StringWriter sw = new StringWriter();
                ObjectDumper.Write(relations, 4, sw);
                errmsg += Environment.NewLine + "Related Entities :" + sw.ToString();
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, errmsg);
            }
        }

        internal void InsertDBObject<T>(IEMDObject<T> emdObject)
        {
            RemoveCache(emdObject.GetType());

            string oguid = emdObject.Guid;

            try
            {
                var dbObject = Activator.CreateInstance(this.GetDBObjectType());

                MapDataToDBObject(ref dbObject, ref emdObject);
                //finally write to db
                transaction.dbContext.Set(this.GetDBObjectType()).Add(dbObject);
                transaction.saveChanges();
                OnObjectChanged();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, "DBObject:" + this.GetDBObjectType().Name + " ,Property: " + validationError.PropertyName + " ,Error: " + validationError.ErrorMessage, ex);
                    }
                }
            }
            catch (Exception e)
            {
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, "cannot insert DB object with Guid " + oguid, e);
            }
        }

        //TODO: decide whether only the last version should be deleted or all data-entries with same history-ID
        internal void DeleteDBObject<T>(IEMDObject<T> emdObject)
        {
            RemoveCache(emdObject.GetType());

            string oguid = emdObject.Guid;

            try
            {
                var dbObject = Activator.CreateInstance(this.GetDBObjectType());
                System.Reflection.PropertyInfo prop = dbObject.GetType().GetProperty("Guid");//TODO change to History GUID (refer to doc)
                prop.SetValue(dbObject, oguid);
                dbObject = transaction.dbContext.Set(this.GetDBObjectType()).Find(oguid);
                transaction.dbContext.Set(this.GetDBObjectType()).Remove(dbObject);
                transaction.saveChanges();
                OnObjectChanged();
            }
            catch (Exception e)
            {
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, "cannot remove DB object with Guid " + oguid, e);
            }

        }

        // http://dynamiclinq.azurewebsites.net/GettingStarted
        internal override IQueryable<T> AddHandlerClauses<T>(IQueryable<T> query, string whereclause = null, DatabasePaging paging = null, bool filteronly = false)
        {
            //Construct a integer to use in switch case to distiunguish different filter-cases
            //First bit is Flag Historical (0x01), second bit is DeliverInActive (0x02)
            int filter = 0;
            if (Historical) filter = filter | 0x01;
            if (DeliverInActive) filter = filter | 0x02;

            //Filter = 0 : no flag was set            - view active only
            //Filter = 1 : Only Historical is set     - view all historical entities 
            //Filter = 2 : Only DeliverInActiv is set - view active and inactive
            //Filter = 3 : Both flags are set         - view all historical entities and also active and inactive
            switch (filter)
            {
                case 2:
                    query = query.Where(this.ValidClause);
                    break;
                case 3:
                    break;
                case 1:
                    query = query.Where(this.ActiveClause);
                    break;
                case 0:
                default:
                    query = query.Where(this.ValidClause + " && " + this.ActiveClause); //Use only one .Where(...) call because it is slow!
                    break;
            }

            return base.AddHandlerClauses<T>(query, whereclause, paging, filteronly);
        }

        //TODO Fleck: Add proper XML-style comment (it's public)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldname"></param>
        /// <param name="ordered"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public List<String> GetDistinctStringField(String fieldname, bool ordered = true, bool descending = false)
        {
            String orderstmt = fieldname;
            var dynQuery = transaction.dbContext.Set(this.GetDBObjectType());
            var query = dynQuery.Where(fieldname + "<>null");
            if (descending) orderstmt += " descending";
            if (ordered) { query = query.OrderBy(fieldname); }
            if (!Historical) { query = query.Where(this.ValidClause); }
            query = query.Select("new (" + fieldname + ")");
            //System.Diagnostics.Debug.WriteLine(query.ToString());
            List<String> result = new List<String>();
            dynamic[] dynArray = query.ToDynamicArray();
            foreach (dynamic elem in dynArray)
            {
                result.Add(elem.ToString().Replace(fieldname + "=", "").Replace("{", "").Replace("}", "").Trim());
            }
            return result.Distinct().ToList();
        }

        /// <summary>
        /// Returns a <see cref="string"/>-representation of a <see cref="DateTime"/>-object specific for dynamic-LINQ-queries.
        /// </summary>
        /// <param name="dat">Date to be represented as <see cref="string"/></param>
        /// <returns><see cref="string"/> representing the given date.</returns>
        public string GenerateDateForWhereClauseInDynamicLinq(DateTime dat)
        {
            return " DateTime(" + dat.Year + ", " + dat.Month + ", " + dat.Day + ")";
        }

        /// <summary>
        /// Notifies all hooked objects of a change.
        /// </summary>
        protected virtual void OnObjectChanged()
        {
            if (this.ObjectChanged != null)
                this.ObjectChanged(this, EventArgs.Empty);
        }
    }
}
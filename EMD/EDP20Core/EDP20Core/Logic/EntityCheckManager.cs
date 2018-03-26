using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic
{
    /// <summary>
    /// Handles all Entities from Interface type IEntityCheck
    /// Checks Entities if they are still allowed by NextCheckDate and sends a request to the process-engine
    /// The Check is done by a Job, which runs every 24 hours
    /// </summary>
    public class EntityCheckManager : BaseManager
    {
        private EntityCheckHandler entityCheckHandler;

        #region Constructors

        public EntityCheckManager()
            : base()
        {
        }

        public EntityCheckManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public EntityCheckManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public EntityCheckManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        private CoreTransaction lastCoreTransaction = null;

        /// <summary>
        /// Singleton for EntityCheckHandler
        /// In case of changing any properties of the Handler, it will be newly initiated 
        /// </summary>
        internal EntityCheckHandler EntityCheckHandler
        {
            get
            {
                if (this.entityCheckHandler == null || lastCoreTransaction != this.Transaction)
                {
                    this.entityCheckHandler = new EntityCheckHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                }
                lastCoreTransaction = this.Transaction;

                // check if modifyComment or modifiedby changed
                if (entityCheckHandler.ModifyComment != ModifyComment || entityCheckHandler.Guid_ModifiedBy != Guid_ModifiedBy)
                {
                    this.entityCheckHandler = new EntityCheckHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                }


                return this.entityCheckHandler;
            }
        }


        /// <summary>
        /// Deletes an entity with a RelationCheck
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public EMDEntityCheck Delete(string guid)
        {
            EMDEntityCheck emdEntityCheck = Get(guid);
            if (emdEntityCheck != null)
            {
                Hashtable hashTable = EntityCheckHandler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDEntityCheck)EntityCheckHandler.DeleteObject<EMDEntityCheck>(emdEntityCheck);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The ProcessEntity with guid: {0} was not found.", guid));
            }
        }

        /// <summary>
        /// creates a new entity from Interface IEntityCheck
        /// </summary>
        /// <param name="iEntityCheck"></param>
        /// <returns></returns>
        public EMDEntityCheck AddEntity(IEntityCheck iEntityCheck)
        {
            EMDEntityCheck entityCheck = GetLastEntityCheck(iEntityCheck.GetGuid());

            if (entityCheck != null)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The EntityGuid: {0} is already configured!", iEntityCheck.GetGuid()));
            }

            switch (iEntityCheck.GetPrefix().ToLower())
            {
                case "empl":
                    EMDEmployment employment = iEntityCheck as EMDEmployment;

                    if (employment == null)
                    {
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The given IEntityCheck with Guid: {0} couldn't be casted to EMDEmployment", iEntityCheck.GetGuid()));
                    }

                    EMDEmploymentType employmentType = (EMDEmploymentType)new EmploymentTypeHandler().GetObject<EMDEmploymentType>(employment.ET_Guid);
                    if (employmentType.DoEntityCheck)
                    {
                        employment.CheckIntervalInDays = employmentType.CheckIntervalInDays.HasValue ? employmentType.CheckIntervalInDays.Value : 0;
                        employment.ReminderIntervalInDays = employmentType.ReminderIntervalInDays.HasValue ? employmentType.ReminderIntervalInDays.Value : 0;

                        DateTime now = DateTime.Now;

                        entityCheck = new EMDEntityCheck()
                        {
                            EntityGuid = employment.Guid,
                            ManagedByType = iEntityCheck.GetManagedBy(),
                            NextCheckDate = now.AddDays(employmentType.CheckIntervalInDays.Value),
                            RemindedTime = EMDEmployment.INFINITY,
                            IsWorkflowInProgress = false
                        };

                        entityCheck = this.Create(entityCheck);
                    }
                    break;
                default:
                    break;
            }

            return entityCheck;
        }

        /// <summary>
        /// Deletes an entity from Interface IEntityCheck with a RelationCheck
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public EMDEntityCheck DeleteEntity(IEntityCheck entityCheckObject)
        {
            string entityGuid = entityCheckObject.GetGuid();

            EMDEntityCheck entityCheck = GetLastEntityCheck(entityGuid);
            if (entityCheck != null)
            {
                entityCheck = Delete(entityCheck.Guid);
            }

            return entityCheck;
        }

        /// <summary>
        /// returns the IEntityCheck-object with dynamic filled Intervals for given object
        /// </summary>
        /// <param name="guidEnch"></param>
        /// <returns></returns>
        public IEntityCheck GetEMDEntityWithIntervals(string guidEnch)
        {
            IEntityCheck returnValue = null;
            EMDEntityCheck entityCheck = Get(guidEnch);

            if (entityCheck != null)
            {
                string prefix = entityCheck.EntityGuid.Substring(0, 4).ToLower();

                switch (prefix)
                {
                    case "empl":
                        EMDEmployment employment = new EmploymentManager().GetEmployment(entityCheck.EntityGuid);
                        if (employment != null)
                        {
                            returnValue = UpdateIntervals(employment);
                        }

                        break;

                    default:
                        break;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Upates the intervals CheckIntervalInDays and ReminderIntervalInDays, based on the underlying object
        /// </summary>
        /// <param name="iEntityCheck"></param>
        /// <returns></returns>
        public IEntityCheck UpdateIntervals(IEntityCheck iEntityCheck)
        {
            IEntityCheck returnValue = iEntityCheck;
            switch (iEntityCheck.GetPrefix().ToLower())
            {
                case "empl":
                    EMDEmployment employment = iEntityCheck as EMDEmployment;

                    if (employment == null)
                    {
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The given IEntityCheck with Guid: {0} couldn't be casted to EMDEmployment", iEntityCheck.GetGuid()));
                    }
                    returnValue = employment;
                    EMDEmploymentType type = (EMDEmploymentType)new EmploymentTypeHandler().GetObject<EMDEmploymentType>(employment.ET_Guid);
                    returnValue.CheckIntervalInDays = type.CheckIntervalInDays.HasValue ? type.CheckIntervalInDays.Value : 0;
                    returnValue.ReminderIntervalInDays = type.ReminderIntervalInDays.HasValue ? type.ReminderIntervalInDays.Value : 0;

                    if (returnValue.ReminderIntervalInDays == 0)
                    {
                        returnValue.ReminderIntervalInDays = returnValue.CheckIntervalInDays / 2;
                    }


                    break;
                default:
                    break;
            }

            return returnValue;
        }

        /// <summary>
        /// Sets the next CheckDate calculated on underlying object
        /// Sets IsWorkflowInProgress = false
        /// Set RemindedTime = INFINITY
        /// </summary>
        /// <param name="iEntityCheck"></param>
        /// <returns></returns>
        public EMDEntityCheck Reset(IEntityCheck iEntityCheck)
        {
            string entityGuid = iEntityCheck.GetGuid();

            EMDEntityCheck entityCheck = GetLastEntityCheck(entityGuid);
            if (entityCheck != null)
            {
                switch (iEntityCheck.GetPrefix().ToLower())
                {
                    case "empl":
                        EMDEmployment employment = iEntityCheck as EMDEmployment;

                        if (employment == null)
                        {
                            throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The given IEntityCheck with Guid: {0} couldn't be casted to EMDEmployment", iEntityCheck.GetGuid()));
                        }
                        iEntityCheck = UpdateIntervals(iEntityCheck);
                        DateTime now = DateTime.Now;
                        entityCheck.NextCheckDate = now.AddDays(iEntityCheck.CheckIntervalInDays);
                        entityCheck.RemindedTime = EMDEmployment.INFINITY;
                        entityCheck.IsWorkflowInProgress = false;

                        entityCheck = Update(entityCheck);

                        break;
                    default:
                        break;
                }
            }

            return entityCheck;
        }

        /// <summary>
        /// Sets the next CheckDate calculated on underlying object
        /// Sets IsWorkflowInProgress = false
        /// Set RemindedTime = INFINITY
        /// </summary>
        /// <param name="entityCheck"></param>
        /// <returns></returns>
        public EMDEntityCheck Reset(EMDEntityCheck entityCheck)
        {
            IEntityCheck iEntityCheck = null;

            if (entityCheck != null)
            {
                string prefix = EMDEntityCheck.GetPrefix(entityCheck.EntityGuid);

                switch (prefix)
                {
                    case "empl":
                        iEntityCheck = new EmploymentManager(this.Guid_ModifiedBy, this.ModifyComment).GetEmployment(entityCheck.EntityGuid);
                        Reset(iEntityCheck);
                        break;
                    default:
                        break;
                }
            }

            return entityCheck;
        }


        /// <summary>
        /// Gets the last relevant entity from Database
        /// </summary>
        /// <param name="guidEntity"></param>
        /// <returns></returns>
        public EMDEntityCheck GetLastEntityCheck(string guidEntity)
        {
            EMDEntityCheck lastProcessEntity = null;
            List<EMDEntityCheck> processEntities = GetList(string.Format("EntityGuid = \"{0}\"", guidEntity))?.OrderBy(a => a.Created).ToList();

            if (processEntities != null && processEntities.Count > 0)
            {
                lastProcessEntity = processEntities.Last();
            }

            return lastProcessEntity;
        }

        /// <summary>
        /// Gets a single object from type EntityCheck
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public EMDEntityCheck Get(string guid)
        {

            return (EMDEntityCheck)EntityCheckHandler.GetObject<EMDEntityCheck>(guid);
        }

        /// <summary>
        /// Gets a list from type EntityCheck
        /// </summary>
        /// <returns></returns>
        public List<EMDEntityCheck> GetList()
        {
            return GetList(null);
        }

        /// <summary>
        /// Gets a list from type EntityCheck
        /// </summary>
        /// <returns></returns>
        public List<EMDEntityCheck> GetList(string whereClause)
        {
            try
            {
                return EntityCheckHandler.GetObjects<EMDEntityCheck, EntityCheck>(whereClause)?.Cast<EMDEntityCheck>().ToList();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// Returns a list of all entities which must be checked on current day     
        /// </summary>
        /// <remarks>
        /// Employments are only check if there is no exit or last day set
        /// </remarks>
        /// <param name="entityPrefix">prefix to check: empl, obre</param>
        /// <returns></returns>
        public List<EMDEntityCheck> GetEntitiesToCheck(string entityPrefix)
        {
            EmploymentManager manager = new EmploymentManager(this.Transaction, this.Guid_ModifiedBy);

            List<EMDEntityCheck> entitiesToCheck = new List<EMDEntityCheck>();


            DateTime searchDay = DateTime.Now;
            DateTime searchBeginOfDay = new DateTime(searchDay.Year, searchDay.Month, searchDay.Day);
            DateTime searchEndOfDay = searchBeginOfDay.AddDays(1);

            List<EMDEntityCheck> foundList = GetList(string.Format("NextCheckDate < DateTime({0},{1},{2}) && IsWorkflowInProgress = false && EntityGuid.Contains(\"{3}\")", searchEndOfDay.Year, searchEndOfDay.Month, searchEndOfDay.Day, entityPrefix.ToUpper()));

            foreach (EMDEntityCheck currentEntityCheck in foundList)
            {
                string prefix = EMDEntityCheck.GetPrefix(currentEntityCheck.EntityGuid);

                switch (prefix)
                {
                    case "empl":
                        EMDEmployment empl = manager.GetEmployment(currentEntityCheck.EntityGuid);
                        // only employments with exit & last day set to infinity are checked
                        if (empl.Exit >= EMDEmployment.INFINITY && empl.LastDay >= EMDEmployment.INFINITY)
                        {
                            entitiesToCheck.Add(currentEntityCheck);
                        }
                        break;
                    default:
                        entitiesToCheck.Add(currentEntityCheck);
                        break;
                }
            }

            return entitiesToCheck;

        }

        /// <summary>
        /// Creates a database entry from type EntityCheck
        /// </summary>
        /// <param name="emdEntityCheck"></param>
        /// <returns></returns>
        public EMDEntityCheck Create(EMDEntityCheck emdEntityCheck)
        {
            return (EMDEntityCheck)this.EntityCheckHandler.CreateObject(emdEntityCheck);
        }

        /// <summary>
        /// Updates a database entry from type EntityCheck
        /// </summary>
        /// <param name="emdEntityCheck"></param>
        /// <returns></returns>
        public EMDEntityCheck Update(EMDEntityCheck emdEntityCheck)
        {
            return (EMDEntityCheck)this.EntityCheckHandler.UpdateObject(emdEntityCheck);
        }


    }
}

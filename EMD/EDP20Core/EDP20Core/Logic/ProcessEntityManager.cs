using Kapsch.IS.EDP.Core.Logic.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using System.Collections;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.DB;

namespace Kapsch.IS.EDP.Core.Logic
{
    /// <summary>
    /// Business Class to handle all read/write/update Methods for the Entity ProcessEntity.
    /// <!--Use the Factory-Class <see cref="Manager"/> for initialization-->
    /// </summary>
    public class ProcessEntityManager : BaseManager, IProcessEntityManager
    {
        private ProcessEntityHandler processEntityHandler;

        #region Constructors

        /// <inheritdoc/>
        public ProcessEntityManager()
            : base()
        {
        }

        /// <inheritdoc/>
        public ProcessEntityManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        /// <inheritdoc/>
        public ProcessEntityManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        /// <inheritdoc/>
        public ProcessEntityManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors


        internal ProcessEntityHandler ProcessEntityHandler
        {
            get
            {
                if (this.processEntityHandler == null)
                {
                    this.processEntityHandler = new ProcessEntityHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                }

                // check if modifyComment or modifiedby changed
                if (processEntityHandler.ModifyComment != ModifyComment || processEntityHandler.Guid_ModifiedBy != Guid_ModifiedBy)
                {
                    this.processEntityHandler = new ProcessEntityHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                }

                processEntityHandler.DeliverInActive = DeliverInActive;

                return this.processEntityHandler;
            }
        }

        /// <summary>
        /// Indicates whether this manager also gets inactive database-objects or not. 
        /// </summary>
        public bool DeliverInActive { get; set; }

        /// <summary>
        /// Deletes the <see cref="EMDProcessEntity"/> with the given GUID.
        /// </summary>
        /// <param name="guid">GUID of the object to be deleted</param>
        /// <returns>Object with auto-updated fields (e.g. <see cref="EMDObject{T}.ValidTo"/>).</returns>
        /// <exception cref="RelatedEntitiesException">Is thrown if there are other objects with references to the given object in the database.</exception>
        /// <exception cref="EntityNotFoundException"> Is thrown if there is no entity with the given GUID.</exception>
        public EMDProcessEntity Delete(string guid)
        {
            ProcessEntityHandler handler = new ProcessEntityHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDProcessEntity emdProcessEntity = Get(guid);
            if (emdProcessEntity != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDProcessEntity)handler.DeleteObject<EMDProcessEntity>(emdProcessEntity);
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
        /// Gets the <see cref="EMDProcessEntity"/> with the given GUID from the database.
        /// </summary>
        /// <param name="guid">GUID of object to get.</param>
        /// <returns>The <see cref="EMDProcessEntity"/> with the given GUID or <see langword="null"/> if and only if no such object exists.</returns>
        public EMDProcessEntity Get(string guid)
        {
            return (EMDProcessEntity)ProcessEntityHandler.GetObject<EMDProcessEntity>(guid);
        }

        //TODO Woller: What does it really?
        /// <summary>
        /// Gets the most recent <see cref="EMDProcessEntity"/> with the given GUID (<see cref="EMDObject{T}.Created"/>).
        /// </summary>
        /// <param name="guidEntity">GUID of the object to get</param>
        /// <returns>The most recent <see cref="EMDProcessEntity"/> with the given GUID or <see langword="null"/> if and only if no such object exists.</returns>
        public EMDProcessEntity GetLastProcessEntity(string guidEntity)
        {
            EMDProcessEntity lastProcessEntity = null;
            List<EMDProcessEntity> processEntities = GetList(string.Format("EntityGuid = \"{0}\"", guidEntity))?.OrderBy(a => a.Created).ToList();

            if (processEntities != null && processEntities.Count > 0)
            {
                lastProcessEntity = processEntities.Last();
            }

            return lastProcessEntity;
        }

        /// <summary>
        /// Gets the most recent <see cref="EMDProcessEntity"/> with the given GUID (<see cref="EMDObject{T}.Created"/>).
        /// </summary>
        /// <param name="guidWoin"></param>
        /// <returns>The most recent <see cref="EMDProcessEntity"/> with the given GUID or <see langword="null"/> if and only if no such object exists.</returns>
        public EMDProcessEntity GetProcessEntityByWorfklowId(string guidWoin)
        {
            EMDProcessEntity lastProcessEntity = null;
            List<EMDProcessEntity> processEntities = GetList(string.Format("WFI_ID = \"{0}\"", guidWoin))?.OrderBy(a => a.Created).ToList();

            if (processEntities != null && processEntities.Count > 0)
            {
                lastProcessEntity = processEntities.Last();
            }

            return lastProcessEntity;
        }

        /// <summary>
        /// Gets all valid <see cref="EMDProcessEntity"/>. Active objects are filtered according to <see cref="DeliverInActive"/>.
        /// </summary>
        /// <returns>List of all found objects.</returns>
        /// <overloads>Gets multiple <see cref="EMDProcessEntity"/>-objects from the database</overloads>
        public List<EMDProcessEntity> GetList()
        {
            return GetList(null);
        }

        /// <summary>
        /// Gets all valid <see cref="EMDProcessEntity"/>. Active objects are filtered according to <see cref="DeliverInActive"/>. Additionally only gets all objects that match the given WHERE-clause.
        /// </summary>
        /// <param name="whereClause">WHERE-clause to filter the objects</param>
        /// <returns>List of all found objects.</returns>
        public List<EMDProcessEntity> GetList(string whereClause)
        {
            try
            {
                return ProcessEntityHandler.GetObjects<EMDProcessEntity, ProcessEntity>(whereClause)?.Cast<EMDProcessEntity>().ToList();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// Writes the given <see cref="EMDProcessEntity"/> to the database.
        /// </summary>
        /// <param name="emdProcessEntity">Object to write.</param>
        /// <returns><see cref="EMDProcessEntity"/> with auto-filled fields (e.g. <see cref="EMDObject{T}.Guid"/>). </returns>
        /// <overloads>Create a new <see cref="EMDProcessEntity"/> in the database.</overloads>
        public EMDProcessEntity Create(EMDProcessEntity emdProcessEntity)
        {
            return (EMDProcessEntity)this.ProcessEntityHandler.CreateObject(emdProcessEntity);
        }

        /// <summary>
        /// Creates a new <see cref="EMDProcessEntity"/> and writes it to the database.
        /// </summary>
        /// <param name="woinGuid"><see cref="EMDProcessEntity.WFI_ID"/></param>
        /// <param name="entityGuid"><see cref="EMDProcessEntity.EntityGuid"/></param>
        /// <param name="wodeGuid"><see cref="EMDProcessEntity.WFD_ID"/></param>
        /// <param name="wodeName"><see cref="EMDProcessEntity.WFD_Name"/></param>
        /// <param name="emplRequestedGuid"><see cref="EMDProcessEntity.RequestorEmplGuid"/></param>
        /// <param name="effectedPersGuid"><see cref="EMDProcessEntity.EffectedPersGuid"/></param>
        /// <param name="targetDate"><see cref="EMDProcessEntity.WFTargetDate"/></param>
        /// <returns>Newly created <see cref="EMDProcessEntity"/>.</returns>
        public EMDProcessEntity Create(string woinGuid, string entityGuid, string wodeGuid, string wodeName, string emplRequestedGuid, string effectedPersGuid, DateTime targetDate)
        {
            EMDProcessMapping mapping = null;
            string methodName = string.Empty;
            try
            {
                mapping = new ProcessMappingHandler(transaction: this.Transaction).GetObjects<EMDProcessMapping, ProcessMapping>(string.Format("WorkflowID = \"{0}\"", wodeGuid)).Cast<EMDProcessMapping>().FirstOrDefault();
            }
            catch (Exception) { }

            if (mapping != null)
            {
                methodName = mapping.Method;
            }

            EMDProcessEntity emdProcessEntity = new EMDProcessEntity()
            {
                WFI_ID = woinGuid,
                EntityGuid = entityGuid,
                WFD_ID = wodeGuid,
                WFD_Name = wodeName,
                WorkflowAction = methodName,
                RequestorEmplGuid = emplRequestedGuid,
                EffectedPersGuid = effectedPersGuid,
                WFStartTime = EMDProcessEntity.INFINITY,
                WFTargetDate = targetDate,
                WFResultMessages = "Created"
            };

            return Create(emdProcessEntity);
        }

        /// <summary>
        /// Write changes in the given <see cref="EMDProcessEntity"/> to the database.
        /// </summary>
        /// <param name="emdProcessEntity">Object to update.</param>
        /// <returns><see cref="EMDProcessEntity"/> with auto-updated fields (e.g. <see cref="EMDObject{T}.Modified"/>).</returns>
        public EMDProcessEntity Update(EMDProcessEntity emdProcessEntity)
        {
            return (EMDProcessEntity)this.ProcessEntityHandler.UpdateObject(emdProcessEntity);
        }

        /// <summary>
        /// Updates the given <see cref="EMDProcessEntity"/> or if it not exists creates it.
        /// </summary>
        /// <param name="pren">Object to update or create</param>
        /// <returns><see cref="EMDProcessEntity"/> with some fields changed (e.g. <see cref="EMDObject{T}.Modified"/>).</returns>
        /// <seealso cref="O:Kapsch.IS.EDP.Core.Logic.ProcessEntityManager.Create"/>
        /// <seealso cref="Update(EMDProcessEntity)"/>
        public EMDProcessEntity UpdateOrCreate(EMDProcessEntity pren)
        {
            if (!String.IsNullOrEmpty(pren.Guid))
            {
                return this.Update(pren);
            }
            else
            {
                return this.Create(pren);
            }

        }
    }
}

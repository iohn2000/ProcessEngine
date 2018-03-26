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
    /// Manager Class for CategoryEntities
    /// </summary>
    public class CategoryEntityManager : BaseManager
    {
        #region Constructors

        public CategoryEntityManager()
            : base()
        {
        }

        public CategoryEntityManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public CategoryEntityManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public CategoryEntityManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors
        /// <summary>
        /// Get the defined <seealso cref="EMDCategoryEntity"/>
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public EMDCategoryEntity Get(string guid)
        {
            CategoryEntityHandler handler = new CategoryEntityHandler(this.Transaction);

            return (EMDCategoryEntity)handler.GetObject<EMDCategoryEntity>(guid);
        }

        /// <summary>
        /// Delete defined <seealso cref="EMDCategoryEntity"/>
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public EMDCategoryEntity Delete(string guid)
        {
            CategoryEntityHandler handler = new CategoryEntityHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDCategoryEntity emdEquipmentDefinitionOwner = Get(guid);
            if (emdEquipmentDefinitionOwner != null)
            {
                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDCategoryEntity)handler.DeleteObject<EMDCategoryEntity>(emdEquipmentDefinitionOwner);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The CategoryEntity with guid: {0} was not found.", guid));
            }
        }

        /// <summary>
        /// Creates an EMDCategoryEntity
        /// </summary>
        /// <param name="emdCategoryEntity"></param>
        /// <returns><see cref="EMDCategoryEntity"/></returns>
        public EMDCategoryEntity Create(EMDCategoryEntity emdCategoryEntity)
        {
            CategoryEntityHandler handler = new CategoryEntityHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            return (EMDCategoryEntity)handler.CreateObject<EMDCategoryEntity>(emdCategoryEntity);
        }

        /// <summary>
        /// Gets all categories for a linked entity
        /// </summary>
        /// <param name="entityGuid">guid of the entity</param>
        /// <param name="categoryType"><seealso cref="EnumCategoryType"/></param>
        /// <returns><seealso cref="List{EMDCategory}"/></returns>
        public List<EMDCategory> GetCategoriesForEntity(string entityGuid, EnumCategoryType categoryType)
        {
            CategoryHandler catHandler = new CategoryHandler(this.Transaction);
            CategoryEntityHandler entityHandler = new CategoryEntityHandler(this.Transaction);
            return (from en in entityHandler.GetObjects<EMDCategoryEntity, CategoryEntity>("EntityGuid=\"" + entityGuid + "\"").Cast<EMDCategoryEntity>() join cat in catHandler.GetObjects<EMDCategory, Category>("CategoryType = " + (int)categoryType).Cast<EMDCategory>() on en.CATE_Guid equals cat.Guid select cat).ToList();
        }

        /// <summary>
        /// Gets all categoryentities for a linked entity
        /// </summary>
        /// <param name="entityGuid">guid of the entity</param>
        /// <param name="categoryType"><seealso cref="EnumCategoryType"/></param>
        /// <returns><seealso cref="List{EMDCategoryEntity}"/></returns>
        public List<EMDCategoryEntity> GetCategoryEntitiesForEntity(string entityGuid, EnumCategoryType categoryType)
        {
            CategoryHandler catHandler = new CategoryHandler(this.Transaction);
            CategoryEntityHandler entityHandler = new CategoryEntityHandler(this.Transaction);
            return (from en in entityHandler.GetObjects<EMDCategoryEntity, CategoryEntity>("EntityGuid=\"" + entityGuid + "\"").Cast<EMDCategoryEntity>() join cat in catHandler.GetObjects<EMDCategory, Category>("CategoryType = " + (int)categoryType).Cast<EMDCategory>() on en.CATE_Guid equals cat.Guid select en).ToList();
        }

        /// <summary>
        /// Gets an <seealso cref="EMDCategoryEntity"/> for the defined category guid, entity guid and categoryType
        /// </summary>
        /// <param name="categoryGuid">guid of the category</param>
        /// <param name="entityGuid">guid of the entity</param>
        /// <param name="categoryType"><seealso cref="EnumCategoryType"/></param>
        /// <returns><seealso cref="List{EMDCategoryEntity}"/></returns>
        public EMDCategoryEntity GetObjectForEntityAndCategory(string categoryGuid, string entityGuid, EnumCategoryType categoryType)
        {
            CategoryHandler catHandler = new CategoryHandler(this.Transaction);
            CategoryEntityHandler entityHandler = new CategoryEntityHandler(this.Transaction);
            return (from en in entityHandler.GetObjects<EMDCategoryEntity, CategoryEntity>("CATE_Guid=\"" + categoryGuid + "\" && EntityGuid=\"" + entityGuid + "\"").Cast<EMDCategoryEntity>() join cat in catHandler.GetObjects<EMDCategory, Category>("CategoryType = " + (int)categoryType).Cast<EMDCategory>() on en.CATE_Guid equals cat.Guid select en).FirstOrDefault();
        }

        /// <summary>
        /// Gets all linked entity for a category
        /// </summary>
        /// <param name="categoryGuid">guid of the category</param>
        /// <param name="categoryType"><seealso cref="EnumCategoryType"/></param>
        /// <returns><seealso cref="List{EMDCategoryEntity}"/></returns>
        public List<EMDCategoryEntity> GetEntitiesForCategory(string categoryGuid, EnumCategoryType categoryType)
        {
            CategoryHandler catHandler = new CategoryHandler(this.Transaction);
            CategoryEntityHandler entityHandler = new CategoryEntityHandler(this.Transaction);
            return (from en in entityHandler.GetObjects<EMDCategoryEntity, CategoryEntity>("CATE_Guid=\"" + categoryGuid + "\"").Cast<EMDCategoryEntity>() join cat in catHandler.GetObjects<EMDCategory, Category>("CategoryType = " + (int)categoryType).Cast<EMDCategory>() on en.CATE_Guid equals cat.Guid select en).ToList();
        }

        public void UpdateCategoriesForEntity(string eqde_guid, EnumCategoryType categoryType, List<string> configuredGuids)
        {
            CategoryManager catManager = new CategoryManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            CategoryEntityManager categoryEntityManager = new CategoryEntityManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<EMDCategory> currentAssignedCategories = categoryEntityManager.GetCategoriesForEntity(eqde_guid, EnumCategoryType.EquipmentDefinition);
            // search for removed categories
            foreach (EMDCategory currentAssignedCategory in currentAssignedCategories)
            {
                var items = (from a in configuredGuids where a == currentAssignedCategory.Guid select a).ToList();
                if (items.Count == 0)
                {
                    EMDCategoryEntity emdCategoryEntity = categoryEntityManager.GetObjectForEntityAndCategory(currentAssignedCategory.Guid, eqde_guid, EnumCategoryType.EquipmentDefinition);
                    if (categoryEntityManager != null)
                        categoryEntityManager.Delete(emdCategoryEntity.Guid);
                }
            }

            // search for newly added categories
            if (configuredGuids != null)
            {
                foreach (string configuredCategory in configuredGuids)
                {
                    if (!currentAssignedCategories.Exists(a => a.Guid == configuredCategory))
                    {
                        EMDCategoryEntity emdCategoryEntity = new EMDCategoryEntity();
                        emdCategoryEntity.CATE_Guid = configuredCategory;
                        emdCategoryEntity.EntityGuid = eqde_guid;

                        categoryEntityManager.Create(emdCategoryEntity);
                    }
                }
            }
            
        }
    }


}

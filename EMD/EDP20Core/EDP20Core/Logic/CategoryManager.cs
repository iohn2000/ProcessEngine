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
    public class CategoryManager : BaseManager
    {
        #region Constructors

        public CategoryManager()
            : base()
        {
        }

        public CategoryManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public CategoryManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public CategoryManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDCategory Get(string guid)
        {
            CategoryHandler handler = new CategoryHandler(this.Transaction);

            return (EMDCategory)handler.GetObject<EMDCategory>(guid);
        }

        /// <summary>
        /// Deletes the category
        /// </summary>
        /// <param name="guid">Guid of the category to delete</param>
        /// <param name="deleteLinkedEntites">Defines if the linked Entities should be removed</param>
        /// <returns>The <seealso cref="EMDCategory"/> that has been removed</returns>
        public EMDCategory Delete(string guid, bool deleteLinkedEntites)
        {
            CategoryHandler handler = new CategoryHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDCategory emdCategory = Get(guid);
            if (emdCategory != null)
            {
                CategoryEntityManager categoryEntityManager = new CategoryEntityManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                if (deleteLinkedEntites)
                {
                    List<EMDCategoryEntity> linkedEntities = categoryEntityManager.GetEntitiesForCategory(guid, EnumCategoryType.EquipmentDefinition);

                    linkedEntities.ForEach(item => {
                        categoryEntityManager.Delete(item.Guid);
                    });
                }

                Hashtable hashTable = handler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDCategory)handler.DeleteObject<EMDCategory>(emdCategory);
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
        /// <param name="emdCategory"></param>
        /// <returns><see cref="EMDCategory"/></returns>
        /// <exception cref="BaseException"></exception>
        public EMDCategory Create(EMDCategory emdCategory)
        {
            EMDCategory existingCategory = this.GetCategoryByName(emdCategory.Name, (EnumCategoryType)emdCategory.CategoryType);
            if (existingCategory == null)
            {
                CategoryHandler handler = new CategoryHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                return (EMDCategory)handler.CreateObject<EMDCategory>(emdCategory);
            }
            else
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK,"A category with this name already exists");
        }

        /// <summary>
        /// Gets a category by it's name for the defined <seealso cref="EnumCategoryType" />
        /// </summary>
        /// <param name="name">Name of the category</param>
        /// <param name="categoryType"><seealso cref="EnumCategoryType"/></param>
        /// <returns></returns>
        public EMDCategory GetCategoryByName(string name, EnumCategoryType categoryType)
        {
            return this.GetCategoriesByName(name, categoryType).FirstOrDefault();
        }

        /// <summary>
        /// Gets a list of EMDCategory by it's name for the defined <seealso cref="EnumCategoryType" />
        /// </summary>
        /// <param name="name">Name of the category</param>
        /// <param name="categoryType"><seealso cref="EnumCategoryType"/></param>
        /// <returns></returns>
        public List<EMDCategory> GetCategoriesByName(string name, EnumCategoryType categoryType)
        {
            CategoryHandler handler = new CategoryHandler();
            string where = "Name.Contains(\"" + name + "\") && CategoryType = " + (int)categoryType;
            return handler.GetObjects<EMDCategory, Category>(where).Cast<EMDCategory>().ToList();
        }

        /// <summary>
        /// Updates an EMDCategoryEntity
        /// </summary>
        /// <param name="emdCategory"></param>
        /// <returns><see cref="EMDCategory"/></returns>
        public void Update(EMDCategory emdCategory)
        {
            List<EMDCategory> existingCategories = this.GetCategoriesByName(emdCategory.Name, (EnumCategoryType)emdCategory.CategoryType);
            if (existingCategories == null || existingCategories.Count() < 2)
            {
                CategoryHandler handler = new CategoryHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                handler.UpdateDBObject(emdCategory);
            }
            else
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "A category with this name already exists");
        }

        /// <summary>
        /// Gets all categories for the specified type
        /// </summary>
        /// <param name="categoryType"><seealso cref="EnumCategoryType"/></param>
        /// <param name="includePseudoCategories">Defines if the pseudo-categories "All" and "Others" are in the result</param>
        /// <returns><seealso cref="List{EMDCategory}"/></returns>
        /// 
        public List<EMDCategory> GetCategories(EnumCategoryType categoryType, bool includePseudoCategories = false)
        {
            CategoryHandler handler = new CategoryHandler();

            List<EMDCategory> cats = handler.GetObjects<EMDCategory, Category>("CategoryType = " + (int)categoryType).Cast<EMDCategory>().ToList();
            if (includePseudoCategories)
                cats = AddPseudoCategory(cats, categoryType);

            return cats;
        }

        /// <summary>
        /// Adds the 2 pseudo-categories "All" and "Other" to the provided list of EMDcategory
        /// </summary>
        /// <param name="categories">The list of <seealso cref="List{EMDCategory}"/> were the pseude-categories should be added</param>
        /// <param name="categoryType"><seealso cref="EnumCategoryType"/></param>
        /// <returns></returns>
        private List<EMDCategory> AddPseudoCategory(List<EMDCategory> categories, EnumCategoryType categoryType)
        {
            EMDCategory catAll = new EMDCategory();
            catAll.CategoryType = (int)categoryType;
            catAll.Description = "Includes entities from all categories";
            catAll.Name = "All";

            EMDCategory catOther = new EMDCategory();
            catOther.CategoryType = (int)categoryType;
            catOther.Description = "Includes entities that do not belong to an category";
            catOther.Name = "Other";

            categories.Add(catAll);
            categories.Add(catOther);
            return categories;
        }









    }
}

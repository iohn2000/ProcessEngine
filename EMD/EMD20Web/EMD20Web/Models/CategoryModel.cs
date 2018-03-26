using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.EDP.Core.Entities.EquipmentDef;
using System.Threading;
using System.Globalization;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class CategoryModel : BaseModel
    {
        static IISLogger logger = ISLogger.GetLogger("CategoryModel");


        public string Guid { get; set; }

        public string HistoryGuid { get; set; }

        public System.DateTime? Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Type of the category <seealso cref="EnumCategoryType"/>
        /// </summary>
        public int CategoryType { get; set; }

        public IList<TextValueModel> AvailableEquipments { get; set; }

        public IList<TextValueModel> ConfiguredEquipments { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.CategoryManagement_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.CategoryManagement_View; } }

        
        public CategoryModel()
        {
            this.Guid = string.Empty;
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.Guid = string.Empty;
            AvailableEquipments = new List<TextValueModel>();
            ConfiguredEquipments = new List<TextValueModel>();
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public static EMDCategory Map(CategoryModel categoryModel)
        {
            return new EMDCategory()
            {
                Name = categoryModel.Name,
                Description = categoryModel.Description,
                CategoryType = categoryModel.CategoryType
            };
        }

        public static EMDCategory Update(EMDCategory emdCategory, CategoryModel categoryModel)
        {
            emdCategory.Name = categoryModel.Name;
            emdCategory.Description = categoryModel.Description;
            emdCategory.CategoryType = categoryModel.CategoryType;
            return emdCategory;
        }

        public static CategoryModel Initialize(EMDCategory category)
        {
            CategoryModel categoryModel = new CategoryModel();
            ReflectionHelper.CopyProperties<EMDCategory, CategoryModel>(ref category, ref categoryModel);
            return categoryModel;
 
       }
        //public static CategoryModel Initialize(EMDAccount acc)
        //{
        //    CategoryModel accmo = new CategoryModel();
        //    ReflectionHelper.CopyProperties(ref acc, ref accmo);
        //    return accmo;
        //}
    }
}
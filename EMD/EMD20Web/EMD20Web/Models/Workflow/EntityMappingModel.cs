using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models.Workflow
{
    public class EntityMappingModel
    {
        public string EntityPrefix { get; set; }

        public string EntityName { get; set; }

        public static List<EntityMappingModel> GetAvailableMappingEntities()
        {
            List<EntityMappingModel> entityMappingModels = new List<EntityMappingModel>();
            List<Type> typelist = EDP.Core.Framework.EntityPrefix.GetCoreTypeList();


            foreach (var item in typelist)
            {
                if (item.GetInterfaces().Contains(typeof(IProcessMapping)))
                {
                    IProcessMapping instanceObject = (IProcessMapping)Activator.CreateInstance(item);
                    entityMappingModels.Add(new EntityMappingModel()
                    {
                        EntityPrefix = instanceObject.GetType().GetProperty("Prefix").GetValue(instanceObject).ToString(),
                        EntityName = ObjectHelper.GetTypeName(instanceObject.GetType())
                    });
                }
            }


            entityMappingModels = entityMappingModels.OrderBy(model => model.EntityName).ToList();

            return entityMappingModels;
        }
    }
}
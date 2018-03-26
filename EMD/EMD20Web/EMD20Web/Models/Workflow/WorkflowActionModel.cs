using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Kapsch.IS.EMD.EMD20Web.Models.Workflow
{
    public class WorkflowActionModel : BaseModel
    {
        static IISLogger logger = ISLogger.GetLogger("ProcessMappingModel");

        private static ProcessServiceClient service;

        public static ProcessServiceClient Service
        {
            get
            {
                if (service == null)
                {
                    service = new ProcessServiceClient();
                }

                return service;
            }
        }

        public WorkflowActionModel()
        {
            AvailableMappingEntities = EntityMappingModel.GetAvailableMappingEntities();
            AvailableMethods = WorkflowActionMethodModel.GetAvailableMethods();

            RuleFilterModel = new RuleFilterModel();
        }

        [Required, Display(Name = "Entity type")]
        public List<EntityMappingModel> AvailableMappingEntities { get; set; }

        public List<WorkflowActionMethodModel> AvailableMethods { get; set; }


        public string Guid { get; set; }

        public string HistoryGuid { get; set; }
        public string TypePrefix { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.WorkflowManagement_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.NotDefined; } }

        [Required, Display(Name = "Prefix")]
        public string PrefixUI
        {
            get
            {
                return TypePrefix;
            }
            set
            {
                TypePrefix = value;
            }
        }


        public string EntityGuid { get; set; }

        [Required, Display(Name = "Entity")]
        public string EntityMappingGuid
        {
            get
            {
                return EntityGuid;
            }
            set
            {
                EntityGuid = value;
            }
        }

        [Required, Display(Name = "Method")]
        public string Method { get; set; }

        public WorkflowActionMethodModel MethodMappingModel
        {
            get
            {
                if (string.IsNullOrEmpty(Method))
                {
                    return null;
                }

                return new WorkflowActionMethodModel((WorkflowAction)System.Enum.Parse(typeof(WorkflowAction), Method, true));
            }
            set
            {
                if (value != null)
                {
                    Method = value.ToString();
                }
            }
        }

        public string WorkflowID { get; set; }

        [Required, Display(Name = "Workflow")]
        public string WorkflowGuid
        {
            get
            {
                return WorkflowID;
            }
            set
            {
                WorkflowID = value;
            }
        }



        public string WorkflowVariables { get; set; }

        public Nullable<System.DateTime> Created { get; set; }

        [Display(Name = "Created")]
        public DateTime? CreatedDateOnly
        {
            get { return this.Created?.Date; }
        }

        public Nullable<System.DateTime> Modified { get; set; }

        [Display(Name = "Modified")]
        public DateTime? ModifiedDateOnly
        {
            get { return this.Modified?.Date; }
        }

        [Display(Name = "Workflow")]
        public string WorkflowName { get; set; }

        [Display(Name = "Prefix")]
        public string MappedObjectTypeName { get; set; }

        [Display(Name = "Entity")]
        public string MappedObjectName { get; set; }

        public RuleFilterModel RuleFilterModel { get; set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public static List<WorkflowActionModel> GetList()
        {
            List<WorkflowActionModel> processMappingList = new List<WorkflowActionModel>();

            List<WorkflowItem> workflowItems = Service.GetWorkflowItems().ToList();

            ProcessMappingHandler processMappingHandler = new ProcessMappingHandler();
            List<IEMDObject<EMDProcessMapping>> processMappingItems = processMappingHandler.GetObjects<EMDProcessMapping, ProcessMapping>();


            foreach (EMDProcessMapping item in processMappingItems)
            {
                WorkflowActionModel processMappingModel = WorkflowActionModel.Map(item, false);
                processMappingModel.MapEnhancedProperties(ref processMappingModel, workflowItems);
                try
                {
                    processMappingList.Add(processMappingModel);
                }

                catch (Exception ex)
                {
                    logger.Error("ProcessMappingModel throwed an exception", ex);
                }

            }
            return processMappingList;
        }

        public void MapEnhancedProperties(ref WorkflowActionModel processMappingModel, List<WorkflowItem> workflowItems = null)
        {
            processMappingModel.MappedObjectTypeName = ObjectHelper.GetTypeName(processMappingModel.TypePrefix);
            string idWorkflow = processMappingModel.WorkflowID;

            if (!string.IsNullOrEmpty(this.Guid))
            {
                this.RuleFilterModel = ObjectHelper.GetRuleFilterModel(this.Guid);
            }

            if (workflowItems == null)
            {
                workflowItems = Service.GetWorkflowItems().ToList();
            }


            WorkflowItem workflow = (from a in workflowItems where a.Id == idWorkflow select a).FirstOrDefault();
            if (workflow != null)
            {
                processMappingModel.WorkflowName = workflow.Name;
            }

            if (processMappingModel.EntityGuid != null)
            {

                object obj = EDP.Core.Framework.EntityPrefix.Instance.GetInstanceFromGuid<object>(processMappingModel.EntityGuid);

                if (obj != null)
                {
                    if (obj.GetType() == typeof(EMDEquipmentDefinition))
                    {
                        EMDEquipmentDefinition equipmentDefinition = obj as EMDEquipmentDefinition;
                        if (equipmentDefinition != null)
                        {
                            processMappingModel.MappedObjectName = equipmentDefinition.Name;
                        }
                    }
                }
            }

        }

        public static WorkflowActionModel Map(EMDProcessMapping processMapping, bool mapEnhancedProperties = true)
        {
            WorkflowActionModel processMappingModel = new WorkflowActionModel();

            //ReflectionHelper.CopyProperties(ref processMapping, ref processMappingModel);

            processMappingModel.ActiveFrom = processMapping.ActiveFrom;
            processMappingModel.ActiveTo = processMapping.ActiveTo;
            processMappingModel.Created = processMapping.Created;
            processMappingModel.EntityGuid = processMapping.EntityGuid;
            processMappingModel.Guid = processMapping.Guid;
            processMappingModel.HistoryGuid = processMapping.HistoryGuid;
            processMappingModel.Method = processMapping.Method;
            processMappingModel.Modified = processMapping.Modified;
            processMappingModel.TypePrefix = processMapping.TypePrefix;
            processMappingModel.ValidFrom = processMapping.ValidFrom;
            processMappingModel.ValidTo = processMapping.ValidTo;
            processMappingModel.WorkflowID = processMapping.WorkflowID;
            processMappingModel.WorkflowVariables = processMapping.WorkflowVariables;

            if (mapEnhancedProperties)
            {
                processMappingModel.MapEnhancedProperties(ref processMappingModel);

            }

            return processMappingModel;
        }

        public static List<WorkflowActionModel> Map(List<EMDProcessMapping> processMappingItems)
        {
            List<WorkflowActionModel> processMappingModels = new List<WorkflowActionModel>();


            foreach (EMDProcessMapping item in processMappingItems)
            {
                processMappingModels.Add(Map(item));
            }

            return processMappingModels;
        }
    }
}
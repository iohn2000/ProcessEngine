using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.EDP.Core.DB;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEquipmentDefinition : EMDObject<EMDEquipmentDefinition>, IProcessMapping
    {


        public int Q_ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Config { get; set; }
        /// <summary>
        /// Indicates if the price information of the equipment is managed in edp or in an external system. If it has a value, the price information comes from a external system 
        /// and this is the name of key field that is used for the import.
        /// </summary>
        public string ClientReferenceIDForPrice { get; set; }
        /// <summary>
        /// Defines the external system, that is used to import the price for the equipmentDefinition <see cref="EnumClientReferenceSystemForPrice"/>
        /// </summary>
        public Nullable<int> ClientReferenceSystemForPrice { get; set; }
        public string WorkingInstructions { get; set; }
        public string DescriptionLong { get; set; }
        public override String Prefix { get { return "EQDE"; } }

        public EMDEquipmentDefinition(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDEquipmentDefinition()
        { }

        public bool HasEntities()
        {
            return true;
        }

        public EquipmentDefinitionConfig GetEquipmentDefinitionConfig()
        {
            return EquipmentDefinitionConfig.Map(this.Config);
        }

        public void SetEquipmentDefinitionConfig(EquipmentDefinitionConfig equipmentDefinitionConfig)
        {
            this.Config = EquipmentDefinitionConfig.Map(equipmentDefinitionConfig);
        }

        public void SetEquipmentDefinitionConfig()
        {
            EquipmentDefinitionConfig.Map(this.Config);
        }

        public List<KeyValuePair<string, string>> GetEntityList()
        {
            List<KeyValuePair<string, string>> entities = new List<KeyValuePair<string, string>>();
            EquipmentDefinitionHandler handler = new EquipmentDefinitionHandler();

            List<IEMDObject<EMDEquipmentDefinition>> definitions = handler.GetObjects<EMDEquipmentDefinition, DB.EquipmentDefinition>();

            foreach (var item in definitions)
            {
                string name = ((EMDEquipmentDefinition)item).Name;
                if (!string.IsNullOrWhiteSpace(((EMDEquipmentDefinition)item).Description))
                {
                    name = string.Format("{0}-{1}", ((EMDEquipmentDefinition)item).Name, ((EMDEquipmentDefinition)item).Description);
                }

                entities.Add(new KeyValuePair<string, string>(item.Guid, name));
            }

            return entities;
        }

        public List<WorkflowAction> GetMappingMethods()
        {
            List<WorkflowAction> processingMethods = new List<WorkflowAction>();

            processingMethods.Add(WorkflowAction.Add);
            processingMethods.Add(WorkflowAction.Change);
            processingMethods.Add(WorkflowAction.Remove);

            return processingMethods;
        }
    }
}

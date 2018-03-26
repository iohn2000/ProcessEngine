namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEquipment
    {
        public string Q_Name { get; set; }

        public string Q_Description { get; set; }
        public string Q_Guid { get; set; }
        public string BR_Guid { get; set; }
        public string BR_Name { get; set; }

        public string Status { get; set; }

        public EMDEquipment(string equipmentName, string equipmentDescription, string equipmentDefinition_Guid, string status)
        {
            Q_Name = equipmentName;
            Q_Description = equipmentDescription;
            Q_Guid = equipmentDefinition_Guid;
            Status = status;
        }

        public EMDEquipment()
        {

        }
    }
}
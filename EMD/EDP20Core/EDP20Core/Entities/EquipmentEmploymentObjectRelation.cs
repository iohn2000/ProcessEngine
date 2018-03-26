using System;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEquipmentEmploymentObjectRelation: EMDHistorizableObject, IEMDHistorizableObject
    {

        public string EQDE_Name { get; set; }
        public string EQDE_Description { get; set; }
        public string EQDE_Guid { get; set; }
        public string BURE_Guid { get; set; }
        public string BURE_Name { get; set; }
        public EquipmentStatus Status { get; set; }
        public string EP_Guid { get; set; }
        public string OBCO_Guid { get; set; }

        public EMDEquipmentEmploymentObjectRelation(string guid, string eqde_Name, string eqde_Description, string eqde_Guid, string bR_Guid, string bR_Name, EquipmentStatus status, string eP_Guid, DateTime validFrom, DateTime validTo, DateTime created, DateTime modified): base(guid, created, modified)
        {
            this.Guid = guid;
            this.EQDE_Name = eqde_Name;
            this.EQDE_Description = eqde_Description;
            this.EQDE_Guid = eqde_Guid;
            this.BURE_Guid = bR_Guid;
            this.BURE_Name = bR_Name;
            this.Status = status;
            this.EP_Guid = eP_Guid;
  //          this.OBCO_Guid = oBCO_Guid;
            this.ValidFrom = validFrom;
            this.ValidTo = validTo;
        }

        public EMDEquipmentEmploymentObjectRelation()
        {
        }
    }

    public enum EquipmentStatus { notset=0, ordered=10, queued=20, processed=30, attached=100, declined=101, timeout=102, error=103 }
}
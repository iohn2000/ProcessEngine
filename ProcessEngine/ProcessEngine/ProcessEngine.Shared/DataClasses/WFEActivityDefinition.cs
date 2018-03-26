using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;

namespace Kapsch.IS.ProcessEngine.Shared.DataClasses
{
    public class WFEActivityDefinition
    {
        public string WFAD_ID { get; set; }

        public string WFAD_Name { get; set; }

        public string WFAD_ConfigTemplate { get; set; }

        public int WFAD_HostLoad { get; set; }

        public DateTime WFAD_Created { get; set; }

        public DateTime WFAD_ValidFrom { get; set; }

        public DateTime WFAD_ValidTo { get; set; }

        public bool WFAD_IsStartActivity { get; set; }

        public EnumActivityType WFAD_Type { get; set; }
        
    }
}

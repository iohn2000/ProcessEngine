using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.DLLStepsConfiguration
{
    
    public class ActivityConfigurationElement
    {
        public string StepName { get; set; }
        public string NameSpace { get; set; }
        public string DLLPath { get; set; }

        public ActivityConfigurationElement()
        {

        }

        public ActivityConfigurationElement(string sName, string nspace, string dllPath)
        {
            this.StepName = sName;
            this.NameSpace = nspace;
            this.DLLPath = dllPath;
        }
    }
}

using System;
using System.Configuration;

namespace Kapsch.IS.ProcessEngine.DLLStepsConfiguration
{
    [Obsolete("Not used anymore, replace with ActivityDLLConfiguration", true)]
    public class DLLStepElement : ConfigurationElement
    {
        private static ConfigurationProperty s_propStepName;
        private static ConfigurationProperty s_propNameSpace;
        private static ConfigurationProperty s_propDLLPath;
        private static ConfigurationPropertyCollection s_properties;

        static DLLStepElement()
        {
            s_propStepName = new ConfigurationProperty(
                "stepName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

            s_propNameSpace = new ConfigurationProperty(
                "nameSpace", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

            s_propDLLPath = new ConfigurationProperty(
                "dllPath", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

            s_properties = new ConfigurationPropertyCollection();
            s_properties.Add(s_propStepName);
            s_properties.Add(s_propNameSpace);
            s_properties.Add(s_propDLLPath);
        }

        [ConfigurationProperty("stepName", IsRequired = true)]
        public string StepName
        {
            get { return (string) base[s_propStepName]; }
        }

        [ConfigurationProperty("nameSpace", IsRequired = true)]
        public string NameSpace
        {
            get { return (string) base[s_propNameSpace]; }
        }

        [ConfigurationProperty("dllPath", IsRequired = true)]
        public string DLLPath
        {
            get { return (string) base[s_propDLLPath]; }
            set { }
        }
    }
}

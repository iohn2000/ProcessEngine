using Kapsch.IS.ProcessEngine.DLLStepsConfiguration;
using System;
using System.Configuration;

namespace Kapsch.IS.ProcessEngine.DLLStepsConfiguration
{
    [Obsolete("Not used anymore, replace with ActivityDLLConfiguration", true)]
    public class DLLStepConfigurationSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection s_properties;
        private static ConfigurationProperty s_propDLLStepsCollection;
        private static ConfigurationProperty s_propName;

        static DLLStepConfigurationSection()
        {
            s_propName = new ConfigurationProperty(
                "name",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
                );

            s_propDLLStepsCollection = new ConfigurationProperty(
                "",
                typeof(DLLStepElementCollection),
                null,
                ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsDefaultCollection);

            s_properties = new ConfigurationPropertyCollection();
            s_properties.Add(s_propName);
            s_properties.Add(s_propDLLStepsCollection);
        }

        #region Properties
        public string Name
        {
            get { return (string) base[s_propName]; }
            set { base[s_propName] = value; }
        }

        public DLLStepElementCollection DLLSteps
        {
            get { return (DLLStepElementCollection) base[s_propDLLStepsCollection]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return s_properties;
            }
        }
        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.DLLConfiguration
{
    /// <summary>
    /// define a location for activity dll
    /// </summary>
    public class ActivityDLLConfigurationSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection s_properties;
        private static ConfigurationProperty s_propDLLStepsCollection;
        private static ConfigurationProperty s_propName;

        static ActivityDLLConfigurationSection()
        {
            s_propName = new ConfigurationProperty(
                "name",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
                );

            s_propDLLStepsCollection = new ConfigurationProperty(
                "",
                typeof(ActivityDLLElementCollection),
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

        public ActivityDLLElementCollection ActivityDLLs
        {
            get { return (ActivityDLLElementCollection) base[s_propDLLStepsCollection]; }
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

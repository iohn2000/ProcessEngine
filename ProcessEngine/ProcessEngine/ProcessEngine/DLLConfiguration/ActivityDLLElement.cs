using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.DLLConfiguration
{
    public class ActivityDLLElement : ConfigurationElement
    {
        private static ConfigurationProperty s_propDllName;
        private static ConfigurationProperty s_propDLLPath;
        private static ConfigurationPropertyCollection s_properties;

        static ActivityDLLElement()
        {
            s_propDllName = new ConfigurationProperty(
                "dllName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

            s_propDLLPath = new ConfigurationProperty(
                "dllPath", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

            s_properties = new ConfigurationPropertyCollection();
            s_properties.Add(s_propDllName);
            s_properties.Add(s_propDLLPath);
        }

        [ConfigurationProperty("dllName", IsRequired = true)]
        public string DLLName
        {
            get { return (string) base[s_propDllName]; }
        }

        [ConfigurationProperty("dllPath", IsRequired = true)]
        public string DLLPath
        {
            get { return (string) base[s_propDLLPath]; }
            set { }
        }
    }
}

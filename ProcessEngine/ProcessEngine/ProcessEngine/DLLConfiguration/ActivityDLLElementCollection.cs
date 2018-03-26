using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.DLLConfiguration
{
    [ConfigurationCollection(
        typeof(ActivityDLLElement), AddItemName = "ActivityDLL", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ActivityDLLElementCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection s_properties;

        static ActivityDLLElementCollection()
        {
            s_properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return s_properties; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "ActivityDLL"; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ActivityDLLElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ActivityDLLElement).DLLName;
        }

         #region Indexer
        public ActivityDLLElement this[int index]
        {
            get { return (ActivityDLLElement)base.BaseGet(index); }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }
        public ActivityDLLElement this[string name]
        {
            get { return (ActivityDLLElement)base.BaseGet(name); }
        }
        #endregion
    }
}

using Kapsch.IS.ProcessEngine.DLLStepsConfiguration;
using System;
using System.Configuration;

namespace Kapsch.IS.ProcessEngine.DLLStepsConfiguration
{
    [Obsolete("Not used anymore, replace with ActivityDLLConfiguration", true)]
    [ConfigurationCollection(
        typeof(DLLStepElement), AddItemName = "DLLStep", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class DLLStepElementCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection s_properties;

        static DLLStepElementCollection()
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
            get { return "DLLStep"; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DLLStepElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as DLLStepElement).NameSpace;
        }

        #region Indexer
        public DLLStepElement this[int index]
        {
            get { return (DLLStepElement) base.BaseGet(index); }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }
        public DLLStepElement this[string name]
        {
            get { return (DLLStepElement) base.BaseGet(name); }
        }
        #endregion
    }
}

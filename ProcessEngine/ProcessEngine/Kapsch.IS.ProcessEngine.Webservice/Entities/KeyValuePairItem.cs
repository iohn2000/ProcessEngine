using System.Runtime.Serialization;

namespace Kapsch.IS.ProcessEngine.Webservice.Entities
{
    /// <summary>
    /// Summary description for KeyValuePairItem
    /// </summary>
    [DataContract]
    public class KeyValuePairItem
    {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Value { get; set; }

        public KeyValuePairItem()
        {
        }

        public KeyValuePairItem(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
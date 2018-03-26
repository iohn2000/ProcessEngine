using System.Runtime.Serialization;

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
        //
        // TODO: Add constructor logic here
        //
    }

    public KeyValuePairItem(string key, string value)
    {
        this.Key = key;
        this.Value = value;
    }
}
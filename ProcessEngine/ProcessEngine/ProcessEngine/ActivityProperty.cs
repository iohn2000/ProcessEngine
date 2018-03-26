using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Xml.Linq;


namespace Kapsch.IS.ProcessEngine
{
    public class ActivityProperty
    {
        public XElement PropertyNode { get; set; }

        public ActivityProperty(XElement propNode)
        {
            this.PropertyNode = propNode;
        }
        public ActivityProperty(string propXmlString)
        {
            this.PropertyNode = XElement.Parse(propXmlString);
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets the name. </summary>
        ///
        /// <value> The name. </value>
        #endregion
        public string Name
        {
            get
            {
                XAttribute attr = this.PropertyNode.Attribute("name");
                if (attr != null)
                    return attr.Value;
                else
                    return null;
            }
        }
        public EnumVariableDirection Direction
        {
            get
            {
                EnumVariableDirection direc;
                XAttribute attr = this.PropertyNode.Attribute("direction");
                if (attr != null)
                {
                    if (Enum.TryParse(attr.Value, out direc))
                    {
                        if (Enum.IsDefined(typeof(EnumVariableDirection), direc))
                        {
                            return direc;
                        }
                    }
                }
                return EnumVariableDirection.input;
            }
        }
        public EnumVariablesDataType DataType
        {
            get
            {
                XAttribute attr = this.PropertyNode.Attribute("dataType");
                if (attr != null)
                {
                    return Variable.ConvertToEnumDataType(attr.Value);
                }
                else
                    return EnumVariablesDataType.stringType;
            }
        }
        public string Value
        {
            get
            {
                return this.PropertyNode.Value;
            }
            set
            {
                this.PropertyNode.Value = value;
            }
        }

        public static EnumVariableDirection ConvertToType(string dType)
        {
            EnumVariableDirection parsedType;
            if (Enum.TryParse(dType, out parsedType))
            {
                if (Enum.IsDefined(typeof(EnumVariableDirection), parsedType))
                {
                    return parsedType;
                }
            }

            return EnumVariableDirection.input;
        }
    }
}


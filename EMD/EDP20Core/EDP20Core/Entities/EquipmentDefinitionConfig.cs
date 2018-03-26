using Kapsch.IS.EDP.Core.Entities.EquipmentDef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EquipmentDefinitionConfig
    {
        /// <summary>
        /// Config is generated from File /Specifications/EquipmentDefinition.xml
        /// </summary>
        public const string ConfigXSD = @"
                        <?xml version=""1.0"" encoding=""utf-8""?>
                        <xs:schema attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
                          <xs:element name=""EquipmentDefinition"">
                            <xs:complexType>
                              <xs:sequence>
                                <xs:element name=""IsDefault"" type=""xs:boolean"" />
                                <xs:element name=""MaxNumberAllowedEquipments"" type=""xs:unsignedByte"" />
                                <xs:element name=""CanKeep"" type=""xs:boolean"" />
                                <xs:element name=""IsAccountingJob"" type=""xs:boolean"" />
                                <xs:element name=""IsAccountingOnMainEmployment"" type=""xs:boolean"" />
                                <xs:element name=""IsPeriodic"" type=""xs:boolean"" />
                                <xs:element name=""GuidApprover"" type=""xs:string""/>
                                <xs:element name=""IdEmailTemplateAdd"" type=""xs:string""/>
                                <xs:element name=""IdEmailTemplateRemove"" type=""xs:string""/>
                                <xs:element name=""IdEmailTemplateChange"" type=""xs:string""/>
                                <xs:element name=""NavisionSourceSystemNumber"" type=""xs:string""/>
                                <xs:element name=""ActiveDirectoryGroupName"" type=""xs:string""/>
                              </xs:sequence>
                            </xs:complexType>
                          </xs:element>
                        </xs:schema>
        ";

        public static List<string> Fields = new List<string>() {
            "IsDefault",
            "MaxNumberAllowedEquipments",
            "CanKeep",
            "IsAccountingJob",
            "IsAccountingOnMainEmployment",
            "IsPeriodic",
            "GuidApprover",
            "IdEmailTemplateAdd",
            "IdEmailTemplateRemove",
            "IdEmailTemplateChange",
            "NavisionSourceSystemNumber",
            "ActiveDirectoryGroupName"
        };

        public bool IsDefault { get; set; }
        public int MaxNumberAllowedEquipments { get; set; }
        public bool CanKeep { get; set; }
        public bool IsAccountingJob { get; set; }
        public bool IsAccountingOnMainEmployment { get; set; }
        public bool IsPeriodic { get; set; }
        public string GuidApprover { get; set; }
        public string IdEmailTemplateAdd { get; set; }
        public string IdEmailTemplateRemove { get; set; }
        public string IdEmailTemplateChange { get; set; }
        public EnumNavisionSourceSystemNumber NavisionSourceSystemNumber { get; set; }
        public string ActiveDirectoryGroupName { get; set; }


        public List<DynamicField> DynamicFields = new List<DynamicField>();

        private static void ValidateXml(string xml)
        {
            XDocument wfModel = XDocument.Parse(xml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

            XmlSchemaSet schemas = new XmlSchemaSet();

            using (StringReader stringReader = new StringReader(EquipmentDefinitionConfig.ConfigXSD.Replace("\n", string.Empty).Replace("\r", string.Empty).Trim()))
            using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
            {
                schemas.Add(null, xmlReader);
            }

            StringBuilder errors = new StringBuilder();

            wfModel.Validate(schemas, (o, e) =>
            {
                // WorkflowError wfErr = new WorkflowError(e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
                errors.AppendFormat("error: {0} line: {1} position: {2}", e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
            });

            if (errors.Length > 0)
            {
                throw new Exception(errors.ToString());
            }
        }

        public static EquipmentDefinitionConfig Map(string xml)
        {
            //  ValidateXml(xml);

            EquipmentDefinitionConfig config = new EquipmentDefinitionConfig();
            if (!string.IsNullOrEmpty(xml))
            {
                XDocument xDocument = XDocument.Parse(xml);
                XElement xElement = xDocument.XPathSelectElement("EquipmentDefinition");



                try
                {
                    if (xElement != null)
                    {
                        config = new EquipmentDefinitionConfig();

                        // Important: handle each property in try/catch block to be sure, every property is parsed
                        try
                        {
                            config.IsDefault = bool.Parse(xElement.XPathSelectElement("IsDefault").Value);
                        }
                        catch (Exception) { }
                        try
                        {
                            config.GuidApprover = xElement.XPathSelectElement("GuidApprover").Value;
                        }
                        catch (Exception) { }
                        try
                        {
                            config.IdEmailTemplateAdd = xElement.XPathSelectElement("IdEmailTemplateAdd").Value;
                        }
                        catch (Exception) { }
                        try
                        {
                            config.IdEmailTemplateRemove = xElement.XPathSelectElement("IdEmailTemplateRemove").Value;
                        }
                        catch (Exception) { }
                        try
                        {
                            config.IdEmailTemplateChange = xElement.XPathSelectElement("IdEmailTemplateChange").Value;
                        }
                        catch (Exception) { }
                        try
                        {
                            config.IsAccountingJob = bool.Parse(xElement.XPathSelectElement("IsAccountingJob").Value);
                        }
                        catch (Exception) { }
                        try
                        {
                            config.IsAccountingOnMainEmployment = bool.Parse(xElement.XPathSelectElement("IsAccountingOnMainEmployment").Value);
                        }
                        catch (Exception) { }
                        try
                        {
                            config.IsPeriodic = bool.Parse(xElement.XPathSelectElement("IsPeriodic").Value);
                        }
                        catch (Exception) { }
                        try
                        {
                            config.CanKeep = bool.Parse(xElement.XPathSelectElement("CanKeep").Value);
                        }
                        catch (Exception) { }
                        try
                        {
                            config.MaxNumberAllowedEquipments = int.Parse(xElement.XPathSelectElement("MaxNumberAllowedEquipments").Value);
                        }
                        catch (Exception) { }
                        try
                        {
                            config.NavisionSourceSystemNumber = (EnumNavisionSourceSystemNumber) Enum.Parse(typeof(EnumNavisionSourceSystemNumber), xElement.XPathSelectElement("NavisionSourceSystemNumber").Value, true);
                        }
                        catch (Exception)
                        {
                            config.NavisionSourceSystemNumber = EnumNavisionSourceSystemNumber.Undefined;
                        }
                        try
                        {
                            config.ActiveDirectoryGroupName = xElement.XPathSelectElement("ActiveDirectoryGroupName") == null ? string.Empty : xElement.XPathSelectElement("ActiveDirectoryGroupName").Value;
                        }
                        catch (Exception)
                        {

                        }

                        try
                        {
                            config.DynamicFields = ParseDynamicFields(xElement.XPathSelectElement("DynamicFields"));
                        }
                        catch (Exception)
                        {
                            config.DynamicFields = new List<DynamicField>();
                        }
                    }
                }
                catch (Exception) { }

            }

            return config;
        }


        public static List<DynamicField> ParseDynamicFields(XElement xElementDynamicFieldsNode)
        {
            List<DynamicField> dynamicFields = new List<DynamicField>();

            if (xElementDynamicFieldsNode != null)
            {

                List<XElement> xElementDynamicFields = xElementDynamicFieldsNode.XPathSelectElements("DynamicField").ToList();

                foreach (XElement xElementDynamicField in xElementDynamicFields)
                {
                    DynamicField dynamicField = new DynamicField();

                    dynamicField.Identifier = (EnumDynamicFieldEquipment) Enum.Parse(typeof(EnumDynamicFieldEquipment), xElementDynamicField.XPathSelectElement("Identifier").Value);
                    dynamicField.Type = (EnumDynamicFieldType) Enum.Parse(typeof(EnumDynamicFieldType), xElementDynamicField.XPathSelectElement("Type").Value);
                    dynamicField.Name = xElementDynamicField.XPathSelectElement("Name").Value;
                    dynamicField.IsMandatory = bool.Parse(xElementDynamicField.XPathSelectElement("IsMandatory").Value);

                    dynamicFields.Add(dynamicField);
                }

            }
            return dynamicFields;
        }

        public static XElement CreateDynamicFields(List<DynamicField> dynamicFields)
        {
            List<XElement> elements = new List<XElement>();

            foreach (DynamicField dynamicField in dynamicFields)
            {
                XElement xElementDynamicField = new XElement("DynamicField",
                    new XElement("Identifier", dynamicField.Identifier),
                    new XElement("Type", dynamicField.Type),
                    new XElement("Name", dynamicField.Name),
                    new XElement("IsMandatory", dynamicField.IsMandatory)
                    );

                elements.Add(xElementDynamicField);
            }

            XElement xElementdynamicFields = new XElement("DynamicFields", elements);

            return xElementdynamicFields;
        }

        public static string Map(EquipmentDefinitionConfig equipmentDefinitionConfig)
        {
            XDocument xDocument = new XDocument();


            XDocument doc =
                  new XDocument(
                    new XElement("EquipmentDefinition",
                      new XElement("IsDefault", equipmentDefinitionConfig.IsDefault),
                      new XElement("GuidApprover", equipmentDefinitionConfig.GuidApprover),
                      new XElement("IdEmailTemplateAdd", equipmentDefinitionConfig.IdEmailTemplateAdd),
                      new XElement("IdEmailTemplateRemove", equipmentDefinitionConfig.IdEmailTemplateRemove),
                      new XElement("IdEmailTemplateChange", equipmentDefinitionConfig.IdEmailTemplateChange),
                      new XElement("IsAccountingJob", equipmentDefinitionConfig.IsAccountingJob),
                      new XElement("IsAccountingOnMainEmployment", equipmentDefinitionConfig.IsAccountingOnMainEmployment),
                      new XElement("IsPeriodic", equipmentDefinitionConfig.IsPeriodic),
                      new XElement("CanKeep", equipmentDefinitionConfig.CanKeep),
                      new XElement("MaxNumberAllowedEquipments", equipmentDefinitionConfig.MaxNumberAllowedEquipments),
                      new XElement("NavisionSourceSystemNumber", equipmentDefinitionConfig.NavisionSourceSystemNumber.ToString()),
                      new XElement("ActiveDirectoryGroupName", equipmentDefinitionConfig.ActiveDirectoryGroupName),
                      CreateDynamicFields(equipmentDefinitionConfig.DynamicFields)
                    )
              );

            return doc.ToString();
        }
    }
}

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class ObjectRelationManager
        : BaseManager
    {
        #region Constructor

        public ObjectRelationManager()
            : base()
        {
        }

        public ObjectRelationManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public ObjectRelationManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public ObjectRelationManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment=null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructor

        public EMDObjectRelation Get(string guid)
        {
            ObjectRelationHandler userHandler = new ObjectRelationHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            return (EMDObjectRelation)userHandler.GetObject<EMDObjectRelation>(guid);
        }

        public void AddMetaData_SimpleValue(EMDObjectRelation obre, string key, string metaValue, bool updateExisting = true)
        {
            XDocument metaX = new XDocument();
            XElement root, simpleValue;
            /*
            <OBRE_Data>
                <SimpleValues>
                    <Item key="EQ_InsAppBY" value="SYSTEM" />
                    <Item key="EQ_InsAppOK" value="True" />
              </SimpleValues>
            </OBRE_Data>
            */
            if (obre != null)
            {
                if (!string.IsNullOrWhiteSpace(obre.Data))
                {
                    try
                    {
                        metaX = XDocument.Parse(obre.Data);
                    }
                    catch (Exception ex)
                    {
                        // catches empty and null and invalid xml
                        throw new BaseException(ErrorCodeHandler.E_INPUT_GENERAL, "ObjectRelation.Data field has invalid XML.", ex);
                    }
                }

                root = metaX.XPathSelectElement("/OBRE_Data");
                if (root == null)
                {
                    root = new XElement("OBRE_Data");
                    try
                    {
                        metaX.Add(root);
                    }
                    catch (Exception ex)
                    {

                        throw new BaseException(ErrorCodeHandler.E_INPUT_GENERAL, ex);
                    }
                }

                simpleValue = metaX.XPathSelectElement("/OBRE_Data/SimpleValues");
                if (simpleValue == null)
                {
                    simpleValue = new XElement("SimpleValues");
                    try
                    {
                        root.Add(simpleValue);
                    }
                    catch (Exception ex)
                    {
                        throw new BaseException(ErrorCodeHandler.E_INPUT_GENERAL, ex);
                    }
                }

                if (!string.IsNullOrWhiteSpace(key) & !string.IsNullOrWhiteSpace(metaValue))
                {
                    try
                    {
                        XElement colItem = new XElement("Item");
                        colItem.SetAttributeValue("key", key);
                        colItem.SetAttributeValue("value", metaValue);
                        simpleValue.Add(colItem);

                        obre.Data = metaX.ToString(SaveOptions.None);
                    }
                    catch (Exception ex)
                    {

                        throw new BaseException(ErrorCodeHandler.E_INPUT_GENERAL, ex);
                    }
                }
                else
                {
                    throw new BaseException(ErrorCodeHandler.E_INPUT_GENERAL, "ObjectRelation.Data key and/or value cannot be null or empty.");
                }
            }
        }

        public string GetMetaData_SimpleValue(EMDObjectRelation obre, string keyValue)
        {
            if (obre != null && obre.Data != null && keyValue != null)
            {
                try
                {
                    XDocument metaX = XDocument.Parse(obre.Data);
                    return this.getValueFromXmlItemKey(metaX,keyValue);
                }
                catch (Exception ex)
                {

                    throw new BaseException(ErrorCodeHandler.E_INPUT_GENERAL,"Error trying to read value from objectrelation meata data.",ex);
                }
            }
            return null;
        }

        public Dictionary<string, string> GetMetaData_AllSimpleValues(EMDObjectRelation obre)
        {
            Dictionary<string, string> allValues = new Dictionary<string, string>();
            if (obre != null && obre.Data != null)
            {
                XDocument xdoc = XDocument.Parse(obre.Data);
                allValues = xdoc.XPathSelectElements("/OBRE_Data/SimpleValues/Item").ToDictionary(el => el.Attribute("key").Value, el => el.Attribute("value").Value);
            }
            return allValues;
        }

        public XDocument GetMetaData_Complex(EMDObjectRelation obre)
        {
            throw new NotImplementedException();
        }

        private string getValueFromXmlItemKey(XDocument el, string keyValue, string keyName="key", string valueName="value")
        {
            string xpQuery = "/OBRE_Data/SimpleValues/Item[@{0}='{1}']";
            XElement itemElement = el.XPathSelectElement(string.Format(xpQuery,keyName,keyValue));
            if (itemElement != null)
            {
                XAttribute valueAttr = itemElement.Attribute(valueName);
                if (valueAttr != null)
                {
                    return valueAttr.Value;
                }
                else
                    return null;
            }
            else
                return null;
        }

        //private List<EMDObjectRelation> GetAllEQsLinkedToEmploymentsByPackage()
        private List<EMDObjectRelation> GetAllObjectRelationsForEquipmentDefinitionByPackage(string eqdeGuid)
        {
            return this.GetAllObjectRelationsForEquipmentDefinitionByObjectRelationType(eqdeGuid, ObjectRelationTypeList.EquipmentByPackage);
        }

        private List<EMDObjectRelation> GetAllObjectRelationsForEquipmentDefinitionAndEmploymentByPackage(string eqdeGuid, string emplGuid)
        {
            return this.GetAllObjectRelationsForEquipmentDefinitionAndEmploymentByObjectRelationType(eqdeGuid, ObjectRelationTypeList.EquipmentByPackage, emplGuid);
        }

        private List<EMDObjectRelation> GetAllObjectRelationsForEquipmentDefinitionByEmploymentPackage(string eqdeGuid)
        {
            return this.GetAllObjectRelationsForEquipmentDefinitionByObjectRelationType(eqdeGuid, ObjectRelationTypeList.EquipmentByEmploymentPackage);
        }

        private List<EMDObjectRelation> GetAllObjectRelationsForEquipmentDefinitionAndEmploymentByEmploymentPackage(string eqdeGuid, string emplGuid)
        {
            return this.GetAllObjectRelationsForEquipmentDefinitionAndEmploymentByObjectRelationType(eqdeGuid, ObjectRelationTypeList.EquipmentByEmploymentPackage, emplGuid);
        }

        private List<EMDObjectRelation> GetAllObjectRelationsForEquipmentDefinitionByObjectRelationType(string eqdeGuid, string objectRelationType)
        {
            List<EMDObjectRelation> allOBREEquipmentLinks = new List<EMDObjectRelation>();

            ObjectRelationTypeHandler obreTypeHandler = new ObjectRelationTypeHandler(this.Transaction);
            ObjectRelationHandler obreHandler = new ObjectRelationHandler(this.Transaction);

            // get all equipments linked to employment (all obre even ones with removed status)
            string ortyGuid = obreTypeHandler.GetGuidForRelationName(objectRelationType);
            string where = string.Format("ORTYGuid = \"{0}\" && Object2 = \"{1}\"", ortyGuid, eqdeGuid);
            allOBREEquipmentLinks = obreHandler.GetObjects<EMDObjectRelation, ObjectRelation>(where).Cast<EMDObjectRelation>().ToList();

            return allOBREEquipmentLinks;
        }

        private List<EMDObjectRelation> GetAllObjectRelationsForEquipmentDefinitionAndEmploymentByObjectRelationType(string eqdeGuid, string objectRelationType, string emplGuid)
        {
            List<EMDObjectRelation> allOBREEquipmentLinks = new List<EMDObjectRelation>();

            ObjectRelationTypeHandler obreTypeHandler = new ObjectRelationTypeHandler(this.Transaction);
            ObjectRelationHandler obreHandler = new ObjectRelationHandler(this.Transaction);

            // get all equipments linked to employment (all obre even ones with removed status)
            string ortyGuid = obreTypeHandler.GetGuidForRelationName(objectRelationType);
            string where = string.Format("ORTYGuid = \"{0}\" && Object2 = \"{1}\" && Object1 = \"{2}\"", ortyGuid, eqdeGuid, emplGuid);
            allOBREEquipmentLinks = obreHandler.GetObjects<EMDObjectRelation, ObjectRelation>(where).Cast<EMDObjectRelation>().ToList();

            return allOBREEquipmentLinks;
        }

        public List<EMDObjectRelation> GetAllEQsLinkedToEmploymentsForEquipmentDefinition(string eqdeGuid)
        {
            //string employmentKey, eqdeKey;
            //List<EMDEquipmentInstance> eqs4employment = new List<EMDEquipmentInstance>();
            List<EMDObjectRelation> eqs = new List<EMDObjectRelation>();
            //ObjectRelationTypeHandler objectRelationTypeHandler = new ObjectRelationTypeHandler(this.Transaction);
            //ObjectRelationHandler objectRelationHandler = new ObjectRelationHandler(this.Transaction);

            // 2)
            //employmentKey = objectRelationHandler.FindObjectField(ObjectRelationTypeList.EquipmentByEmploymentPackage, new EMDEmployment().Prefix);
            //eqdeKey = objectRelationHandler.FindObjectField(ObjectRelationTypeList.EquipmentByEmploymentPackage, new EMDEquipmentDefinition().Prefix);

            // 2a) get key for OBRE employment by empployment package, 40,11
            //eqs4employment.AddRange(this.getEQInstancesForEmploymentPackage(employmentGuid, employmentKey, eqdeKey));

            // 2b) get key for OBRE employment by normal package
            eqs.AddRange(this.GetAllObjectRelationsForEquipmentDefinitionByPackage(eqdeGuid));
            eqs.AddRange(this.GetAllObjectRelationsForEquipmentDefinitionByEmploymentPackage(eqdeGuid));

            return eqs;
        }

        public List<EMDObjectRelation> GetAllEQsLinkedToEmploymentForEquipmentDefinition(string eqdeGuid, string emplGuid)
        {
            //string employmentKey, eqdeKey;
            //List<EMDEquipmentInstance> eqs4employment = new List<EMDEquipmentInstance>();
            List<EMDObjectRelation> eqs = new List<EMDObjectRelation>();
            //ObjectRelationTypeHandler objectRelationTypeHandler = new ObjectRelationTypeHandler(this.Transaction);
            //ObjectRelationHandler objectRelationHandler = new ObjectRelationHandler(this.Transaction);

            // 2)
            //employmentKey = objectRelationHandler.FindObjectField(ObjectRelationTypeList.EquipmentByEmploymentPackage, new EMDEmployment().Prefix);
            //eqdeKey = objectRelationHandler.FindObjectField(ObjectRelationTypeList.EquipmentByEmploymentPackage, new EMDEquipmentDefinition().Prefix);

            // 2a) get key for OBRE employment by empployment package, 40,11
            //eqs4employment.AddRange(this.getEQInstancesForEmploymentPackage(employmentGuid, employmentKey, eqdeKey));

            // 2b) get key for OBRE employment by normal package
            eqs.AddRange(this.GetAllObjectRelationsForEquipmentDefinitionAndEmploymentByPackage(eqdeGuid, emplGuid));
            eqs.AddRange(this.GetAllObjectRelationsForEquipmentDefinitionAndEmploymentByEmploymentPackage(eqdeGuid, emplGuid));

            return eqs;
        }

    }
}

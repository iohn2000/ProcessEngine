using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EquipmentDefinitionHandler : EMDObjectHandler
    {
        public EquipmentDefinitionHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public EquipmentDefinitionHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public EquipmentDefinitionHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public EquipmentDefinitionHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }


        public override Type GetDBObjectType()
        {
            return new DB.EquipmentDefinition().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            DB.EquipmentDefinition eqde = (DB.EquipmentDefinition)dbObject;
            EMDEquipmentDefinition emdObject = new EMDEquipmentDefinition(eqde.Guid, eqde.Created, eqde.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        [Obsolete("Don't use this method because it returns all Equipments inclusive all historized items")]
        public List<string> GetAllEquipmentGuids()
        {
            IQueryable<string> temp = (from item in transaction.dbContext.EquipmentDefinition select item.Guid);
            if (temp != null)
                return temp.ToList();
            else
                return new List<string>();
        }

        public List<EMDEquipmentDefinition> GetAllEquipmentDefinitions_DIRECT()
        {
            SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["EMD_Direct"].ConnectionString);
            sqlConnection.Open();
            string allEqdeQuery = @"SELECT 
[Guid]
,[HistoryGuid]
,[Q_ID]
,[Name]
,[Description]
,[ValidFrom]
,[ValidTo]
,[Created]
,[Modified]
,[Config]
,[ActiveFrom]
,[ActiveTo]
,[Guid_ModifiedBy]
,[ModifyComment]
,[ClientReferenceIDForPrice]
,[ClientReferenceSystemForPrice]
,[WorkingInstructions]
,[DescriptionLong]
FROM EquipmentDefinition where guid = historyguid";

            List<EMDEquipmentDefinition> allEqdes = new List<EMDEquipmentDefinition>();
            try
            {
                SqlCommand sqlCommand = new SqlCommand(allEqdeQuery, sqlConnection);
                var reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    EMDEquipmentDefinition e = new EMDEquipmentDefinition();
                    e.Guid = SafeGetString(reader, 0);
                    e.HistoryGuid = SafeGetString(reader, 1);
                    e.Q_ID = reader.GetInt32(2);
                    e.Name = SafeGetString(reader, 3);
                    e.Description = SafeGetString(reader, 4);
                    e.ValidFrom = reader.GetDateTime(5);
                    e.ValidTo = reader.GetDateTime(6);
                    e.Created = reader.GetDateTime(7);
                    e.Modified = SafeGetDate(reader, 8);
                    e.Config = SafeGetString(reader, 9);
                    e.ActiveFrom = reader.GetDateTime(10);
                    e.ActiveTo = reader.GetDateTime(11);
                    e.Guid_ModifiedBy = SafeGetString(reader, 12);
                    e.ModifyComment = SafeGetString(reader, 13);
                    e.ClientReferenceIDForPrice = SafeGetString(reader, 14);
                    e.ClientReferenceSystemForPrice = SafeGetInt(reader, 15);
                    e.WorkingInstructions = SafeGetString(reader, 16);
                    e.DescriptionLong = SafeGetString(reader, 17);

                    allEqdes.Add(e);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, ex);
            }
            sqlConnection.Close();

            return allEqdes;
        }

        [Obsolete]
        /// <summary>
        /// @@ function
        /// get setting from eqdeGuid
        /// </summary>
        /// <param name="eqdeGuid"></param>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public string GetEqdeXmlSettingByNameAttribute(string eqdeGuid, string elementName)
        {

            // first get eqde
            EMDEquipmentDefinition eqde = (EMDEquipmentDefinition)this.GetObject<EMDEquipmentDefinition>(eqdeGuid);
            if (eqde != null)
            {
                var cfg = eqde.GetEquipmentDefinitionConfig();

                if (cfg != null)
                {
                    object cfgValue = ReflectionHelper.GetPropValue(cfg, elementName);

                    if (cfgValue != null)
                    {
                        return cfgValue.ToString();
                    }
                    else
                    {
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "cannot find eqde xml setting '" + elementName + "' for eqdeGuid '" + eqdeGuid + "'");
                    }
                }
                else
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "cannot load config for eqde Guid '" + eqdeGuid + "'");
                }
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "cannot find eqde Guid '" + eqdeGuid + "'");
            }
        }

        private string SafeGetString(SqlDataReader reader, int colIndex, string defaultValue = null)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return defaultValue;
        }
        private int? SafeGetInt(SqlDataReader reader, int colIndex, int? defaultValue = null)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetInt32(colIndex);
            return defaultValue;
        }

        private DateTime? SafeGetDate(SqlDataReader reader, int colIndex, DateTime? defaultValue = null)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetDateTime(colIndex);
            return defaultValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.Framework;

using Kapsch.IS.Util.ReflectionHelper;
using System.IO;
using Kapsch.IS.Util.Logging;
using System.Data.SqlClient;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class FilterRuleHandler : EMDObjectHandler, IFilterRuleHandler
    {
        public FilterRuleHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public FilterRuleHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public FilterRuleHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public FilterRuleHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new FilterRule().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            FilterRule firu = (FilterRule)dbObject;
            EMDFilterRule emdObject = new EMDFilterRule(firu.Guid, firu.Created, firu.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        private string SafeGetString(SqlDataReader reader, int colIndex, string defaultValue = null)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return defaultValue;
        }

        //From IFilterRuleHandler
        public List<EMDFilterRule> ReadRulesFromDatase(string objectGuid)
        {
            List<EMDFilterRule> rules = new List<EMDFilterRule>();


            SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["EMD_Direct"].ConnectionString);
            sqlConnection.Open();


            try
            {
                //                                                0	   1            2          3             4                5         6               7             8        9      10        
                SqlCommand sqlCommand = new SqlCommand("SELECT [Guid],[HistoryGuid],[Obj_Guid],[FilterOrder],[FilterAction], [E_Guid],[EnteIsInherited],[L_Guid],[ET_Guid],[ACC_Guid],[USTY_Enum] FROM [FilterRule] where guid=historyguid and ValidFrom < getdate() and ValidTo > getdate() and ActiveFrom < getdate() and ActiveTo > getdate() and Obj_Guid='" + objectGuid + "'", sqlConnection);
                var reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    EMDFilterRule e = new EMDFilterRule();

                    e.Guid = reader.GetString(0);
                    e.HistoryGuid = reader.GetString(1);
                    e.Obj_Guid = this.SafeGetString(reader, 2);
                    e.FilterOrder = reader.GetInt32(3);
                    e.FilterAction = this.SafeGetString(reader, 4);
                    e.E_Guid = this.SafeGetString(reader, 5);
                    e.EnteIsInherited = reader.GetBoolean(6);
                    e.L_Guid = this.SafeGetString(reader, 7);
                    e.ET_Guid = this.SafeGetString(reader, 8);
                    e.ACC_Guid = this.SafeGetString(reader, 9);
                    e.USTY_Enum = this.SafeGetString(reader, 10);


                    rules.Add(e);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, ex);
            }

            finally
            {
                try
                {
                    sqlConnection.Close();
                }
                catch { }
            }

            //old code

            //List<IEMDObject<EMDFilterRule>> xx = this.GetRuleSetByObjGuid(objectGuid);
            //foreach (IEMDObject<EMDFilterRule> item in xx)
            //    rules.Add((EMDFilterRule)item);

            return rules;
        }

        //From IFilterRuleHandler
        public List<IEMDObject<EMDFilterRule>> GetRuleSetByObjGuid(String objectGuid)
        {
            string whereClause = string.Format("Obj_Guid = \"{0}\"", objectGuid);
            List<IEMDObject<EMDFilterRule>> ruleSet = (List<IEMDObject<EMDFilterRule>>)GetObjects<EMDFilterRule, FilterRule>(whereClause, null).ToList();
            return ruleSet;
        }

        /// <summary>
        /// throws baseexception
        /// </summary>
        /// <param name="objectIDs"></param>
        /// <returns></returns>
        public List<EMDFilterRule> ReadMultipleRulesFromDatabase(List<string> objectIDs)
        {
            List<EMDFilterRule> rules = new List<EMDFilterRule>();
            EMD_Entities emdEntities = new EMD_Entities();
            DateTime now = DateTime.Now;

            if (objectIDs.Count < 1)
            {
                return rules;
            }

            try
            {
                (from ru in emdEntities.FilterRule
                 where objectIDs.Contains(ru.Obj_Guid) &&
                       ru.ActiveFrom <= now && ru.ValidFrom <= now && ru.ActiveTo >= now && ru.ValidTo >= now
                 select ru).ToList().ForEach(item =>
                 {
                     EMDFilterRule eFiru = new EMDFilterRule(item.Guid, item.Created, item.Modified);
                     ReflectionHelper.CopyProperties(ref item, ref eFiru);
                     rules.Add(eFiru);
                 });
            }
            catch (Exception ex)
            {
                StringWriter tw = new StringWriter();
                ObjectDumper.Write(objectIDs, 5, tw);
                string msg = "error reading multiple filter rules" + tw.ToString();
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg, ex);
            }


            return rules;
        }
    }
}

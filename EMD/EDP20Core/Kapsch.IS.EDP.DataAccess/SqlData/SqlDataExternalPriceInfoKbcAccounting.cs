using Kapsch.IS.EDP.DataAccess.Entities;
using Kapsch.IS.EDP.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.DataAccess.SqlData
{
    public class SqlDataExternalPriceInfoKbcAccounting : Kapsch.IS.Util.Sql.SqlData.BaseDataAccess, IExternalPriceInfo
    {
        public SqlDataExternalPriceInfoKbcAccounting(String connectionStringName) : base(connectionStringName)
        {

        }


        public ExternalPriceInfo GetPrice(string idReference)
        {
            ExternalPriceInfo externalPriceInfo = null;

            String queryString = "SELECT * FROM [common].[v_PriceList] where AccountingItemID = @idReference";

            try
            {
                sqlConnection.Open();
                sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = queryString;

                sqlCommand.Parameters.Add(new SqlParameter("idReference", idReference));

                sqlDataReader = sqlCommand.ExecuteReader();

                while (sqlDataReader.HasRows && sqlDataReader.Read())
                {
                    new ExternalPriceInfo();
                    externalPriceInfo = GetExternalPriceInfo(sqlDataReader);
                }
            }
            catch (Exception ex)
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }

            if (sqlConnection != null)
            {
                sqlConnection.Close();
            }

            return externalPriceInfo;
        }

        public List<ExternalPriceInfo> GetPrices()
        {
            List<ExternalPriceInfo> externalPriceInfo = new List<ExternalPriceInfo>();

            String queryString = "SELECT * FROM [common].[v_PriceList]";

            try
            {
                sqlConnection.Open();
                sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = queryString;

                //sqlCommand.Parameters.Add(new SqlParameter("userId", userId));

                sqlDataReader = sqlCommand.ExecuteReader();

                while (sqlDataReader.HasRows && sqlDataReader.Read())
                {
                    ExternalPriceInfo user = GetExternalPriceInfo(sqlDataReader);
                    externalPriceInfo.Add(user);
                }
            }
            catch (Exception ex)
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }

            if (sqlConnection != null)
            {
                sqlConnection.Close();
            }

            return externalPriceInfo;
        }

        private static ExternalPriceInfo GetExternalPriceInfo(SqlDataReader sqlDataReader)
        {
            ExternalPriceInfo priceInfo = new ExternalPriceInfo()
            {
                IdClientReference = Convert.ToString(sqlDataReader["AccountingItemID"]),
                Name = Convert.ToString(sqlDataReader["AccountingItem"]),
                Price = Convert.ToDecimal(sqlDataReader["Price Current FY"])
            };

            return priceInfo;
        }
    }
}

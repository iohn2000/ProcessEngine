using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Kapsch.IS.EDP.Core.Report
{
    /// <summary>
    /// BaseClass for EDPReports which are derived from this.
    /// </summary>
    /// <typeparam name="T">Class which derives from this baseclass</typeparam
    /// <remark>
    /// Implementation example:
    /// <code>
    /// 
    /// </code>
    /// </remark>
    public abstract class EDPReport<T> where T : EDPReport<T>
    {
        #region Properties;
        public List<string> Fields;

        public string Errors;

        public String ViewName { get; private set; } = "UNDEFINED";

        public DataTable QueryResult { get; private set; }

        public String QueryString { get; private set; } = "UNDEFINED";
        #endregion Properties

        #region methods
        public virtual T Query()
        {
            if (ViewName == "UNDEFINED")
                this.SetViewName();

            if (QueryString == "UNDEFINED")
            {
                this.SetQueryString(whereClause: null);
            }

            EMD_Entities ef = new EMD_Entities();

            using (SqlConnection sqlConnection = new SqlConnection(ef.Database.Connection.ConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(this.QueryString, sqlConnection))
                {
                    this.QueryResult = new DataTable();
                    this.QueryResult.Load(sqlCommand.ExecuteReader());
                }
            }

            return (T)this;
        }

        internal virtual T SetViewName(string viewName = null)
        {
            if (viewName == null)
            {
                this.ViewName = "REP_" + typeof(T).Name;
            }
            else if (viewName.ToUpper().StartsWith("REP_"))
            {
                this.ViewName = viewName;
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "ViewName for EDP Reports must start with 'REP_', the given name was: " + viewName);
            }
            return (T)this;
        }

        internal virtual T SetQueryString(string whereClause = null)
        {
            if (this.QueryString == "UNDEFINED")
            {
                this.QueryString = "SELECT * FROM {0} ";
            }
            if (this.ViewName == "UNDEFINED")
            {
                // do nothing else
                return (T)this;
            }
            this.QueryString = String.Format(this.QueryString, this.ViewName);

            if (whereClause != null)
            {
                this.QueryString = String.Format(this.QueryString + "WHERE {0}",whereClause);
            }
            return (T)this;
        }
        #endregion methods
    }

}
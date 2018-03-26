using System;

using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.EDP.Core.DB
{
    public class EMDDB_Helper
    {
        public const string CONNECTION_STRING_EMD_SETTING = "EMD_Direct";

        #region Documentation
        /// <summary>   Gets emd connection string. </summary>
        ///
        /// <remarks>   Stagl, 03.12.2014. </remarks>
        ///
        /// <exception cref="BaseException">    Thrown when a Base error condition occurs. </exception>
        ///
        /// <returns>   The emd connection string. </returns>
        #endregion

        public static String GetEMDConnectionString()
        {
            String EMDConnectionString = "";
            try
            {
                EMDConnectionString = Util.Configuration.ConfigUtil.getConnectionString(EMDDB_Helper.CONNECTION_STRING_EMD_SETTING);
            }
            catch (BaseException ex)
            {
                throw ex;
            }
            return EMDConnectionString;
        }
    }
}

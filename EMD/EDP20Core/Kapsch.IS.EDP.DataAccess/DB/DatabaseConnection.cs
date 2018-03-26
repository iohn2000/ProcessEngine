using Kapsch.IS.EDP.DataAccess.Mockup;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.DataAccess.DB
{
    public class DatabaseConnection : Util.Sql.DB.DbConnection
    {
        public static Interface.IExternalPriceInfo ExternalPriceInfoKbcAccounting
        {
            get
            {
                Interface.IExternalPriceInfo externalPriceInfo = null;

                bool hasMockup = false;
                object isMockup = ConfigurationManager.AppSettings["DataAccess.IsMockup"];
                if (isMockup != null)
                {
                    bool.TryParse(isMockup.ToString(), out hasMockup);
                }

                if (hasMockup)
                {
                    externalPriceInfo = new MockupExternalPriceInfo();
                }
                else
                {
                    externalPriceInfo = new SqlData.SqlDataExternalPriceInfoKbcAccounting("EMD.DataAccess.Price.KBCAccounting");
                }

                return externalPriceInfo;
            }
        }
    }
}

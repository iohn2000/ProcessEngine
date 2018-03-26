using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework
{
    public class EDPSecurityErrorCodeHandler
    {
        public static int E_OK = 0;

        public static int E_DB_NOT_FOUND = -1000;

        private static Dictionary<int, String> ErrorCodes = new Dictionary<int, String>
        {
             { E_OK, "ok"}
        };


        public static String GetMessage(int errorcode)
        {
            ILookup<int, String> lookup = ErrorCodes.ToLookup(x => x.Key, x => x.Value);
            String message = "";

            try
            {
                message = lookup[errorcode].Single();
            }
            catch (Exception)
            {
                message = "no errordescription found for errorcode: " + errorcode.ToString();
            }

            return message;
        }
    }
}

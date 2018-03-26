using Kapsch.IS.EDP.EDPExports.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.EDPExports
{
    public class StaticInterface
    {
        public static string EDPDataForIT_ItemExists(string UserId)
        {
            EDPDataForITHandler handler = new EDPDataForITHandler("EMD_Export");
            return handler.ItemExists(UserId).ToString();
        }
    }
}

using System.Web;
using System.Web.Mvc;

namespace Kapsch.IS.EMD.EMD20Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.HelperExtensions
{
    public class HtmlStringHelper
    {
        public static string StripHTML(string htmlText)
        {
            // Decode first to HTML
            htmlText = HttpUtility.HtmlDecode(htmlText);
            if (!string.IsNullOrEmpty(htmlText))
            {
                var reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                return reg.Replace(htmlText, "");
            }
            return string.Empty;
        }
    }
}
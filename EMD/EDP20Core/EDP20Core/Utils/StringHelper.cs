using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Utils
{
    public class StringHelper
    {
        /// <summary>
        /// remove line breaks from a given string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ReplaceNewlines(string text)
        {
            return text.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
        }
    }
}

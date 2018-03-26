using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kapsch.IS.ProcessEngine
{
    public interface IActivityValidator
    {
        string Validate(string activityXml);
        string Validate(XElement activity);
    }
}

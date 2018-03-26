using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Utils
{
    /// <summary>
    /// Class for the result of a <seealso cref="Monitoring"/>. The class holds the status of the monitoring and the error if an error occurs
    /// </summary>
    public class MonitoringResult
    {
        public EnumMonitoringStatus Status { get; set; }
        public string Error { get; set; }

        //Constructor
        public MonitoringResult()
        {
            this.Status = EnumMonitoringStatus.NOTSET;
            this.Error = string.Empty;
        }
    }
}

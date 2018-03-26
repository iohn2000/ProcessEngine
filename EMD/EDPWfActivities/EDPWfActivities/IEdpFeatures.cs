using Kapsch.IS.ProcessEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.WFActivity
{
    public interface IEdpFeatures
    {
         Variable GetProcessedActivityVariable(EngineContext engineContext, string propertyName);
    }
}

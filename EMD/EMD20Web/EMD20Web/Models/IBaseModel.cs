using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public interface IBaseModel
    {
        void InitializeSecurity(SecurityUser securityUser);
    }
}

using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface IBaseManager
    {
        CoreTransaction Transaction { get; set; }

        string Guid_ModifiedBy { get; set; }

        string ModifyComment { get; set; }
    }
}

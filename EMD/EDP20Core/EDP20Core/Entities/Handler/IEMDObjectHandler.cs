using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Basic interface for object-handler.
    /// </summary>
    public interface IEMDObjectHandler
    {
        /// <summary>
        /// The database-transaction used by this handler.
        /// </summary>
        CoreTransaction Transaction { get; set; }

        /// <summary>
        /// The GUID of the person ("PERS") which 'uses' the handler to modify database-objects.
        /// </summary>
        string Guid_ModifiedBy { get; set; }

        /// <summary>
        /// Modification comment for all changed database-objects.
        /// </summary>
        string ModifyComment { get; set; }
    }
}

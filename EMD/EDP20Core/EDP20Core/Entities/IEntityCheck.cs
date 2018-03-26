using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Types of IEntityCheck are allowed to access EntityCheckManager and to access the underlying data-table
    /// </summary>
    public interface IEntityCheck
    {
        int CheckIntervalInDays
        {
            get;
            set;
        }

        int ReminderIntervalInDays
        {
            get;
            set;
        }

        /// <summary>
        /// returns the prefix (4 letters) for the underyling EMD Entity
        /// </summary>
        /// <returns></returns>
        string GetPrefix();

        /// <summary>
        /// returns the GUID (includes the prefix) for the underlying EMD Entity
        /// </summary>
        /// <returns></returns>
        string GetGuid();

        /// <summary>
        /// Necessary to know by with type of action the check is managed
        /// </summary>
        /// <returns></returns>
        EnumManagedByType GetManagedBy();
    }
}

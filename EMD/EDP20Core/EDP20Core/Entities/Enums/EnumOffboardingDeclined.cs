using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.Enums
{
    /// <summary>
    /// A list of reason why not to start an offboarding.
    /// </summary>
    /// <remarks>
    /// The job starting offboardings automatically, based on ExitDates and Status needs to know of it is allowed to start an offboarding.
    /// An empty set means no problems offboading.
    /// </remarks>
    public enum EnumOffboardingDeclined
    {
        /// <summary>
        /// MainEmployment must not be offboarded, if other employments exist
        /// </summary>
        OtherEmploymentsExist = 1,
        NoOrgUnitExists = 2,
        MoreThanOneOrgUnitExists = 4
    }
}

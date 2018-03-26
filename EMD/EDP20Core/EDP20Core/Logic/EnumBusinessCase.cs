using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic
{
    public enum EnumBusinessCase
    {
        NotDefined = 0,
        Onboarding = 10,
        Offboarding = 20,
        OffboardingAuto = 21,
        Change = 30,
        EquipmentRequest = 40
    }
}

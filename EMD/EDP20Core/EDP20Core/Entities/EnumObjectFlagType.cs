using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public enum EnumObjectFlagType
    {
        // ---- Employment ----
        MainEmployment,
        UpdateDNA,
        UpdateAD,
        Visible,

        //-----EmploymentAccount----
        MainAccount,

        // --- Person ----
        VisiblePhone,
        PictureVisible,
        PictureVisibleAD,

        // --- Enterprise ---
        AdPictureEnterprise,
        HasEmployees,

        // --- EnterpriseLocation ---
        MainLocation,
        EFaxUpdate,
    }
}

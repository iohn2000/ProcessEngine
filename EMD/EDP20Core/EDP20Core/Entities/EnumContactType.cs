

using System.ComponentModel;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Entities
{
    public enum EnumContactType
    {
        [Description("E-Mail")]
        EMAIL = 1,
        [Description("Phone")]
        PHONE = 2,
        FAX = 3,
        [Description("Mobile Phone")]
        MOBILE = 4,
        [Description("eFax")]
        EFAX = 5,
        [Description("Direct 1")]
        DIRECTDIAL = 6,
        [Description("Room number")]
        ROOM = 7,
        AGENTKATCE = 8,
        MONITORKATCE = 9,
        ISMDKATCE = 10,
        SERVERKATCE = 11,
        [Description("Job Title")]
        JOBTITLE = 12,
        HIERARCHY = 13,
        [Description("Direct 2")]
        DIRECTDIAL2 = 14,
        DIRECTDIALKATCE = 15,
        LOCATIONPHONE = 16,
        LOCATIONFAX = 17,
        LOCATIONEMAIL = 18,
        [Description("Direct-eFax")]
        DIRECTEFAX = 19,
    }


}

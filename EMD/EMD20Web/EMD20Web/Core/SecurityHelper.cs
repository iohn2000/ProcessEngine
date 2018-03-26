using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Core
{
    public static class SecurityHelper
    {
        public const string NoPermissionText = "You do not have the required permission!";

        public static bool HasPermission(string userName, string permission)
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser(userName);
            SecurityUserParameterFlags flags = new SecurityUserParameterFlags(checkPlainPermisson: true);
            return secUser.hasPermission(permission, flags);
        }

    }
}
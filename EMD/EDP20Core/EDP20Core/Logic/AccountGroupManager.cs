using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using Kapsch.IS.EDP.Core.Entities.Enums;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.Core.Logic
{
    /// <summary>
    /// Manager for accountgroups and all related entites
    /// </summary>
    public class AccountGroupManager : BaseManager
    {
        internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Checks if the given employment is an active assistance of any costcenter
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <returns><seealso cref="bool"/></returns>
        public bool IsEmpoymentAssistence(string emplGuid)
        {
            AccountGroupHandler accGrpHandler = new AccountGroupHandler();
            GroupHandler grpHandler = new GroupHandler();
            GroupMemberHandler grpMemberHandler = new GroupMemberHandler();
            AccountHandler accHandler = new AccountHandler();

            string whereClause = string.Format("EP_Guid = \"{0}\"", emplGuid);

            try
            {
                int items = (from grpMemb in grpMemberHandler.GetObjects<EMDGroupMember, GroupMember>(whereClause).Cast<EMDGroupMember>()
                             join grp in grpHandler.GetObjects<EMDGroup, Group>().Cast<EMDGroup>() on grpMemb.G_Guid equals grp.Guid
                             join accGrp in accGrpHandler.GetObjects<EMDAccountGroup, AccountGroup>().Cast<EMDAccountGroup>() on grp.Guid equals accGrp.G_Guid
                             join acc in accHandler.GetObjects<EMDAccount, Account>().Cast<EMDAccount>() on accGrp.AC_Guid equals acc.Guid
                             select new { grpMemb, grp, accGrp, acc }).Count();

                if (items > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error in SecurityUser.IsEmpoymentAssistence: Could not execute query. Exception: {0}, StackTrace: {1}", ex.Message, ex.StackTrace));
                return false;
            }
        }

        /// <summary>
        /// Gets a list of all <seealso cref="EMDGroupMember"/> instances the employment has
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <returns></returns>
        public List<EMDGroupMember> GetAllAssistenceInstancesForEmployment(string emplGuid)
        {
            GroupMemberHandler grpMemberHandler = new GroupMemberHandler();
            string whereClause = string.Format("EP_Guid = \"{0}\"", emplGuid);
            List<EMDGroupMember> listMembers = grpMemberHandler.GetObjects<EMDGroupMember, GroupMember>(whereClause).Cast<EMDGroupMember>().ToList();
            return listMembers;
        }

        /// <summary>
        /// Deletes all <seealso cref="EMDGroupMember"/> instances the employment has e.g. for offboarding
        /// </summary>
        /// <param name="emplGuid"></param>
        public void RemoveEmploymentFromAllAssistenceGroups(string emplGuid)
        {
            GroupMemberHandler grpMemberHandler = new GroupMemberHandler();
            List<EMDGroupMember> listMembers =GetAllAssistenceInstancesForEmployment(emplGuid);
            listMembers.ForEach(item => {
                grpMemberHandler.DeleteObject(item);
            });
        }
    }
}

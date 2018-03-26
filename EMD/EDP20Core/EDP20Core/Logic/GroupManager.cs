using Kapsch.IS.EDP.Core.Logic.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.Util.Logging;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class GroupManager : BaseManager, IGroupManager
    {
        public List<EMDGroup> GetGroups(string guidEnterprise)
        {
            EMD_Entities emdDataContext = new EMD_Entities();

            List<EMDGroup> groups = new List<EMDGroup>();
            try
            {

                GroupHandler emplHandler = new GroupHandler(this.Transaction);
                      

                var foundItems = (from grp in emdDataContext.Group
                                  join accgrp in emdDataContext.AccountGroup on grp.Guid equals accgrp.G_Guid
                                  join acc in emdDataContext.Account on accgrp.AC_Guid equals acc.Guid
                                  join emplAcc in emdDataContext.EmploymentAccount on acc.Guid equals emplAcc.AC_Guid
                                  where acc.E_Guid == guidEnterprise

                                  && grp.ActiveTo > DateTime.Now && grp.ValidTo > DateTime.Now && grp.ValidFrom < DateTime.Now && grp.ActiveFrom < DateTime.Now
                                  && accgrp.ActiveTo > DateTime.Now && accgrp.ValidTo > DateTime.Now && accgrp.ValidFrom < DateTime.Now && accgrp.ActiveFrom < DateTime.Now
                                  && acc.ActiveTo > DateTime.Now && acc.ValidTo > DateTime.Now && acc.ValidFrom < DateTime.Now && acc.ActiveFrom < DateTime.Now
                                  && emplAcc.ActiveTo > DateTime.Now && emplAcc.ValidTo > DateTime.Now && emplAcc.ValidFrom < DateTime.Now && emplAcc.ActiveFrom < DateTime.Now
                                  select grp).Distinct().ToList();


                foreach (var item in foundItems)
                {
                    EMDGroup emdGroup = new EMDGroup();
                    Group refItem = (Group)item;
                    ReflectionHelper.CopyProperties<Group, EMDGroup>(ref refItem, ref emdGroup);

                    groups.Add(emdGroup);
                }
            }
            catch (BaseException ex)
            {
                IISLogger logger = ISLogger.GetLogger("GroupManager - GetGroups");
                logger.Error("function GetGroups - Error :" + ex.Message + " " + ex.StackTrace, ex);
            }
            catch (Exception ex)
            {
                IISLogger logger = ISLogger.GetLogger("GroupManager - GetGroups");
                logger.Error("function GetGroups - Error :" + ex.Message + " " + ex.StackTrace, ex);
                throw;
            }
            return groups;
        }

        public List<EMDGroup> GetCostCenterGroups()
        {
            EMD_Entities emdDataContext = new EMD_Entities();

            List<EMDGroup> groups = new List<EMDGroup>();
            try
            {

                GroupHandler emplHandler = new GroupHandler(this.Transaction);


                var foundItems = (from grp in emdDataContext.Group
                                  join accgrp in emdDataContext.AccountGroup on grp.Guid equals accgrp.G_Guid
                                  join acc in emdDataContext.Account on accgrp.AC_Guid equals acc.Guid
                                  //  join emplAcc in emdDataContext.EmploymentAccount on acc.Guid equals emplAcc.AC_Guid


                                  where grp.ActiveTo > DateTime.Now && grp.ValidTo > DateTime.Now && grp.ValidFrom < DateTime.Now && grp.ActiveFrom < DateTime.Now
                                  && accgrp.ActiveTo > DateTime.Now && accgrp.ValidTo > DateTime.Now && accgrp.ValidFrom < DateTime.Now && accgrp.ActiveFrom < DateTime.Now
                                  && acc.ActiveTo > DateTime.Now && acc.ValidTo > DateTime.Now && acc.ValidFrom < DateTime.Now && acc.ActiveFrom < DateTime.Now
                                  //   && emplAcc.ActiveTo > DateTime.Now && emplAcc.ValidTo > DateTime.Now && emplAcc.ValidFrom < DateTime.Now && emplAcc.ActiveFrom < DateTime.Now
                                  select grp).Distinct().ToList();


                foreach (var item in foundItems)
                {
                    EMDGroup emdGroup = new EMDGroup();
                    Group refItem = (Group)item;
                    ReflectionHelper.CopyProperties<Group, EMDGroup>(ref refItem, ref emdGroup);

                    groups.Add(emdGroup);
                }
            }
            catch (BaseException ex)
            {
                IISLogger logger = ISLogger.GetLogger("GroupManager - GetGroups");
                logger.Error("function GetGroups - Error :" + ex.Message + " " + ex.StackTrace, ex);
            }
            catch (Exception ex)
            {
                IISLogger logger = ISLogger.GetLogger("GroupManager - GetGroups");
                logger.Error("function GetGroups - Error :" + ex.Message + " " + ex.StackTrace, ex);
                throw;
            }
            return groups;
        }


        public List<EMDGroup> GetAssignedCostCenterGroups(string guidCostCenter)
        {
            EMD_Entities emdDataContext = new EMD_Entities();

            List<EMDGroup> groups = new List<EMDGroup>();
            try
            {

                GroupHandler emplHandler = new GroupHandler(this.Transaction);


                var foundItems = (from grp in emdDataContext.Group
                                  join accgrp in emdDataContext.AccountGroup on grp.Guid equals accgrp.G_Guid
                                  join acc in emdDataContext.Account on accgrp.AC_Guid equals acc.Guid
                                //  join emplAcc in emdDataContext.EmploymentAccount on acc.Guid equals emplAcc.AC_Guid
                                  where acc.Guid == guidCostCenter

                                  && grp.ActiveTo > DateTime.Now && grp.ValidTo > DateTime.Now && grp.ValidFrom < DateTime.Now && grp.ActiveFrom < DateTime.Now
                                  && accgrp.ActiveTo > DateTime.Now && accgrp.ValidTo > DateTime.Now && accgrp.ValidFrom < DateTime.Now && accgrp.ActiveFrom < DateTime.Now
                                  && acc.ActiveTo > DateTime.Now && acc.ValidTo > DateTime.Now && acc.ValidFrom < DateTime.Now && acc.ActiveFrom < DateTime.Now
                               //   && emplAcc.ActiveTo > DateTime.Now && emplAcc.ValidTo > DateTime.Now && emplAcc.ValidFrom < DateTime.Now && emplAcc.ActiveFrom < DateTime.Now
                                  select grp).Distinct().ToList();


                foreach (var item in foundItems)
                {
                    EMDGroup emdGroup = new EMDGroup();
                    Group refItem = (Group)item;
                    ReflectionHelper.CopyProperties<Group, EMDGroup>(ref refItem, ref emdGroup);

                    groups.Add(emdGroup);
                }
            }
            catch (BaseException ex)
            {
                IISLogger logger = ISLogger.GetLogger("GroupManager - GetGroups");
                logger.Error("function GetGroups - Error :" + ex.Message + " " + ex.StackTrace, ex);
            }
            catch (Exception ex)
            {
                IISLogger logger = ISLogger.GetLogger("GroupManager - GetGroups");
                logger.Error("function GetGroups - Error :" + ex.Message + " " + ex.StackTrace, ex);
                throw;
            }
            return groups;
        }
    }
}

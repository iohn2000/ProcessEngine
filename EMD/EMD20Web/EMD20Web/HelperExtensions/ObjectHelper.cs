using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.HelperExtensions
{
    public class ObjectHelper
    {
        static IISLogger LOGGER = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetTypeName(string prefix)
        {
            return GetTypeName(EntityPrefix.Instance.GetTypeFromPrefix(prefix));
        }


        public static string GetTypeName(Type type)
        {
            string typeName = type.Name;

            if (type == typeof(EMDEquipmentDefinition))
            {
                typeName = "Equipment Definition";
            }

            if (type == typeof(EMDEmployment))
            {
                typeName = "Employment";
            }

            if (type == typeof(EMDEnterpriseLocation))
            {
                typeName = "Enterprise Location";
            }

            return typeName;
        }

        public static RuleFilterModel GetRuleFilterModel(string guid)
        {
            FilterRuleHandler filterRuleHandler = new FilterRuleHandler();
            List<IEMDObject<EMDFilterRule>> filterRules = filterRuleHandler.GetRuleSetByObjGuid(guid);

            RuleFilterModel ruleFilterModel = new RuleFilterModel();
            foreach (EMDFilterRule fr in filterRules)
            {
                if (fr.FilterOrder == 0)
                {
                    if (fr.E_Guid != null)
                        ruleFilterModel.EnterpriseInvertFlag = GetBaseFilterActionFlagFromString(fr.E_Guid);

                    if (fr.L_Guid != null)
                        ruleFilterModel.LocationInvertFlag = GetBaseFilterActionFlagFromString(fr.L_Guid);

                    if (fr.ET_Guid != null)
                        ruleFilterModel.EmploymentTypeInvertFlag = GetBaseFilterActionFlagFromString(fr.ET_Guid);

                    if (fr.ACC_Guid != null)
                        ruleFilterModel.AccountInvertFlag = GetBaseFilterActionFlagFromString(fr.ACC_Guid);

                    if (fr.USTY_Enum != null)
                        ruleFilterModel.UserTypeInvertFlag = GetBaseFilterActionFlagFromString(fr.USTY_Enum);
                }
                if (fr.E_Guid != null && fr.FilterOrder > 0)
                {
                    ruleFilterModel.Enterprises.Add(fr.E_Guid);
                }
                if (fr.L_Guid != null && fr.FilterOrder > 0)
                {
                    ruleFilterModel.Locations.Add(fr.L_Guid);
                }
                if (fr.ET_Guid != null && fr.FilterOrder > 0)
                {
                    ruleFilterModel.EmploymentTypes.Add(fr.ET_Guid);
                }
                if (fr.ACC_Guid != null && fr.FilterOrder > 0)
                {
                    ruleFilterModel.Accounts.Add(fr.ACC_Guid);
                }
                if (fr.USTY_Enum != null && fr.FilterOrder > 0)
                {
                    ruleFilterModel.UserTypes.Add(fr.USTY_Enum);
                }
                ruleFilterModel.EnteIsNotInherited = !fr.EnteIsInherited;
            }

            return ruleFilterModel;
        }

        private static bool GetBaseFilterActionFlagFromString(string baseFilterAction)
        {
            if (baseFilterAction == BaseFilterAction.ALLOWALL)
                return true;
            else if (baseFilterAction == BaseFilterAction.DENYALL)
                return false;
            else
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "The BaseFilterAction was expected to be true or false but was: " + baseFilterAction);
        }


        public static void CreateOrUpdateFilterRules(RuleFilterModel ruleFilterModel, string guid)
        {
            List<FilterRuleSubSetForCriteria> subSets = new List<FilterRuleSubSetForCriteria>();
            if (ruleFilterModel.Enterprises != null && ruleFilterModel.Enterprises.Count > 0)
            {
                subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.Company, GetBaseFilterActionFromFlag(ruleFilterModel.EnterpriseInvertFlag), ruleFilterModel.Enterprises, !ruleFilterModel.EnteIsNotInherited));
            }

            if (ruleFilterModel.Locations != null && ruleFilterModel.Locations.Count > 0)
            {
                subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.Location, GetBaseFilterActionFromFlag(ruleFilterModel.LocationInvertFlag), ruleFilterModel.Locations));
            }

            if (ruleFilterModel.Accounts != null && ruleFilterModel.Accounts.Count > 0)
            {
                subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.CostCenter, GetBaseFilterActionFromFlag(ruleFilterModel.AccountInvertFlag), ruleFilterModel.Accounts));
            }

            if (ruleFilterModel.EmploymentTypes != null && ruleFilterModel.EmploymentTypes.Count > 0)
            {
                subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.EmploymentType, GetBaseFilterActionFromFlag(ruleFilterModel.EmploymentTypeInvertFlag), ruleFilterModel.EmploymentTypes));
            }

            if (ruleFilterModel.UserTypes != null && ruleFilterModel.UserTypes.Count > 0)
            {
                subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.UserType, GetBaseFilterActionFromFlag(ruleFilterModel.UserTypeInvertFlag), ruleFilterModel.UserTypes));
            }

            //TransactionHandler th = TransactionHandler.Instance;
            CoreTransaction ta = new CoreTransaction();

            try
            {
                ta.Begin();

                // 1b) BaseContainer ?


                if (subSets != null)
                {
                    // 2a) update filter for it
                    // delete old rules
                    FilterManager fm = new FilterManager(guid);
                    fm.DeleteFilterRule(ta);
                    // create new ones
                    fm.CreateFilterRule(guid, subSets, ta);
                }

                ta.Commit();
            }
            catch (Exception ex)
            {
                ta.Rollback();
                LOGGER.Error("Error CreateOrUpdateFilterRules. Transaction rolled back.", ex);
            }
        }

        private static String GetBaseFilterActionFromFlag(bool InvertFlag)
        {
            if (InvertFlag)
                return BaseFilterAction.ALLOWALL;
            else
                return BaseFilterAction.DENYALL;
        }

        public static string GetEnumDescription(EnumContactType value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
}
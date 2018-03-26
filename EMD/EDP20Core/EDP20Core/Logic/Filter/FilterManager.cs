using Ciloci.Flee;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Logic.Filter
{
    public class FilterManager
        : BaseManager
    {
        protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public const string CONST_ALLOWALL = "allowall";
        public const string CONST_DENYALL = "denyall";
        public const string CONST_DENY = "deny";
        public const string CONST_ALLOW = "allow";

        //                                                              names below are direclty linked with names of columns in filterrule table :-(
        //                                                               0         1          2           3          4
        private readonly List<string> allColProps = EMDFilterRule.GetFilterableProperties();
        
        // next two are not thread safe
        //private List<EMDFilterRule> myRules;
        //private string objectGuid = null;

        private List<EMDFilterRule> allRules;
        
        private bool useTreeSearchForEnterprise = true;
        private bool batchModeActive = false;

        #region Constructors

        /// <summary>
        /// only use this if you know what you are doing!
        /// </summary>
        public FilterManager()
            : base()
        {
        }

        /// <summary>
        /// create filter manager object for a specific object     
        /// filter rule set for object will be loaded from db
        /// </summary>
        /// <param name="objectGuid">guid of the object that has a filter rule set configured</param>
        public FilterManager(string objectGuid)
            : base()
        {
            CoreTransaction transi = new CoreTransaction();
            transi.Begin();

            try
            {
                this.doConstructor(objectGuid, transi);
                transi.Commit();
            }
            catch (BaseException bEx)
            {
                transi.Rollback();
                throw bEx;
            }
            catch (Exception ex)
            {
                transi.Rollback();
                string errMsg = "error callling FilterManager constructor with objectGuid:" + objectGuid;
                logger.Error(errMsg, ex);
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY, errMsg, ex);
            }
        }

        public FilterManager(string objectGuid, CoreTransaction transaction)
            : base(transaction)
        {
            this.doConstructor(objectGuid, transaction);
        }

        public FilterManager(string objectGuid, string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
            CoreTransaction transaction = new CoreTransaction();
            transaction.Begin();

            try
            {
                this.doConstructor(objectGuid, transaction);
                transaction.Commit();
            }
            catch (BaseException bEx)
            {
                transaction.Rollback();
                throw bEx;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                string errMsg = "error callling FilterManager constructor with objectGuid:" + objectGuid;
                logger.Error(errMsg, ex);
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY, errMsg, ex);
            }
        }

        public FilterManager(string objectGuid, CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
            this.doConstructor(objectGuid, transaction);
        }

        /// <summary>
        /// to reduce amount of db queries you can specify a list of objectguids
        /// all the rulesets will be loaded in one query
        /// </summary>
        /// <param name="objectGuids"></param>
        public FilterManager(List<string> objectGuids)
            : base()
        {
            this.allRules = new FilterRuleHandler().ReadMultipleRulesFromDatabase(objectGuids);
            this.batchModeActive = true;
        }

        public FilterManager(List<string> objectGuids, CoreTransaction transaction)
            : base(transaction)
        {
            this.allRules = new FilterRuleHandler(this.Transaction).ReadMultipleRulesFromDatabase(objectGuids);
            this.batchModeActive = true;
        }

        public FilterManager(List<string> objectGuids, CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
            this.allRules = new FilterRuleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment).ReadMultipleRulesFromDatabase(objectGuids);
            this.batchModeActive = true;
        }

        /// <summary>
        /// do not use - work in progress
        /// </summary>
        /// <param name="objectGuid"></param>
        /// <param name="filterRuleHandler"></param>
        public FilterManager(string objectGuid, IFilterRuleHandler filterRuleHandler)
        {
            this.allRules = filterRuleHandler.ReadRulesFromDatase(objectGuid);
            this.batchModeActive = false;
        }

        public FilterManager(string objectGuid, IFilterRuleHandler filterRuleHandler, CoreTransaction transaction)
            : base(transaction)
        {
            this.allRules = filterRuleHandler.ReadRulesFromDatase(objectGuid);
            this.batchModeActive = false;
        }

        #endregion Contructors

        private void doConstructor(string objectGuid, CoreTransaction transaction)
        {
            FilterRuleHandler filterRuleHandler = new FilterRuleHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);
            this.allRules = filterRuleHandler.ReadRulesFromDatase(objectGuid);
            this.batchModeActive = false;
        }

        public bool CheckRule(string objectGuid, FilterCriteria filterCrit)
        {
            return this.CheckRule(objectGuid, filterCrit.Company, filterCrit.Location, filterCrit.EmploymentType, filterCrit.CostCenter, filterCrit.UserTypeIds);
        }

        /// <summary>
        /// Checks against Filter-rule with multiple user types
        /// </summary>
        /// <param name="objectGuid"></param>
        /// <param name="ente"></param>
        /// <param name="loca"></param>
        /// <param name="emty"></param>
        /// <param name="acco"></param>
        /// <param name="ustyMultiple"></param>
        /// <returns></returns>
        public bool CheckRule (string objectGuid, string ente, string loca, string emty, string acco, List<string> ustyMultiple)
        {
            bool result;
            bool isAllButSet;
            List<EMDFilterRule> myRules = this.allRules.Where(p => p.Obj_Guid == objectGuid).ToList();

            // get rule nr 0 (for "allbut")
            EMDFilterRule ruleZero = myRules.Find(x => x.FilterOrder == 0);
            if (ruleZero != null && !string.IsNullOrWhiteSpace(ruleZero.USTY_Enum))
            {
                string ustyBaseAction = ruleZero.USTY_Enum.ToLower(); // denyall or allowall
                isAllButSet = ustyBaseAction == CONST_ALLOWALL;

                if (isAllButSet)
                {
                    // if AllBut ist set 
                    // if any of the given usertypes is denied whole CheckRule is denied
                    result = true;
                    foreach (string usty in ustyMultiple)
                    {
                        bool tmpResult = this.CheckRule(objectGuid, ente, loca, emty, acco, usty);
                        if (tmpResult == false)
                        {
                            result = false;
                            break;
                        }
                    }
                }
                else
                {
                    // link separate Checkrule calls with OR
                    // if any of the usertypes given is allowed the whole CheckRule returns allowed (true)
                    result = false;
                    foreach (string usty in ustyMultiple)
                    {
                        bool tmpResult = this.CheckRule(objectGuid, ente, loca, emty, acco, usty);
                        if (tmpResult == true)
                        {
                            result = true;
                            break; // one result=true is enough; stop loop
                        }
                    }
                }
            }
            else
            {
                result = this.CheckRule(objectGuid, ente, loca, emty, acco);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectGuid"></param>
        /// <param name="ente"></param>
        /// <param name="loca"></param>
        /// <param name="emty"></param>
        /// <param name="acco"></param>
        /// <param name="user"></param>
        /// <returns>true is item is allowed, no rule means not allowed (false)</returns>
        public bool CheckRule(string objectGuid, string ente, string loca, string emty, string acco, string user = null)
        {
            List<EMDFilterRule> myRules = this.allRules.Where(p => p.Obj_Guid == objectGuid).ToList();
            if (ente == null) ente = "";
            if (loca == null) loca = "";
            if (emty == null) emty = "";
            if (acco == null) acco = "";
            if (user == null) user = "";

            if (myRules.Count > 1)
            {
                string booleanString = "";
                try
                {
                    booleanString = this.buildRuleAsString(myRules, ente, loca, emty, acco, user);

                    ExpressionContext context = new ExpressionContext();

                    context.Variables[allColProps[0]] = ente;
                    context.Variables[allColProps[3]] = loca;
                    context.Variables[allColProps[1]] = emty;
                    context.Variables[allColProps[2]] = acco;
                    context.Variables[allColProps[4]] = user;

                    IGenericExpression<bool> e = context.CompileGeneric<bool>(booleanString);
                    return e.Evaluate();
                }
                catch (Exception ex)
                {
                    string eMsg = string.Format("error evaluating filter rule. expression was '{0}' and variables where ente: {1} loca: {2} emty: {3} acco: {4} user:{5}",
                        booleanString, ente, loca, emty, acco, user);
                    logger.Error(eMsg, ex);
                    //no exception thrown - trying to be fault tolerant
                    return false;
                }
            }
            else if (myRules.Count == 1)
            {
                string fAction = myRules[0].FilterAction;
                // if only one rule in set and filteraction = allowall then allow
                if (fAction != null && fAction.ToLowerInvariant() == CONST_ALLOWALL)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }



 



        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterCrit"></param>
        /// <returns></returns>
        public bool CheckRuleWithCustomRuleSet(List<EMDFilterRule> myRules, List<FilterRuleSubSetForCriteria> filterRuleSubSets)
        {
            if (myRules.Count > 0)
            {
                string ente = "", loca = "", emty = "", acco = "", user = "";

                foreach (FilterRuleSubSetForCriteria subCrit in filterRuleSubSets)
                {
                    switch (subCrit.Criteria)
                    {
                        case EnumFilterCriteria.Company:
                            if (this.hasAValue(subCrit.ObjectGUIDs))
                                ente = subCrit.ObjectGUIDs[0];
                            break;
                        case EnumFilterCriteria.Location:
                            if (this.hasAValue(subCrit.ObjectGUIDs))
                                loca = subCrit.ObjectGUIDs[0];
                            break;
                        case EnumFilterCriteria.CostCenter:
                            if (this.hasAValue(subCrit.ObjectGUIDs))
                                acco = subCrit.ObjectGUIDs[0];
                            break;
                        case EnumFilterCriteria.EmploymentType:
                            if (this.hasAValue(subCrit.ObjectGUIDs))
                                emty = subCrit.ObjectGUIDs[0];
                            break;
                        case EnumFilterCriteria.UserType:
                            if (this.hasAValue(subCrit.ObjectGUIDs))
                                user = subCrit.ObjectGUIDs[0];
                            break;
                        default:
                            break;
                    }
                }
                string booleanString = this.buildRuleAsString(myRules, ente, loca, emty, acco, user);

                ExpressionContext context = new ExpressionContext();
                context.Variables[allColProps[0]] = ente;
                context.Variables[allColProps[3]] = loca;
                context.Variables[allColProps[1]] = emty;
                context.Variables[allColProps[2]] = acco;
                context.Variables[allColProps[4]] = user;

                IGenericExpression<bool> e = context.CompileGeneric<bool>(booleanString);
                return e.Evaluate();
            }
            else
            {
                return false;
            }
        }

        public void DeleteFilterRule(CoreTransaction ta)
        {
            FilterRuleHandler firuH = new FilterRuleHandler(ta, this.Guid_ModifiedBy, this.ModifyComment);

            foreach (EMDFilterRule item in this.allRules)
            {
                firuH.DeleteObject(item, historize: true);
            }
        }

        public void DeleteFilterRule()
        {
            FilterRuleHandler firuH = new FilterRuleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            foreach (EMDFilterRule item in this.allRules)
            {
                firuH.DeleteObject(item, historize: true);
            }
        }

        private bool hasAValue(List<string> obj)
        {
            if (obj != null && obj.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// create a new filter rule in the database with objectGuid handed over at constructor
        /// </summary>
        /// <param name="filterRuleSubSets"></param>
        public void CreateFilterRule(string objectGuid, List<FilterRuleSubSetForCriteria> filterRuleSubSets)
        {
            FilterRuleHandler frh = new FilterRuleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            this.doCreateFilterRuleSet(objectGuid, filterRuleSubSets, frh);
        }

        public void CreateFilterRule(string objectGuid, List<FilterRuleSubSetForCriteria> filterRuleSubSets, CoreTransaction transaction)
        {
            FilterRuleHandler frh = new FilterRuleHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);
            this.doCreateFilterRuleSet(objectGuid, filterRuleSubSets, frh);
        }

        private void doCreateFilterRuleSet(string objectGuid, List<FilterRuleSubSetForCriteria> filterRuleSubSets, FilterRuleHandler frh)
        {
            int ruleOrderCounter = 0;

            if (filterRuleSubSets == null || filterRuleSubSets.Count < 1)
            {
                // create a rule that allows everthing
                EMDFilterRule row0 = new EMDFilterRule();
                row0.E_Guid = null;
                row0.FilterAction = CONST_ALLOWALL;
                row0.Obj_Guid = objectGuid;
                row0.FilterOrder = 0;
                row0 = (EMDFilterRule)frh.CreateObject(row0);
            }
            else
            {
                // normal rule
                // ROW 0 bauen mit base filter actions per criteria (column)
                EMDFilterRule row0 = new EMDFilterRule();
                row0.EnteIsInherited = false;
                foreach (FilterRuleSubSetForCriteria subCrit in filterRuleSubSets)
                {
                    switch (subCrit.Criteria)
                    {
                        case EnumFilterCriteria.Company:
                            row0.E_Guid = subCrit.FilterAction;
                            row0.EnteIsInherited = subCrit.EnteIsInherited;
                            break;
                        case EnumFilterCriteria.Location:
                            row0.L_Guid = subCrit.FilterAction;
                            break;
                        case EnumFilterCriteria.CostCenter:
                            row0.ACC_Guid = subCrit.FilterAction;
                            break;
                        case EnumFilterCriteria.EmploymentType:
                            row0.ET_Guid = subCrit.FilterAction;
                            break;
                        case EnumFilterCriteria.UserType:
                            row0.USTY_Enum = subCrit.FilterAction;
                            break;
                        default:
                            break;
                    }

                    row0.FilterAction = null;
                    row0.Obj_Guid = objectGuid;
                    row0.FilterOrder = 0;

                    foreach (string objGUID in subCrit.ObjectGUIDs)
                    {
                        EMDFilterRule filterRow = new EMDFilterRule();

                        filterRow.FilterAction = subCrit.FilterAction.ToLowerInvariant().StartsWith("allow") ? "deny" : "allow";

                        filterRow.Obj_Guid = objectGuid;
                        ruleOrderCounter++;
                        filterRow.FilterOrder = ruleOrderCounter;

                        switch (subCrit.Criteria)
                        {
                            case EnumFilterCriteria.Company:
                                filterRow.E_Guid = objGUID;
                                filterRow.EnteIsInherited = subCrit.EnteIsInherited;
                                break;
                            case EnumFilterCriteria.Location:
                                filterRow.L_Guid = objGUID;
                                break;
                            case EnumFilterCriteria.CostCenter:
                                filterRow.ACC_Guid = objGUID;
                                break;
                            case EnumFilterCriteria.EmploymentType:
                                filterRow.ET_Guid = objGUID;
                                break;
                            case EnumFilterCriteria.UserType:
                                filterRow.USTY_Enum = objGUID;
                                break;
                            default:
                                break;
                        }
                        filterRow = (EMDFilterRule)frh.CreateObject(filterRow);
                    }

                }
                row0 = (EMDFilterRule)frh.CreateObject(row0);
            }
        }

        private List<EMDFilterRule> readRulesFromDatase(string objectGuid)
        {
            List<EMDFilterRule> rules = new List<EMDFilterRule>();

            FilterRuleHandler frh = new FilterRuleHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDFilterRule>> xx = frh.GetRuleSetByObjGuid(objectGuid);
            foreach (IEMDObject<EMDFilterRule> item in xx)
                rules.Add((EMDFilterRule)item);

            return rules;
        }

        private string buildRuleAsString(List<EMDFilterRule> myRules, string ente, string loca, string emty, string acco, string user)
        {
            List<ColumnRuleSet> colRulesSet = new List<ColumnRuleSet>();
            List<RowRuleSet> linkedRules = new List<RowRuleSet>();
            int fv;
            string firmaBoolean = "";
            string maTypeBoolean = "";
            string standortBoolean = "";
            string kostenSTBoolean = "";
            string userBoolean = "";
            string linkedRowsBoolean = "";

            myRules.Sort((r1, r2) => r1.FilterOrder.Value.CompareTo(r2.FilterOrder.Value));

            // ------------- Enterprise --------------
            foreach (EMDFilterRule fr in myRules)
            {
                // filter per column, where no connection to other columns is given (same line all other columns null or empty)
                if (this.allNull_ButTheOne(fr, allColProps, allColProps[0]))
                {
                    //
                    // if use treesearch modify ente to its parent ente (specified in rule) 
                    // this is done to keep the filtercode the same
                    // TODO : ideally the flee expression thing is replaced with csc-script and tree search function can be built into expression
                    //
                    string treeSearchEnte = "";
                    if (this.useTreeSearchForEnterprise && fr.EnteIsInherited)
                    {
                        //treeSearchEnte = this.returnChildIfIsChild(fr.E_Guid, ente);
                        treeSearchEnte = this.ReturnChildIfIsChild(fr.E_Guid, ente); //replaced by EnterpriseTree
                    }
                    else
                        treeSearchEnte = fr.E_Guid;
                    colRulesSet.Add(new ColumnRuleSet(fr.FilterAction, treeSearchEnte /*fr.E_Guid*/, allColProps[0]));
                }
            }
            firmaBoolean = this.calcBooleanForCriteria(myRules, colRulesSet);
            // ------------- Enterprise --------------


            // ------------- MA Type --------------
            colRulesSet.Clear();
            foreach (EMDFilterRule fr in myRules)
            {
                // filter per column, where no connection to other columns is given (in same line all other columns null or empty)
                if (this.allNull_ButTheOne(fr, allColProps, allColProps[1]))
                    colRulesSet.Add(new ColumnRuleSet(fr.FilterAction, fr.ET_Guid, allColProps[1]));
            }
            maTypeBoolean = this.calcBooleanForCriteria(myRules, colRulesSet);
            // ------------- MA Type --------------


            // ------------- Location --------------
            colRulesSet.Clear();
            foreach (EMDFilterRule fr in myRules)
            {
                // filter per column, where no connection to other columns is given (in same line all other columns null or empty)
                if (this.allNull_ButTheOne(fr, allColProps, allColProps[3]))
                    colRulesSet.Add(new ColumnRuleSet(fr.FilterAction, fr.L_Guid, allColProps[3]));
            }
            standortBoolean = this.calcBooleanForCriteria(myRules, colRulesSet);
            // ------------- Location --------------

            // ------------- Kostenstelle --------------
            colRulesSet.Clear();
            foreach (EMDFilterRule fr in myRules)
            {
                // filter per column, where no connection to other columns is given (in same line all other columns null or empty)
                if (this.allNull_ButTheOne(fr, allColProps, allColProps[2]))
                    colRulesSet.Add(new ColumnRuleSet(fr.FilterAction, fr.ACC_Guid, allColProps[2]));
            }
            kostenSTBoolean = this.calcBooleanForCriteria(myRules, colRulesSet);
            // ------------- Kostenstelle --------------

            // ------------- USER --------------
            colRulesSet.Clear();
            foreach (EMDFilterRule fr in myRules)
            {
                // filter per column, where no connection to other columns is given (in same line all other columns null or empty)
                if (this.allNull_ButTheOne(fr, allColProps, allColProps[4]))
                    colRulesSet.Add(new ColumnRuleSet(fr.FilterAction, fr.USTY_Enum, allColProps[4]));
            }
            userBoolean = this.calcBooleanForCriteria(myRules, colRulesSet);
            // ------------- USER end --------------


            // ------------- linked Criteria columns
            colRulesSet.Clear();
            foreach (EMDFilterRule fr in myRules)
            {
                fv = 0;
                string lineBool = "";
                if (fr.FilterAction == null)
                    continue;
                string bOperator = fr.FilterAction.ToLowerInvariant() == CONST_ALLOW ? "AND  " : "OR  ";
                // get all columns that have 2+ FilterValues per row
                // count amount of FilterValues

                if (!string.IsNullOrWhiteSpace(fr.E_Guid))
                {   // firma
                    lineBool += string.Format(" {0} ({1}) ", bOperator, this.buildBoolean(new ColumnRuleSet(fr.FilterAction, fr.E_Guid, allColProps[0])));
                    fv++;
                }
                if (!string.IsNullOrWhiteSpace(fr.ACC_Guid))
                {   // kostenstelle
                    lineBool += string.Format(" {0} ({1}) ", bOperator, this.buildBoolean(new ColumnRuleSet(fr.FilterAction, fr.ACC_Guid, allColProps[2])));
                    fv++;
                }
                if (!string.IsNullOrWhiteSpace(fr.ET_Guid))
                {   // ma type
                    lineBool += string.Format(" {0} ({1}) ", bOperator, this.buildBoolean(new ColumnRuleSet(fr.FilterAction, fr.ET_Guid, allColProps[1])));
                    fv++;
                }
                if (!string.IsNullOrWhiteSpace(fr.L_Guid))
                {   // standort
                    lineBool += string.Format(" {0} ({1}) ", bOperator, this.buildBoolean(new ColumnRuleSet(fr.FilterAction, fr.L_Guid, allColProps[3])));
                    fv++;
                }
                if (fv > 1)
                {
                    //remove first and
                    lineBool = lineBool.Substring(4);
                    linkedRules.Add(new RowRuleSet(fr.FilterAction, lineBool));
                }
            }

            if (linkedRules.Count > 0)
            {
                linkedRowsBoolean = this.calcLinkedCriteria(myRules, linkedRules);
            }
            // ------------- linked Criteria columns

            string allCriteria = "";
            if (firmaBoolean != "")
                allCriteria = " AND " + firmaBoolean;
            if (maTypeBoolean != "")
                allCriteria += " AND " + maTypeBoolean;
            if (standortBoolean != "")
                allCriteria += " AND " + standortBoolean;
            if (kostenSTBoolean != "")
                allCriteria += " AND " + kostenSTBoolean;
            if (userBoolean != "")
                allCriteria += " AND " + userBoolean;
            if (linkedRowsBoolean != "")
                allCriteria += " AND " + linkedRowsBoolean;

            if (allCriteria.StartsWith(" AND "))
                allCriteria = allCriteria.Substring(4);

            return allCriteria;
        }

        private string calcLinkedCriteria(List<EMDFilterRule> myRules, List<RowRuleSet> linkedLinesBoolean)
        {
            string boolExpression = "";
            if (linkedLinesBoolean.Count > 0)
            {
                boolExpression = this.calcFirstRowFilter(myRules, null);

                foreach (RowRuleSet rowRule in linkedLinesBoolean)
                {
                    // figure out boolschen operator
                    string bOperator = rowRule.Criteria.ToLowerInvariant() == CONST_ALLOW ? " OR " : " AND ";
                    string currentExpression = rowRule.RowBoolean;
                    boolExpression = "(" + boolExpression + bOperator + currentExpression + ")";
                }
            }
            return boolExpression;
        }

        private string calcBooleanForCriteria(List<EMDFilterRule> myRules, List<ColumnRuleSet> colRuleSet)
        {
            string boolExpression = "";

            if (myRules.Count < 1)
                return "";

            if (colRuleSet.Count > 0)
            {
                boolExpression = this.calcFirstRowFilter(myRules, colRuleSet[0].FilterName); // could be any row in RuleSet they all the same filtername

                for (int idx = 0; idx < colRuleSet.Count; idx++)
                {
                    // figure out boolschen operator
                    string bOperator = colRuleSet[idx].FilterExpression.ToLowerInvariant() == CONST_ALLOW ? " OR " : " AND ";
                    string currentExpression = buildBoolean(colRuleSet[idx]);
                    boolExpression = "(" + boolExpression + bOperator + currentExpression + ")";
                }

            }
            return boolExpression;
        }

        private string buildBoolean(ColumnRuleSet colRuleSet)
        {
            string currentExpression = "";
            string filterValue = colRuleSet.FilterValue;
            string filterName = colRuleSet.FilterName;

            switch (colRuleSet.FilterExpression.ToLowerInvariant())
            {
                case "_all":
                    currentExpression = " 1=1 ";
                    break;
                case "allow":
                    if (filterValue.ToLowerInvariant() == "_all")
                        currentExpression = " 1 = 1 ";
                    else
                        currentExpression = string.Format(" {0} = \"{1}\"", filterName, filterValue);
                    break;
                case "deny":
                    if (filterValue.ToLowerInvariant() == "_all")
                        currentExpression = " 1 = 2 ";
                    else
                        currentExpression = string.Format(" {0} <> \"{1}\"", filterName, filterValue);
                    break;
            }

            return currentExpression;
        }

        private string calcFirstRowFilter(List<EMDFilterRule> myRules, string filterName)
        {
            // von colRuleSet.FilterName in Zeile[0] entsprechende Spalte auslesen
            string boolExpression = "";
            EMDFilterRule r0 = myRules[0];
            bool allCriteriaNull = this.allNull(r0, allColProps, "irgendwas");
            if (allCriteriaNull)
            {
                // take what filteraction says
                // first rule is always deny all or allow all
                boolExpression = buildBaseFilterCondition(r0.FilterAction);
            }
            else
            {
                // take what specific column of colRuleSet.FilterName says
                string filerAction = this.getPropertyValue(filterName, r0);
                boolExpression = buildBaseFilterCondition(filerAction);
            }

            return boolExpression;
        }

        private string buildBaseFilterCondition(string filterAction)
        {
            string bExpr = "";
            if (filterAction.ToLowerInvariant() == CONST_DENYALL)
                bExpr = "(1=2)";
            else if (filterAction.ToLowerInvariant() == CONST_ALLOWALL)
                bExpr = "(1=1)";
            return bExpr;
        }

        private string getPropertyValue(string filterName, EMDFilterRule r0)
        {
            PropertyInfo pInfo = r0.GetType().GetProperty(filterName);
            string filterAction = pInfo.GetValue(r0) != null ? pInfo.GetValue(r0).ToString() : null;
            return filterAction;
        }

        private bool allNull_ButTheOne(EMDFilterRule rule, List<string> allProps, string theOneProp)
        {
            // make sure row 0 (general rule deny allow is not considered)
            if (rule.FilterAction == null ||
                (rule.FilterAction.ToLowerInvariant().Equals(CONST_DENYALL) | rule.FilterAction.ToLowerInvariant().Equals(CONST_ALLOWALL))
               )
                return false;

            bool allNull = this.allNull(rule, allProps, theOneProp);

            if (allNull)
            {
                PropertyInfo pInfo = rule.GetType().GetProperty(theOneProp);
                string theOneValue = pInfo.GetValue(rule) != null ? pInfo.GetValue(rule).ToString() : null;
                if (!string.IsNullOrWhiteSpace(theOneValue))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        private bool allNull(EMDFilterRule rule, List<string> allProps, string theOneProp)
        {
            bool allNull = true;
            foreach (string pName in allProps)
            {
                if (pName != theOneProp)
                {
                    PropertyInfo pInfo = rule.GetType().GetProperty(pName);
                    if (pInfo != null)
                    {
                        string value = pInfo.GetValue(rule) != null ? pInfo.GetValue(rule).ToString() : null;
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            allNull = false;
                            break;
                        }
                    }
                }
            }

            return allNull;
        }

        /// <summary>
        /// if childGuid is a child of parentGuid this will return the childGuid
        /// This way the existing code in FilterManager can be used without changes.
        /// </summary>
        /// <param name="parentGuid">is the enterprise guid coming from the ruleset (from db)</param>
        /// <param name="childGuid">is the enterprise guid coming from user input</param>
        /// <returns>parentGuid or childGuid</returns>
        [Obsolete]private string returnChildIfIsChild(string parentGuid, string childGuid)
        {
            string returnResult = null;
            try
            {
                
                EnterpriseHandler enteH = new EnterpriseHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

                bool isChild = enteH.IsEnterpriseUnderParent(parentGuid, childGuid);
                if (isChild)
                    returnResult = childGuid;
                else
                    returnResult = parentGuid;

            }
            catch (Exception ex)
            {
                logger.Warn("finding children failed. ", ex);
                returnResult = parentGuid;
            }
            return returnResult;
        }

        private string ReturnChildIfIsChild(string parentGuid, string childGuid)
        {
            string returnResult = null;
            try
            {

                EnterpriseManager enterpriseManager = new EnterpriseManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                EnterpriseTree enterpriseTree = new EnterpriseTree();
                enterpriseTree.Fill(enterpriseManager.Get(enterpriseManager.Get(parentGuid).Guid_Root));

                bool isChild = false;
                var childs = enterpriseTree.GetAllChildrenOf(parentGuid);
                foreach (var child in childs)
                {
                    if ((child.Guid == childGuid)) isChild = true;
                }

                if (isChild)
                    returnResult = childGuid;
                else
                    returnResult = parentGuid;

            }
            catch (Exception ex)
            {
                logger.Warn("finding children failed. ", ex);
                returnResult = parentGuid;
            }
            return returnResult;
        }
    }
}

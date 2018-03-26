using Ciloci.Flee;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Xml.Linq;
using Kapsch.IS.ProcessEngine.Variables;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   A class to evaluate c# expressions.
    ///             This is needed to evaluate expressions and conditions for workflow transitions
    ///             and also to calculate values in the workflow iteself. </summary>
    ///
    /// <remarks>   Fleckj, 19.03.2015. </remarks>
    #endregion
    public class ExpressionEvaluator
    {
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private object returnObjectRecursive = null;
        private WorkflowModel wfModel;

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Default constructor. </summary>
        ///
        /// <remarks>   Fleckj, 19.03.2015. </remarks>
        #endregion
        public ExpressionEvaluator()
        {
        }

        public ExpressionEvaluator(WorkflowModel wfModel)
        {
            this.wfModel = wfModel;
        }

        [Obsolete]
        public object Evaluate(string expression, SortedList<string, Variable> wfVariables)
        {
            return this.Evaluate(expression);
        }

        /// <summary>
        /// Evaluate an expression with workflow variables and all dependencies (and @@ expressions are left untoched)
        /// ProcessEngine does not have access to Core (i.e. Entity().Query()
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="wfVariables"></param>
        /// <returns></returns>
        public object Evaluate(string expression)
        {
            string processedValue = new WFReplacer(expression, wfModel).Replace().ProcessedValue;
            return processedValue;
        }

        [Obsolete]
        private string makeValidVarName(string wfName)
        {
            string numbers = Regex.Match(wfName, @"(^[\d]*)").Value;
            string noNumbers = Regex.Replace(wfName, @"(^[\d]*)\.", "");
            string validName = noNumbers + numbers;

            return validName;
        }

        [Obsolete]
        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Replace variables with values. An expression in a workflow or transition can
        ///             contain values and variables (defined for the workflow). This function replaces the 
        ///             variables names with the current values for them. </summary>
        ///
        /// <remarks>   Fleckj, 19.03.2015. </remarks>
        ///
        /// <param name="expression">   The expression. </param>
        /// <param name="variables">    The variables. </param>
        /// <param name="errorMessage"> [in,out] Message describing the error. </param>
        ///
        /// <returns>   return the expression with values instead of variable names </returns>
        /// <example> for a given expression "variable.index > 5 * variable.faktor" 
        ///           and variable.index=11 
        ///           and variable.faktor=3
        ///           this function will return a string "11 > 15"</example>
        #endregion
        public static string ReplaceVariablesWithValues(string expression, SortedList<string, Variable> variables, ref string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(expression))
            {
                if (!Regex.IsMatch(expression, "variable.", RegexOptions.IgnoreCase))
                    return expression;

                IList<string> keys = variables.Keys;
                for (int i = keys.Count - 1; i > -1; i--)
                {
                    Variable item = variables[keys[i]];
                    string variableValue = item.VarValue.ToString();
                    if (Regex.IsMatch(expression, string.Concat("variable.", item.Name), RegexOptions.IgnoreCase))
                    {
                        expression = Regex.Replace(expression, string.Concat("variable.", item.Name), variableValue, RegexOptions.IgnoreCase);
                    }
                }
            }
            return expression;
        }
    }
}

using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.ProcessEngine.Variables
{
    public class WFReplacer : Replacer<WFReplacer>
    {
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private WorkflowModel wfModel;

        public WFReplacer(string value, WorkflowModel wfModel) : base(value)
        {
            this.wfModel = wfModel;
        }

        public override WFReplacer Replace()
        {
            this.ProcessedValue = this.ReplaceWfVariables(this.Value).ToString();
            return this;
        }

        private string ReplaceWfVariables(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            string currentVarName = "";
            string currentVarNameWithCurlies = "";
            int curlyPosStart = 0;
            int curlyPosEnd = 0;

            

            do
            {
                currentVarName = "";

                curlyPosStart = expression.IndexOf(this.BeginBracket, 0, StringComparison.Ordinal);
                curlyPosEnd = expression.IndexOf(this.EndBracket, 0, StringComparison.Ordinal);
                if (curlyPosStart == -1)
                    break; // no curly brackets anymore - we are finished

                currentVarName = expression.Substring(curlyPosStart + this.BeginBracket.Length, curlyPosEnd - (curlyPosStart + this.BeginBracket.Length));
                currentVarNameWithCurlies = expression.Substring(curlyPosStart, curlyPosEnd + this.EndBracket.Length - curlyPosStart);

                string myVariableValue;
                
                string xpathQuery = "/workflow/variables/variable[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" + currentVarName.ToLower() + "']";
                XElement xElement = this.wfModel.wfModel.XPathSelectElement(xpathQuery);
                if (xElement != null)
                {
                    string uniqueName;
                    EnumVariableDirection d;
                    EnumVariablesDataType t;
                    this.wfModel.ExtraktVariableProperties(xElement, out myVariableValue, out uniqueName, out d, out t);

                   

                    logger.Debug("WFReplacer.replaceWfVariables : expression of varvalue for " + currentVarName + " is " + myVariableValue);

                    expression = expression.Replace(currentVarNameWithCurlies, myVariableValue);
                }
                else
                {
                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, "WFReplacer.replaceWfVariables cannot find variable in xml:" + xpathQuery);
                }
            }
            while (true);

            return expression;
        }
    }
}

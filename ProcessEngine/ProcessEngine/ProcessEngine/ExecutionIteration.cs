using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using Kapsch.IS.ProcessEngine;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary> Every Activity in an instance of a workflow will be executed.
    ///           To record the execution an ExecutionIteration is created with variables,
    ///           status and logging(messages) information. 
    ///           In case of a loop activity more than one ExecutionIteration can exist.</summary>
    ///
    /// <remarks>   Fleckj, 10.02.2015. </remarks>
    #endregion
    public class ExecutionIteration
    {
        private XElement executionIterationNode;

        public string IterationNumber
        {
            get
            {
                XAttribute iterNr = this.executionIterationNode.Attribute("iteration");
                if (iterNr != null)
                    return iterNr.Value;
                else
                    return "";
            }
        }
        public string ReturnValue
        {
            get
            {
                if (this.executionIterationNode.Attribute("returnValue") == null)
                {
                    return "";
                }
                return this.executionIterationNode.Attribute("returnValue").Value;
            }
            set
            {
                this.executionIterationNode.SetAttributeValue("returnValue", value);
            }
        }
        public string ExecutionStatus
        {
            get
            {
                XAttribute execStatus = this.executionIterationNode.Attribute("stepExecStatus");
                if (execStatus != null)
                    return execStatus.Value;
                else
                    return "";
            }
        }

        public Activity getActivity()
        {
            return new Activity(this.executionIterationNode.Parent);
        }

        public ExecutionIteration(XElement execIterationNode)
        {
            this.executionIterationNode = execIterationNode;
        }

        public void AddMessage(string msgKey, string msgVal)
        {
            if (msgVal == null) msgVal="";
            XElement msgNode = this.executionIterationNode.Descendants().Where<XElement>((XElement e) => { return e.Name.LocalName == "messages"; }).FirstOrDefault();
            XElement msg = this.findMessageOrVariableByKey("message", msgKey);
            if (msg != null) msg.RemoveAll(); else msg = new XElement("message");
            msg.Add(msgVal);
            msg.SetAttributeValue("key", msgKey);
            int nextNr = msgNode.Elements().Count() + 1;
            msg.SetAttributeValue("index", nextNr.ToString());
            msgNode.Add(msg);
        }
        public string GetMessage(string msgKey)
        {
            XElement msg = this.findMessageOrVariableByKey("message", msgKey);
            if (msg == null)
            {
                return "";
            }
            return msg.Value;
        }

        private XElement findMessageOrVariableByKey(string element, string msgKey)
        {
            string keyName = element == "variable" ? "name" : "key";
            XElement msg = this.executionIterationNode.Descendants().Where<XElement>((XElement e) =>
            {
                if (e.Name.LocalName == element)
                {
                    if (e.HasAttributes && e.Attribute(keyName).Value == msgKey) return true;
                }
                return false;
            }).FirstOrDefault();
            return msg;
        }

        public string GetVariable(string variableName)
        {
            XElement v = this.findMessageOrVariableByKey("variable", variableName);
            if (v != null)
                return v.Value;
            else
                return "";
        }
        public void UpdateVariable(string variableName, string variableValue)
        {
            XElement xElement = this.executionIterationNode.XPathSelectElement(string.Concat("./variables/variable[@name='", variableName, "']"));
            if (xElement == null)
            {
                XElement xElement1 = this.executionIterationNode.Descendants().Where<XElement>((XElement xe) => { return xe.Name.LocalName == "variables"; } ).FirstOrDefault();
                xElement = new XElement("variable");
                xElement1.Add(xElement);
            }
            xElement.SetAttributeValue("name", variableName);
            xElement.Value = variableValue;
        }
        public void UpdateVariables(SortedList<string, Variable> variablesDic)
        {
            if (variablesDic == null)
                return;

            foreach (KeyValuePair<string, Variable> keyValuePair in variablesDic)
                this.UpdateVariable(keyValuePair.Value.Name, keyValuePair.Value.VarValue);
        }

        /// <summary>
        /// set a property on iteration node
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="propVal"></param>
        public void SetProperty(string propName, string propVal)
        {
            XAttribute a = this.executionIterationNode.Attribute(propName);
            if (a != null)
            {
                this.executionIterationNode.SetAttributeValue(propName, propVal);
            }
            else
            {
                // create new attribute
                this.executionIterationNode.Add(new XAttribute(propName,propVal));
            }
            
        }
    }
}

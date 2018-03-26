using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------
    /// <summary>   The cls activity is the object representation of an 
    ///             xpdl activity for an instance of a workflow definition. 
    ///             (--> figure in instance xml)</summary>
    ///
    /// <remarks>   Fleckj, 06.02.2015. </remarks>
    #endregion
    public class Activity
    {
        public const string CONST_TransistionNodeName = "transition";

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   The attribute ID is used to save the namespace of the activity.
        ///             This corresponds to "What type" it is. </summary>
        ///
        /// <value> ID </value>
        #endregion
        public string Id /* Id in xml is the namespace */
        {
            get
            {
                if (this.ActivityNode.HasAttributes)
                {
                    XAttribute idAttr = this.ActivityNode.Attribute("id");
                    if (idAttr != null)
                        return idAttr.Value;
                    else
                        return null;
                }
                else
                    return null;
            }
            set
            {
                this.ActivityNode.SetAttributeValue("id", value);
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets or sets the running nr. </summary>
        ///
        /// <value> The nr. </value>
        #endregion
        public string Nr
        {
            get
            {
                if (this.ActivityNode.HasAttributes)
                {
                    XAttribute idAttr = this.ActivityNode.Attribute("nr");
                    if (idAttr != null)
                        return idAttr.Value;
                    else
                        return null;
                }
                else
                    return null;
            }
            set
            {
                this.ActivityNode.SetAttributeValue("nr", value);


                XAttribute idAttr = this.ActivityNode.Attribute("instance");

                if (idAttr != null)
                {
                    if (!string.IsNullOrEmpty(this.Nr))
                    {
                        idAttr.Value = string.Format("{0}.{1}", this.Nr, idAttr.Value.Split('.').Last());
                    }
                }
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Intance attribte is the (automatically) genereated,
        ///             but still readable name of that specific instance. </summary>
        ///
        /// <value> Instance. </value>
        #endregion
        public string Instance
        {
            get
            {
                if (this.ActivityNode.HasAttributes)
                {
                    XAttribute idAttr = this.ActivityNode.Attribute("instance");
                    return idAttr.Value;

                }

                return null;
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Name is a value the user can choose to describe
        ///             the activity in business language terms.</summary>
        ///
        /// <value> Name </value>
        #endregion
        public string Name
        {
            get
            {
                XElement elName = this.ActivityNode.XPathSelectElement("name");
                if (elName != null)
                    return elName.Value;
                else
                    return null;
            }
        }

        /// <summary>
        /// links to async activity belonging to this activity
        /// </summary>
        public string WaitInstanceID
        {
            get
            {
                XElement elName = this.ActivityNode.XPathSelectElement("linkedTo");
                if (elName != null)
                {
                    XAttribute inst = elName.Attribute("instance");
                    if (inst != null)
                        return inst.Value;
                    else
                        return null;
                }
                else
                    return null;
            }
            set
            {
                XElement elName = this.ActivityNode.XPathSelectElement("linkedTo");
                if (elName != null)
                {
                    XAttribute inst = elName.Attribute("instance");
                    if (inst != null)
                        inst.SetValue(value);
                }
            }
        }

        public string GetWaitIteration()
        {
            XElement elExecution = this.ActivityNode.XPathSelectElement("./execution[@stepExecStatus='" + EnumStepState.Wait.ToString() + "']");
            if (elExecution == null)
            {
                return "";
            }
            return elExecution.Attribute("iteration").Value;
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>  Property that contains the XML Element for activity. </summary>
        ///
        /// <value> The activity node. </value>
        #endregion
        public XElement ActivityNode { get; set; }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Fleckj, 13.11.2015. </remarks>
        ///
        /// <param name="activityNode"> The activity node. </param>
        #endregion
        public Activity(XElement activityNode)
        {
            this.ActivityNode = activityNode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityXml"></param>
        public Activity(string activityXml)
        {
            this.ActivityNode = XElement.Parse(activityXml);
        }

        /// <summary>
        /// Get a specific property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ActivityProperty GetActivityProperty(string propertyName)
        {
            XElement propNode = this.ActivityNode.XPathSelectElement(string.Format("./properties/property[@name='{0}']", propertyName));
            if (propNode != null)
                return new ActivityProperty(propNode);
            else
                return null;
        }

        public List<ActivityProperty> GetAllActivityProperties
        {
            get
            {
                IEnumerable<XElement> allXElementsProps = this.ActivityNode.XPathSelectElements(".//property");
                List<ActivityProperty> allProps = new List<ActivityProperty>();

                foreach (XElement e in allXElementsProps)
                {
                    allProps.Add(new ActivityProperty(e));
                }
                return allProps;
            }
        }

        public List<Transition> GetAllTransitions
        {
            get
            {
                IEnumerable<XElement> allXElementsTrans = this.ActivityNode.XPathSelectElements(".//transition");
                List<Transition> allProps = new List<Transition>();

                foreach (XElement e in allXElementsTrans)
                {
                    allProps.Add(new Transition(e));
                }
                return allProps;
            }
        }

        [Obsolete("do not use anymore, create a ActivityProperty objekt and get '.Value'")]
        public string GetPropertyValue(string propertyName)
        {
            XElement propNode = this.ActivityNode.XPathSelectElement(string.Format("./properties/property[@name='{0}']", propertyName));
            if (propNode == null)
                return null;
            else
                return propNode.Value;
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Looks for an existing iteration with wait status 
        ///             if doesnt exist create a new one and increase iteration counter </summary>
        ///
        /// <remarks>   Fleckj, 19.02.2015. </remarks>
        ///
        /// <returns>   The next execution iteration. </returns>
        #endregion
        public ExecutionIteration GetNextExecutionIteration()
        {
            XElement wait = this.ActivityNode.Descendants().Where<XElement>((XElement el) =>
                {
                    if (el.Name.LocalName == "execution")
                    {
                        XAttribute stepStatus = el.Attribute("stepExecStatus");
                        if (stepStatus != null && el.Attribute("stepExecStatus").Value == "wait") return true;
                    }
                    return false;
                }).FirstOrDefault();
            if (wait != null)
                return new ExecutionIteration(wait);

            // get list of all elements "execution" with an iteration attibute 
            IEnumerable<XElement> iterationElements = this.ActivityNode.Elements("execution").Where<XElement>((XElement el) =>
            {
                if (el.Attribute("iteration") == null) return false;
                return !string.IsNullOrWhiteSpace(el.Attribute("iteration").Value); // count in if has a value
            });

            // get highest iteration number
            int num = iterationElements.DefaultIfEmpty<XElement>().Max<XElement>((XElement i) =>
            {
                if (i == null) return 0;
                return int.Parse(i.Attribute("iteration").Value);
            }) + 1;


            XElement newIteration = new XElement("execution");
            this.ActivityNode.Add(newIteration);
            newIteration.SetAttributeValue("iteration", num.ToString());
            newIteration.Add(new XElement("variables"));
            newIteration.Add(new XElement("messages"));

            return new ExecutionIteration(newIteration);
        }

        public ExecutionIteration GetExecutionIteration(int iterationNumber)
        {
            XElement elIternationNode = this.ActivityNode.XPathSelectElement(string.Format("./execution[@iteration='{0}']", iterationNumber));
            if (elIternationNode == null)
            {
                return null;
            }
            return new ExecutionIteration(elIternationNode);
        }

        public void SetAllExecutionIterationsToComplete()
        {
            List<XElement> ll = this.ActivityNode.XPathSelectElements("./execution").ToList();
            foreach (XElement e in ll)
            {
                e.SetAttributeValue("stepExecStatus", EnumStepState.Complete);
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets incoming transistions. </summary>
        ///
        /// <remarks>   Fleckj, 13.11.2015. </remarks>
        ///
        /// <returns>   The incoming transistions. </returns>
        #endregion
        public List<Transition> GetIncomingTransistions()
        {
            List<Transition> inTrans = new List<Transition>();

            XAttribute instanceAttr = this.ActivityNode.Attribute("instance");

            if (instanceAttr != null)
            {
                string instanceID = instanceAttr.Value;
                string xquery = "//transition[@to='{0}']";
                IEnumerable<XElement> incomingTrans = this.ActivityNode.Document.XPathSelectElements(string.Format(xquery, instanceID));
                if (incomingTrans != null)
                {
                    foreach (XElement trans in incomingTrans)
                    {
                        inTrans.Add(new Transition(trans));
                    }
                }
            }
            return inTrans;
        }

        public int GetConfiguredErrorTransistionCount()
        {
            int result = 0;
            List<Transition> outTr = this.GetOutgoingTransistions();
            
            foreach (Transition t in outTr)
            {
                if (t.IsErrorTransistion)
                    result++;
            }
            return result;
        }

        /// <summary>
        /// link from currentActivity to an ErrorActivity
        /// creates new XML
        /// </summary>
        /// <param name="targetActivityIntance"></param>
        public void CreateErrorTransitionToActivity(string targetActivityIntance)
        {
            XElement newTrans = new XElement(CONST_TransistionNodeName);
            newTrans.SetAttributeValue("to", targetActivityIntance);
            newTrans.SetAttributeValue("errorTransition","true");
            this.ActivityNode.Add(newTrans);
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets outgoing transistions. </summary>
        ///
        /// <remarks>   Fleckj, 13.11.2015. </remarks>
        ///
        /// <returns>   The outgoing transistions. </returns>
        #endregion
        public List<Transition> GetOutgoingTransistions()
        {
            List<Transition> outTrans = new List<Transition>();

            //alle transition nodes innerhalb activity
            IEnumerable<XElement> allOutTransitions = this.ActivityNode.XPathSelectElements(CONST_TransistionNodeName);
            if (allOutTransitions != null)
            {
                foreach (XElement trans in allOutTransitions)
                {
                    outTrans.Add(new Transition(trans));
                }
            }

            // it is necessary to have an order that transitions are processed !
            outTrans.Sort(new TransistionOrderClass());
            return outTrans;
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   All in coming transitions processing completed. </summary>
        ///
        /// <remarks>   Fleckj, 13.11.2015. </remarks>
        ///
        /// <param name="iterationNr">          The iteration nr. </param>
        /// <param name="currentTransition">    The current transition. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        #endregion
        public bool AllInComingTransitionsProcessingCompleted(string iterationNr, Transition currentTransition)
        {
            bool isCompleted = true;

            foreach (Transition incomingTrans in this.GetIncomingTransistions())
            {
                if (incomingTrans.IsProcessed() || Transition.AreEqual(incomingTrans, currentTransition))
                {
                    // transistion is completed
                }
                else
                    isCompleted = false;
            }

            return isCompleted;
        }

        public bool CheckIfOutGoingConnectionHasReturnValue(string sReturnValue)
        {
            //bool flag = this.ActivityNode.Document.Descendants().Cast<XElement>().Where<XElement>((XElement xe) =>
            //{
            //    if (xe.Name.LocalName == transistionNodeName && xe.Attribute("From") != null && xe.Attribute("From").Value != this.Id)
            //    {
            //        return false;
            //    }
            //    return xe.Attribute("label").Value == sReturnValue;
            //}).Count<XElement>() > 1;
            //return flag;

            throw new NotImplementedException();
        }


    }
}

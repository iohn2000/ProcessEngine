using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   The class Transition represents a connection between two activities. </summary>
    ///
    /// <remarks>   Fleckj, 09.02.2015. </remarks>
    #endregion
    public class Transition
    {
        private const string errorTransistionAttrbute = "errorTransition";
        private const string defaultTransistionAttrbute = "default";
        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets or sets the transition node. </summary>
        ///
        /// <value> The transition node. </value>
        #endregion
        public XElement TransitionNode { get; set; }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets the instance (name) where transistion comes from. </summary>
        ///
        /// <value> instance </value>
        #endregion
        public string FromActivityID
        {
            get
            {
                XElement activity = this.TransitionNode.Parent;
                if (activity != null)
                {
                    XAttribute id = activity.Attribute("instance");
                    if (id != null)
                        return id.Value;
                }
                return null; // fell through
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets or sets the target activity of transistion. </summary>
        ///
        /// <value> instance. </value>
        #endregion
        public string ToActivityID
        {
            get
            {
                if (this.TransitionNode != null)
                {
                    XAttribute id = this.TransitionNode.Attribute("to");
                    if (id != null)
                        return id.Value;
                }
                return null; // fell through                
            }
            set
            {
                this.TransitionNode.SetAttributeValue("to", value);
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets or sets the condition. </summary>
        ///
        /// <value> The condition. </value>
        #endregion
        public string Condition
        {
            get
            {
                XElement cond = this.TransitionNode.XPathSelectElement("condition");
                return cond != null ? cond.Value : "";
            }
            set
            {
                XElement cond = this.TransitionNode.XPathSelectElement("condition");
                cond.Value = value != null ? value : "";
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets or sets a value indicating whether this transition is the default one. </summary>
        ///
        /// <value> true if this object is default, false if not. </value>
        #endregion
        public bool IsDefault
        {
            get
            {
                XAttribute a = this.TransitionNode.Attribute(defaultTransistionAttrbute);
                string dummy = a != null ? a.Value : "false";
                bool isDef = false;
                bool.TryParse(dummy, out isDef);
                return isDef;
            }
            set
            {
                this.TransitionNode.SetAttributeValue(defaultTransistionAttrbute, value);
            }
        }

        /// <summary>
        /// return is this transistion is the error transition
        /// default is false
        /// </summary>
        public bool IsErrorTransistion
        {
            get
            {
                XAttribute a = this.TransitionNode.Attribute(errorTransistionAttrbute);
                string dummy = a != null ? a.Value : "false";
                bool isDef = false;
                bool.TryParse(dummy, out isDef);
                return isDef;
            }
            set
            {
                this.TransitionNode.SetAttributeValue(errorTransistionAttrbute, value);
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Fleckj, 16.11.2015. </remarks>
        ///
        /// <param name="transXml"> The transaction XML. </param>
        #endregion
        public Transition(XElement transXml)
        {
            this.TransitionNode = transXml;
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   follows the transistion back to the source activity </summary>
        ///
        /// <remarks>   Fleckj, 12.02.2015. </remarks>
        ///
        /// <returns>   the class Activity for the source of transistion. </returns>
        #endregion
        public Activity GetSourceActivity()
        {
            return new Activity(this.TransitionNode.Parent);
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Gets target activity. </summary>
        ///
        /// <remarks>   Fleckj, 17.11.2015. </remarks>
        ///
        /// <returns>   The target activity. </returns>
        #endregion
        public Activity GetTargetActivity()
        {
            string targetInstanceName = this.ToActivityID;
            XElement toElement = this.TransitionNode.Document.XPathSelectElement("workflow/activities/activity[@instance='" + targetInstanceName + "']");
            if (toElement != null)
                return new Activity(toElement);
            else
                return null;
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Creates a new executionIteration with iteration counter increased by 1 (last + 1)
        ///             A Transition also gets the ExecutionIteration node when executed.
        ///             This records the fact that it has been processed and how often (iteration)
        ///             also the place to write messages for documentation related to transistion.</summary>
        ///
        /// <remarks>   Fleckj, 12.02.2015. </remarks>
        ///
        /// <returns>   The next execution iteration. </returns>
        #endregion
        public ExecutionIteration GetNextExecutionIteration()
        {
            int num = this.TransitionNode.Elements("execution").Where<XElement>((XElement el) =>
            {
                if (el.Attribute("iteration") == null)
                {
                    return false;
                }
                return !string.IsNullOrWhiteSpace(el.Attribute("iteration").Value);
            }
            ).DefaultIfEmpty<XElement>().Max<XElement>((XElement i) =>
            {
                if (i == null)
                {
                    return 0;
                }
                return int.Parse(i.Attribute("iteration").Value);
            }) + 1;

            XElement xElement = new XElement("execution");
            this.TransitionNode.Add(xElement);
            xElement.SetAttributeValue("iteration", num.ToString());
            xElement.SetAttributeValue("processed", 0);
            xElement.Add(new XElement("messages"));
            xElement.Add(new XElement("variables"));

            return new ExecutionIteration(xElement);
        }

        /// <summary>
        /// returns if this transistion has been processed.
        /// the relates to the last execution iteration if there are multiple
        /// </summary>
        /// <returns></returns>
        public bool IsProcessed(/*string iteration*/)
        {
            int maxIteration;
            IEnumerable<XElement> all = this.TransitionNode.XPathSelectElements(".//execution");
            if (all != null && all.Count() > 0)
            {
                maxIteration = all.Max((e) =>
                {
                    XAttribute attrI = e.Attribute("iteration");
                    if (attrI != null)
                        return int.Parse(e.Attribute("iteration").Value);
                    else
                        return 1;
                });
            }
            else
            {
                return false;
            }

            XElement elIteration = this.TransitionNode.XPathSelectElement(".//execution[@iteration='" + maxIteration + "']");
            if (elIteration != null)
            {
                XAttribute processed = elIteration.Attribute("processed");
                if (processed != null && processed.Value == "1")
                    return true;
                else
                    return false;
            }
            else
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Cannot find Attribute processed in ExecutionIteration in Transition.\r\n" + this.TransitionNode.ToString());
            }
        }

        private Activity getActivity(string nodeAttribute)
        {

            IEnumerable<XElement> sActivity = this.TransitionNode.Document.Descendants()
            .Where<XElement>((XElement e) =>
            {
                if (e.HasAttributes)
                {
                    XAttribute a = e.Attribute("Id");
                    if (a != null)
                    {
                        if (a.Value == this.TransitionNode.Attribute(nodeAttribute).Value)
                            return true;
                    }
                }
                return false;
            });

            XElement ac = sActivity.FirstOrDefault();
            if (ac != null)
                return new Activity(ac);
            else
                return null;
        }

        public static bool AreEqual(Transition c1, Transition c2)
        {
            if (object.ReferenceEquals(c1, c2))
            {
                return true;
            }
            if (c1 == null || c2 == null)
            {
                return false;
            }
            if (c1.FromActivityID != c2.FromActivityID)
            {
                return false;
            }
            return c1.ToActivityID == c2.ToActivityID;
        }
        public static bool IsNotEqual(Transition c1, Transition c2)
        {
            if (c1 == null)
            {
                return false;
            }
            if (c2 == null)
            {
                return false;
            }
            if (c1.FromActivityID == c2.FromActivityID)
            {
                return false;
            }
            return c1.ToActivityID == c2.ToActivityID;
        }
    }
    internal class TransistionOrderClass : IComparer<Transition>
    {
        public int Compare(Transition x, Transition y)
        {
            return x.ToActivityID.CompareTo(y.ToActivityID);
        }
    }
}

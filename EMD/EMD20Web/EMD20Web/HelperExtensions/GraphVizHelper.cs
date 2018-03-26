using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;

namespace Kapsch.IS.EMD.EMD20Web.HelperExtensions
{
    public class GraphVizHelper
    {

        private string nodeTemplate = "{0} [label=\"{1}\", {2}, id=\"{3}\"]; \r\n"; // 2 = style 3 = id for javascript
        private string transitionTemplate = "{0} -> {1} [label=\"{2}\", id=\"{3}\", {4} ];\r\n"; // 4 = style e.g.  color=red, style=bold
        private XDocument wfModel;
        private string viz = "";

        public GraphVizHelper()
        {

        }

        public byte[] ReturnGraph(string wfXml, Dictionary<string, string> vizConfig, Enums.GraphReturnType graphReturnType)
        {
            this.viz = "digraph  G { overlap = scale;\r\n";
            this.wfModel = XDocument.Parse(wfXml);

            foreach (XElement a in this.wfModel.XPathSelectElements("//activity"))
            {
                //check if activity has correct parrent
                if (a.Parent.Name.LocalName != "activities")
                {
                    throw new Exception(a.Attribute("instance").Value + " has wrong parent.");
                }


                string nodeShape = "shape=\"box\"";
                string elementNamespace = a.Attribute("id").Value;
                elementNamespace = elementNamespace == null ? "default" : elementNamespace;

                if (vizConfig.ContainsKey(elementNamespace)) nodeShape = vizConfig[elementNamespace];

                string instanceName = a.Attribute("instance").Value;
                string activityText = "";
                XElement activityName = a.XPathSelectElement("name");

                activityText = instanceName;

                if (activityName != null)
                {
                    //add name to activity box
                    activityText = instanceName + "\r\n" + activityName.Value;
                }

                this.viz += string.Format(nodeTemplate,
                    this.formatNameForGraphViz(instanceName, enumGraphvizNameType.NodeName),
                    this.formatNameForGraphViz(activityText, enumGraphvizNameType.Label),
                    nodeShape,
                    this.formatNameForGraphViz(instanceName, enumGraphvizNameType.Id));
            }

            foreach (XElement t in this.wfModel.XPathSelectElements("//transition"))
            {

                string condition = "", fromA;
                XAttribute fA = t.Parent.Attribute("instance");
                if (fA != null)
                    fromA = fA.Value;
                else
                    fromA = "error";
                XElement cond = t.XPathSelectElement("condition");
                if (cond != null) condition = cond.Value;
                string toA = t.Attribute("to").Value;
                bool isErrorTransistion;
                string tranistionStyle;
                try
                {
                    isErrorTransistion = t.Attribute("errorTransition") != null ? t.Attribute("errorTransition").Value.ToLowerInvariant() == "true" : false;
                }
                catch (Exception)
                {
                    isErrorTransistion = false;
                }
                if (isErrorTransistion)
                {
                    // format arrow red
                    tranistionStyle =  "color=red"; //", style=bold";
                }
                else
                {
                    // make black
                    tranistionStyle ="color=black";
                }
                string transition = string.Format(this.transitionTemplate,
                                              this.formatNameForGraphViz(fromA, enumGraphvizNameType.NodeName),
                                              this.formatNameForGraphViz(toA, enumGraphvizNameType.NodeName),
                                              this.formatNameForGraphViz(condition, enumGraphvizNameType.Label),
                                              this.formatNameForGraphViz(fromA + toA, enumGraphvizNameType.Id),
                                              tranistionStyle);
                this.viz += transition;
            }
            this.viz += "\r\n}";

            //Console.WriteLine(this.viz); 
         //   File.WriteAllText("output.txt", this.viz);

            // START graphviz wrapper

            // These three instances can be injected via the IGetStartProcessQuery, 
            //                                               IGetProcessStartInfoQuery and 
            //                                               IRegisterLayoutPluginCommand interfaces

            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            // GraphGeneration can be injected via the IGraphGeneration interface

            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);
            wrapper.GraphvizPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "graphviz");

            wrapper.RenderingEngine = Enums.RenderingEngine.Dot;
            byte[] output = wrapper.GenerateGraph(this.viz, graphReturnType);
          

            return output;
        }

        private string formatNameForGraphViz(string input, enumGraphvizNameType nameType /*bool isLabel*/)
        {
            string ret = input;

            switch (nameType)
            {
                case enumGraphvizNameType.NodeName:
                    ret = input.Replace(".", "");
                    ret = "_" + ret;
                    break;
                case enumGraphvizNameType.Label:
                    ret = ret.Replace("\"", "'");
                    break;
                case enumGraphvizNameType.Id:
                    ret = input.Replace(".", "");
                    ret = ret.Replace("\"", "");
                    ret = ret.Replace("'", "");
                    ret = "_" + ret;
                    break;
                default:
                    break;
            }
            return ret;
        }

    }


    internal enum enumGraphvizNameType
    {
        NodeName,
        Label,
        Id
    }
}
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;

namespace Kapsch.IS.EDP.Core.Utils
{
    public class EnterpriseTreeBuilder
    {

        private string nodePushTemplate = "nodes.push({{id:{0},label:'{1}', shape: 'box'}});";
        private string edgePushTemplate = "edges.push({{from: {0}, to: {1} }});";
        private string nodeLevelTemplate = "nodes[{0}][\"level\"] = {1};";
        private string rootID = "";

        public EnterpriseTreeBuilder()
        {

        }

        /// <summary>
        /// build part of javascript that created the nodes and edges
        /// still needs to be put into a html/javascript template file to make it work.
        /// see EnterpiseTreeTemplate.html attached to EDP20Core Solution
        /// ----
        /// 2 javascript arrays 'nodes' and 'edges' are created and must be used in surrounding html/js code
        /// </summary>
        /// <param name="rootID"></param>
        /// <returns></returns>
        private string BuildJavascriptForTree(string rootID = "5")
        {
            this.rootID = rootID;
            string jsPart = "";
            EnterpriseHandler enteH = new EnterpriseHandler();
            List<IEMDObject<EMDEnterprise>> allEnterprises
                       = (List<IEMDObject<EMDEnterprise>>) enteH.GetObjects<EMDEnterprise, Enterprise>("E_ID_Root = " + this.rootID);
            for (int idx = 0; idx < allEnterprises.Count; idx++)
            {
                EMDEnterprise e = (EMDEnterprise) allEnterprises[idx];
                string oneEnte = string.Format(nodePushTemplate, e.E_ID.ToString(), e.NameShort);
                jsPart += oneEnte + Environment.NewLine;
            }

            for (int idx = 0; idx < allEnterprises.Count; idx++)
            {
                string oneLevel = "";
                string oneEdge = "";

                EMDEnterprise enterpriseNode = (EMDEnterprise) allEnterprises[idx];
                // dont let root node point to itself
                if (enterpriseNode.E_ID != enterpriseNode.E_ID_Parent)
                {

                    oneEdge = string.Format(edgePushTemplate,
                                                    enterpriseNode.E_ID.ToString(),
                                                    enterpriseNode.E_ID_Parent.ToString());
                }
               

                oneLevel = string.Format(nodeLevelTemplate,
                                                    idx.ToString(),
                                                    this.calcNodeLevel(allEnterprises, enterpriseNode));


                jsPart += oneEdge + Environment.NewLine + oneLevel + Environment.NewLine;
            }


            return jsPart;
        }
        /// <summary>
        /// builds a html page to display enterprise tree.
        /// if no html template is given a default one will be used
        /// </summary>
        /// <param name="rootID"></param>
        /// <returns></returns>
        public string GetEnterpriseTreeHtmlPart(string rootID  ="5", string htmlTemplate = defaultHtmlTemplate)
        {
            string js = this.BuildJavascriptForTree(rootID);
            return htmlTemplate.Replace("// GraphJavascript Here //",js);
        }

        /// <summary>
        /// find out the level of node (how deep from root)
        /// </summary>
        /// <param name="allEnterprises"></param>
        /// <param name="currentNode"></param>
        /// <returns></returns>
        private string calcNodeLevel(List<IEMDObject<EMDEnterprise>> allEnterprises, EMDEnterprise currentNode)
        {
            string currentParentID = currentNode.E_ID_Parent.ToString();

            if (currentNode.E_ID.ToString() == this.rootID)
                return "0";

            if (currentParentID == this.rootID)
                return "1";

            int level = 1;
            EMDEnterprise nextNodeUp = null;

            do
            {
                // find parent
                nextNodeUp = (EMDEnterprise) allEnterprises.Find(m => ((EMDEnterprise) m).E_ID.ToString() == currentParentID);
                currentParentID = nextNodeUp.E_ID_Parent.ToString();
                level++;
            } while (currentParentID != this.rootID);

            return level.ToString();
        }

        #region default html template
        private const string defaultHtmlTemplate = @"
    <script type=""text/javascript"">
        var nodes = null;
        var edges = null;
        var network = null;
        var directionInput = 'UD';

        function destroy() {
            if (network !== null) {
                network.destroy();
                network = null;
            }
        }

        function draw() {
            destroy();
            nodes = [];
            edges = [];
            var connectionCount = [];

            // GraphJavascript Here //

            // create a network
            var container = document.getElementById('treeview');
            var data = {nodes: nodes, edges: edges};

            var options = {
                edges: {
                    smooth: {
                        type: 'cubicBezier',
                        forceDirection: (directionInput.value == ""UD"" || directionInput.value == ""DU"") ? 'vertical' : 'horizontal',
                        roundness: 0.4
                    }
                },
                layout: {
                    hierarchical: {
                        direction: directionInput.value
                    }
                },
                physics:false
            };
            network = new vis.Network(container, data, options);

            // add event listeners
            network.on('select', function (params) {
                document.getElementById('selection').innerHTML = 'Selection: ' + params.nodes;
            });
        }

    </script>
";
        #endregion
    }
}

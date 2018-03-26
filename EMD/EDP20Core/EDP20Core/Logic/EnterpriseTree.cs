using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class EnterpriseTree : EntityTree<EMDEnterprise>
    {
        private List<IEMDObject<EMDEnterprise>> enterpriseList = null;
        public List<IEMDObject<EMDEnterprise>> EnterpriseList
        {
            get
            {
                return enterpriseList;
            }
        }
        private EntityTreeNode<EMDEnterprise> GetOrAddParent(EMDEnterprise entity)
        {
            EntityTreeNode<EMDEnterprise> result = this.Root.FindByGuid(entity.Guid);
            if (result == null)
            {
                EMDEnterprise parent = (from ente1 in enterpriseList
                                        where ente1.Guid == entity.Guid_Parent
                                        select (EMDEnterprise)ente1).FirstOrDefault();

                if (parent != null)
                {
                    EntityTreeNode<EMDEnterprise> parentNode = GetOrAddParent(parent);
                    result = this.Add(entity, parentNode);
                }
            }

            return result;
        }
        public override void Fill(IEMDObject<EMDEnterprise> rootEntity)
        {
            EMDEnterprise re = (EMDEnterprise)rootEntity;
            //check whether the given entity has parents and result an error if so
            if (re.Guid_Parent != re.Guid)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY, "The given enterprise is not a root enterprise (parent_guid must be equal guid)");
            } else
            {
                this.Root = new EntityTreeNode<EMDEnterprise>(re);
            }

            if (enterpriseList == null)
            {
                EnterpriseHandler eh = new EnterpriseHandler();
                DatabasePaging dp = new DatabasePaging();
                enterpriseList = eh.GetObjects<EMDEnterprise, Enterprise>("Guid_Root = \"" + rootEntity.Guid + "\"");                
            }

            //iterate whole list and add all nodes
            foreach(IEMDObject<EMDEnterprise> ente in enterpriseList)
            {
                if (ente.Guid != Root.Entity.Guid && ((EMDEnterprise)ente).Guid_Parent!=null )
                {
                    EMDEnterprise parent = (from ente1 in enterpriseList
                                            where ente1.Guid == ((EMDEnterprise)ente).Guid_Parent
                                            select (EMDEnterprise)ente1).FirstOrDefault();

                    if (parent != null)
                    {
                        EntityTreeNode<EMDEnterprise> parentnode = this.GetOrAddParent(parent);
                        if (parentnode != null)
                        {
                            if (this.Root.FindByGuid(ente.Guid) == null)
                                this.Add((EMDEnterprise)ente, parentnode);
                        }
                    }
                    
                }
            }

        }

        public override string Print()
        {
            String result = this.PrintNodeRecursive(Root,0);
            return result;
        }

        private string PrintNodeRecursive(EntityTreeNode<EMDEnterprise> node, int level, string indent="", bool last=true)
        {
            String result = indent;
            if (last)
            {
                result += ("\\-");
                indent += "    ";
            }
            else
            {
                result += ("|-");
                indent += "|   ";
            }

            result += Convert.ToString(level)+":"+((EMDEnterprise)node.Entity).NameShort+"\n";

            int i = 0;
            if (node.FirstChild != null) {
                List<EntityTreeNode<EMDEnterprise>> children = node.FirstChild.GetAllSiblings(new List<EntityTreeNode<EMDEnterprise>>());
                foreach (EntityTreeNode<EMDEnterprise> child in children) {
                    result += PrintNodeRecursive(child, level + 1, indent+children.Count, i == children.Count-1);
                    i++;
                }
            }
            return result;
        }
    }
}

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
    public class OrgUnitTree : EntityTree<EMDOrgUnit>
    {
        private List<IEMDObject<EMDOrgUnit>> orgUnitList = null;
        public List<IEMDObject<EMDOrgUnit>> OrgUnitList
        {
            get
            {
                return orgUnitList;
            }
        }
        private EntityTreeNode<EMDOrgUnit> GetOrAddParent(EMDOrgUnit entity)
        {
            EntityTreeNode<EMDOrgUnit> result = this.Root.FindByGuid(entity.Guid);
            if (result == null)
            {
                EMDOrgUnit parent = (from ente1 in orgUnitList
                                        where ente1.Guid == entity.Guid_Parent
                                        select (EMDOrgUnit)ente1).FirstOrDefault();

                if (parent != null)
                {
                    EntityTreeNode<EMDOrgUnit> parentNode = GetOrAddParent(parent);
                    result = this.Add(entity, parentNode);
                }
            }

            return result;
        }
        public override void Fill(IEMDObject<EMDOrgUnit> rootEntity)
        {
            EMDOrgUnit re = (EMDOrgUnit)rootEntity;
            //check whether the given entity has parents and result an error if so
            if (re.Guid_Parent != re.Guid)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_ENTITY, "The given enterprise is not a root enterprise (parent_guid must be equal guid)");
            }
            else
            {
                this.Root = new EntityTreeNode<EMDOrgUnit>(re);
            }

            if (orgUnitList == null)
            {
                EnterpriseHandler eh = new EnterpriseHandler();
                DatabasePaging dp = new DatabasePaging();
                orgUnitList = eh.GetObjects<EMDOrgUnit, OrgUnit>("Guid_Root = \"" + rootEntity.Guid + "\"");
            }

            //iterate whole list and add all nodes
            foreach (IEMDObject<EMDOrgUnit> orgu in orgUnitList)
            {
                if (orgu.Guid != Root.Entity.Guid && ((EMDOrgUnit)orgu).Guid_Parent != null)
                {
                    EMDOrgUnit parent = (from ente1 in orgUnitList
                                            where ente1.Guid == ((EMDOrgUnit)orgu).Guid_Parent
                                            select (EMDOrgUnit)ente1).FirstOrDefault();

                    if (parent != null)
                    {
                        EntityTreeNode<EMDOrgUnit> parentnode = this.GetOrAddParent(parent);
                        if (parentnode != null)
                        {
                            if (this.Root.FindByGuid(orgu.Guid) == null)
                                this.Add((EMDOrgUnit)orgu, parentnode);
                        }
                    }
                    
                }
            }

        }

        public override string Print()
        {
            String result = this.PrintNodeRecursive(Root, 0);
            return result;
        }

        private string PrintNodeRecursive(EntityTreeNode<EMDOrgUnit> node, int level, string indent = "", bool last = true)
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

            result += Convert.ToString(level) + ":" + ((EMDOrgUnit)node.Entity).Name + "\n";

            int i = 0;
            if (node.FirstChild != null)
            {
                List<EntityTreeNode<EMDOrgUnit>> children = node.FirstChild.GetAllSiblings(new List<EntityTreeNode<EMDOrgUnit>>());
                foreach (EntityTreeNode<EMDOrgUnit> child in children)
                {
                    result += PrintNodeRecursive(child, level + 1, indent + children.Count, i == children.Count - 1);
                    i++;
                }
            }
            return result;
        }
    }
}

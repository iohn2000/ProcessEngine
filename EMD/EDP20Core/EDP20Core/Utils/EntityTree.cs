using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Utils
{
    public class EntityTree<T>
    {
        private EntityTreeNode<T> root;
        public EntityTree()
        {
            root = null;
        }

        public virtual void Clear()
        {
            root = null;
        }

        public virtual String Print()
        {
            return "not implemented";
        }

        /// <summary>
        /// Not implemented for the baseclass, only adds the given Element as root without further connecting
        /// </summary>
        public virtual void Fill(IEMDObject<T> rootEntity)
        {
            this.root = new EntityTreeNode<T>((T)rootEntity);
        }
        
        public EntityTreeNode<T> Root
        {
            get
            {
                return root;
            }
            set
            {
                root = value;
            }
        }

        public EntityTreeNode<T> Add(T element, EntityTreeNode<T> parent)
        {
            EntityTreeNode<T> result = new EntityTreeNode<T>(element);
            if (parent.FirstChild == null)
            {
                parent.FirstChild = result;
            }
            else
            {
                parent.FirstChild.GetLastSibling().NextSibling = result;
            }
            return result;
        }

        public List<T> GetAllChildrenOf(String guid)
        {
            List<T> result = new List<T>();
            List<EntityTreeNode<T>> nodes = new List<EntityTreeNode<T>>();

            // such das Element
            EntityTreeNode<T> first = Root.FindByGuid(guid);

            // nicht gefunden
            if (first == null) return result;
            // keine Kinder
            if (first.FirstChild == null) return result;

            //nimm das erste Kind und hole alle Elemente ab hier
            nodes = first.FirstChild.GetAllElements(nodes);

            foreach (EntityTreeNode<T> node in nodes)
            {
                result.Add((T)node.Entity);
            }

            return result;
        }        

    }

    public class EntityTreeNode<T> : TreeNode<T>
    {
        public EntityTreeNode(): base(){}
        public EntityTreeNode(T entity) : base(entity, null) { }
        public EntityTreeNode(T entity, EntityTreeNode<T> firstChild, EntityTreeNode<T> nextSibling)
        {
            base.Entity = (IEMDObject<T>)entity;
            EntityTreeNodeList<T> children = new EntityTreeNodeList<T>(2);
            children[0] = firstChild;
            children[1] = nextSibling;

            base.Children = children;
        }

        public EntityTreeNode<T> FirstChild
        {
            get
            {
                if (base.Children == null)
                    return null;
                else
                    return (EntityTreeNode<T>)base.Children[0];
            }
            set
            {
                if (base.Children == null)
                    base.Children = new EntityTreeNodeList<T>(2);

                base.Children[0] = value;
            }
        }

        /// <summary>
        /// Next Neighbour (element in thze same 
        /// </summary>
        public EntityTreeNode<T> NextSibling
        {
            get
            {
                if (base.Children == null)
                    return null;
                else
                    return (EntityTreeNode<T>)base.Children[1];
            }
            set
            {
                if (base.Children == null)
                    base.Children = new EntityTreeNodeList<T>(2);

                base.Children[1] = value;
            }
        }

        internal List<EntityTreeNode<T>> GetAllSiblings(List<EntityTreeNode<T>> result)
        {
            result.Add(this);
            if (this.NextSibling != null)
                result = this.NextSibling.GetAllSiblings(result);
            return result;
        }

        /// <summary>
        /// Find a specific node with a given guid beyond or at the neighbours
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>null if nothing was found</returns>
        public EntityTreeNode<T> FindByGuid(String guid)
        {
            EntityTreeNode<T> result = null;
            //prüfe ob der Node selbst der gesuchte ist.
            if (this.Entity.Guid == guid) result = this;
            //iteriere durch FirstChilds
            if (result == null)
            {
                if (this.FirstChild != null)
                {
                    result = this.FirstChild.FindByGuid(guid);
                }
            }
            //wenn keines davon fündig wurde prüfe NextSiblings
            if (result == null)
            {
                if (this.NextSibling != null)
                {
                    result = this.NextSibling.FindByGuid(guid);
                }
            }

            return result;
        }

        /// <summary>
        /// get All Elements starting with this node, all children beyond and all siblings to the right.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public List<EntityTreeNode<T>> GetAllElements(List<EntityTreeNode<T>> result)
        {
            result.Add(this);
            if (this.FirstChild != null)
                result = this.FirstChild.GetAllElements(result);
            if (this.NextSibling != null)
                result = this.NextSibling.GetAllElements(result);
            return result;
        }

        /// <summary>
        /// Get the last Sibling of the Node e.g. to add another one. If there are no Siblings returns the node itself.
        /// </summary>
        /// <returns></returns>
        public EntityTreeNode<T> GetLastSibling()
        {
            if (this.NextSibling == null)
                return this;
            else
                return this.NextSibling.GetLastSibling();
        }

        /// <summary>
        /// checks whether the given Element is a parent of the instance
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool IsParentOf(string guid)
        {
            if (this.FirstChild == null)
                return false;
            else
            {
                if (this.FirstChild.FindByGuid(guid) != null)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// checks whether the given Element is a child of the instance
        /// </summary>
        /// <param name="parentnode"></param>
        /// <returns></returns>        
        public bool IsChildOf(EntityTreeNode<T> parentnode) {
            return parentnode.IsParentOf(this.Entity.Guid);
        }

    }

    public class EntityTreeNodeList<T> : TreeNodeList<T>
    {
        public EntityTreeNodeList(): base(){}
        /// <summary>
        /// creates an EntityTreeNodeList filled a number of "initialSize" empty constructed Elements
        /// </summary>
        /// <param name="initialSize"></param>
        public EntityTreeNodeList(int initialSize) : base(initialSize) { }

        /// <summary>
        /// find element with by given Guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>returns found entity or null if nothing was found</returns>
        public TreeNode<T> FindByGuid(String guid)
        {
            foreach (TreeNode<T> node in Items)
                if (node.Entity.Guid.Equals(guid))
                    return node;
            return null;
        }
    }

    public class TreeNode<T>
    {
        private IEMDObject<T> entity;
        private TreeNodeList<T> children = null;

        public IEMDObject<T> Entity {
            get { return entity; }
            set { entity = value; }
        }

        protected TreeNodeList<T> Children
        {
            get { return children; }
            set { children = value; }
        }

        public TreeNode(){}
        public TreeNode(T entity): this(entity, null){}
        public TreeNode(T entity, TreeNodeList<T> children)
        {
            this.entity = (IEMDObject<T>)entity;                        
        }

    }

    public class TreeNodeList<T>: Collection<TreeNode<T>>
    {
        /// <summary>
        /// creates an empty EntityTreeNodeList
        /// </summary>
        public TreeNodeList() : base() { }

        /// <summary>
        /// creates an EntityTreeNodeList filled a number of "initialSize" empty constructed Elements
        /// </summary>
        /// <param name="initialSize"></param>
        public TreeNodeList(int initialSize)
        {
            for (int i = 0; i < initialSize; i++)
                base.Items.Add(default(TreeNode<T>));
        }



    }
}

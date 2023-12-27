using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ConfluenceAccess
{
    internal class ConfluenceTree : ITree
    {
        public ConfluenceNode Root { get; private set; }

        private Dictionary<long, ConfluenceNode> NodeLookupById { get; } = new Dictionary<long, ConfluenceNode>();

        public ConfluenceTree(long rootId, string rootTitle, string baseUrl, string confluenceSpace)
        {
            Root = new ConfluenceNode(this, rootId, rootTitle, null, baseUrl, confluenceSpace);
            NodeLookupById.Add(rootId, Root);
        }
        

        override public string ToString()
        {
            //recursively print the tree
            StringBuilder builder = new StringBuilder();
            RecursivelyPrintTree(this.Root, builder, 0);
            return builder.ToString();

        }

        private void RecursivelyPrintTree(ConfluenceNode nodes, StringBuilder builder, int tabLevel)
        {
            string tab = new string('\t', tabLevel);
            foreach (ConfluenceNode node in nodes.Children)
            {
                builder.AppendLine($"{tab}{node.ToString()}");
                RecursivelyPrintTree(node, builder, tabLevel + 1);
            }
        }

        public void ReplaceRoot(ConfluenceNode newRoot)
        {
            this.Root = newRoot;
            this.Root.Parent = null;
            NodeLookupById.Remove(0);
            
        }

        public ConfluenceNode GetNodeById(long id)
        {
            return NodeLookupById[id];
        }

        internal void RegisterNode(ConfluenceNode child)
        {
            NodeLookupById.Add(child.ID, child);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ConfluenceAccess
{
    public class ConfluenceTree : ITree
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

        [Flags]
        public enum NodeAction
        {
            NoAction = 0,
            Create = 1,
            Update = 2,
            Move = 4,
            Delete = 8
        }

        public static List<(ConfluenceNode Node, NodeAction Action)> CompareSourceAndDestTrees(ConfluenceTree source, ConfluenceTree dest)
        {
            List<(ConfluenceNode Node, NodeAction Action)> actions = new List<(ConfluenceNode Node, NodeAction Action)>();

            foreach (ConfluenceNode destNode in dest.NodeLookupById.Values)
            {
                if (source.NodeLookupById.ContainsKey(destNode.ID) == false)
                {
                    actions.Add((destNode, NodeAction.Create));
                }
            }
            
            foreach (ConfluenceNode sourceNode in source.NodeLookupById.Values)
            {
                NodeAction action = NodeAction.NoAction;

                if (dest.NodeLookupById.ContainsKey(sourceNode.ID) == false)
                {
                    action |= NodeAction.Delete;
                    actions.Add((sourceNode, action));
                    continue;
                }

                ConfluenceNode destNode = dest.NodeLookupById[sourceNode.ID];
                if (sourceNode.Title != destNode.Title)
                {
                    action |= NodeAction.Update;
                    actions.Add((sourceNode, action));
                }

                if (sourceNode.Parent != null && sourceNode.Parent.ID != destNode.Parent.ID)
                {
                    action |= NodeAction.Move;
                    actions.Add((sourceNode, action));
                }
            }

            return actions;
        }
    }
}
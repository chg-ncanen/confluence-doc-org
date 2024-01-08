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

        internal void UnregisterNode(ConfluenceNode child)
        {
            NodeLookupById.Remove(child.ID);
        }


        public class ConfluenceAction
        {
            public NodeAction Action { get; set; }
            public ConfluenceNode OldNode { get; set; }
            public ConfluenceNode NewNode { get; set; }

            public ConfluenceAction(NodeAction action, ConfluenceNode oldNode, ConfluenceNode newNode)
            {
                Action = action;
                OldNode = oldNode;
                NewNode = newNode;
            }

            public override string ToString()
            {
                if (Action == NodeAction.Delete)
                {
                    return $"[{Action}] {OldNode}";
                }
                else if (Action == NodeAction.Create)
                {
                    return $"[{Action}] {NewNode} --> UNDER {NewNode.Parent}";
                }
                else
                {
                    if ((Action & NodeAction.Update) == NodeAction.Update)
                    {
                        return $"[{Action}] {OldNode} --> {NewNode.Title}";
                    }

                    if ((Action & NodeAction.Move) == NodeAction.Move)
                    {
                        return $"[{Action}] {OldNode} --> UNDER {NewNode.Parent}";
                    }
                }

                return $"[{Action}] {OldNode} --> {NewNode}";
            }
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

        public static List<ConfluenceAction> CompareSourceAndDestTrees(ConfluenceTree source, ConfluenceTree dest)
        {
            var actions = new List<ConfluenceAction>();

            foreach (ConfluenceNode destNode in dest.NodeLookupById.Values)
            {
                if (source.NodeLookupById.ContainsKey(destNode.ID) == false)
                {
                    actions.Add(new ConfluenceAction(NodeAction.Create, null, destNode));
                }
            }
            
            foreach (ConfluenceNode sourceNode in source.NodeLookupById.Values)
            {
                NodeAction action = NodeAction.NoAction;

                if (dest.NodeLookupById.ContainsKey(sourceNode.ID) == false)
                {
                    action = NodeAction.Delete;
                    actions.Add(new ConfluenceAction(action, sourceNode, null));
                    continue;
                }

                ConfluenceNode destNode = dest.NodeLookupById[sourceNode.ID];
                if (sourceNode.Title != destNode.Title)
                {
                    action |= NodeAction.Update;
                }

                if (sourceNode.Parent != null && sourceNode.Parent.ID != destNode.Parent.ID)
                {
                    action |= NodeAction.Move;
                }

                if (action != NodeAction.NoAction)
                {
                    actions.Add(new ConfluenceAction(action, sourceNode, destNode));
                }
            }

            return actions;
        }

        internal ConfluenceNode Remove(long nodeID)
        {

            var nodeToRemove = GetNodeById(nodeID);
            if (nodeToRemove == null) return null;

            if (nodeToRemove.Parent == null)
            {
                throw new ArgumentException("Cannot remove root node");
            }

            UnregisterNode(nodeToRemove);
            nodeToRemove.Parent.Children.Remove(nodeToRemove);
            return nodeToRemove;
        }

        internal void Move(long nodeID, long toParentID)
        {
            var nodeMoving = Remove(nodeID);
            var newParent = GetNodeById(toParentID);
            newParent.Add(nodeMoving);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Windows.Forms.LinkLabel;

namespace ConfluenceAccess
{

    internal class ConfluenceMindMap
    {
        public string CreateXmlFromTree(ConfluenceTree tree)
        {
            var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            StringBuilder builder = new StringBuilder();
            builder
                .AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>")
                .AppendLine("<map version=\"0.8.1\">");
            if (tree.Root != null) { AppendMindMapNodeXml(tree.Root, builder, time); }
            builder.AppendLine("</map>");

            return builder.ToString();
        }

        private static void AppendMindMapNodeXml(ConfluenceNode node, StringBuilder builder, long time)
        {
            builder
                .Append($"""<node CREATED="{time}" ID="{node.NodeID}" MODIFIED="{time}" TEXT="{System.Security.SecurityElement.Escape(node.Title)}" """)
                .Append($"""LINK = "{node.Url}" """)
                .AppendLine(node.Children.Count() == 0 ? "/>" : ">");

            foreach (ConfluenceNode n in node.Children)
            {
                AppendMindMapNodeXml(n, builder, time);
            }

            builder.Append(node.Children.Count() == 0 ? "" : "</node>\r\n");
        }

        public static async Task<ConfluenceTree> ReadMindMap(string file, string confluenceUrl, string confluenceSpace)
        {
            ConfluenceTree tree = new ConfluenceTree(0, "", confluenceUrl, confluenceSpace);
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            //recursively read the node elements heirarchy and print in heirarchal view, with tabs to show depth
            XmlNodeList nodes = doc.ChildNodes[1].ChildNodes;
            RecursivelyReadMindMapXml(tree.Root, nodes);
            tree.ReplaceRoot(tree.Root.Children.First());
            return tree;
        }

        static int nextUnknownId = -1;
        private static void RecursivelyReadMindMapXml(ConfluenceNode currentTreeNode, XmlNodeList nodes, int tabLevel = 0)
        {

            if (nodes == null) { return; }
            string tab = new string('\t', tabLevel);
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "icon")
                {
                    continue;
                }
                string url = node.Attributes["LINK"]?.Value;
                long id = !string.IsNullOrWhiteSpace(url) ? long.Parse(url.Substring(url.LastIndexOf('/') + 1)) : nextUnknownId--;
                string title = node.Attributes["TEXT"].Value;
                ConfluenceNode newNode = currentTreeNode.Add(id, title);
                RecursivelyReadMindMapXml(newNode, node.ChildNodes, tabLevel + 1);
            }
        }

    }
}
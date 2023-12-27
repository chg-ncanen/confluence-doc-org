using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfluenceAccess.MindMap
{
    internal class Tree
    {
        public Node Root { get; internal set; }   
        
        public Tree(long rootId, string rootTitle, string baseUrl, string confluenceSpace)
        {
            Root = new Node(rootId, rootTitle, null, baseUrl, confluenceSpace);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder
                .AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>")
                .AppendLine("<map version=\"0.8.1\">")
                    .Append(Root != null ? Root.ToString() : "")
                .AppendLine("</map>");

            return builder.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConfluenceAccess
{
    internal class Node
    {
        static Random random = new Random();
        static string GenerateUniqueAlphaNumericString(int length)
        {
            const string validChars = "123456789abcdefghijklmnopqrstuvwxyz";

            string uniqueString = new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());

            return uniqueString;
        }

        private string NodeID { get; } = GenerateUniqueAlphaNumericString(26);

        public long ID { get; }
        public string Title { get; }

        public Node Parent { get; }

        public IList<Node> Children { get; } = new List<Node>();

        public Node Add(long id, string title)
        {
            Node child = new Node(id, title, this);
            Children.Add(child);
            return child;
        }

        public Node(long id, string title, Node parent = null)
        {
            this.ID = id;
            this.Title = title;
            this.Parent = parent;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            BuildXMLString(builder);
            return builder.ToString();
        }

        private void BuildXMLString(StringBuilder builder)
        {
            builder
                .Append($"<node CREATED=\"1697068041472\" ID=\"{this.NodeID}\" LINK=\"https://chghealthcare.atlassian.net/wiki/content/{ID}\" MODIFIED=\"1697068041472\" TEXT=\"{System.Security.SecurityElement.Escape(this.Title)}\"")
                .AppendLine(this.Children.Count() == 0 ? "/>" : ">");

            foreach (Node n in this.Children)
            {
                builder.Append(n.ToString());
            }

            builder.Append(this.Children.Count() == 0 ? "" : "</node>\r\n");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConfluenceAccess
{
    public class ConfluenceNode
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

        public string NodeID { get; } = GenerateUniqueAlphaNumericString(26);

        public long ID { get; }
        public string Title { get; }
        public ConfluenceNode Parent { get; set; }
        public string BaseUrl { get; }
        public string ConfluenceSpace { get; }
        public IList<ConfluenceNode> Children { get; } = new List<ConfluenceNode>();

        public ConfluenceTree Tree { get; }

        public string Url => $"{BaseUrl}/spaces/{ConfluenceSpace}/pages/{ID}";

        public ConfluenceNode Add(long id, string title)
        {
            ConfluenceNode child = new ConfluenceNode(Tree, id, title, this, this.BaseUrl, this.ConfluenceSpace);
            Children.Add(child);
            Tree.RegisterNode(child);
            return child;            
        }

        public ConfluenceNode(ConfluenceTree tree, long id, string title, ConfluenceNode parent, string baseUrl, string confluenceSpace)
        {
            this.ID = id;
            this.Title = title;
            this.Parent = parent;
            this.BaseUrl = baseUrl.EndsWith(@"/") == false ? baseUrl : baseUrl.Substring(baseUrl.Length - 2);
            this.ConfluenceSpace = confluenceSpace;
            this.Tree = tree;
        }

        public override string ToString()
        {
            return "[" + this.ID + "]" + Title + $" ({this.Url})";
        }
    }
}
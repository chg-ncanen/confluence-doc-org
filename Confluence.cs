using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfluenceAccess;
using Dapplo.Confluence;
using Dapplo.Confluence.Entities;

namespace ConfluenceAccess
{
    internal class Confluence
    {
        public string ConfluenceBaseUrl { get; }
        public string ConfluenceSpace { get; }
        public IConfluenceClient Client { get; }
        public int PagingSize { get; set; } = 100;

        public Confluence(string apiUser, string apiToken, string url, string space)
        {
            ConfluenceBaseUrl = url;
            ConfluenceSpace = space;
            Client = ConfluenceClient.Create(new Uri(ConfluenceBaseUrl));
            Client.SetBasicAuthentication(apiUser, apiToken);
        }

        public async Task<ConfluenceTree> GetTree(int confluenceTopPage)
        {
            PagingInformation paging = new PagingInformation() { Limit = this.PagingSize };

            var content = await Client.Content.GetAsync(confluenceTopPage);

            Queue<ConfluenceNode> queue = new Queue<ConfluenceNode>();

            ConfluenceTree tree = new ConfluenceTree(content.Id, content.Title, ConfluenceBaseUrl, ConfluenceSpace);
            queue.Enqueue(tree.Root);

            int count = 0;
            while (queue.Any() && count++ < 40)
            //while (queue.Any())
            {
                ConfluenceNode node = queue.Dequeue();
                var confluenceChildren = await Client.Content.GetChildrenAsync(node.ID, paging);

                //Console.WriteLine(confluenceChildren.Count());
                foreach (var c in confluenceChildren)
                {
                    ConfluenceNode newChild = node.Add(c.Id, c.Title);
                    queue.Enqueue(newChild);

                    Console.WriteLine($"{c.Id}: {c.Title}");
                }
            }

            return tree;
        }
    }
}
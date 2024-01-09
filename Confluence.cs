using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using ConfluenceAccess;
using Dapplo.Confluence;
using Dapplo.Confluence.Entities;
using Dapplo.Confluence.Query;

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

        public async Task<bool> MovePage(long pageId, long newParentId)
        {
            if (pageId == newParentId)
            {
                return true;
            }

            if (pageId < 0 || newParentId < 0)
            {
                //this is the case when one or both of the pages have not been created yet
                return false;
            }

            string result = null;

            try
            {
                result = await Client.Content.MoveAsync(pageId, Positions.Append, newParentId);
            }
            catch (Dapplo.Confluence.ConfluenceException ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                return false;
            }

            return result == pageId.ToString();
        }

        public async Task<bool> DeletePage(long pageId)
        {
            if (pageId < 0)
            {
                //this is the case the page has not been created yet
                return true;
            }

            await Client.Content.DeleteAsync(pageId, false);

            return true;
        }

        public async Task<bool> Rename(long pageId, string newTitle)
        {
            if (pageId < 0)
            {
                //this is the case the page has not been created yet
                return true;
            }

            var content = await Client.Content.GetAsync(pageId);
            if (content.Title != newTitle)
            {
                content.Version.Number++;
                content.Title = newTitle;
                try
                {
                    await Client.Content.UpdateAsync(content);
                }
                catch (Dapplo.Confluence.ConfluenceException ex)
                {
                    Console.Error.WriteLine("ERROR: " + ex.Message);
                    return false;
                }
            }

            return true;

        }

        public async Task<long> CreatePage(string newTitle, long parentId)
        {
            Content content = null;
            try
            {
                content = await Client.Content.CreateAsync(
                ContentTypes.Page,
                newTitle,
                ConfluenceSpace, " ", parentId);
            }
            catch (Dapplo.Confluence.ConfluenceException ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                return 0;
            }
            return content.Id;
        }

        public async Task<ConfluenceTree> GetTree(int confluenceTopPage)
        {
            PagingInformation paging = new PagingInformation() { Limit = this.PagingSize };

            var content = await Client.Content.GetAsync(confluenceTopPage);

            Queue<ConfluenceNode> queue = new Queue<ConfluenceNode>();

            ConfluenceTree tree = new ConfluenceTree(content.Id, content.Title, ConfluenceBaseUrl, ConfluenceSpace);
            queue.Enqueue(tree.Root);

            int count = 0;
            //while (queue.Any() && count++ < 40)
            while (queue.Any())
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
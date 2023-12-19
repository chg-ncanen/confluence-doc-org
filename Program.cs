using Dapplo.Confluence;
using System.Linq;
using System.Collections;
using Dapplo.Confluence.Query;
using ConfluenceAccess;
using Dapplo.Confluence.Entities;
using ConfluenceAccess.MindMap;

var apiToken = Environment.GetEnvironmentVariable("CONFLUENCE_API");
var apiUser = Environment.GetEnvironmentVariable("CONFLUENCE_USER");

IConfluenceClient client = ConfluenceClient.Create(new Uri("https://chghealthcare.atlassian.net/wiki"));
//ConfluenceClientConfig.ExpandGetContent = new string[] { "container", "children" };
client.SetBasicAuthentication(apiUser, apiToken);
PagingInformation paging = new PagingInformation() { Limit = 100};

var content = await client.Content.GetAsync(930532);

Queue<Node> que = new Queue<Node>();

Tree tree = new Tree(content.Id, content.Title);
que.Enqueue(tree.Root);

//int count = 0;
//while (que.Any() && count++ < 40)
while (que.Any())
{
    Node node = que.Dequeue();
    var confluenceChildren = await client.Content.GetChildrenAsync(node.ID, paging);

    Console.WriteLine(confluenceChildren.Count());
    foreach (var c in confluenceChildren)
    {
        Node newChild = node.Add(c.Id, c.Title);
        que.Enqueue(newChild);

      Console.WriteLine($"{c.Id}: {c.Title}");
    }

    //Console.ReadLine();
}

string result = tree.ToString();
Console.WriteLine(tree.ToString());
File.WriteAllText(@"e:\temp\temp2.mm", result, System.Text.Encoding.UTF8);


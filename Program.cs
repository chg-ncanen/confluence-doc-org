using Dapplo.Confluence;
using System.Linq;
using System.Collections;
using Dapplo.Confluence.Query;
using ConfluenceAccess;
using Dapplo.Confluence.Entities;
using ConfluenceAccess.MindMap;
using System.Configuration;



var apiToken = ConfigurationManager.AppSettings["CONFLUENCE_API"];
var apiUser = ConfigurationManager.AppSettings["CONFLUENCE_USER"];
var confluenceUrl = ConfigurationManager.AppSettings["CONFLUENCE_URl"];
var confluenceSpace = ConfigurationManager.AppSettings["CONFLUENCE_SPACE"];
var confluenceTopPage = ConfigurationManager.AppSettings["CONFLUENCE_TOP_PAGE_NUM"];
var outputFile = args.Length > 0 ? args[0] : ConfigurationManager.AppSettings["OUTPUT_FILE"];

IConfluenceClient client = ConfluenceClient.Create(new Uri(confluenceUrl));
//ConfluenceClientConfig.ExpandGetContent = new string[] { "container", "children" };
client.SetBasicAuthentication(apiUser, apiToken);
PagingInformation paging = new PagingInformation() { Limit = 100};

var content = await client.Content.GetAsync(int.Parse(confluenceTopPage));

Queue<Node> que = new Queue<Node>();

Tree tree = new Tree(content.Id, content.Title, confluenceUrl, confluenceSpace);
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
File.WriteAllText(outputFile, result, System.Text.Encoding.UTF8);


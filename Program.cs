using Dapplo.Confluence;
using System.Linq;
using System.Collections;
using Dapplo.Confluence.Query;
using ConfluenceAccess;
using Dapplo.Confluence.Entities;
using System.Configuration;
using System.Xml;
using System.Diagnostics;

IConfig config = new Config();

if (string.IsNullOrEmpty(config.ApiToken))
{
    Console.WriteLine("CONFLUENCE_API api key was not specified, please update the app.config before running");
    return;
}


var originalTree = await ConfluenceMindMap.ReadMindMap(args[0], config.ConfluenceUrl, config.ConfluenceSpace);

var tree2 = await ConfluenceMindMap.ReadMindMap(args[1], config.ConfluenceUrl, config.ConfluenceSpace);

var actions = ConfluenceTree.CompareSourceAndDestTrees(originalTree, tree2);

var actionOrder = new ConfluenceTree.NodeAction[] {
    ConfluenceTree.NodeAction.Delete,
    ConfluenceTree.NodeAction.Create,
    ConfluenceTree.NodeAction.Update,
    ConfluenceTree.NodeAction.Move};

//Print all the actions
foreach (var nextAction in actionOrder)
{
    Console.WriteLine($"******************** {nextAction} ********************");
    foreach (var action in actions.Where(a => (a.Action & nextAction) == nextAction))
    {
        Console.WriteLine($"{action}");
    }

    Console.WriteLine();
}


//await ReadConfluenceTreeAndSave(config);



static async Task ReadConfluenceTreeAndSave(IConfig config)
{
    var confluence = new Confluence(config.ApiUser, config.ApiToken, config.ConfluenceUrl, config.ConfluenceSpace);
    var tree = await confluence.GetTree(int.Parse(config.ConfluenceTopPage));

    string xml = new ConfluenceMindMap().CreateXmlFromTree(tree);
    Console.WriteLine(xml);

    File.WriteAllText(config.OutputFile, xml, System.Text.Encoding.UTF8);
}


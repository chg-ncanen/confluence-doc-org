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

await ExecuteChanges(args, config);


//await ReadConfluenceTreeAndSave(config);




static async Task ReadConfluenceTreeAndSave(IConfig config)
{
    var confluence = new Confluence(config.ApiUser, config.ApiToken, config.ConfluenceUrl, config.ConfluenceSpace);
    var tree = await confluence.GetTree(int.Parse(config.ConfluenceTopPage));

    WriteTreeToFile(config.OutputFile, tree, true);
}

static void PrintActions(List<ConfluenceTree.ConfluenceAction> actions)
{
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
}

async Task ExecuteChanges(string[] args, IConfig config)
{
    var originalTree = await ConfluenceMindMap.ReadMindMap(args[0], config.ConfluenceUrl, config.ConfluenceSpace);

    var tree2 = await ConfluenceMindMap.ReadMindMap(args[1], config.ConfluenceUrl, config.ConfluenceSpace);

    var actions = ConfluenceTree.CompareSourceAndDestTrees(originalTree, tree2);

    PrintActions(actions);

    Console.WriteLine("Action Count: " + actions.Count());



    try
    {
        CreateAction(actions, originalTree);
        UpdateAction(actions);
        MoveAction(actions, originalTree);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }



    Console.WriteLine("SAVING PROGRESS...");
    Debug.Assert(ConfluenceTree.CompareSourceAndDestTrees(originalTree, tree2).Count() == 0);
    WriteTreeToFile(args[1], tree2, false);
    WriteTreeToFile(args[0], originalTree, false);
}

void UpdateAction(List<ConfluenceTree.ConfluenceAction> actions)
{
    Console.WriteLine($"****************************************");
    Console.WriteLine($"UPDATING");
    Console.WriteLine($"****************************************");
    foreach (var action in actions.Where(a => (a.Action & ConfluenceTree.NodeAction.Update) == ConfluenceTree.NodeAction.Update))
    {
        Console.WriteLine(action);
        action.OldNode.Title = action.NewNode.Title;

    }
}

void MoveAction(List<ConfluenceTree.ConfluenceAction> actions, ConfluenceTree original)
{
    Console.WriteLine($"****************************************");
    Console.WriteLine($"MOVING");
    Console.WriteLine($"****************************************");
    foreach (var action in actions.Where(a => (a.Action & ConfluenceTree.NodeAction.Move) == ConfluenceTree.NodeAction.Move))
    {
        Console.WriteLine(action);
        if (action.NewNode.Parent.ID < 0)
        {
            Console.WriteLine("------------------> ERROR: Cannot move to a non-existant node");
        }
        original.Move(action.NewNode.ID, action.NewNode.Parent.ID);

        
    }
}

static void CreateAction(List<ConfluenceTree.ConfluenceAction> actions, ConfluenceTree original)
{
    Console.WriteLine($"****************************************");
    Console.WriteLine($"CREATING");
    Console.WriteLine($"****************************************");
    //Do creation
    var createActions = new Queue<ConfluenceTree.ConfluenceAction>(actions.Where(a => a.Action == ConfluenceTree.NodeAction.Create).ToArray().Reverse());
    while (createActions.Count > 0)
    {
        var action = createActions.Dequeue();
        if (action.NewNode.Parent.ID < 0)
        {
            createActions.Enqueue(action);
            continue;
        }
        Console.WriteLine(action);
        action.NewNode.ID = action.NewNode.ID * -1; // TODO: get this from the api
        var parentInOriginal = original.GetNodeById(action.NewNode.Parent.ID);
        parentInOriginal.Add(action.NewNode.ID, action.NewNode.Title);

        Console.WriteLine($"{action}");
    }
}

static void WriteTreeToFile(string outFile, ConfluenceTree tree, bool verbose)
{
    string xml = new ConfluenceMindMap().CreateXmlFromTree(tree);
    if (verbose) Console.WriteLine(xml);

    File.WriteAllText(outFile, xml, System.Text.Encoding.UTF8);
}


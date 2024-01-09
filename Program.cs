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
var confluence = new Confluence(config.ApiUser, config.ApiToken, config.ConfluenceUrl, config.ConfluenceSpace);

bool isSimulate = false;

await ExecuteChanges(args, config, isSimulate ? null : confluence);


//await ReadConfluenceTreeAndSave(config, confluence);




static async Task ReadConfluenceTreeAndSave(IConfig config, Confluence confluence)
{
    
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

async Task ExecuteChanges(string[] args, IConfig config, Confluence confluence)
{
    var originalTree = await ConfluenceMindMap.ReadMindMap(args[0], config.ConfluenceUrl, config.ConfluenceSpace);

    var tree2 = await ConfluenceMindMap.ReadMindMap(args[1], config.ConfluenceUrl, config.ConfluenceSpace);

    var actions = ConfluenceTree.CompareSourceAndDestTrees(originalTree, tree2);

    PrintActions(actions);
    Console.WriteLine("Action Count: " + actions.Count());
    try
    {
        await CreateAction(actions, confluence, originalTree);
        await UpdateAction(actions, confluence);
        await MoveAction(actions, confluence, originalTree);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }



    Console.WriteLine("SAVING PROGRESS...");
    //Debug.Assert(ConfluenceTree.CompareSourceAndDestTrees(originalTree, tree2).Count() == 0);
    WriteTreeToFile(args[1], tree2, false);
    WriteTreeToFile(args[0], originalTree, false);
}

async Task UpdateAction(List<ConfluenceTree.ConfluenceAction> actions, Confluence confluence)
{
    Console.WriteLine($"****************************************");
    Console.WriteLine($"UPDATING");
    Console.WriteLine($"****************************************");
    foreach (var action in actions.Where(a => (a.Action & ConfluenceTree.NodeAction.Update) == ConfluenceTree.NodeAction.Update))
    {
        bool success = true;
        Console.WriteLine(action);
        if (confluence != null)
        {
            success = await confluence.Rename(action.NewNode.ID, action.NewNode.Title);
        }

        //If successful, record the change, otherwise do nothing so it can be tried again another time
        if (success) { action.OldNode.Title = action.NewNode.Title; }
    }
}

async Task MoveAction(List<ConfluenceTree.ConfluenceAction> actions, Confluence confluence, ConfluenceTree original)
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
            return;
        }
        bool success = await confluence.MovePage(action.NewNode.ID, action.NewNode.Parent.ID);

        //If success then record the change
        if (success)
        {
            original.Move(action.NewNode.ID, action.NewNode.Parent.ID);
        }
    }
}

static async Task CreateAction(List<ConfluenceTree.ConfluenceAction> actions, Confluence confluence, ConfluenceTree original)
{
    Console.WriteLine($"****************************************");
    Console.WriteLine($"CREATING");
    Console.WriteLine($"****************************************");

    int inifinteLoopBreakerCount = actions.Count() * 100;
    //Do creation
    var createActions = new Queue<ConfluenceTree.ConfluenceAction>(actions.Where(a => a.Action == ConfluenceTree.NodeAction.Create).ToArray().Reverse());
    while (createActions.Count > 0 && inifinteLoopBreakerCount-- >= 0)
    {
        var action = createActions.Dequeue();
        if (action.NewNode.Parent.ID < 0)
        {
            createActions.Enqueue(action);
            continue;
        }

        Console.WriteLine(action);
        long newID = 0;
        if (confluence != null)
        {
            newID = await confluence.CreatePage(action.NewNode.Title, action.NewNode.Parent.ID);
        }
        else
        {
            newID = Math.Abs(action.NewNode.ID);
        }

        //If successful, record the change, otherwise do nothing so it can be tried again another time
        //This could cause an infinite loop which is prevented by the counter
        if (newID > 0)
        {
            action.NewNode.ID = newID;
            var parentInOriginal = original.GetNodeById(action.NewNode.Parent.ID);
            parentInOriginal.Add(action.NewNode.ID, action.NewNode.Title);
        }
        else
        {

        }
    }
}

static void WriteTreeToFile(string outFile, ConfluenceTree tree, bool verbose)
{
    string xml = new ConfluenceMindMap().CreateXmlFromTree(tree);
    if (verbose) Console.WriteLine(xml);

    File.WriteAllText(outFile, xml, System.Text.Encoding.UTF8);
}


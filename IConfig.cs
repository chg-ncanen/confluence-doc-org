namespace ConfluenceAccess
{
    internal interface IConfig
    {
        string ApiToken { get; }
        string ApiUser { get; }
        string ConfluenceUrl { get; }
        string ConfluenceSpace { get; }
        string ConfluenceTopPage { get; }
        string OutputFile { get; }
    }
}
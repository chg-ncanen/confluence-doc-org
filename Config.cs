using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfluenceAccess
{
    internal class Config : IConfig
    {
        public string ApiToken { get; }
        public string ApiUser { get; }
        public string ConfluenceUrl { get; }
        public string ConfluenceSpace { get; }
        public string ConfluenceTopPage { get; }
        public string OutputFile { get; }
        public Config()
        {
            ApiToken = ConfigurationManager.AppSettings["CONFLUENCE_API"];
            ApiUser = ConfigurationManager.AppSettings["CONFLUENCE_USER"];
            ConfluenceUrl = ConfigurationManager.AppSettings["CONFLUENCE_URl"];
            ConfluenceSpace = ConfigurationManager.AppSettings["CONFLUENCE_SPACE"];
            ConfluenceTopPage = ConfigurationManager.AppSettings["CONFLUENCE_TOP_PAGE_NUM"];
            OutputFile = ConfigurationManager.AppSettings["OUTPUT_FILE"];
        }
    }
}
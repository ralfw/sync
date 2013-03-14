using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using sync.contracts;
using sync.remotefilestore.parse.api;

namespace sync.remotesynctable.parse
{
    public class RemoteSyncTable : IRemoteSyncTable
    {
        private readonly string _repoName;
        private readonly ParseObjects _parseObjects;
        private JavaScriptSerializer _jss;

        public RemoteSyncTable(string repoName, string parseAppId, string parseRestKey)
        {
            _repoName = repoName;
            _parseObjects = new ParseObjects(parseAppId, parseRestKey);

            _jss = new JavaScriptSerializer();
        }


        public void AddEntry(RepoFile repoFile)
        {
            var jsonRepoFile = repoFile.ToJson();
            Console.WriteLine(jsonRepoFile);
            _parseObjects.New(_repoName, jsonRepoFile);
        }


        public void UpdateEntry(RepoFile repoFile, Action<RepoFile> onEntryUpdated, Action<RepoFile> onNoEntry)
        {
            Dictionary<string, object> item;
            if (TryFindEntry(repoFile, out item))
            {
                _parseObjects[_repoName, item["objectId"].ToString()] = repoFile.ToJson();

                onEntryUpdated(repoFile);
            }
            else
                onNoEntry(repoFile);
        }


        public RepoFile DeleteEntry(RepoFile repoFile)
        {
            Dictionary<string, object> item;
            if (TryFindEntry(repoFile, out item))
            {
                _parseObjects.Delete(_repoName, item["objectId"].ToString());   
            }
            return repoFile;
        }


        public void CollectRepoFiles(Action<RepoFile> continueWith)
        {
            var jsonQueryResults = _parseObjects.Query(_repoName);

            var queryResults = (Dictionary<string, object>)_jss.DeserializeObject(jsonQueryResults);
            var queryResultItems = (object[])queryResults["results"];

            foreach(var item in queryResultItems)
            {
                var repoFile = ((Dictionary<string, object>)item).ToRepoFile();
                continueWith(repoFile);
            }
        }


        public void FilterExistingFiles(RepoFile repoFile, Action<RepoFile> onNonExistingFile)
        {
            Dictionary<string, object> _;
            if (!TryFindEntry(repoFile, out _))
                onNonExistingFile(repoFile);
        }


        bool TryFindEntry(RepoFile repoFile, out Dictionary<string, object> dictRepoFile)
        {
            var jsonQueryResults = _parseObjects.Query(_repoName, "{\"relativeFilename\":\"" + repoFile.RelativeFileName + "\"}");

            var queryResults = (Dictionary<string,object>)_jss.DeserializeObject(jsonQueryResults);
            var queryResultItems = (object[])queryResults["results"];

            if (queryResultItems.Length > 0)
                dictRepoFile = (Dictionary<string, object>) queryResultItems[0];
            else
                dictRepoFile = null;

            return dictRepoFile != null;
        }
    }


    static class RepoFileSerializer
    {
        public static string ToJson(this RepoFile repoFile)
        {
            return "{" + 
                        string.Format("\"relativeFilename\": \"{0}\",\n", repoFile.RelativeFileName) +
                        string.Format("\"idInFilestore\": \"{0}\",\n", repoFile.Id) +
                        string.Format("\"timeStamp\": \"{0}\",\n", repoFile.TimeStamp.ToString("s")) +
                        string.Format("\"user\": \"{0}\"", repoFile.User) +
                   "}";

        }

        public static RepoFile ToRepoFile(this Dictionary<string, object> dictRepoFile)
        {
            return new RepoFile
                {
                    Id = dictRepoFile["idInFilestore"].ToString(),
                    RelativeFileName = dictRepoFile["relativeFilename"].ToString(),
                    TimeStamp = DateTime.Parse(dictRepoFile["timeStamp"].ToString()),
                    User = dictRepoFile["user"].ToString()
                };
        }
    }
}

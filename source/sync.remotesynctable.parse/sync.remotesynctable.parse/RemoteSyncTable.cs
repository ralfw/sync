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
            _parseObjects.New(_repoName, jsonRepoFile);
        }


        public void UpdateEntry(RepoFile repoFile, Action<RepoFile> onEntryUpdated, Action<RepoFile> onNoEntry)
        {
            Dictionary<string, object> item;
            if (TryFindEntry(repoFile, out item))
            {
                _parseObjects[_repoName, item["objectId"].ToString()] = repoFile.ToJson();

                onEntryUpdated(item.ToRepoFile());
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


        internal bool TryFindEntry(RepoFile repoFile, out Dictionary<string, object> dictRepoFile)
        {
            var jsonQueryResults = _parseObjects.Query(_repoName, "{\"relativeFilename\":\"" + RepoFileSerializer.Encode_RelativeFilename(repoFile.RelativeFileName) + "\"}");

            var queryResults = (Dictionary<string,object>)_jss.DeserializeObject(jsonQueryResults);
            var queryResultItems = (object[])queryResults["results"];

            if (queryResultItems.Length > 0)
                dictRepoFile = (Dictionary<string, object>) queryResultItems[0];
            else
                dictRepoFile = null;

            return dictRepoFile != null;
        }
    }
}

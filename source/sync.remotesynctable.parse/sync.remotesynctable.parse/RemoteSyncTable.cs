using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using sync.contracts;
using sync.remotefilestore.parse.api;

namespace sync.remotesynctable.parse
{
    public class RemoteSyncTable : IRemoteSyncTable
    {
        private readonly string _repoName;
        private readonly ParseObjects _parseObjects;
        private readonly JavaScriptSerializer _jss;

        public RemoteSyncTable(string repoName, string parseAppId, string parseRestKey)
        {
            _repoName = repoName;
            _parseObjects = new ParseObjects(parseAppId, parseRestKey);

            _jss = new JavaScriptSerializer();

            Create_lock();
        }


        public void AddEntry(RepoFile repoFile)
        {
            var jsonRepoFile = repoFile.ToJson();
            _parseObjects.New(_repoName, jsonRepoFile);
        }


        public void UpdateEntry(RepoFile repoFile, Action<RepoFile> onEntryUpdated, Action<RepoFile> onNoEntry)
        {
            Dictionary<string, object> item;
            if (_parseObjects.TryFindByFieldvalue(_repoName, "relativeFilename", HttpUtility.UrlEncode(repoFile.RelativeFileName), out item))
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
            if (_parseObjects.TryFindByFieldvalue(_repoName, "relativeFilename", HttpUtility.UrlEncode(repoFile.RelativeFileName), out item))
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
            if (!_parseObjects.TryFindByFieldvalue(_repoName, "relativeFilename", HttpUtility.UrlEncode(repoFile.RelativeFileName), out _))
                onNonExistingFile(repoFile);
        }


        public void Lock(Action onLocked, Action onUnableToLock)
        {
            Dictionary<string, object> lockObject;
            _parseObjects.TryFindByFieldvalue("synclocks", "name", _repoName, out lockObject);
            _parseObjects.Inc("synclocks", lockObject["objectId"].ToString(), "flag", 1);

            var jsonLockObject = _parseObjects["synclocks", lockObject["objectId"].ToString()];
            lockObject = (Dictionary<string,object>)_jss.DeserializeObject(jsonLockObject);
            if ((int)lockObject["flag"] == 1)
            {
                try
                {
                    onLocked();
                }
                finally
                {
                    _parseObjects.Inc("synclocks", lockObject["objectId"].ToString(), "flag", -1);
                }
            }
            else
            {
                _parseObjects.Inc("synclocks", lockObject["objectId"].ToString(), "flag", -1);
                onUnableToLock();
            }
        }

        public void FreeLock()
        {
            throw new NotImplementedException();
        }

        internal void Create_lock()
        {
            Dictionary<string, object> _;
            if (!_parseObjects.TryFindByFieldvalue("synclocks", "name", _repoName, out _))
                _parseObjects.New("synclocks", "{\"name\":\"" + _repoName + "\", \"flag\":0}");
        }
    }
}

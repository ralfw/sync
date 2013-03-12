using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sync.contracts;

namespace sync.remotesynctable.parse
{
    public class RemoteSyncTable : IRemoteSyncTable
    {
        public RemoteSyncTable(string repoName)
        {
            
        }

        public void CollectRepoFiles(Action<RepoFile> continueWith)
        {
            throw new NotImplementedException();
        }

        public void UpdateEntry(RepoFile repoFile, Action<RepoFile> onEntryUpdated, Action<RepoFile> onNoEntry)
        {
            throw new NotImplementedException();
        }

        public void AddEntry(RepoFile repoFile)
        {
            throw new NotImplementedException();
        }

        public RepoFile DeleteEntry(RepoFile repoFile)
        {
            throw new NotImplementedException();
        }

        public void FilterExistingFiles(RepoFile repoFile, Action<RepoFile> onNonExistingFile)
        {
            throw new NotImplementedException();
        }
    }
}

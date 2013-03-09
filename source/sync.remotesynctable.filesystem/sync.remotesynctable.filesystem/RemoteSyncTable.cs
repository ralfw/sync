using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sync.contracts;

namespace sync.remotesynctable.filesystem
{
    public class RemoteSyncTable : IRemoteSyncTable
    {
        private readonly string _repoPath;
        private readonly List<SyncTableEntry> _syncTable;

        public RemoteSyncTable(string repoPath) : this(repoPath, null) {}
        internal RemoteSyncTable(string repoPath, List<SyncTableEntry> syncTable)
        {
            this._repoPath = repoPath;
            _syncTable = syncTable ?? SyncTablePersistor.Load(_repoPath);
        }


        public void AddEntry(RepoFile repoFile)
        {
            _syncTable.Add(SyncTableEntry.CreateFrom(repoFile));
            SyncTablePersistor.Save(_repoPath, _syncTable);
        }

        public void UpdateEntry(RepoFile repoFile, Action<RepoFile> onEntryUpdated, Action<RepoFile> onNoEntry)
        {
            var entry = _syncTable.Find(_ => _.RelativeFilename == repoFile.RelativeFileName);
            if (entry != null)
            {
                var oldRepoFile = entry.ToRepoFile();

                entry.Id = repoFile.Id;
                entry.User = repoFile.User;
                entry.TimeStamp = repoFile.TimeStamp;

                SyncTablePersistor.Save(_repoPath, _syncTable);

                onEntryUpdated(oldRepoFile);
            }
            else
                onNoEntry(repoFile);
        }
        
        public RepoFile DeleteEntry(RepoFile repoFile)
        {
            var entry = _syncTable.Find(_ => _.RelativeFilename == repoFile.RelativeFileName);
            if (entry != null) _syncTable.Remove(entry);

            SyncTablePersistor.Save(_repoPath, _syncTable);

            return repoFile;
        }


        public void FilterExistingFiles(RepoFile repoFile, Action<RepoFile> onNonExistingFile)
        {
            if (_syncTable.Any(e => e.RelativeFilename == repoFile.RelativeFileName)) return;

            onNonExistingFile(repoFile);
        }


        public void CollectRepoFiles(Action<RepoFile> continueWith)
        {
            _syncTable.ForEach(entry => continueWith(entry.ToRepoFile()));
        }
    }
}

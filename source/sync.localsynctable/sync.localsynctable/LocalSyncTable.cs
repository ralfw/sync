using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using sync.contracts;

namespace sync.localsynctable
{
    public class LocalSyncTable : ILocalSyncTable
    {
        private readonly string _repoPath;
        private readonly List<SyncTableEntry> _syncTable;

        public LocalSyncTable(string repoPath) : this(repoPath, null) {}
        internal LocalSyncTable(string repoPath, List<SyncTableEntry> syncTable)
        {
            _repoPath = repoPath;
            _syncTable = syncTable ?? SyncTablePersistor.Load(_repoPath);
        }


        public void AddOrUpdateEntry(RepoFile repoFile)
        {
            DeleteEntry(repoFile);

            _syncTable.Add(SyncTableEntry.CreateFrom(repoFile));

            SyncTablePersistor.Save(_repoPath, _syncTable);
        }


        public void FilterUnchangedByTimeStamp(RepoFile repoFile, Action<RepoFile> onChangedRepoFile)
        {
            if (_syncTable.Any(e => e.RelativeFilename == repoFile.RelativeFileName && 
                                    e.TimeStamp.ToString("s") == repoFile.TimeStamp.ToString("s"))) return;

            onChangedRepoFile(repoFile);
        }


        public void FilterUnchangedById(RepoFile repoFile, Action<RepoFile> onChangedRepoFile)
        {
            if (_syncTable.Any(e => e.RelativeFilename == repoFile.RelativeFileName &&
                                    e.Id == repoFile.Id)) return;

            onChangedRepoFile(repoFile);
        }


        public void CollectRepoFiles(Action<RepoFile> onRepoFile)
        {
            foreach (var e in _syncTable.ToArray())
                onRepoFile(e.ToRepoFile());
        }


        public RepoFile DeleteEntry(RepoFile repoFile)
        {
            var entry = _syncTable.Find(e => e.RelativeFilename == repoFile.RelativeFileName);
            if (entry != null) _syncTable.Remove(entry);

            SyncTablePersistor.Save(_repoPath, _syncTable);

            return repoFile;
        }


        public RepoFile GetTimeStamp(RepoFile repoFile)
        {
            var entry = _syncTable.Find(e => e.RelativeFilename == repoFile.RelativeFileName);
            if (entry == null) return repoFile;

            repoFile.TimeStamp = entry.TimeStamp;
            return new RepoFile {Id=repoFile.Id, User=repoFile.User,RelativeFileName = repoFile.RelativeFileName, RepoRoot = repoFile.RepoRoot, TimeStamp = entry.TimeStamp};
        }
    }
}

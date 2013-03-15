using System;
using sync.contracts;
using sync.ignore;
using sync.localfilesystem;
using sync.localsynctable;
using sync.remotesynctable.filesystem;
using sync.ui;

namespace sync.push
{
    internal class Integration
    {
        private readonly string _pathToRemoteFileStore;

        private readonly IRemoteFileStore _remoteFileStore;
        private readonly IRemoteSyncTable _remoteSyncTable;

        public Integration(string pathToRemoteFileStore)
        {
            _pathToRemoteFileStore = pathToRemoteFileStore;
            
            var factory = new Factory(_pathToRemoteFileStore);
            _remoteSyncTable = factory.Build_remote_sync_table();
            _remoteFileStore = factory.Build_remote_file_store();
        }


        public void Push()
        {
            Console.WriteLine("Pushing to repository {0}...", _pathToRemoteFileStore);

            AddOrUpdate();
            Delete();
        }


        private void AddOrUpdate()
        {
            ILocalFileSystem localFileSystem = new LocalFileSystem();
            IIgnoreFilter ignoreFilter = new IgnoreFilter(".");
            IUi ui = new Ui();
            ILocalSyncTable localSyncTable = new LocalSyncTable(".");

            var repoRoot = localFileSystem.GetRepoRoot();
            localFileSystem.CollectRepoFiles(repoRoot,
                                             repoFile => ignoreFilter.Filter(repoFile, r1 =>
                                                 {
                                                     r1 = localFileSystem.EnrichWithMetadata(r1);
                                                     localSyncTable.FilterUnchangedByTimeStamp(r1, r2 =>
                                                         {
                                                             ui.LogBeginOfOperation(r2);
                                                             var stream = localFileSystem.LoadFromFile(r2);
                                                             r2 = _remoteFileStore.Upload(r2, stream);
                                                             r2 = UpdateRemoteSyncTable(r2);
                                                             localSyncTable.AddOrUpdateEntry(r2);
                                                             ui.LogEndOfOperation(r2);
                                                         });
                                                 }));
        }


        private RepoFile UpdateRemoteSyncTable(RepoFile repoFile)
        {
            _remoteSyncTable.UpdateEntry(repoFile,
                                         file => _remoteFileStore.Delete(file),
                                         _remoteSyncTable.AddEntry);

            return repoFile;
        }


        private void Delete()
        {
            ILocalSyncTable localSyncTable = new LocalSyncTable(".");
            ILocalFileSystem localFileSystem = new LocalFileSystem();
            IUi ui = new Ui();

            localSyncTable.CollectRepoFiles(repoFile =>
                {
                    repoFile = localFileSystem.EnrichWithRepoRoot(repoFile);
                    localFileSystem.FilterExistingFiles(repoFile, r =>
                        {
                            ui.LogBeginOfOperation(repoFile);
                            r = _remoteSyncTable.DeleteEntry(r);
                            r = _remoteFileStore.Delete(r);
                            r = localSyncTable.DeleteEntry(r);
                            ui.LogEndOfOperation(r);
                        });
                });
        }
    }
}
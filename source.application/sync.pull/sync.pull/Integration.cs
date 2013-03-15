using System;
using sync.conflicts;
using sync.contracts;
using sync.localfilesystem;
using sync.localsynctable;
using sync.ui;

namespace sync.pull
{
    class Integration
    {
        private readonly string _remoteRepoPath;

        private readonly IRemoteSyncTable _remoteSyncTable;
        private readonly IRemoteFileStore _remoteFileStore;


        public Integration(string remoteRepoPath)
        {
            _remoteRepoPath = remoteRepoPath;

            var factory = new Factory(remoteRepoPath);
            _remoteSyncTable = factory.Build_remote_sync_table();
            _remoteFileStore = factory.Build_remote_file_store();
        }


        public void Pull()
        {
            Console.WriteLine("Pulling from repository {0}...", _remoteRepoPath);

            AddOrUpdate();
            Delete();
        }


        private void AddOrUpdate()
        {
            ILocalFileSystem localFileSystem = new LocalFileSystem();
            ILocalSyncTable localSyncTable = new LocalSyncTable(".");
            IUi ui = new Ui();

            _remoteSyncTable.CollectRepoFiles(remoteFile =>
                {
                    remoteFile = localFileSystem.EnrichWithRepoRoot(remoteFile);
                    localSyncTable.FilterUnchangedById(remoteFile, changedRemoteFile =>
                        {
                            changedRemoteFile = ResolveConflicts(changedRemoteFile);
                            ui.LogBeginOfOperation(changedRemoteFile);
                            var result = _remoteFileStore.Download(changedRemoteFile);
                            changedRemoteFile = localFileSystem.SaveToFile(result.Item1, result.Item2);
                            localSyncTable.AddOrUpdateEntry(changedRemoteFile);
                            ui.LogEndOfOperation(changedRemoteFile);
                        });
                });
        }

        private RepoFile ResolveConflicts(RepoFile changedRemoteFile)
        {
            ILocalSyncTable localSyncTable = new LocalSyncTable(".");
            ILocalFileSystem localFileSystem = new LocalFileSystem();
            IConflictMediator conflictMediator = new ConflictMediator();

            var fromLocalSyncTable = localSyncTable.GetTimeStamp(changedRemoteFile);
            var fromLocalFileSystem = localFileSystem.GetTimeStamp(changedRemoteFile);
            conflictMediator.DetectUpdateConflct(fromLocalSyncTable, fromLocalFileSystem, changedRemoteFile,
                                                 _ => { }, localFile => localFileSystem.Rename(localFile));

            return changedRemoteFile;
        }

        private void Delete()
        {
            ILocalSyncTable localSyncTable = new LocalSyncTable(".");
            ILocalFileSystem localFileSystem = new LocalFileSystem();
            IUi ui = new Ui();

            localSyncTable.CollectRepoFiles(localFile =>
                                            _remoteSyncTable.FilterExistingFiles(localFile, missingRemoteFile =>
                                                {
                                                    missingRemoteFile = localFileSystem.EnrichWithRepoRoot(missingRemoteFile);
                                                    ui.LogBeginOfOperation(missingRemoteFile);
                                                    missingRemoteFile = localSyncTable.DeleteEntry(missingRemoteFile);
                                                    missingRemoteFile = localFileSystem.Delete(missingRemoteFile);
                                                    ui.LogEndOfOperation(missingRemoteFile);
                                                }));
        }
    }
}
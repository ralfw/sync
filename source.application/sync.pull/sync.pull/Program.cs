using System;
using sync.conflicts;
using sync.contracts;
using sync.localfilesystem;
using sync.localsynctable;
using sync.remotefilestore.filesystem;
using sync.remotesynctable.filesystem;
using sync.ui;

namespace sync.pull
{
    internal class Program
    {
        private static string repoPath;

        private static void Main(string[] args) {
            repoPath = args[0];

            AddOrUpdate();
            Delete();
        }

        private static void AddOrUpdate() {
            IRemoteSyncTable remoteSyncTable = new RemoteSyncTable(repoPath);
            IRemoteFileStore remoteFileStore = new RemoteFileStore(repoPath);
            ILocalFileSystem localFileSystem = new LocalFileSystem();
            ILocalSyncTable localSyncTable = new LocalSyncTable(".");
            IUi ui = new Ui();

            remoteSyncTable.CollectRepoFiles(remoteFile => {
                remoteFile = localFileSystem.EnrichWithRepoRoot(remoteFile);
                localSyncTable.FilterUnchangedById(remoteFile, changedRemoteFile => {
                    changedRemoteFile = ResolveConflicts(changedRemoteFile);
                    ui.LogBeginOfOperation(changedRemoteFile);
                    var result = remoteFileStore.Download(changedRemoteFile);
                    changedRemoteFile = localFileSystem.SaveToFile(result.Item1, result.Item2);
                    localSyncTable.AddOrUpdateEntry(changedRemoteFile);
                    ui.LogEndOfOperation(changedRemoteFile);
                });
            });
        }

        private static RepoFile ResolveConflicts(RepoFile changedRemoteFile) {
            ILocalSyncTable localSyncTable = new LocalSyncTable(".");
            ILocalFileSystem localFileSystem = new LocalFileSystem();
            IConflictMediator conflictMediator = new ConflictMediator();

            var fromLocalSyncTable = localSyncTable.GetTimeStamp(changedRemoteFile);
            var fromLocalFileSystem = localFileSystem.GetTimeStamp(changedRemoteFile);
            conflictMediator.DetectUpdateConflct(fromLocalSyncTable, fromLocalFileSystem, changedRemoteFile,
                _ => {}, localFile => localFileSystem.Rename(localFile));

            return changedRemoteFile;
        }

        private static void Delete() {
            ILocalSyncTable localSyncTable = new LocalSyncTable(".");
            IRemoteSyncTable remoteSyncTable = new RemoteSyncTable(repoPath);
            ILocalFileSystem localFileSystem = new LocalFileSystem();
            IUi ui = new Ui();

            localSyncTable.CollectRepoFiles(localFile =>
                remoteSyncTable.FilterExistingFiles(localFile, missingRemoteFile => {
                    missingRemoteFile = localFileSystem.EnrichWithRepoRoot(missingRemoteFile);
                    ui.LogBeginOfOperation(missingRemoteFile);
                    missingRemoteFile = localSyncTable.DeleteEntry(missingRemoteFile);
                    missingRemoteFile = localFileSystem.Delete(missingRemoteFile);
                    ui.LogEndOfOperation(missingRemoteFile);
                }));
        }
    }
}
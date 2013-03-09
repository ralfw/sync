using sync.contracts;
using sync.ignore;
using sync.localfilesystem;
using sync.localsynctable;
using sync.remotefilestore.filesystem;
using sync.remotesynctable.filesystem;
using sync.ui;

namespace sync.push
{
    internal class Program
    {
        private static string pathToRemoteFileStore;

        private static void Main(string[] args) {
            pathToRemoteFileStore = args[0];

            AddOrUpdate();
            Delete();
        }

        private static void AddOrUpdate() {
            ILocalFileSystem localFileSystem = new LocalFileSystem();
            IRemoteFileStore remoteFileStore = new RemoteFileStore(pathToRemoteFileStore);
            IIgnoreFilter ignoreFilter = new IgnoreFilter(".");
            IUi ui = new Ui();
            ILocalSyncTable localSyncTable = new LocalSyncTable(".");

            var repoRoot = localFileSystem.GetRepoRoot();
            localFileSystem.CollectRepoFiles(repoRoot,
                repoFile => ignoreFilter.Filter(repoFile, r1 => {
                    r1 = localFileSystem.EnrichWithMetadata(r1);
                    localSyncTable.FilterUnchangedByTimeStamp(r1, r2 => {
                        ui.LogBeginOfOperation(r2);
                        var stream = localFileSystem.LoadFromFile(r2);
                        r2 = remoteFileStore.Upload(r2, stream);
                        r2 = UpdateRemoteSyncTable(r2);
                        localSyncTable.AddOrUpdateEntry(r2);
                        ui.LogEndOfOperation(r2);
                    });
                }));
        }

        private static RepoFile UpdateRemoteSyncTable(RepoFile repoFile) {
            IRemoteSyncTable remoteSyncTable = new RemoteSyncTable(pathToRemoteFileStore);
            IRemoteFileStore remoteFileStore = new RemoteFileStore(pathToRemoteFileStore);

            remoteSyncTable.UpdateEntry(repoFile,
                file => remoteFileStore.Delete(file),
                remoteSyncTable.AddEntry);

            return repoFile;
        }

        private static void Delete() {
            ILocalSyncTable localSyncTable = new LocalSyncTable(".");
            ILocalFileSystem localFileSystem = new LocalFileSystem();
            IUi ui = new Ui();
            IRemoteSyncTable remoteSyncTable = new RemoteSyncTable(pathToRemoteFileStore);
            IRemoteFileStore remoteFileStore = new RemoteFileStore(pathToRemoteFileStore);

            localSyncTable.CollectRepoFiles(repoFile => {
                repoFile = localFileSystem.EnrichWithRepoRoot(repoFile);
                localFileSystem.FilterExistingFiles(repoFile, r => {
                    ui.LogBeginOfOperation(repoFile);
                    r = remoteSyncTable.DeleteEntry(r);
                    r = remoteFileStore.Delete(r);
                    r = localSyncTable.DeleteEntry(r);
                    ui.LogEndOfOperation(r);
                });
            });
        }
    }
}
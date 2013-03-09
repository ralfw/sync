using System;

namespace sync.contracts
{
    public interface IConflictMediator
    {
        void DetectUpdateConflct(RepoFile fromLocalSyncTable, RepoFile fromLocalFileSystem, RepoFile changedRemoteFile,
            Action<RepoFile> onNoConflict, Action<RepoFile> onConflict);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sync.contracts;

namespace sync.conflicts
{
    public class ConflictMediator : IConflictMediator
    {
        public void DetectUpdateConflct(RepoFile fromLocalSyncTable, RepoFile fromLocalFileSystem, RepoFile changedRemoteFile,
                                        Action<RepoFile> onNoConflict, Action<RepoFile> onConflict)
        {
            if (No_local_file(fromLocalSyncTable, fromLocalFileSystem)) { onNoConflict(changedRemoteFile); return; }
            if (Local_file_is_new(fromLocalSyncTable, fromLocalFileSystem)) { onConflict(changedRemoteFile); return; }
            if (Local_file_was_updated(fromLocalSyncTable, fromLocalFileSystem)) { onConflict(changedRemoteFile); return; }
            onNoConflict(changedRemoteFile);
        }


        private static bool No_local_file(RepoFile fromLocalSyncTable, RepoFile fromLocalFileSystem)
        {
            return fromLocalSyncTable == null && fromLocalFileSystem == null;
        }

        private static bool Local_file_is_new(RepoFile fromLocalSyncTable, RepoFile fromLocalFileSystem)
        {
            return fromLocalSyncTable == null && fromLocalFileSystem != null;
        }

        private static bool Local_file_was_updated(RepoFile fromLocalSyncTable, RepoFile fromLocalFileSystem)
        {
            return fromLocalSyncTable.TimeStamp.ToString("s") != fromLocalFileSystem.TimeStamp.ToString("s");
        }
    }
}

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
            if (fromLocalSyncTable.TimeStamp.ToString("s") == fromLocalFileSystem.TimeStamp.ToString("s"))
                onNoConflict(changedRemoteFile);
            else
                onConflict(changedRemoteFile);
        }
    }
}

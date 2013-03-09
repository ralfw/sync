using System;
using sync.contracts;

namespace sync.remotesynctable.filesystem
{
    internal class SyncTableEntry
    {
        public string RelativeFilename;
        public DateTime TimeStamp;
        public string User;
        public string Id;

        public RepoFile ToRepoFile()
        {
            return new RepoFile
            {
                Id = this.Id,
                RelativeFileName = this.RelativeFilename,
                TimeStamp = this.TimeStamp,
                User = this.User
            };
        }


        public static SyncTableEntry CreateFrom(RepoFile repoFile)
        {
            return new SyncTableEntry
            {
                Id = repoFile.Id,
                RelativeFilename = repoFile.RelativeFileName,
                TimeStamp = repoFile.TimeStamp,
                User = repoFile.User
            };
        }
    }
}
using System;
using System.IO;

namespace sync.contracts
{
    public interface IRemoteFileStore
    {
        RepoFile Upload(RepoFile repoFile, Stream stream);

        Tuple<RepoFile, Stream> Download(RepoFile repoFile);

        RepoFile Delete(RepoFile repoFile);
    }
}
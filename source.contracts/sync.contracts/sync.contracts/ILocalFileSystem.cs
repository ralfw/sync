using System;
using System.IO;

namespace sync.contracts
{
    public interface ILocalFileSystem
    {
        string GetRepoRoot();

        void CollectRepoFiles(string repoRoot, Action<RepoFile> continueWith);

        RepoFile EnrichWithRepoRoot(RepoFile repoFile);

        RepoFile EnrichWithMetadata(RepoFile repoFile);

        Stream LoadFromFile(RepoFile repoFile);

        RepoFile SaveToFile(RepoFile repoFile, Stream stream);

        void FilterExistingFiles(RepoFile repoFile, Action<RepoFile> onNonExistingFile);

        RepoFile Delete(RepoFile repoFile);

        RepoFile Rename(RepoFile repoFile);

        RepoFile GetTimeStamp(RepoFile repoFile);
    }
}
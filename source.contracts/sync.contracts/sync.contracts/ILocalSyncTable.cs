using System;

namespace sync.contracts
{
    public interface ILocalSyncTable
    {
        void AddOrUpdateEntry(RepoFile repoFile);

        void FilterUnchangedByTimeStamp(RepoFile repoFile, Action<RepoFile> onChangedRepoFile);

        void FilterUnchangedById(RepoFile repoFile, Action<RepoFile> onChangedRepoFile);

        void CollectRepoFiles(Action<RepoFile> onRepoFile);

        RepoFile DeleteEntry(RepoFile repoFile);

        RepoFile GetTimeStamp(RepoFile repoFile);
    }
}
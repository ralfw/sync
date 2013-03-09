using System;

namespace sync.contracts
{
    public interface IRemoteSyncTable
    {
        void CollectRepoFiles(Action<RepoFile> continueWith);

        void UpdateEntry(RepoFile repoFile, Action<RepoFile> onEntryUpdated, Action<RepoFile> onNoEntry);

        void AddEntry(RepoFile repoFile);

        RepoFile DeleteEntry(RepoFile repoFile);

        void FilterExistingFiles(RepoFile repoFile, Action<RepoFile> onNonExistingFile);
    }
}
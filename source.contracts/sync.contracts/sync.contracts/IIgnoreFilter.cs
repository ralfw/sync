using System;

namespace sync.contracts
{
    public interface IIgnoreFilter
    {
        void Filter(RepoFile repoFile, Action<RepoFile> onNotIgnored);
    }
}
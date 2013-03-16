namespace sync.contracts
{
    public interface IUi
    {
        void LogBeginOfOperation(RepoFile repoFile);
        void LogConflict(RepoFile repoFile);
        void LogEndOfOperation(RepoFile repoFile);
    }
}
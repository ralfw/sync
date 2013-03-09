namespace sync.contracts
{
    public interface IUi
    {
        void LogBeginOfOperation(RepoFile repoFile);

        void LogEndOfOperation(RepoFile repoFile);
    }
}
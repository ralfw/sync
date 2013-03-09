using System;
using sync.contracts;

namespace sync.ui
{
    public class Ui : IUi
    {
        public void LogBeginOfOperation(RepoFile repoFile) {
            Console.Write("{0}...", repoFile.RelativeFileName);
        }

        public void LogEndOfOperation(RepoFile repoFile) {
            Console.WriteLine("done.");
        }
    }
}
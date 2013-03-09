using System;

namespace sync.contracts
{
    public class RepoFile
    {
        public string RelativeFileName { get; set; }

        public string RepoRoot { get; set; }

        public string Id { get; set; }

        public string User { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
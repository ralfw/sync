using System;
using sync.remotefilestore.filesystem;
using sync.remotesynctable.filesystem;

namespace sync.pull
{
    internal class Program
    {
        private static void Main(string[] args) {
            new Integration(args[0])
                .Pull();
        }
    }
}
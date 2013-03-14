using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sync.contracts;

namespace sync.pull
{
    class Factory
    {
        private readonly string _remoteRepoPath;

        private readonly string _appId;
        private readonly string _restKey;
        private readonly string _masterKey;


        public Factory(string remoteRepoPath)
        {
            _remoteRepoPath = remoteRepoPath;

            if (File.Exists(".syncconfig"))
                using (var sr = new StreamReader(@".syncconfig"))
                {
                    _appId = sr.ReadLine();
                    _restKey = sr.ReadLine();
                    _masterKey = sr.ReadLine();
                }
        }


        public IRemoteFileStore Build_remote_file_store()
        {
            if (_remoteRepoPath.IndexOf(":") > 0 || _remoteRepoPath.IndexOf("..") > 0)
                return new remotefilestore.filesystem.RemoteFileStore(_remoteRepoPath);

            Console.WriteLine("Loading files from the cloud...");
            return new remotefilestore.parse.RemoteFileStore(_appId, _restKey, _masterKey);
        }


        public IRemoteSyncTable Build_remote_sync_table()
        {
            if (_remoteRepoPath.IndexOf(":") > 0 || _remoteRepoPath.IndexOf("..") > 0)
                return new remotesynctable.filesystem.RemoteSyncTable(_remoteRepoPath);

            Console.WriteLine("Storing sync table in the cloud...");
            return new remotesynctable.parse.RemoteSyncTable(_remoteRepoPath, _appId, _restKey);
        }
    }
}

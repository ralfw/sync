using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sync.contracts;

namespace sync.remotefilestore.filesystem
{
    public class RemoteFileStore : IRemoteFileStore
    {
        private readonly string _repoPath;

        public RemoteFileStore(string repoPath)
        {
            _repoPath = repoPath;
            if (!Directory.Exists(_repoPath)) Directory.CreateDirectory(_repoPath);
        }


        public RepoFile Upload(RepoFile repoFile, Stream stream)
        {
            repoFile.Id = Guid.NewGuid() + ".txt";
            var repoFilename = Build_repoFilename(repoFile);
            using(var fs = new FileStream(repoFilename, FileMode.Create, FileAccess.Write))
            {
                var buffer = new byte[16*1024];
                int n;
                while ((n = stream.Read(buffer, 0, buffer.Length)) > 0)
                    fs.Write(buffer, 0, n);
            }
            return repoFile;
        }


        public Tuple<RepoFile, Stream> Download(RepoFile repoFile)
        {
            var repoFilename = Build_repoFilename(repoFile);
            var stream = new FileStream(repoFilename, FileMode.Open, FileAccess.Read);
            return new Tuple<RepoFile, Stream>(repoFile, stream);
        }


        public RepoFile Delete(RepoFile repoFile)
        {
            var repoFilename = Build_repoFilename(repoFile);
            File.Delete(repoFilename);
            return repoFile;
        }


        private string Build_repoFilename(RepoFile repoFile)
        {
            return _repoPath + @"\" + repoFile.Id;
        }
    }
}

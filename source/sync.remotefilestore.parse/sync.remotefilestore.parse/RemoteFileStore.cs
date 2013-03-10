using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using sync.contracts;
using sync.remotefilestore.parse.api;

namespace sync.remotefilestore.parse
{
    public class RemoteFileStore : IRemoteFileStore
    {
        private readonly ParseFiles _parseFiles;

        public RemoteFileStore(string parseAppId, string parseRestKey, string parseMasterKey)
        {
            _parseFiles = new ParseFiles(parseAppId, parseRestKey, parseMasterKey);
        }


        public RepoFile Upload(RepoFile repoFile, Stream stream)
        {
            var uploadInfo = _parseFiles.Upload(stream, repoFile.RelativeFileName);
            stream.Dispose();

            var pfi = new ParseFileInfo {Url = uploadInfo.Url, Name = uploadInfo.Name};
            return new RepoFile {
                                    Id = pfi.ToString(),
                                    RelativeFileName = repoFile.RelativeFileName,
                                    RepoRoot = repoFile.RepoRoot,
                                    User = repoFile.User,
                                    TimeStamp = repoFile.TimeStamp
                                };
        }


        public Tuple<RepoFile, Stream> Download(RepoFile repoFile)
        {
            var pfi = ParseFileInfo.Parse(repoFile.Id);
            var stream = _parseFiles.Download(pfi.Url);
            return new Tuple<RepoFile, Stream>(repoFile, stream);
        }


        public RepoFile Delete(RepoFile repoFile)
        {
            var pfi = ParseFileInfo.Parse(repoFile.Id);
            _parseFiles.Delete(pfi.Name);
            return repoFile;
        }
    }
}

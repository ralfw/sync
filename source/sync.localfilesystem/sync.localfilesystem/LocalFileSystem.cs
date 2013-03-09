using System;
using System.IO;
using sync.contracts;

namespace sync.localfilesystem
{
    public class LocalFileSystem : ILocalFileSystem
    {
        private Func<string> getUsername = () => Environment.MachineName;
        private Func<RepoFile, DateTime> getTimestamp = repoFile => File.GetLastWriteTime(GetAbsoluteFilename(repoFile));

        public string GetRepoRoot() {
            return Directory.GetCurrentDirectory();
        }

        public void CollectRepoFiles(string repoRoot, Action<RepoFile> continueWith) {
            var absoluteRepoRoot = Path.GetFullPath(repoRoot);

            var files = Directory.GetFiles(absoluteRepoRoot, "*.*", SearchOption.AllDirectories);
            foreach (var file in files) {
                var repoFile = new RepoFile {
                    RepoRoot = absoluteRepoRoot,
                    RelativeFileName = file.Substring(absoluteRepoRoot.Length + 1)
                };
                continueWith(repoFile);
            }
        }

        public RepoFile EnrichWithRepoRoot(RepoFile repoFile) {
            repoFile.RepoRoot = GetRepoRoot();
            return repoFile;
        }

        public RepoFile EnrichWithMetadata(RepoFile repoFile) {
            repoFile.User = getUsername();
            repoFile.TimeStamp = getTimestamp(repoFile);
            return repoFile;
        }

        public Stream LoadFromFile(RepoFile repoFile) {
            var filename = GetAbsoluteFilename(repoFile);
            var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            return stream;
        }

        public RepoFile SaveToFile(RepoFile repoFile, Stream stream) {
            var filename = GetAbsoluteFilename(repoFile);
            var path = Path.GetDirectoryName(Path.GetFullPath(filename));
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            using (var filestream = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
                using (stream) {
                    var buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0) {
                        filestream.Write(buffer, 0, bytesRead);
                    }
                }
            }

            File.SetLastWriteTime(filename, repoFile.TimeStamp);
            return repoFile;
        }

        public void FilterExistingFiles(RepoFile repoFile, Action<RepoFile> onNonExistingFile) {
            var filename = GetAbsoluteFilename(repoFile);
            if (!File.Exists(filename)) {
                onNonExistingFile(repoFile);
            }
        }

        public RepoFile Delete(RepoFile repoFile) {
            var filename = GetAbsoluteFilename(repoFile);
            File.Delete(filename);
            return repoFile;
        }

        public RepoFile Rename(RepoFile repoFile) {
            var filename = GetAbsoluteFilename(repoFile);
            File.Move(filename, filename + ".synclocal");
            return repoFile;
        }

        private static string GetAbsoluteFilename(RepoFile repoFile) {
            return Path.Combine(repoFile.RepoRoot, repoFile.RelativeFileName);
        }

        public RepoFile GetTimeStamp(RepoFile repoFile) {
            var filename = GetAbsoluteFilename(repoFile);
            if (!File.Exists(filename)) return repoFile;

            var timeStamp = File.GetLastWriteTime(filename);
            return new RepoFile{Id=repoFile.Id, RelativeFileName = repoFile.RelativeFileName, RepoRoot = repoFile.RepoRoot, User = repoFile.User, TimeStamp = timeStamp};
        }

        internal void InjectMetadataFunctions(Func<string> getUsername, Func<RepoFile, DateTime> getTimestamp) {
            this.getUsername = getUsername;
            this.getTimestamp = getTimestamp;
        }
    }
}
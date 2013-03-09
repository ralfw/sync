using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sync.contracts;
using System.Text.RegularExpressions;

namespace sync.ignore
{
    public class IgnoreFilter : IIgnoreFilter
    {
        private readonly string _repoPath;
        private readonly IEnumerable<string> _ignorePatterns;

        public IgnoreFilter(string repoPath) : this(repoPath, null) {}
        internal IgnoreFilter(string repoPath, IEnumerable<string> ignorePatterns)
        {
            _repoPath = repoPath;
            _ignorePatterns = ignorePatterns ?? Load_ignore_patterns_from_file();
        }


        public void Filter(RepoFile repoFile, Action<RepoFile> onNotIgnored)
        {
            var normalizedFilename = repoFile.RelativeFileName.Replace(@"\", "/");

            if (normalizedFilename == ".sync" || normalizedFilename == ".syncconfig")
                return;

            if (_ignorePatterns.Where(pattern => !string.IsNullOrWhiteSpace(pattern))
                               .Any(pattern => Regex.Match(normalizedFilename, pattern, RegexOptions.IgnoreCase).Success))
                return;

            onNotIgnored(repoFile);
        }


        private IEnumerable<string> Load_ignore_patterns_from_file()
        {
            var ignoreFilename = _repoPath + @"\.syncignore";
            if (!File.Exists(ignoreFilename)) return new string[0];

            return File.ReadAllLines(ignoreFilename);
        }
    }
}

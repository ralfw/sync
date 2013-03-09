using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using equalidator;
using sync.contracts;

namespace sync.ignore.tests
{
    [TestFixture]
    public class test_IgnoreFilter
    {
        private const string REPO_PATH = null;


        [Test]
        public void No_file_to_ignore()
        {
            var sut = new IgnoreFilter(REPO_PATH, new string[]{});

            RepoFile result = null;
            var rf = new RepoFile {RelativeFileName = "hello.txt"};
            sut.Filter(rf, _ => result = _);

            Equalidator.AreEqual(result, rf);
        }


        [TestCase(".sync")]
        [TestCase(".syncconfig")]
        public void Ignore_certain_dotSync_files_in_reporoot(string relativeFilename)
        {
            var sut = new IgnoreFilter(REPO_PATH, new string[]{});

            RepoFile result = null;
            sut.Filter(new RepoFile { RelativeFileName = relativeFilename }, _ => result = _);

            Assert.IsNull(result);
        }

        [TestCase("test/.sync")]
        [TestCase("test/.syncconfig")]
        public void Dont_ignore_dotSync_files_in_subfolders(string relativeFilename)
        {
            var sut = new IgnoreFilter(REPO_PATH, new string[] { });

            RepoFile result = null;
            sut.Filter(new RepoFile { RelativeFileName = relativeFilename }, _ => result = _);

            Assert.IsNotNull(result);
        }


        [TestCase(@"testrepo\myfile.sync")]
        public void Dont_ignore_files_with_dotSync_extension(string relativeFilename)
        {
            var sut = new IgnoreFilter(REPO_PATH, new string[]{});

            var rf = new RepoFile {RelativeFileName = relativeFilename};
            RepoFile result = null;
            sut.Filter(rf, _ => result = _);

            Equalidator.AreEqual(result, rf);
        }


        [TestCase(@"myfile.txt", ".txt$", Description = "ignore on file extension")]
        [TestCase(@"testrepo/bin/myfile.txt", "/bin/", Description = "ignore folder")]
        [TestCase(@"bin/myfile.txt", "^bin/", Description = "ignore top level folder")]
        [TestCase(@"source/_resharper_foo/somefile.xyz", "/_resharper", Description = "ignore file or folder")]
        public void Ignore_on_single_pattern(string relativeFilename, string ignorePattern)
        {
            var sut = new IgnoreFilter(REPO_PATH, new string[] { ignorePattern });

            RepoFile result = null;
            sut.Filter(new RepoFile { RelativeFileName = relativeFilename }, _ => result = _);

            Assert.IsNull(result); 
        }


        [TestCase(@"bin/myfile.txt", "^bin/,/bin/")]
        [TestCase(@"testrepo/bin/myfile.txt", "^bin/,/bin/")]
        public void Ignore_on_multiple_patterns(string relativeFilename, string ignorePatterns)
        {
            var sut = new IgnoreFilter(REPO_PATH, ignorePatterns.Split(','));

            RepoFile result = null;
            sut.Filter(new RepoFile { RelativeFileName = relativeFilename }, _ => result = _);

            Assert.IsNull(result);
        }


        [TestCase(@"testrepo\abc")]
        [TestCase(@"testrepo/abc")]
        public void Ignore_on_normalized_path_delimiter(string relativeFilename)
        {
            var sut = new IgnoreFilter(REPO_PATH, new string[] { "/abc" });

            RepoFile result = null;
            sut.Filter(new RepoFile { RelativeFileName = relativeFilename }, _ => result = _);

            Assert.IsNull(result);
        }


        [TestCase(@"testrepo/bin/myfile.txt")]
        public void Load_ignore_patterns_from_ignorefile(string relativeFilename)
        {
            File.WriteAllText(".syncignore", "/bin/");
            var sut = new IgnoreFilter(".");

            RepoFile result = null;
            sut.Filter(new RepoFile { RelativeFileName = relativeFilename }, _ => result = _);

            Assert.IsNull(result);
        }


        [Test]
        public void Tolerate_nonexistent_ignorefile()
        {
            File.Delete(".syncignore");
            var sut = new IgnoreFilter(".");
        }


        [TestCase(@"x hello y")]
        [TestCase(@"HELLO")]
        public void Filter_case_insenstive(string relativeFilename)
        {
            var sut = new IgnoreFilter(null, new[]{"hello"});

            RepoFile result = null;
            sut.Filter(new RepoFile { RelativeFileName = relativeFilename }, _ => result = _);

            Assert.IsNull(result);
        }


        [TestCase(@"A", null)]
        [TestCase(@"B", null)]
        [TestCase(@"C", "C")]
        public void Ignore_empty_patterns(string relativeFilename, string expected)
        {
            var sut = new IgnoreFilter(null, new[] { "A", "", "B" });

            RepoFile result = null;
            sut.Filter(new RepoFile { RelativeFileName = relativeFilename }, _ => result = _);

            if (relativeFilename == "C") 
                Assert.AreEqual("C", result.RelativeFileName);
            else
                Assert.IsNull(result);
        }
    }
}

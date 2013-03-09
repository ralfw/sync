using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using sync.contracts;
using equalidator;

namespace sync.remotefilestore.filesystem.tests
{
    [TestFixture]
    public class test_RemoteFileStore
    {
        private const string REPO_PATH = "testrepo";
        private RemoteFileStore _sut;


        [SetUp]
        public void Setup()
        {
            if (Directory.Exists(REPO_PATH)) Directory.Delete(REPO_PATH, true);
            Directory.CreateDirectory(REPO_PATH);

            _sut = new RemoteFileStore(REPO_PATH);
        }


        [Test]
        public void Upload()
        {
            var rf = new RepoFile {RelativeFileName = "myfile.txt"};
            var ms = new MemoryStream(Encoding.ASCII.GetBytes("hello"));
            rf = _sut.Upload(rf, ms);
            Assert.IsNotNull(rf.Id);
            Assert.AreEqual("hello", File.ReadAllText(@"testrepo\" + rf.Id));
        }


        [Test]
        public void Download()
        {
            var rf = new RepoFile {RelativeFileName = "myfile.txt"};
            var ms = new MemoryStream(Encoding.ASCII.GetBytes("hello"));
            rf = _sut.Upload(rf, ms);

            var result = _sut.Download(rf);

            Assert.AreEqual(rf.RelativeFileName, result.Item1.RelativeFileName);

            ms = new MemoryStream();
            using (var fs = result.Item2)
            {
                var buffer = new byte[1024];
                int n;
                while((n = fs.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, n);
            }

            ms.Seek(0, SeekOrigin.Begin);
            var sr = new StreamReader(ms);
            Assert.AreEqual("hello", sr.ReadLine());
        }


        [Test]
        public void Delete()
        {
            var rf = new RepoFile { RelativeFileName = "myfile.txt" };
            var ms = new MemoryStream(Encoding.ASCII.GetBytes("hello"));
            rf = _sut.Upload(rf, ms);

            var result = _sut.Delete(rf);

            Equalidator.AreEqual(result, rf);
            Assert.IsFalse(File.Exists(@"testrepo\" + rf.Id));
        }


        [Test]
        public void Create_repo_folder_if_nonexistent()
        {
            Directory.Delete(REPO_PATH);

            var sut = new RemoteFileStore(REPO_PATH);

            Assert.IsTrue(Directory.Exists(REPO_PATH));
        }
    }
}

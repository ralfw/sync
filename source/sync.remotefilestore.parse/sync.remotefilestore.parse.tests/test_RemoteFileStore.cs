using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using equalidator;
using sync.contracts;
using sync.remotefilestore.parse.api;

namespace sync.remotefilestore.parse.tests
{
    [TestFixture]
    public class test_RemoteFileStore
    {
        private string _appId;
        private string _restKey;
        private string _masterKey;

        private ParseFiles _pf;

        [SetUp]
        public void Setup()
        {
            using (var sr = new StreamReader(@"..\..\..\..\..\unversioned\.syncconfig"))
            {
                _appId = sr.ReadLine();
                _restKey = sr.ReadLine();
                _masterKey = sr.ReadLine();
            }
            _pf = new ParseFiles(_appId, _restKey, _masterKey);
        }


        [Test, Explicit]
        public void Explore_parse_files()
        {
            var data = new MemoryStream(Encoding.ASCII.GetBytes("hello"));

            // Upload
            var ui = _pf.Upload(data, "myfn");
            Console.WriteLine("filename: {0}", ui.Name);
            Console.WriteLine("fileurl: {0}", ui.Url);

            // Download
            var dataString = new WebClient().DownloadString(ui.Url);
            Assert.AreEqual("hello", dataString);

            // Delete
            _pf.Delete(ui.Name);
        }


        [Test, Explicit]
        public void Upload_file()
        {
            var sut = new RemoteFileStore(_appId, _restKey, _masterKey);

            var data = new MemoryStream(Encoding.ASCII.GetBytes("hello"));
            var result = sut.Upload(new RepoFile{RelativeFileName = "myfn"}, data);

            Assert.AreEqual("myfn", result.RelativeFileName);
            Assert.IsTrue(result.Id.IndexOf("#") > 0);

            Console.WriteLine("repo file id: {0}", result.Id);

            var pfi = ParseFileInfo.Parse(result.Id);
            Console.WriteLine("Url: {0}", pfi.Url);
            Console.WriteLine("Name: {0}", pfi.Name);

            _pf.Delete(pfi.Name);
        }


        [Test, Explicit]
        public void Delete_file()
        {
            var sut = new RemoteFileStore(_appId, _restKey, _masterKey);
            var data = new MemoryStream(Encoding.ASCII.GetBytes("hello"));
            var rf = sut.Upload(new RepoFile { RelativeFileName = "myfn" }, data);

            var result = sut.Delete(rf);
            Assert.AreEqual(result, rf);

            Assert.Throws<WebException>(() =>
                {
                    var pfi = ParseFileInfo.Parse(result.Id);
                    new WebClient().DownloadString(pfi.Url);
                });
        }


        [Test, Explicit]
        public void Download_file()
        {
            var sut = new RemoteFileStore(_appId, _restKey, _masterKey);
            var data = new MemoryStream(Encoding.ASCII.GetBytes("hello"));
            var rf = sut.Upload(new RepoFile { RelativeFileName = "myfn" }, data);
            try
            {
                var result = sut.Download(rf);
                Equalidator.AreEqual(result.Item1, rf);
                using (var sr = new StreamReader(result.Item2))
                {
                    Assert.AreEqual("hello", sr.ReadLine());
                }
            }
            finally
            {
                sut.Delete(rf);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using sync.contracts;

namespace sync.remotesynctable.parse.tests
{
    [TestFixture]
    public class test_RemoteSyncTable
    {
        private RemoteSyncTable _sut;

        [SetUp]
        public void Setup()
        {
            using (var sr = new StreamReader(@"..\..\..\..\..\unversioned\.syncconfig"))
            {
                var appId = sr.ReadLine();
                var restKey = sr.ReadLine();

                _sut = new RemoteSyncTable("testrepo", appId, restKey);
            }
        }


        [Test, Explicit]
        public void Add_entry()
        {
            _sut.AddEntry(new RepoFile{Id="myid" + DateTime.Now.ToString("s"),
                                       RelativeFileName = "myfn",
                                       RepoRoot = "myroot",
                                       TimeStamp = DateTime.Now,
                                       User="myuser"});
        }


        [Test, Explicit]
        public void Find_entry()
        {
            _sut.AddEntry(new RepoFile  {
                                            Id = "myid" + DateTime.Now.ToString("s"),
                                            RelativeFileName = "myfn",
                                            RepoRoot = "myroot",
                                            TimeStamp = DateTime.Now,
                                            User = "myuser"
                                        });

            Dictionary<string, object> result;
            Assert.IsTrue(_sut.TryFindEntry(new RepoFile {RelativeFileName = "myfn"}, out result));
        }

        [Test, Explicit]
        public void Find_entry_with_subdir_in_relativeFilename()
        {
            _sut.AddEntry(new RepoFile
            {
                Id = "myid" + DateTime.Now.ToString("s"),
                RelativeFileName = @"folder\myfn",
                RepoRoot = "myroot",
                TimeStamp = DateTime.Now,
                User = "myuser"
            });

            Dictionary<string, object> result;
            Assert.IsTrue(_sut.TryFindEntry(new RepoFile { RelativeFileName = @"folder\myfn" }, out result));
        }


        [Test, Explicit]
        public void Update_entry()
        {
            var oldId = "myid" + DateTime.Now.ToString("s");
            var repoFile = new RepoFile{Id=oldId,
                                       RelativeFileName = "myfn",
                                       RepoRoot = "myroot",
                                       TimeStamp = DateTime.Now,
                                       User="myuser"};
            _sut.AddEntry(repoFile);

            repoFile.Id = "myid new " + DateTime.Now.ToString("s");
            repoFile.User = "myuser new";

            RepoFile result = null;
            _sut.UpdateEntry(repoFile, _ => result = _, null); 
  
            Assert.AreEqual(oldId, result.Id);
        }

        [Test, Explicit]
        public void Unknown_entry_to_update()
        {
            var repoFile = new RepoFile
            {
                Id = "myid" + DateTime.Now.ToString("s"),
                RelativeFileName = "my unknown fn",
                RepoRoot = "myroot",
                TimeStamp = DateTime.Now,
                User = "myuser"
            };

            RepoFile result = null;
            _sut.UpdateEntry(repoFile, null, _ => result = _);

            Assert.AreEqual(repoFile.Id, result.Id);
        }

        [Test, Explicit]
        public void Delete_entry()
        {
            var repoFile = new RepoFile {
                                            Id = "myid" + DateTime.Now.ToString("s"),
                                            RelativeFileName = "myfntobedeleted",
                                            RepoRoot = "myroot",
                                            TimeStamp = DateTime.Now,
                                            User = "myuser"
                                        };
            _sut.AddEntry(repoFile);

            var result = _sut.DeleteEntry(repoFile);

            Assert.AreEqual(repoFile.Id, result.Id);
        }

        [Test, Explicit]
        public void Delete_nonexisting_entry()
        {
            var repoFile = new RepoFile
            {
                Id = "myid" + DateTime.Now.ToString("s"),
                RelativeFileName = "my nonexisting entry",
                RepoRoot = "myroot",
                TimeStamp = DateTime.Now,
                User = "myuser"
            };

            var result = _sut.DeleteEntry(repoFile);

            Assert.AreEqual(repoFile.Id, result.Id);
        }

        [Test, Explicit]
        public void Test()
        {
            _sut.AddEntry(new RepoFile  {
                                            Id = "myid" + DateTime.Now.ToString("s"),
                                            RelativeFileName = "myfn",
                                            RepoRoot = "myroot",
                                            TimeStamp = DateTime.Now,
                                            User = "myuser"
                                        });

            var results = new List<RepoFile>();

            _sut.CollectRepoFiles(results.Add);

            Assert.IsTrue(results.Count > 0);
        }

        [Test]
        public void Filter_existing_file()
        {
            _sut.AddEntry(new RepoFile  {
                                            Id = "myid" + DateTime.Now.ToString("s"),
                                            RelativeFileName = "myfn",
                                            RepoRoot = "myroot",
                                            TimeStamp = DateTime.Now,
                                            User = "myuser"
                                        });

            RepoFile result = null;
            _sut.FilterExistingFiles(new RepoFile{RelativeFileName = "myfn"}, _ => result = _);

            Assert.IsNull(result);
        }

        [Test]
        public void Pass_on_nonexisting_file()
        {
            RepoFile result = null;
            _sut.FilterExistingFiles(new RepoFile { RelativeFileName = "my nonexisting fn" }, _ => result = _);

            Assert.AreEqual("my nonexisting fn", result.RelativeFileName);
        }
    }
}

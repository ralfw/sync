using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using equalidator;
using sync.contracts;

namespace sync.remotesynctable.filesystem.tests
{
    [TestFixture]
    public class test_RemoteSyncTable
    {
        private const string REPO_PATH = "testrepo";


        [SetUp]
        public void Setup()
        {
            if (Directory.Exists(REPO_PATH)) Directory.Delete(REPO_PATH, true);
            Directory.CreateDirectory(REPO_PATH);
        }


        [Test]
        public void AddEntry()
        {
            var syncTable = new List<SyncTableEntry>();
            var sut = new RemoteSyncTable(REPO_PATH, syncTable);

            sut.AddEntry(new RepoFile{Id="myid", User="myuser", TimeStamp=new DateTime(2000,5,12), RelativeFileName = "myfn"});

            Equalidator.AreEqual(new List<SyncTableEntry>{new SyncTableEntry{Id="myid", User="myuser", TimeStamp=new DateTime(2000,5,12), RelativeFilename="myfn"}}, syncTable);
        }


        [Test]
        public void Update_existing_entry()
        {
            var syncTable = new List<SyncTableEntry> {new SyncTableEntry {Id = "myid", User="olduser", TimeStamp=new DateTime(1999, 12, 31), RelativeFilename="myfn"}};
            var sut = new RemoteSyncTable(REPO_PATH, syncTable);

            RepoFile result = null;
            sut.UpdateEntry(new RepoFile { Id = "my new id", User = "myuser", TimeStamp = new DateTime(2000, 5, 12), RelativeFileName = "myfn" }, 
                            _ => result = _, null);

            Equalidator.AreEqual(result, new RepoFile{Id="myid", User="olduser", TimeStamp=new DateTime(1999,12,31), RelativeFileName="myfn"});
            Equalidator.AreEqual(new List<SyncTableEntry> { new SyncTableEntry { Id = "my new id", User="myuser", TimeStamp=new DateTime(2000,5,12), RelativeFilename = "myfn" } }, syncTable);
        }

        [Test]
        public void Update_nonexisting_entry()
        {
            var syncTable = new List<SyncTableEntry> {new SyncTableEntry {Id = "myid", RelativeFilename = "myfn"}};
            var sut = new RemoteSyncTable(REPO_PATH, syncTable);

            var rf = new RepoFile { Id = "my new id", RelativeFileName = "my non existing fn" };
            RepoFile result = null;
            sut.UpdateEntry(rf, null, _ => result = _);

            Equalidator.AreEqual(result, rf);
        }


        [Test]
        public void CollectRepoFiles()
        {
            var syncTable = new List<SyncTableEntry> { 
                                                        new SyncTableEntry { Id = "myid", User="myuser", TimeStamp=new DateTime(1999,12,31), RelativeFilename = "myfn" },
                                                        new SyncTableEntry { Id = "myid2", User="myuser2", TimeStamp=new DateTime(2000,5,12), RelativeFilename = "myfn2" }
                                                     };
            var sut = new RemoteSyncTable(REPO_PATH, syncTable);

            var repoFiles = new List<RepoFile>();
            sut.CollectRepoFiles(repoFiles.Add);

            Equalidator.AreEqual(repoFiles, new List<RepoFile> {
                                                        new RepoFile{Id="myid", User="myuser", TimeStamp=new DateTime(1999,12,31), RelativeFileName="myfn"},
                                                        new RepoFile{Id="myid2", User="myuser2", TimeStamp=new DateTime(2000,5,12), RelativeFileName="myfn2"}
                                                  });
        }


        [Test]
        public void Persist_sync_table_after_add_and_update()
        {
            var sut = new RemoteSyncTable(REPO_PATH, new List<SyncTableEntry>());
            
            sut.AddEntry(new RepoFile{Id="myid", User="myuser", TimeStamp=new DateTime(2000,5,12, 10, 11, 12), RelativeFileName="myfn"});
            Assert.AreEqual("myfn\tmyid\tmyuser\t2000-05-12T10:11:12\r\n", File.ReadAllText(REPO_PATH + @"\.sync"));

            sut.UpdateEntry(new RepoFile { Id = "myid2", User = "myuser2", TimeStamp = new DateTime(1999, 12, 31), RelativeFileName = "myfn" }, _ => { }, null);
            Assert.AreEqual("myfn\tmyid2\tmyuser2\t1999-12-31T00:00:00\r\n", File.ReadAllText(REPO_PATH + @"\.sync"));

            sut.AddEntry(new RepoFile { Id = "yourid", User = "myuser3", TimeStamp = new DateTime(2001, 3, 26), RelativeFileName = "yourfn" });
            Assert.AreEqual("myfn\tmyid2\tmyuser2\t1999-12-31T00:00:00\r\nyourfn\tyourid\tmyuser3\t2001-03-26T00:00:00\r\n", File.ReadAllText(REPO_PATH + @"\.sync"));
        }


        [Test]
        public void Load_sync_table_upon_creation()
        {
            var sut = new RemoteSyncTable(REPO_PATH, new List<SyncTableEntry>());
            var rf = new RepoFile { Id = "myid", User = "myuser", TimeStamp = new DateTime(2000, 5, 12), RelativeFileName = "myfn" };
            sut.AddEntry(rf);

            sut = new RemoteSyncTable(REPO_PATH);

            RepoFile result = null;
            sut.CollectRepoFiles(_ => result = _);
            Equalidator.AreEqual(rf, result);

            sut.UpdateEntry(new RepoFile { Id = "myid2", User = "myuser2", TimeStamp = new DateTime(1999, 12, 31), RelativeFileName = "myfn" }, _ => { }, null);
            Assert.AreEqual("myfn\tmyid2\tmyuser2\t1999-12-31T00:00:00\r\n", File.ReadAllText(REPO_PATH + @"\.sync"));
        }


        [Test]
        public void Initialize_sync_table_if_nonexistent()
        {
            var sut = new RemoteSyncTable(REPO_PATH);

            sut.AddEntry(new RepoFile { Id = "myid", User="myuser", TimeStamp=new DateTime(2000,5,12), RelativeFileName = "myfn" });

            Assert.AreEqual("myfn\tmyid\tmyuser\t2000-05-12T00:00:00\r\n", File.ReadAllText(REPO_PATH + @"\.sync"));
        }


        [Test]
        public void Create_repo_folder_if_nonexistent()
        {
            Directory.Delete(REPO_PATH, true);

            var sut = new RemoteSyncTable(REPO_PATH);

            sut.AddEntry(new RepoFile { Id = "myid", User="myuser", TimeStamp=new DateTime(2000,5,12), RelativeFileName = "myfn" });

            Assert.AreEqual("myfn\tmyid\tmyuser\t2000-05-12T00:00:00\r\n", File.ReadAllText(REPO_PATH + @"\.sync"));
        }


        [Test]
        public void Delete_entry()
        {
            var syncTable = new List<SyncTableEntry> {new SyncTableEntry{RelativeFilename = "myfn"}};
            var sut = new RemoteSyncTable(REPO_PATH, syncTable);

            var rf = new RepoFile {RelativeFileName = "myfn"};
            var result = sut.DeleteEntry(rf);

            Equalidator.AreEqual(result, rf);
            Assert.AreEqual(0, syncTable.Count);
        }

        [Test]
        public void Persist_sync_table_after_delete()
        {
            var sut = new RemoteSyncTable(REPO_PATH, new List<SyncTableEntry>());

            sut.AddEntry(new RepoFile { Id = "myid", User = "myuser", TimeStamp = new DateTime(2000, 5, 12), RelativeFileName = "myfn" });
            sut.AddEntry(new RepoFile { Id = "yourid", User = "myuser3", TimeStamp = new DateTime(2001, 3, 26), RelativeFileName = "yourfn" });

            sut.DeleteEntry(new RepoFile {RelativeFileName = "yourfn"});

            Assert.AreEqual("myfn\tmyid\tmyuser\t2000-05-12T00:00:00\r\n", File.ReadAllText(REPO_PATH + @"\.sync"));
        }


        [Test]
        public void Filter_existing_file()
        {
            var syncTable = new List<SyncTableEntry> { new SyncTableEntry { RelativeFilename = "myfn" } };
            var sut = new RemoteSyncTable(null, syncTable);

            RepoFile result = null;
            sut.FilterExistingFiles(new RepoFile{RelativeFileName = "myfn"}, _ => result = _);

            Assert.IsNull(result);
        }

        [Test]
        public void Pass_through_nonexisting_file()
        {
            var syncTable = new List<SyncTableEntry> { new SyncTableEntry { RelativeFilename = "myfn" } };
            var sut = new RemoteSyncTable(null, syncTable);

            RepoFile result = null;
            sut.FilterExistingFiles(new RepoFile { RelativeFileName = "my nonexisting fn" }, _ => result = _);

            Equalidator.AreEqual(result, new RepoFile{RelativeFileName = "my nonexisting fn"});
        }
    }
}

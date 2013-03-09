using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using equalidator;
using sync.contracts;

namespace sync.localsynctable.tests
{
    [TestFixture]
    public class test_LocalSyncTable
    {
        private const string REPO_PATH = "testrepo";

        [Test]
        public void Add_repo_file()
        {
            var syncTable = new List<SyncTableEntry>();
            var sut = new LocalSyncTable(REPO_PATH, syncTable);

            sut.AddOrUpdateEntry(new RepoFile{Id="myid", RelativeFileName="myfn", User="myuser", TimeStamp=new DateTime(2000,5,12)});

            Equalidator.AreEqual(syncTable, new List<SyncTableEntry> { new SyncTableEntry{ Id = "myid", RelativeFilename = "myfn", User = "myuser", TimeStamp = new DateTime(2000, 5, 12) } });
        }

        [Test]
        public void Update_repo_file()
        {
            var syncTable = new List<SyncTableEntry> {new SyncTableEntry {RelativeFilename = "myfn"}};
            var sut = new LocalSyncTable(REPO_PATH, syncTable);

            sut.AddOrUpdateEntry(new RepoFile {Id="myid", RelativeFileName="myfn", User="myuser", TimeStamp=new DateTime(2000, 5, 12) });

            Equalidator.AreEqual(syncTable, 
                                 new List<SyncTableEntry> { new SyncTableEntry{ Id = "myid", RelativeFilename = "myfn", User = "myuser", TimeStamp = new DateTime(2000, 5, 12) } },
                                 true);
        }

        [Test]
        public void Save_synctable_after_each_update()
        {
            Directory.CreateDirectory(REPO_PATH);
            File.Delete(REPO_PATH + @"\.sync");

            var sut = new LocalSyncTable(REPO_PATH);

            sut.AddOrUpdateEntry(new RepoFile { Id = "myid", RelativeFileName = "myfn", User = "myuser", TimeStamp = new DateTime(2000, 5, 12) });

            Assert.AreEqual("myfn\tmyid\tmyuser\t2000-05-12T00:00:00\r\n", File.ReadAllText(REPO_PATH + @"\.sync"));
        }

        [Test]
        public void Load_sync_table_on_creation()
        {
            Directory.CreateDirectory(REPO_PATH);
            File.WriteAllText(REPO_PATH + @"\.sync", "myfn\tmyid\tmyuser\t2000-05-12T00:00:00\r\n");

            var sut = new LocalSyncTable(REPO_PATH);

            sut.AddOrUpdateEntry(new RepoFile { Id = "myid2", RelativeFileName = "myfn2", User = "myuser2", TimeStamp = new DateTime(1999, 12, 31) });

            Assert.AreEqual("myfn\tmyid\tmyuser\t2000-05-12T00:00:00\r\nmyfn2\tmyid2\tmyuser2\t1999-12-31T00:00:00\r\n", File.ReadAllText(REPO_PATH + @"\.sync"));

        }


        [Test]
        public void Filter_unchanged_file_by_timestamp()
        {
            var sut = new LocalSyncTable(null, new List<SyncTableEntry> { new SyncTableEntry { RelativeFilename = "myfn", TimeStamp = new DateTime(2000, 5, 12) } });

            RepoFile result = null;
            sut.FilterUnchangedByTimeStamp(new RepoFile { RelativeFileName = "myfn", TimeStamp = new DateTime(2000, 5, 12) }, _ => result = _);

            Assert.IsNull(result);
        }

        [Test]
        public void Pass_on_changed_file_by_timestamp()
        {
            var sut = new LocalSyncTable(null, new List<SyncTableEntry> { new SyncTableEntry { RelativeFilename = "myfn", TimeStamp = new DateTime(2000, 5, 12) } });

            var rf = new RepoFile {RelativeFileName = "myfn", TimeStamp = new DateTime(2000, 5, 12, 10, 11, 12)};
            RepoFile result = null;
            sut.FilterUnchangedByTimeStamp(rf, _ => result = _);

            Equalidator.AreEqual(result, rf);
        }

        [Test]
        public void Pass_on_new_file_by_timestamp()
        {
            var sut = new LocalSyncTable(null, new List<SyncTableEntry> { new SyncTableEntry { RelativeFilename = "myfn", TimeStamp = new DateTime(2000, 5, 12) } });

            var rf = new RepoFile { RelativeFileName = "myfn2", TimeStamp = new DateTime(2000, 5, 12) };
            RepoFile result = null;
            sut.FilterUnchangedByTimeStamp(rf, _ => result = _);

            Equalidator.AreEqual(result, rf);   
        }


        [Test]
        public void Filter_with_timestamp_resolution_of_seconds()
        {
            var sut = new LocalSyncTable(null, new List<SyncTableEntry> { new SyncTableEntry { RelativeFilename = "myfn", TimeStamp = new DateTime(2000, 5, 12, 10, 11, 12, 123) } });

            RepoFile result = null;
            sut.FilterUnchangedByTimeStamp(new RepoFile { RelativeFileName = "myfn", TimeStamp = new DateTime(2000, 5, 12, 10, 11, 12, 456) }, _ => result = _);

            Assert.IsNull(result);
        }


        [Test]
        public void Filter_unchanged_file_by_id()
        {
            var sut = new LocalSyncTable(null, new List<SyncTableEntry> { new SyncTableEntry { RelativeFilename = "myfn", Id="myid" } });

            RepoFile result = null;
            sut.FilterUnchangedById(new RepoFile { RelativeFileName = "myfn", Id = "myid"}, _ => result = _);

            Assert.IsNull(result);
        }

        [Test]
        public void Pass_on_changed_file_by_id()
        {
            var sut = new LocalSyncTable(null, new List<SyncTableEntry> { new SyncTableEntry { RelativeFilename = "myfn", Id="myid" } });

            var rf = new RepoFile { RelativeFileName = "myfn", Id="my new id" };
            RepoFile result = null;
            sut.FilterUnchangedById(rf, _ => result = _);

            Equalidator.AreEqual(result, rf);
        }


        [Test]
        public void Collect_repo_files()
        {
            var sut = new LocalSyncTable(null, new List<SyncTableEntry>
                {
                    new SyncTableEntry { RelativeFilename = "myfn", Id = "myid" },
                    new SyncTableEntry { RelativeFilename = "myfn2", Id = "myid2" }
                });

            var result = new List<RepoFile>();
            sut.CollectRepoFiles(result.Add);

            Equalidator.AreEqual(result, new List<RepoFile>
                {
                    new RepoFile{RelativeFileName = "myfn", Id = "myid"},
                    new RepoFile{RelativeFileName = "myfn2", Id = "myid2"}
                });
        }


        [Test]
        public void Delete_entry()
        {
            var sut = new LocalSyncTable(REPO_PATH, new List<SyncTableEntry>
                {
                    new SyncTableEntry { RelativeFilename = "myfn", Id = "myid" },
                    new SyncTableEntry { RelativeFilename = "myfn2", Id = "myid2" }
                });

            var rf = new RepoFile {RelativeFileName = "myfn2"};
            var result = sut.DeleteEntry(rf);

            Equalidator.AreEqual(result, rf);
        }


        [Test]
        public void Sync_table_is_persisted_after_delete()
        {
            Directory.CreateDirectory(REPO_PATH);
            File.Delete(REPO_PATH + @"\.sync");

            var sut = new LocalSyncTable(REPO_PATH, new List<SyncTableEntry>
                {
                    new SyncTableEntry { RelativeFilename = "myfn", Id = "myid", User = "myuser", TimeStamp = new DateTime(2000,5,12)},
                    new SyncTableEntry { RelativeFilename = "myfn2", Id = "myid2" }
                });

            var rf = new RepoFile { RelativeFileName = "myfn2" };
            var result = sut.DeleteEntry(rf);

            Assert.AreEqual("myfn\tmyid\tmyuser\t2000-05-12T00:00:00\r\n", File.ReadAllText(REPO_PATH + @"\.sync"));
        }

        [Test]
        public void Delete_entries_while_collecting()
        {
            var syncTable = new List<SyncTableEntry>
                {
                    new SyncTableEntry {RelativeFilename = "myfn", Id = "myid"},
                    new SyncTableEntry {RelativeFilename = "myfn2", Id = "myid2"}
                };
            var sut = new LocalSyncTable(REPO_PATH, syncTable);

            sut.CollectRepoFiles(_ => sut.DeleteEntry(_));

            Assert.AreEqual(0, syncTable.Count);
        }


        [Test]
        public void Get_timestamp()
        {
            var syncTable = new List<SyncTableEntry> { new SyncTableEntry {RelativeFilename = "myfn", Id = "myid", TimeStamp = new DateTime(2000, 5, 12), User = "myuser"} };
            var sut = new LocalSyncTable(REPO_PATH, syncTable);

            var rf = new RepoFile {RelativeFileName = "myfn", Id = "new id", User = "new user"};
            var result = sut.GetTimeStamp(rf);

            Assert.AreNotSame(result, rf);
            Equalidator.AreEqual(result, new RepoFile { RelativeFileName = "myfn", Id = "new id", User = "new user", TimeStamp = new DateTime(2000,5,12)});
        }


        [Test]
        public void Get_timestamp_for_nonexistent_file()
        {
            var syncTable = new List<SyncTableEntry>();
            var sut = new LocalSyncTable(REPO_PATH, syncTable);

            var result = sut.GetTimeStamp(new RepoFile { RelativeFileName = "myfn", Id = "new id", User = "new user", TimeStamp = new DateTime(2000,5,12)});

            Assert.IsNull(result);
        }
    }
}

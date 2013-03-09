using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using sync.contracts;

namespace sync.localfilesystem.tests
{
    [TestFixture]
    public class LocalFileSystemTests
    {
        private LocalFileSystem sut;

        [SetUp]
        public void Setup() {
            sut = new LocalFileSystem();
        }

        [Test]
        public void RepoRoot_is_current_directory() {
            Assert.That(sut.GetRepoRoot(), Is.EqualTo(Directory.GetCurrentDirectory()));
        }

        [Test]
        public void CollectRepoFiles_collects_relative_filenames() {
            var repoFiles = new List<RepoFile>();
            sut.CollectRepoFiles("TestData", repoFiles.Add);

            var absoluteRepoRoot = Path.Combine(Directory.GetCurrentDirectory(), "TestData");

            Assert.That(repoFiles.Select(x => x.RepoRoot).ToArray(), Is.EqualTo(
                new[] {
                    absoluteRepoRoot,
                    absoluteRepoRoot,
                    absoluteRepoRoot,
                    absoluteRepoRoot,
                    absoluteRepoRoot,
                    absoluteRepoRoot
                }));
            Assert.That(repoFiles.Select(x => x.RelativeFileName).ToArray(), Is.EqualTo(new[] {
                ".sync",
                "File1.txt",
                "SubDir1\\File2.txt",
                "SubDir1\\SubDir1a\\File3.txt",
                "SubDir1\\SubDir1b\\File4.txt",
                "SubDir2\\File5.txt"
            }));
        }

        [Test]
        public void EnrichWithRepoRoot_adds_the_RepoRoot_to_the_RepoFile() {
            var repoFile = sut.EnrichWithRepoRoot(new RepoFile());
            Assert.That(repoFile.RepoRoot, Is.EqualTo(sut.GetRepoRoot()));
        }

        [Test]
        public void Enrich_with_Metadata_adds_machine_name_and_timestamp() {
            sut.InjectMetadataFunctions(() => "MyMachine", r => new DateTime(2013, 2, 4));
            var repoFile = sut.EnrichWithMetadata(new RepoFile());
            Assert.That(repoFile.User, Is.EqualTo("MyMachine"));
            Assert.That(repoFile.TimeStamp, Is.EqualTo(new DateTime(2013, 2, 4)));
        }

        [Test]
        public void LoadFromFile() {
            var stream = sut.LoadFromFile(new RepoFile { RepoRoot = "TestData", RelativeFileName = "SubDir1\\File2.txt" });
            FileAssert.AreEqual("Hallo Welt".ToStream(), stream);
        }

        [Test]
        public void SaveToFile_saves_the_stream() {
            sut.SaveToFile(new RepoFile { RepoRoot = ".", RelativeFileName = "test.txt", TimeStamp = new DateTime(2013, 2, 4) }, "Hallo Welt".ToStream());
            FileAssert.AreEqual("TestData\\SubDir1\\File2.txt", "test.txt");
        }

        [Test]
        public void SaveToFile_creates_the_directory_structure_if_needed() {
            try {
                Directory.Delete("SubDir3\\SubDir3a", true);
            }
            catch {
            }
            sut.SaveToFile(new RepoFile { RepoRoot = ".", RelativeFileName = "SubDir3\\SubDir3a\\test.txt", TimeStamp = new DateTime(2013, 2, 4) },
                "Hallo Welt".ToStream());
            FileAssert.AreEqual("TestData\\SubDir1\\File2.txt", "SubDir3\\SubDir3a\\test.txt");
        }

        [Test]
        public void SaveToFile_sets_the_timestamp() {
            sut.SaveToFile(new RepoFile { RepoRoot = ".", RelativeFileName = "test.txt", TimeStamp = new DateTime(2013, 2, 4) }, "Hallo Welt".ToStream());
            Assert.That(File.GetLastWriteTime("test.txt"), Is.EqualTo(new DateTime(2013, 2, 4)));
        }

        [Test]
        public void FilterExistingFiles_returns_non_existing_file() {
            RepoFile repoFile = null;
            sut.FilterExistingFiles(new RepoFile { RepoRoot = ".", RelativeFileName = "non-existing" }, f => repoFile = f);
            Assert.NotNull(repoFile);
        }

        [Test]
        public void FilterExistingFiles_filters_non_existing_file() {
            RepoFile repoFile = null;
            sut.FilterExistingFiles(new RepoFile { RepoRoot = "TestData", RelativeFileName = "file1.txt" }, f => repoFile = f);
            Assert.Null(repoFile);
        }

        [Test]
        public void Delete() {
            using (File.Create("to-be-deleted")) {
                ;
            }
            Assert.That(File.Exists("to-be-deleted"), Is.True);
            sut.Delete(new RepoFile { RepoRoot = ".", RelativeFileName = "to-be-deleted" });
            Assert.That(File.Exists("to-be-deleted"), Is.False);
        }

        [Test]
        public void Rename_adds_dot_local_to_filename() {
            using (File.Create("to-be-renamed")) {
                ;
            }
            File.Delete("to-be-renamed.synclocal");
            
            sut.Rename(new RepoFile { RepoRoot = ".", RelativeFileName = "to-be-renamed" });
            Assert.That(File.Exists("to-be-renamed"), Is.False);
            Assert.That(File.Exists("to-be-renamed.synclocal"), Is.True);
        }

        [Test]
        public void TimeStamp_from_local_file_system_is_read() {
            using (File.Create("new-file")) {
                ;
            }
            File.SetLastWriteTime("new-file", new DateTime(2010, 2, 4, 11, 12, 13));

            var rf = new RepoFile {RepoRoot = ".", RelativeFileName = "new-file", TimeStamp = new DateTime(2013, 1, 3)};
            var result = sut.GetTimeStamp(rf);

            Assert.AreNotSame(result, rf);
            Assert.That(result.TimeStamp, Is.EqualTo(new DateTime(2010, 2, 4, 11, 12, 13)));
 
            File.Delete("new-file");
        }

        [Test]
        public void TimeStamp_for_non_eisting_file() {
            Assert.That(File.Exists("non-existing"), Is.False);
            
            var result = sut.GetTimeStamp(new RepoFile { RepoRoot = ".", RelativeFileName = "new-file", TimeStamp = new DateTime(2013, 1, 3) });
            Assert.IsNull(result);
         }
    }
}
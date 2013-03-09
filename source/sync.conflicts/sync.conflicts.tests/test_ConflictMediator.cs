using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using equalidator;
using sync.contracts;

namespace sync.conflicts.tests
{
    [TestFixture]
    public class test_ConflictMediator
    {
        [Test]
        public void No_conflict()
        {
            var sut = new ConflictMediator();

            RepoFile result = null;
            var local = new RepoFile {TimeStamp = new DateTime(2000,5,12,10,11,12, 123)};
            var current = new RepoFile {TimeStamp = new DateTime(2000,5,12,10,11,12,456)};
            var remote = new RepoFile {TimeStamp = new DateTime(2000, 12, 31, 12, 13, 14, 789)};
            sut.DetectUpdateConflct(local, current, remote, _ => result = _, null);

            Equalidator.AreEqual(remote, result);
        }


        [Test]
        public void Conflict_with_new_version_of_existing_file()
        {
            var sut = new ConflictMediator();

            RepoFile result = null;
            var local = new RepoFile { TimeStamp = new DateTime(2000, 5, 12, 10, 11, 12, 123) };
            var current = new RepoFile { TimeStamp = new DateTime(2000, 7, 28, 9, 10, 11, 456) };
            var remote = new RepoFile { TimeStamp = new DateTime(2000, 12, 31, 12, 13, 14, 789) };
            sut.DetectUpdateConflct(local, current, remote, null, _ => result = _);

            Equalidator.AreEqual(remote, result);
        }


        [Test]
        public void Conflict_with_new_file()
        {
            var sut = new ConflictMediator();

            RepoFile result = null;
            var current = new RepoFile { TimeStamp = new DateTime(2000, 7, 28, 9, 10, 11, 456) };
            var remote = new RepoFile { TimeStamp = new DateTime(2000, 12, 31, 12, 13, 14, 789) };
            sut.DetectUpdateConflct(null, current, remote, null, _ => result = _);

            Equalidator.AreEqual(remote, result);
        }

        [Test]
        public void No_conflict_with_nonexistent_local_file()
        {
            var sut = new ConflictMediator();

            RepoFile result = null;
            var remote = new RepoFile { TimeStamp = new DateTime(2000, 12, 31, 12, 13, 14, 789) };
            sut.DetectUpdateConflct(null, null, remote, _ => result = _, null);

            Equalidator.AreEqual(remote, result);
        }
    }
}

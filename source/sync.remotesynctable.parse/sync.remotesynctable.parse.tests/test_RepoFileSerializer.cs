using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using NUnit.Framework;
using sync.contracts;

namespace sync.remotesynctable.parse.tests
{
    [TestFixture]
    public class test_RepoFileSerializer
    {
        [Test]
        public void Encode_backslashes_in_relativeFilename()
        {
            var repoFile = new RepoFile
                {
                    RelativeFileName = @"folder\filename",
                    Id = "myid",
                    RepoRoot = "myroot",
                    TimeStamp = new DateTime(2000,5,12,10,11,12),
                    User = "myuser"
                };

            var jsonRepoFile = repoFile.ToJson();

            Assert.IsTrue(jsonRepoFile.IndexOf(@"folder\filename")<0);
        }

        [Test]
        public void Decode_backslashes_in_relativeFilename()
        {
            var repoFile = new RepoFile
            {
                RelativeFileName = @"folder\filename",
                Id = "myid",
                RepoRoot = "myroot",
                TimeStamp = new DateTime(2000, 5, 12, 10, 11, 12),
                User = "myuser"
            };

            var jsonRepoFile = repoFile.ToJson();

            var jss = new JavaScriptSerializer();
            var result = ((Dictionary<string, object>) jss.DeserializeObject(jsonRepoFile)).ToRepoFile();

            Assert.AreEqual(repoFile.RelativeFileName, result.RelativeFileName);
        }
    }
}

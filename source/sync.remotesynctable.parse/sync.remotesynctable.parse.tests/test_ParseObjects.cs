using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using sync.remotefilestore.parse.api;

namespace sync.remotesynctable.parse.tests
{
    [TestFixture]
    public class test_ParseObjects
    {
        private ParseObjects _sut;

        [SetUp]
        public void Setup()
        {
            using (var sr = new StreamReader(@"..\..\..\..\..\unversioned\.syncconfig"))
            {
                var appId = sr.ReadLine();
                var restKey = sr.ReadLine();

                _sut = new ParseObjects(appId, restKey);
            }
        }

        [Test]
        public void Inc_on_existing_object()
        {
            var id = _sut.New("testrepo_lock", "{\"counter\":0}");
            Console.WriteLine(id);

            _sut.Inc("testrepo_lock", id, "counter", 42);

            var json = _sut["testrepo_lock", id];
            Console.WriteLine(json);

            Assert.IsTrue(json.IndexOf("42")>0);
        }
    }
}

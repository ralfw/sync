using System.IO;
using System.Text;

namespace sync.localfilesystem.tests
{
    public static class TestHelper
    {
         public static Stream ToStream(this string s) {
             var bytearray = Encoding.UTF8.GetBytes(s);
             var memoryStream = new MemoryStream(bytearray);
             return memoryStream;
         }
    }
}
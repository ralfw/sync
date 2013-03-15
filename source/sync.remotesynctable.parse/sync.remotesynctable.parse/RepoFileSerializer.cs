using System;
using System.Collections.Generic;
using System.Web;
using sync.contracts;

namespace sync.remotesynctable.parse
{
    static class RepoFileSerializer
    {
        public static string ToJson(this RepoFile repoFile)
        {
            return "{" + 
                        Build_Json_field("relativeFilename", HttpUtility.UrlEncode(repoFile.RelativeFileName)) + "," +
                        Build_Json_field("idInFilestore", repoFile.Id) + "," +
                        Build_Json_field("timeStamp", repoFile.TimeStamp.ToString("s")) + "," +
                        Build_Json_field("user", repoFile.User) +
                   "}";
        }

        static string Build_Json_field(string fieldname, string content)
        {
            return "\"" + fieldname + "\": \"" + content + "\"";
        }


        public static RepoFile ToRepoFile(this Dictionary<string, object> dictRepoFile)
        {
            return new RepoFile
                {
                    Id = dictRepoFile["idInFilestore"].ToString(),
                    RelativeFileName = HttpUtility.UrlDecode(dictRepoFile["relativeFilename"].ToString()),
                    TimeStamp = DateTime.Parse(dictRepoFile["timeStamp"].ToString()),
                    User = dictRepoFile["user"].ToString()
                };
        }
    }
}
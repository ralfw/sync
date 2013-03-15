using System;
using System.Collections.Generic;
using System.Web;
using sync.contracts;

namespace sync.remotesynctable.parse
{
    static class RepoFileSerializer
    {
        public static string Encode_RelativeFilename(string relativeFilename)
        {
            return HttpUtility.UrlEncode(relativeFilename);
        }

        public static string Decode_RelativeFilename(string relativeFilename)
        {
            return HttpUtility.UrlDecode(relativeFilename);
        }


        public static string ToJson(this RepoFile repoFile)
        {
            return "{" + 
                   string.Format("\"relativeFilename\": \"{0}\",\n", Encode_RelativeFilename(repoFile.RelativeFileName)) +
                   string.Format("\"idInFilestore\": \"{0}\",\n", repoFile.Id) +
                   string.Format("\"timeStamp\": \"{0}\",\n", repoFile.TimeStamp.ToString("s")) +
                   string.Format("\"user\": \"{0}\"", repoFile.User) +
                   "}";

        }

        public static RepoFile ToRepoFile(this Dictionary<string, object> dictRepoFile)
        {
            return new RepoFile
                {
                    Id = dictRepoFile["idInFilestore"].ToString(),
                    RelativeFileName = Decode_RelativeFilename(dictRepoFile["relativeFilename"].ToString()),
                    TimeStamp = DateTime.Parse(dictRepoFile["timeStamp"].ToString()),
                    User = dictRepoFile["user"].ToString()
                };
        }
    }
}
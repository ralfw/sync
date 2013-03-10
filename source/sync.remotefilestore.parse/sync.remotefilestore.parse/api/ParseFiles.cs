using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace sync.remotefilestore.parse.api
{
    public class ParseFiles
    {
        public class UploadInfo
        {
            public UploadInfo(string name, string url)
            {
                Name = name;
                Url = url;
            }

            public string Name { get; private set; }
            public string Url { get; private set; }
        }


        private const string BASE_URL = "https://api.parse.com/1/files/";

        private readonly string _appId;
        private readonly string _restKey;
        private readonly string _masterKey;


        public ParseFiles(string appId, string restKey, string masterKey)
        {
            _appId = appId;
            _restKey = restKey;
            _masterKey = masterKey;
        }


        public UploadInfo Upload(string filepath)
        {
            using (var s = new FileStream(filepath, FileMode.Open))
            {
                return Upload(s, Path.GetFileName(filepath));
            }
        }

        public UploadInfo Upload(Stream source, string filename)
        {
            var req = WebRequest.Create(BASE_URL + filename);
            req.Method = "POST";
            req.Headers.Add("X-Parse-Application-Id", _appId);
            req.Headers.Add("X-Parse-REST-API-Key", _restKey);
            req.ContentType = Get_contentType_from_filename_extension(filename);

            using (var sreq = req.GetRequestStream())
            {
                var buffer = new byte[4*1024];
                int n;
                while ((n = source.Read(buffer, 0, buffer.Length)) > 0)
                    sreq.Write(buffer, 0, n);
            }

            using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                var jsonresponse = sr.ReadToEnd();

                var jss = new JavaScriptSerializer();
                var dict = jss.Deserialize<Dictionary<string, string>>(jsonresponse);

                return new UploadInfo(dict["name"], dict["url"]);
            }
        }


        public Stream Download(string parseUrl)
        {
            var req = WebRequest.Create(parseUrl);
            var resp = req.GetResponse();
            return resp.GetResponseStream();
        }


        public void Delete(string parseFilename)
        {
            var req = WebRequest.Create(BASE_URL + parseFilename);
            req.Method = "DELETE";
            req.Headers.Add("X-Parse-Application-Id", _appId);
            req.Headers.Add("X-Parse-Master-Key", _masterKey);

            using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                sr.ReadToEnd();
            }
        }


        // http://www.iana.org/assignments/media-types
        // http://webdesign.about.com/od/sound/a/sound_mime_type.htm
        private string Get_contentType_from_filename_extension(string filename)
        {
            switch (Path.GetExtension(filename).ToLower())
            {
                case ".txt":
                    return "text/plain";
                case ".csv":
                    return "text/csv";
                case ".pdf":
                    return "application/pdf";
                case ".xml":
                    return "text/xml";

                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";

                case ".mp3":
                    return "audio/mpeg";
                case ".wav":
                    return "audio/wav";

                default:
                    return "application/octet-stream";
            }
        }
    }
}
